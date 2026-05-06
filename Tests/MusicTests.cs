using System;
using System.Collections.Generic;
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
    public class MusicTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public MusicTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var authHandler = new AuthHandler(new HttpClientHandler(), apiKey);
            _httpClient = new HttpClient(authHandler);
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            var adapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, _httpClient, null);
            _client = new MiniMaxClient(adapter);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-music-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public async Task Music_Cover_Preprocess_Success()
        {
            var audioFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "music", "我的祖国.mp3");
            audioFilePath = Path.GetFullPath(audioFilePath);
            if (!File.Exists(audioFilePath))
            {
                Console.WriteLine($"Skipping test: Audio file not found at {audioFilePath}");
                return;
            }

            var audioBytes = await File.ReadAllBytesAsync(audioFilePath);
            var audioBase64 = Convert.ToBase64String(audioBytes);

            var request = new CoverPreprocessReq
            {
                Model = CoverPreprocessReq_model.MusicCover,
                AudioBase64 = audioBase64
            };

            var response = await _client.V1.Music_cover_preprocess.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.CoverFeatureId);
            Assert.NotNull(response.FormattedLyrics);
            Assert.NotNull(response.AudioDuration);
            Console.WriteLine($"CoverFeatureId: {response.CoverFeatureId}");
            Console.WriteLine($"AudioDuration: {response.AudioDuration}s");
            Console.WriteLine($"FormattedLyrics:\n{response.FormattedLyrics}");
            if (!string.IsNullOrEmpty(response.StructureResult))
            {
                Console.WriteLine($"StructureResult: {response.StructureResult}");
            }
        }

        [Fact]
        public async Task Music_Lyrics_Generation_Write_Full_Song_Success()
        {
            var request = new GenerateLyricsReq
            {
                Mode = GenerateLyricsReq_mode.Write_full_song,
                Prompt = "一首关于春天和希望的抒情歌曲，温暖而充满活力"
            };

            var response = await _client.V1.Lyrics_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.SongTitle);
            Assert.NotNull(response.Lyrics);
            Console.WriteLine($"SongTitle: {response.SongTitle}");
            if (!string.IsNullOrEmpty(response.StyleTags))
            {
                Console.WriteLine($"StyleTags: {response.StyleTags}");
            }
            Console.WriteLine($"Lyrics:\n{response.Lyrics}");
        }

        [Fact]
        public async Task Music_Lyrics_Generation_With_Title_Success()
        {
            var request = new GenerateLyricsReq
            {
                Mode = GenerateLyricsReq_mode.Write_full_song,
                Prompt = "一首轻快的流行歌曲",
                Title = "青春的旋律"
            };

            var response = await _client.V1.Lyrics_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.SongTitle);
            Assert.Equal("青春的旋律", response.SongTitle);
            Assert.NotNull(response.Lyrics);
            Console.WriteLine($"SongTitle: {response.SongTitle}");
            Console.WriteLine($"Lyrics:\n{response.Lyrics}");
        }

        [Fact]
        public async Task Music_Lyrics_Generation_Edit_Success()
        {
            var existingLyrics = @"[Verse 1]
这是一段旧歌词
需要续写新的内容

[Chorus]
这是副歌部分";

            var request = new GenerateLyricsReq
            {
                Mode = GenerateLyricsReq_mode.Edit,
                Prompt = "续写这段歌词，保持相同的风格",
                Lyrics = existingLyrics
            };

            var response = await _client.V1.Lyrics_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Lyrics);
            Console.WriteLine($"Edited Lyrics:\n{response.Lyrics}");
        }

        [Fact]
        public async Task Music_Generation_Music26_Success()
        {
            var request = new GenerateMusicReq
            {
                Model = GenerateMusicReq_model.Music26,
                Prompt = "流行音乐, 欢快, 阳光, 夏天",
                Lyrics = @"[Verse 1]
阳光洒在海面上
浪花轻轻拍打着
我们一起奔跑
在这金色沙滩上

[Chorus]
快乐时光如此短暂
但回忆永远珍藏
让这旋律带你飞翔
到那梦想的地方",
                OutputFormat = GenerateMusicReq_output_format.Url
            };

            var response = await _client.V1.Music_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.True(response.Data.Status == 2, $"Status: {response.Data.Status}, expected 2 (completed)");
            Assert.NotNull(response.Data.Audio);
            Console.WriteLine($"Audio URL: {response.Data.Audio}");

            var audioPath = Path.Combine(_outputDir, "music_26_song.mp3");
            await DownloadFileAsync(response.Data.Audio, audioPath);
            Console.WriteLine($"Audio saved to: {audioPath}");
            Assert.True(File.Exists(audioPath));
            var fileInfo = new FileInfo(audioPath);
            Assert.True(fileInfo.Length > 0, "Downloaded file is empty");
            Console.WriteLine($"File size: {fileInfo.Length} bytes");
        }

        [Fact]
        public async Task Music_Generation_With_CoverFeatureId_Success()
        {
            var audioFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "music", "我的祖国.mp3");
            audioFilePath = Path.GetFullPath(audioFilePath);
            if (!File.Exists(audioFilePath))
            {
                Console.WriteLine($"Skipping test: Audio file not found at {audioFilePath}");
                return;
            }

            var audioBytes = await File.ReadAllBytesAsync(audioFilePath);
            var audioBase64 = Convert.ToBase64String(audioBytes);

            var preprocessRequest = new CoverPreprocessReq
            {
                Model = CoverPreprocessReq_model.MusicCover,
                AudioBase64 = audioBase64
            };

            var preprocessResponse = await _client.V1.Music_cover_preprocess.PostAsync(preprocessRequest);
            Assert.NotNull(preprocessResponse);
            Assert.NotNull(preprocessResponse.BaseResp);
            Assert.True(preprocessResponse.BaseResp.StatusCode == 0, "Preprocess failed");
            Assert.NotNull(preprocessResponse.CoverFeatureId);
            Console.WriteLine($"CoverFeatureId: {preprocessResponse.CoverFeatureId}");

            var lyrics = preprocessResponse.FormattedLyrics;
            if (string.IsNullOrEmpty(lyrics) || lyrics.Length < 10)
            {
                lyrics = @"[Verse 1]
这是一首经典老歌
旋律优美动听

[Chorus]
让我们一起歌唱";
            }

            var musicRequest = new GenerateMusicReq
            {
                Model = GenerateMusicReq_model.MusicCover,
                Prompt = "抒情的流行版本",
                CoverFeatureId = preprocessResponse.CoverFeatureId,
                Lyrics = lyrics,
                OutputFormat = GenerateMusicReq_output_format.Url
            };

            var musicResponse = await _client.V1.Music_generation.PostAsync(musicRequest);

            Assert.NotNull(musicResponse);
            Assert.NotNull(musicResponse.BaseResp);
            Assert.True(musicResponse.BaseResp.StatusCode == 0, $"StatusCode: {musicResponse.BaseResp.StatusCode}");
            Assert.NotNull(musicResponse.Data);
            Assert.True(musicResponse.Data.Status == 2, $"Status: {musicResponse.Data.Status}");
            Assert.NotNull(musicResponse.Data.Audio);
            Console.WriteLine($"Cover music URL: {musicResponse.Data.Audio}");

            var audioPath = Path.Combine(_outputDir, "music_cover_song.mp3");
            await DownloadFileAsync(musicResponse.Data.Audio, audioPath);
            Console.WriteLine($"Audio saved to: {audioPath}");
            Assert.True(File.Exists(audioPath));
            var fileInfo = new FileInfo(audioPath);
            Assert.True(fileInfo.Length > 0, "Downloaded file is empty");
            Console.WriteLine($"File size: {fileInfo.Length} bytes");
        }

        [Fact]
        public async Task Music_Generation_Instrumental_Success()
        {
            var request = new GenerateMusicReq
            {
                Model = GenerateMusicReq_model.Music26,
                Prompt = "古典钢琴, 安静, 平和, 冥想",
                IsInstrumental = true,
                OutputFormat = GenerateMusicReq_output_format.Url
            };

            var response = await _client.V1.Music_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.True(response.Data.Status == 2, $"Status: {response.Data.Status}");
            Assert.NotNull(response.Data.Audio);
            Console.WriteLine($"Instrumental URL: {response.Data.Audio}");

            var audioPath = Path.Combine(_outputDir, "music_instrumental.mp3");
            await DownloadFileAsync(response.Data.Audio, audioPath);
            Console.WriteLine($"Audio saved to: {audioPath}");
            Assert.True(File.Exists(audioPath));
            var fileInfo = new FileInfo(audioPath);
            Assert.True(fileInfo.Length > 0, "Downloaded file is empty");
            Console.WriteLine($"File size: {fileInfo.Length} bytes");
        }

        [Fact]
        public async Task Music_Generation_With_AudioSetting_Success()
        {
            var request = new GenerateMusicReq
            {
                Model = GenerateMusicReq_model.Music26,
                Prompt = "电子音乐, 动感, 活力",
                Lyrics = @"[Verse 1]
电波穿越夜空
节奏点燃心中火

[Chorus]
跟随这节拍起舞",
                AudioSetting = new AudioSetting
                {
                    SampleRate = 44100,
                    Bitrate = 256000,
                    Format = AudioSetting_format.Mp3
                },
                OutputFormat = GenerateMusicReq_output_format.Url
            };

            var response = await _client.V1.Music_generation.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.True(response.Data.Status == 2, $"Status: {response.Data.Status}");
            Assert.NotNull(response.Data.Audio);
            Console.WriteLine($"Audio setting music URL: {response.Data.Audio}");

            var audioPath = Path.Combine(_outputDir, "music_with_setting.mp3");
            await DownloadFileAsync(response.Data.Audio, audioPath);
            Console.WriteLine($"Audio saved to: {audioPath}");
            Assert.True(File.Exists(audioPath));
            var fileInfo = new FileInfo(audioPath);
            Assert.True(fileInfo.Length > 0, "Downloaded file is empty");
            Console.WriteLine($"File size: {fileInfo.Length} bytes");
        }

        private async Task DownloadFileAsync(string url, string outputPath)
        {
            using var downloadClient = new HttpClient();
            downloadClient.Timeout = TimeSpan.FromMinutes(5);
            var response = await downloadClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputPath, bytes);
        }
    }
}