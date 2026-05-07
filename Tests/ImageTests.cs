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
            var portraitRequest = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "A beautiful young woman with long black hair, portrait photo, front view, realistic",
                AspectRatio = ImageAspectRatio.R1_1,
                ResponseFormat = ImageResponseFormat.Url,
                N = 1
            };

            var portraitResponse = await _client.GenerateImageAsync(portraitRequest);

            Assert.NotNull(portraitResponse);
            Assert.NotNull(portraitResponse.BaseResp);
            Assert.True(portraitResponse.BaseResp.StatusCode == 0, $"StatusCode: {portraitResponse.BaseResp.StatusCode}");
            Assert.NotNull(portraitResponse.Data);
            Assert.NotNull(portraitResponse.Data.ImageUrls);
            Assert.NotEmpty(portraitResponse.Data.ImageUrls);

            var inputPath = await DownloadImageAsync(portraitResponse.Data.ImageUrls[0], "i2i_portrait.jpg");
            Console.WriteLine("Portrait image saved to: " + inputPath);

            var imageBase64 = await ImageToBase64Async(inputPath);

            var i2iRequest = new ImageGenerationReq
            {
                Model = ImageModel.Image01,
                Prompt = "The same person wearing a red dress, elegant style, professional photography",
                AspectRatio = ImageAspectRatio.R16_9,
                ResponseFormat = ImageResponseFormat.Url,
                N = 1,
                SubjectReference = new List<ImageSubjectReference>
                {
                    new ImageSubjectReference
                    {
                        Type = "character",
                        ImageFile = imageBase64
                    }
                }
            };

            var i2iResponse = await _client.GenerateImageAsync(i2iRequest);

            Assert.NotNull(i2iResponse);
            Assert.NotNull(i2iResponse.BaseResp);
            Assert.True(i2iResponse.BaseResp.StatusCode == 0, $"StatusCode: {i2iResponse.BaseResp.StatusCode}");
            Assert.NotNull(i2iResponse.Data);
            Assert.NotNull(i2iResponse.Data.ImageUrls);
            Assert.NotEmpty(i2iResponse.Data.ImageUrls);

            var outputPath = await DownloadImageAsync(i2iResponse.Data.ImageUrls[0], "i2i_output.jpg");
            Console.WriteLine("I2I image saved to: " + outputPath);
            Assert.True(new FileInfo(outputPath).Length > 0);
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

        private async Task<string> ImageToBase64Async(string imagePath)
        {
            var bytes = await File.ReadAllBytesAsync(imagePath);
            var base64 = Convert.ToBase64String(bytes);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}
