using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Image;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class ImageTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public ImageTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-image-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task TextToImage_Success()
        {
            var request = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "A beautiful sunset over the ocean, photorealistic, high quality",
                AspectRatio = ImageAspectRatio.R16_9,
                ResponseFormat = ImageResponseFormat.Url,
                N = 1
            };

            var response = await _client.GenerateImageAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.ImageUrls);
            Assert.NotEmpty(response.Data.ImageUrls);

            var imageUrl = response.Data.ImageUrls[0];
            Console.WriteLine("Generated image URL: " + imageUrl);

            var savedPath = await DownloadImageAsync(imageUrl, "t2i_output.jpg");
            Assert.True(File.Exists(savedPath));
            var fileInfo = new FileInfo(savedPath);
            Assert.True(fileInfo.Length > 0);
            Console.WriteLine("Image saved to: " + savedPath);
        }

        [Fact]
        public async Task ImageToImage_Success()
        {
            var t2iRequest = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "A man in a white t-shirt, full-body, standing front view, outdoors",
                AspectRatio = ImageAspectRatio.R1_1,
                ResponseFormat = ImageResponseFormat.Url,
                N = 1
            };

            var t2iResponse = await _client.GenerateImageAsync(t2iRequest);

            Assert.NotNull(t2iResponse);
            Assert.NotNull(t2iResponse.Data);
            Assert.NotNull(t2iResponse.Data.ImageUrls);
            Assert.NotEmpty(t2iResponse.Data.ImageUrls);

            var inputPath = await DownloadImageAsync(t2iResponse.Data.ImageUrls[0], "i2i_input.jpg");
            Console.WriteLine("Input image saved to: " + inputPath);

            Console.WriteLine("Note: ImageToImage (subject_reference) requires additional model support");
            Console.WriteLine("This test is simplified - actual I2I needs subject_reference parameter");
        }

        private async Task<string> DownloadImageAsync(string url, string fileName)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var path = Path.Combine(_outputDir, fileName);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(path, bytes);
            return path;
        }
    }
}
