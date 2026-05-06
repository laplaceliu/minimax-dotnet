using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Xunit;
using MiniMax.Models;
using MiniMaxClient = MiniMax.MiniMaxClient;

namespace Tests
{
    public class ImageTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public ImageTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var authHandler = new AuthHandler(new HttpClientHandler(), apiKey);
            _httpClient = new HttpClient(authHandler);
            var adapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, _httpClient, null);
            _client = new MiniMaxClient(adapter);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-image-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public async Task TextToImage_Saves_File_To_Disk()
        {
            var request = new ImageGenerationReq
            {
                Model = ImageGenerationReq_model.Image01,
                Prompt = "A beautiful sunset over the ocean, photorealistic, high quality",
                AspectRatio = ImageGenerationReq_aspect_ratio.OneSixNine,
                ResponseFormat = ImageGenerationReq_response_format.Url,
                N = 1
            };

            var response = await _client.V1.Image_generation.PostAsync(request);

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
        public async Task ImageToImage_Uses_Saved_File()
        {
            var t2iRequest = new ImageGenerationReq
            {
                Model = ImageGenerationReq_model.Image01,
                Prompt = "A man in a white t-shirt, full-body, standing front view, outdoors",
                AspectRatio = ImageGenerationReq_aspect_ratio.OneOne,
                ResponseFormat = ImageGenerationReq_response_format.Url,
                N = 1
            };

            var t2iResponse = await _client.V1.Image_generation.PostAsync(t2iRequest);

            Assert.NotNull(t2iResponse);
            Assert.NotNull(t2iResponse.Data);
            Assert.NotEmpty(t2iResponse.Data.ImageUrls);

            var inputPath = await DownloadImageAsync(t2iResponse.Data.ImageUrls[0], "i2i_input.jpg");
            Console.WriteLine("Input image saved to: " + inputPath);

            var imageBytes = await File.ReadAllBytesAsync(inputPath);
            var base64Image = Convert.ToBase64String(imageBytes);
            var dataUrl = "data:image/jpeg;base64," + base64Image;

            var subjectRef = new Dictionary<string, object>
            {
                { "type", "character" },
                { "image_file", dataUrl }
            };

            var i2iRequest = new ImageGenerationReq
            {
                Model = ImageGenerationReq_model.Image01,
                Prompt = "Transform the person into a medieval knight",
                AspectRatio = ImageGenerationReq_aspect_ratio.OneOne,
                ResponseFormat = ImageGenerationReq_response_format.Url,
                N = 1
            };
            i2iRequest.AdditionalData["subject_reference"] = new List<Dictionary<string, object>> { subjectRef };

            var i2iResponse = await _client.V1.Image_generation.PostAsync(i2iRequest);

            Assert.NotNull(i2iResponse);
            Assert.NotNull(i2iResponse.BaseResp);
            Assert.True(i2iResponse.BaseResp.StatusCode == 0, $"StatusCode: {i2iResponse.BaseResp.StatusCode}");
            Assert.NotNull(i2iResponse.Data);
            Assert.NotEmpty(i2iResponse.Data.ImageUrls);

            var outputPath = await DownloadImageAsync(i2iResponse.Data.ImageUrls[0], "i2i_output.jpg");
            Assert.True(File.Exists(outputPath));
            Console.WriteLine("I2I output saved to: " + outputPath);
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