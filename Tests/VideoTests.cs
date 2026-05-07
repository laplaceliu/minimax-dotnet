using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Video;
using MiniMax.Models.Image;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class VideoTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;
        private readonly string _taskIdFile;
        private readonly string _fileIdFile;

        public VideoTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-video-tests");
            Directory.CreateDirectory(_outputDir);
            _taskIdFile = Path.Combine(_outputDir, "last_task_id.txt");
            _fileIdFile = Path.Combine(_outputDir, "last_file_id.txt");
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task Video_Generation_Create_Success()
        {
            var request = new VideoGenerationReq
            {
                Model = VideoModel.MiniMaxHailuo23,
                Prompt = "A beautiful sunset over the ocean with waves crashing on the beach",
                Duration = 6,
                Resolution = VideoResolution.P768,
                PromptOptimizer = false
            };

            var response = await _client.GenerateVideoAsync(request);

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

            var response = await _client.QueryVideoGenerationAsync(taskId);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Query failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.TaskId);
            Assert.NotNull(response.Status);
            Console.WriteLine($"TaskId: {response.TaskId}");
            Console.WriteLine($"Status: {response.Status}");

            if (response.Status == "success" && !string.IsNullOrEmpty(response.FileId))
            {
                Console.WriteLine($"FileId: {response.FileId}");
                await File.WriteAllTextAsync(_fileIdFile, response.FileId);
                Console.WriteLine($"FileId saved to: {_fileIdFile}");
            }
            else
            {
                Console.WriteLine($"Note: Video is still {response.Status}. Query again later.");
            }
        }

        [Fact]
        public async Task Video_Generation_Download_Success()
        {
            if (!File.Exists(_fileIdFile))
            {
                Console.WriteLine($"No FileId file found at {_fileIdFile}. Run Video_Generation_Query_Status after video is ready.");
                Assert.Fail("FileId file not found. Run Video_Generation_Query_Status first.");
                return;
            }

            var fileIdStr = (await File.ReadAllTextAsync(_fileIdFile)).Trim();
            var fileId = long.Parse(fileIdStr);
            Console.WriteLine($"Downloading FileId: {fileId}");

            var fileResponse = await _client.RetrieveFileAsync(fileId);

            Assert.NotNull(fileResponse);
            Assert.NotNull(fileResponse.BaseResp);
            Assert.True(fileResponse.BaseResp.StatusCode == 0, $"Retrieve failed: {fileResponse.BaseResp.StatusCode}");
            Console.WriteLine($"File retrieved: {fileResponse.File?.Filename}");
        }

        [Fact]
        public async Task Video_I2V_Generation_Create_Success()
        {
            var imageRequest = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "A cute cat sitting on a windowsill, photorealistic, high quality",
                AspectRatio = ImageAspectRatio.R16_9,
                ResponseFormat = ImageResponseFormat.Url,
                N = 1
            };

            var imageResponse = await _client.GenerateImageAsync(imageRequest);
            Assert.NotNull(imageResponse);
            Assert.NotNull(imageResponse.BaseResp);
            Assert.True(imageResponse.BaseResp.StatusCode == 0, $"Image generation failed: {imageResponse.BaseResp.StatusCode}");
            Assert.NotNull(imageResponse.Data);
            Assert.NotNull(imageResponse.Data.ImageUrls);
            Assert.True(imageResponse.Data.ImageUrls.Count > 0, "No image URLs returned");
            var imageUrl = imageResponse.Data.ImageUrls[0];
            Console.WriteLine($"Generated image URL: {imageUrl}");

            Console.WriteLine("Note: I2V (Image to Video) requires first_frame_image parameter which is not in the current model");
            Console.WriteLine("This test is simplified - actual I2V needs additional request properties");
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
    }
}
