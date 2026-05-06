using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using Xunit;
using MiniMax.Models;
using MiniMaxClient = MiniMax.MiniMaxClient;

namespace Tests
{
    public class VideoTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;
        private readonly string _taskIdFile;

        public VideoTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var authHandler = new AuthHandler(new HttpClientHandler(), apiKey);
            _httpClient = new HttpClient(authHandler);
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            var adapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, _httpClient, null);
            _client = new MiniMaxClient(adapter);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-video-tests");
            Directory.CreateDirectory(_outputDir);
            _taskIdFile = Path.Combine(_outputDir, "last_task_id.txt");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public async Task Video_Generation_Create_Success()
        {
            var request = new VideoGenerationReq
            {
                Model = VideoGenerationReq_model.MiniMaxHailuo23,
                Prompt = "A beautiful sunset over the ocean with waves crashing on the beach [固定]",
                Duration = 6,
                Resolution = VideoGenerationReq_resolution.SevenSixEightP,
                PromptOptimizer = false
            };

            var response = await _client.V1.Video_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Create failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.TaskId);
            Console.WriteLine($"Task created successfully!");
            Console.WriteLine($"TaskId: {response.TaskId}");

            await File.WriteAllTextAsync(_taskIdFile, response.TaskId);
            Console.WriteLine($"TaskId saved to: {_taskIdFile}");
        }

        [Fact]
        public async Task Video_Generation_Query_Status_Success()
        {
            if (!File.Exists(_taskIdFile))
            {
                Console.WriteLine($"No TaskId file found at {_taskIdFile}. Run Video_Generation_Create first.");
                Assert.Fail("TaskId file not found. Run Video_Generation_Create first.");
                return;
            }

            var taskId = (await File.ReadAllTextAsync(_taskIdFile)).Trim();
            Assert.NotNull(taskId);
            Console.WriteLine($"Querying TaskId: {taskId}");

            var response = await _client.V1.Query.Video_generation.GetAsync(x =>
            {
                x.QueryParameters.TaskId = taskId;
            });

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Query failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.TaskId);
            Assert.NotNull(response.Status);
            Console.WriteLine($"TaskId: {response.TaskId}");
            Console.WriteLine($"Status: {response.Status}");

            if (response.Status == VideoProcessStatus.Success)
            {
                Assert.NotNull(response.FileId);
                Console.WriteLine($"FileId: {response.FileId}");
                Console.WriteLine($"Video dimensions: {response.VideoWidth}x{response.VideoHeight}");

                var fileIdFile = Path.Combine(_outputDir, "last_file_id.txt");
                await File.WriteAllTextAsync(fileIdFile, response.FileId);
                Console.WriteLine($"FileId saved to: {fileIdFile}");
            }
            else
            {
                Console.WriteLine($"Note: Video is still {response.Status}. Query again later.");
            }
        }

        [Fact]
        public async Task Video_Generation_Download_Success()
        {
            var fileIdFile = Path.Combine(_outputDir, "last_file_id.txt");
            if (!File.Exists(fileIdFile))
            {
                Console.WriteLine($"No FileId file found at {fileIdFile}. Run Video_Generation_Query_Status after video is ready.");
                Assert.Fail("FileId file not found. Run Video_Generation_Query_Status first.");
                return;
            }

            var fileIdStr = (await File.ReadAllTextAsync(fileIdFile)).Trim();
            var fileId = long.Parse(fileIdStr);
            Console.WriteLine($"Downloading FileId: {fileId}");

            var fileResponse = await _client.V1.Files.Retrieve.GetAsync(x =>
            {
                x.QueryParameters.FileId = fileId;
            });

            Assert.NotNull(fileResponse);
            Assert.NotNull(fileResponse.File);

            string? downloadUrl = null;
            if (fileResponse.File.AdditionalData != null &&
                fileResponse.File.AdditionalData.ContainsKey("download_url"))
            {
                downloadUrl = fileResponse.File.AdditionalData["download_url"] as string;
            }
            Assert.True(downloadUrl != null, "download_url not found in response");
            Console.WriteLine($"Download URL: {downloadUrl}");

            var videoPath = Path.Combine(_outputDir, $"video_{fileId}.mp4");
            await DownloadFileAsync(downloadUrl, videoPath);
            Console.WriteLine($"Video saved to: {videoPath}");
            Assert.True(File.Exists(videoPath));
            var fileInfo = new FileInfo(videoPath);
            Assert.True(fileInfo.Length > 0, "Downloaded file is empty");
            Console.WriteLine($"File size: {fileInfo.Length} bytes");
        }

        private async Task DownloadFileAsync(string url, string outputPath)
        {
            using var downloadClient = new HttpClient();
            downloadClient.Timeout = TimeSpan.FromMinutes(10);
            var response = await downloadClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, bytes);
        }

        [Fact]
        public async Task Video_I2V_Generation_Create_Success()
        {
            var imageRequest = new ImageGenerationReq
            {
                Model = ImageGenerationReq_model.Image01,
                Prompt = "A cute cat sitting on a windowsill, photorealistic, high quality",
                AspectRatio = ImageGenerationReq_aspect_ratio.OneSixNine,
                ResponseFormat = ImageGenerationReq_response_format.Url,
                N = 1
            };

            var imageResponse = await _client.V1.Image_generation.PostAsync(imageRequest);
            Assert.NotNull(imageResponse);
            Assert.NotNull(imageResponse.BaseResp);
            Assert.True(imageResponse.BaseResp.StatusCode == 0, $"Image generation failed: {imageResponse.BaseResp.StatusCode}");
            Assert.NotNull(imageResponse.Data);
            Assert.NotNull(imageResponse.Data.ImageUrls);
            Assert.True(imageResponse.Data.ImageUrls.Count > 0, "No image URLs returned");
            var imageUrl = imageResponse.Data.ImageUrls[0];
            Console.WriteLine($"Generated image URL: {imageUrl}");

            var videoRequest = new VideoGenerationReq
            {
                Model = VideoGenerationReq_model.MiniMaxHailuo23Fast,
                Prompt = "The cat looks around curiously and stretches [固定]",
                Duration = 6,
                Resolution = VideoGenerationReq_resolution.SevenSixEightP,
                PromptOptimizer = false
            };
            videoRequest.AdditionalData["first_frame_image"] = imageUrl;

            var videoResponse = await _client.V1.Video_generation.PostAsync(videoRequest);
            Assert.NotNull(videoResponse);
            Assert.NotNull(videoResponse.BaseResp);
            Assert.True(videoResponse.BaseResp.StatusCode == 0, $"Video creation failed: {videoResponse.BaseResp.StatusCode} - {videoResponse.BaseResp.StatusMsg}");
            Assert.NotNull(videoResponse.TaskId);
            Console.WriteLine($"I2V Task created successfully!");
            Console.WriteLine($"TaskId: {videoResponse.TaskId}");

            await File.WriteAllTextAsync(_taskIdFile, videoResponse.TaskId);
            Console.WriteLine($"TaskId saved to: {_taskIdFile}");
        }
    }
}