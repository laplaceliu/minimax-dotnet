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
        private readonly string _i2vTaskIdFile;
        private readonly string _i2vFileIdFile;

        public VideoTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-video-tests");
            Directory.CreateDirectory(_outputDir);
            _taskIdFile = Path.Combine(_outputDir, "last_task_id.txt");
            _fileIdFile = Path.Combine(_outputDir, "last_file_id.txt");
            _i2vTaskIdFile = Path.Combine(_outputDir, "last_i2v_task_id.txt");
            _i2vFileIdFile = Path.Combine(_outputDir, "last_i2v_file_id.txt");
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

            if (response.Status == VideoProcessStatus.Success && !string.IsNullOrEmpty(response.FileId))
            {
                Console.WriteLine($"FileId: {response.FileId}");
                await File.WriteAllTextAsync(_fileIdFile, response.FileId);
                Console.WriteLine($"FileId saved to: {_fileIdFile}");
            }
            else if (response.Status == VideoProcessStatus.Success && string.IsNullOrEmpty(response.FileId))
            {
                Console.WriteLine($"Video status is Success but FileId is empty. Video may still be processing...");
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
            Console.WriteLine($"File metadata retrieved: {fileResponse.File?.Filename}");

            Assert.NotNull(fileResponse.File?.DownloadUrl);
            var downloadUrl = fileResponse.File!.DownloadUrl!;
            Console.WriteLine($"Downloading from: {downloadUrl}");

            var outputPath = Path.Combine(_outputDir, $"video_{fileId}.mp4");
            using var downloadClient = new HttpClient();
            var videoBytes = await downloadClient.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(outputPath, videoBytes);
            Console.WriteLine($"Video downloaded to: {outputPath}");
            Assert.True(File.Exists(outputPath), "Video file should exist after download");
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

            var videoRequest = new VideoGenerationReq
            {
                Model = VideoModel.MiniMaxHailuo23Fast,
                FirstFrameImage = imageUrl,
                Prompt = "The cat looks around curiously, then yawns",
                Duration = 6,
                Resolution = VideoResolution.P768,
                PromptOptimizer = false
            };

            var videoResponse = await _client.GenerateVideoAsync(videoRequest);
            Assert.NotNull(videoResponse);
            Assert.NotNull(videoResponse.BaseResp);
            Assert.True(videoResponse.BaseResp.StatusCode == 0, $"I2V create failed: {videoResponse.BaseResp.StatusCode} - {videoResponse.BaseResp.StatusMsg}");
            Assert.NotNull(videoResponse.TaskId);
            Console.WriteLine($"I2V Task created successfully!");
            Console.WriteLine($"TaskId: {videoResponse.TaskId}");

            await File.WriteAllTextAsync(_i2vTaskIdFile, videoResponse.TaskId);
            Console.WriteLine($"TaskId saved to: {_i2vTaskIdFile}");
        }

        [Fact]
        public async Task Video_I2V_Generation_Query_Status_Success()
        {
            if (!File.Exists(_i2vTaskIdFile))
            {
                Console.WriteLine($"No TaskId file found at {_i2vTaskIdFile}. Run Video_I2V_Generation_Create_Success first.");
                Assert.Fail("TaskId file not found. Run Video_I2V_Generation_Create_Success first.");
                return;
            }

            var taskId = (await File.ReadAllTextAsync(_i2vTaskIdFile)).Trim();
            Assert.NotNull(taskId);
            Console.WriteLine($"Querying I2V TaskId: {taskId}");

            var response = await _client.QueryVideoGenerationAsync(taskId);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Query failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.TaskId);
            Assert.NotNull(response.Status);
            Console.WriteLine($"TaskId: {response.TaskId}");
            Console.WriteLine($"Status: {response.Status}");

            if (response.Status == VideoProcessStatus.Success && !string.IsNullOrEmpty(response.FileId))
            {
                Console.WriteLine($"FileId: {response.FileId}");
                await File.WriteAllTextAsync(_i2vFileIdFile, response.FileId);
                Console.WriteLine($"FileId saved to: {_i2vFileIdFile}");
            }
            else if (response.Status == VideoProcessStatus.Success && string.IsNullOrEmpty(response.FileId))
            {
                Console.WriteLine($"Video status is Success but FileId is empty. Video may still be processing...");
            }
            else
            {
                Console.WriteLine($"Note: Video is still {response.Status}. Query again later.");
            }
        }

        [Fact]
        public async Task Video_I2V_Generation_Download_Success()
        {
            if (!File.Exists(_i2vFileIdFile))
            {
                Console.WriteLine($"No FileId file found at {_i2vFileIdFile}. Run Video_I2V_Generation_Query_Status after video is ready.");
                Assert.Fail("FileId file not found. Run Video_I2V_Generation_Query_Status first.");
                return;
            }

            var fileIdStr = (await File.ReadAllTextAsync(_i2vFileIdFile)).Trim();
            var fileId = long.Parse(fileIdStr);
            Console.WriteLine($"Downloading I2V FileId: {fileId}");

            var fileResponse = await _client.RetrieveFileAsync(fileId);

            Assert.NotNull(fileResponse);
            Assert.NotNull(fileResponse.BaseResp);
            Assert.True(fileResponse.BaseResp.StatusCode == 0, $"Retrieve failed: {fileResponse.BaseResp.StatusCode}");
            Console.WriteLine($"File metadata retrieved: {fileResponse.File?.Filename}");

            Assert.NotNull(fileResponse.File?.DownloadUrl);
            var downloadUrl = fileResponse.File!.DownloadUrl!;
            Console.WriteLine($"Downloading from: {downloadUrl}");

            var outputPath = Path.Combine(_outputDir, $"i2v_video_{fileId}.mp4");
            using var downloadClient = new HttpClient();
            var videoBytes = await downloadClient.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(outputPath, videoBytes);
            Console.WriteLine($"Video downloaded to: {outputPath}");
            Assert.True(File.Exists(outputPath), "Video file should exist after download");
        }

        [Fact]
        public async Task Video_S2V_Generation_Create_Success()
        {
            Console.WriteLine("Note: S2V model (S2V-01) may not be available for this API key");
            Console.WriteLine("This test will show the actual API response");

            var imageRequest = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "A smiling young woman face, front view, neutral background",
                AspectRatio = ImageAspectRatio.R3_4,
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
            Console.WriteLine($"Generated subject image URL: {imageUrl}");

            var videoRequest = new VideoGenerationReq
            {
                Model = VideoModel.S2V01,
                Prompt = "The person turns their head slightly to the left",
                SubjectReference = new List<SubjectReference>
                {
                    new SubjectReference
                    {
                        Type = "character",
                        Image = new List<string> { imageUrl }
                    }
                },
                PromptOptimizer = false
            };

            var videoResponse = await _client.GenerateVideoAsync(videoRequest);
            Assert.NotNull(videoResponse);
            Assert.NotNull(videoResponse.BaseResp);
            Console.WriteLine($"S2V Response - StatusCode: {videoResponse.BaseResp.StatusCode}, StatusMsg: {videoResponse.BaseResp.StatusMsg}");
            if (videoResponse.BaseResp.StatusCode == 0)
            {
                Assert.NotNull(videoResponse.TaskId);
                Console.WriteLine($"S2V Task created successfully! TaskId: {videoResponse.TaskId}");
            }
            else
            {
                Console.WriteLine($"S2V Task creation failed (expected if no permission): {videoResponse.BaseResp.StatusCode} - {videoResponse.BaseResp.StatusMsg}");
            }
        }

        [Fact]
        public async Task Video_FL2V_Generation_Create_Success()
        {
            Console.WriteLine("Note: FL2V model (MiniMax-Hailuo-02) may not be available for this API key");
            Console.WriteLine("This test will show the actual API response");

            var imageRequest = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "A beautiful flower in full bloom, garden setting",
                AspectRatio = ImageAspectRatio.R3_4,
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
            Console.WriteLine($"Generated first frame image URL: {imageUrl}");

            var videoRequest = new VideoGenerationReq
            {
                Model = VideoModel.MiniMaxHailuo02,
                FirstFrameImage = imageUrl,
                LastFrameImage = imageUrl,
                Prompt = "The flower gently sways in the breeze",
                Duration = 6,
                Resolution = VideoResolution.P768,
                PromptOptimizer = false
            };

            var videoResponse = await _client.GenerateVideoAsync(videoRequest);
            Assert.NotNull(videoResponse);
            Assert.NotNull(videoResponse.BaseResp);
            Console.WriteLine($"FL2V Response - StatusCode: {videoResponse.BaseResp.StatusCode}, StatusMsg: {videoResponse.BaseResp.StatusMsg}");
            if (videoResponse.BaseResp.StatusCode == 0)
            {
                Assert.NotNull(videoResponse.TaskId);
                Console.WriteLine($"FL2V Task created successfully! TaskId: {videoResponse.TaskId}");
            }
            else
            {
                Console.WriteLine($"FL2V Task creation failed (expected if no permission): {videoResponse.BaseResp.StatusCode} - {videoResponse.BaseResp.StatusMsg}");
            }
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
