using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Music;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class MusicTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public MusicTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-music-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _client.Dispose();
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
                Model = "cover-preprocess-01",
                AudioBase64 = audioBase64
            };

            var response = await _client.CoverPreprocessAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.CoverFeatureId);
            Console.WriteLine($"CoverFeatureId: {response.CoverFeatureId}");
            if (response.AudioDuration.HasValue)
                Console.WriteLine($"AudioDuration: {response.AudioDuration}s");
        }

        [Fact]
        public async Task Music_Lyrics_Generation_Write_Full_Song_Success()
        {
            var request = new GenerateLyricsReq
            {
                Mode = LyricsMode.WriteFullSong,
                Prompt = "一首关于春天和希望的抒情歌曲，温暖而充满活力"
            };

            var response = await _client.GenerateLyricsAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.TaskId);
            Console.WriteLine($"TaskId: {response.TaskId}");
        }

        [Fact]
        public async Task Music_Lyrics_Generation_With_Title_Success()
        {
            var request = new GenerateLyricsReq
            {
                Mode = LyricsMode.WriteFullSong,
                Prompt = "一首轻快的流行歌曲",
                Title = "青春的旋律"
            };

            var response = await _client.GenerateLyricsAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.SongTitle);
            Assert.Equal("青春的旋律", response.SongTitle);
            Console.WriteLine($"SongTitle: {response.SongTitle}");
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
                Mode = LyricsMode.Edit,
                Prompt = "续写这段歌词，保持相同的风格",
                Lyrics = existingLyrics
            };

            var response = await _client.GenerateLyricsAsync(request);

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
                Model = MusicModel.Music26,
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
                OutputFormat = MusicOutputFormat.Url
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(600));
            var response = await _client.GenerateMusicAsync(request, cts.Token);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Audio);
            Console.WriteLine($"Music Data Status: {response.Data.Status}");

            var outputPath = Path.Combine(_outputDir, "music26_output.mp3");
            await DownloadFileAsync(response.Data.Audio, outputPath);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Console.WriteLine($"Music saved to: {outputPath}");

            if (response.ExtraInfo != null)
            {
                Console.WriteLine($"Duration: {response.ExtraInfo.MusicDuration}ms");
            }
        }

        [Fact]
        public async Task Music_Generation_Instrumental_Success()
        {
            var request = new GenerateMusicReq
            {
                Model = MusicModel.Music26,
                Prompt = "古典钢琴, 安静, 平和, 冥想",
                IsInstrumental = true,
                OutputFormat = MusicOutputFormat.Url
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(600));
            var response = await _client.GenerateMusicAsync(request, cts.Token);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Audio);
            Console.WriteLine($"Music Data Status: {response.Data.Status}");

            var outputPath = Path.Combine(_outputDir, "instrumental_output.mp3");
            await DownloadFileAsync(response.Data.Audio, outputPath);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Console.WriteLine($"Music saved to: {outputPath}");
        }

        [Fact]
        public async Task Music_Generation_With_AudioSetting_Success()
        {
            var request = new GenerateMusicReq
            {
                Model = MusicModel.Music26,
                Prompt = "电子音乐, 动感, 活力",
                Lyrics = @"[Verse 1]
电波穿越夜空
节奏点燃心中火

[Chorus]
跟随这节拍起舞",
                AudioSetting = new MusicAudioSetting
                {
                    SampleRate = 44100,
                    Bitrate = 256000,
                    Format = "mp3"
                },
                OutputFormat = MusicOutputFormat.Url
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(600));
            var response = await _client.GenerateMusicAsync(request, cts.Token);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Audio);
            Console.WriteLine($"Music Data Status: {response.Data.Status}");

            var outputPath = Path.Combine(_outputDir, "electronic_output.mp3");
            await DownloadFileAsync(response.Data.Audio, outputPath);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Console.WriteLine($"Music saved to: {outputPath}");

            if (response.ExtraInfo != null)
            {
                Console.WriteLine($"Duration: {response.ExtraInfo.MusicDuration}ms");
                Console.WriteLine($"Sample Rate: {response.ExtraInfo.MusicSampleRate}");
                Console.WriteLine($"Bitrate: {response.ExtraInfo.Bitrate}");
                Console.WriteLine($"Size: {response.ExtraInfo.MusicSize} bytes");
            }
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
                Model = "cover-preprocess-01",
                AudioBase64 = audioBase64
            };

            var preprocessResponse = await _client.CoverPreprocessAsync(preprocessRequest);
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
                Model = MusicModel.MusicCover,
                Prompt = "抒情的流行版本",
                CoverFeatureId = preprocessResponse.CoverFeatureId,
                Lyrics = lyrics,
                OutputFormat = MusicOutputFormat.Url
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(600));
            var musicResponse = await _client.GenerateMusicAsync(musicRequest, cts.Token);

            Assert.NotNull(musicResponse);
            Assert.NotNull(musicResponse.BaseResp);
            Assert.True(musicResponse.BaseResp.StatusCode == 0, $"StatusCode: {musicResponse.BaseResp.StatusCode}");
            Assert.NotNull(musicResponse.Data);
            Assert.NotNull(musicResponse.Data.Audio);
            Console.WriteLine($"Cover music Data Status: {musicResponse.Data.Status}");

            var outputPath = Path.Combine(_outputDir, "cover_output.mp3");
            await DownloadFileAsync(musicResponse.Data.Audio, outputPath);
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Console.WriteLine($"Cover music saved to: {outputPath}");
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
