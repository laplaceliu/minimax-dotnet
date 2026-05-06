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
    public class VoiceTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;
        private readonly string _voiceIdFile;

        public VoiceTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var authHandler = new AuthHandler(new HttpClientHandler(), apiKey);
            _httpClient = new HttpClient(authHandler);
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            var adapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, _httpClient, null);
            _client = new MiniMaxClient(adapter);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-voice-tests");
            Directory.CreateDirectory(_outputDir);
            _voiceIdFile = Path.Combine(_outputDir, "last_voice_id.txt");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public async Task Voice_Design_Create_Success()
        {
            var tokenKey = Environment.GetEnvironmentVariable("MINIMAX_TOKEN_KEY") ?? throw new InvalidOperationException("MINIMAX_TOKEN_KEY not set");
            using var tokenClient = new HttpClient(new AuthHandler(new HttpClientHandler(), tokenKey));
            tokenClient.Timeout = TimeSpan.FromMinutes(5);
            var tokenAdapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, tokenClient, null);
            var tokenClientInstance = new MiniMaxClient(tokenAdapter);

            var request = new T2VReq
            {
                Prompt = "一位温柔的女性声音，柔和亲切，适合讲述睡前故事",
                PreviewText = "夜深了，古屋里只有他一人。窗外传来若有若无的脚步声，他屏住呼吸，慢慢地，慢慢地，走向那扇吱呀作响的门"
            };

            var response = await tokenClientInstance.V1.Voice_design.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Voice design failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.VoiceId);
            Assert.NotNull(response.TrialAudio);
            Console.WriteLine($"Voice created successfully!");
            Console.WriteLine($"VoiceId: {response.VoiceId}");
            Console.WriteLine($"TrialAudio length: {response.TrialAudio.Length} chars");

            await File.WriteAllTextAsync(_voiceIdFile, response.VoiceId);
            Console.WriteLine($"VoiceId saved to: {_voiceIdFile}");

            if (!string.IsNullOrEmpty(response.TrialAudio))
            {
                var audioPath = Path.Combine(_outputDir, $"voice_trial_{response.VoiceId}.mp3");
                var audioBytes = HexToBytes(response.TrialAudio);
                await File.WriteAllBytesAsync(audioPath, audioBytes);
                Console.WriteLine($"Trial audio saved to: {audioPath}");
            }
        }

        [Fact]
        public async Task Voice_Get_All_Success()
        {
            var request = new GetVoiceReq
            {
                VoiceType = GetVoiceReq_voice_type.All
            };

            var response = await _client.V1.Get_voice.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Get voice failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");

            Console.WriteLine("Voice query successful!");
            if (response.SystemVoice != null)
            {
                Console.WriteLine($"System voices: {response.SystemVoice.Count}");
            }
            if (response.VoiceCloning != null)
            {
                Console.WriteLine($"Voice cloning: {response.VoiceCloning.Count}");
            }
            if (response.VoiceGeneration != null)
            {
                Console.WriteLine($"Voice generation: {response.VoiceGeneration.Count}");
            }
        }

        [Fact]
        public async Task Voice_Get_System_Success()
        {
            var request = new GetVoiceReq
            {
                VoiceType = GetVoiceReq_voice_type.System
            };

            var response = await _client.V1.Get_voice.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Get voice failed: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.SystemVoice);
            Assert.NotEmpty(response.SystemVoice);
            Console.WriteLine($"System voices count: {response.SystemVoice.Count}");
            foreach (var voice in response.SystemVoice)
            {
                Console.WriteLine($"  - {voice.VoiceId}: {voice.VoiceName}");
            }
        }

        [Fact]
        public async Task Voice_Get_VoiceGeneration_Success()
        {
            var request = new GetVoiceReq
            {
                VoiceType = GetVoiceReq_voice_type.Voice_generation
            };

            var response = await _client.V1.Get_voice.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Get voice failed: {response.BaseResp.StatusCode}");
            Console.WriteLine($"Voice generation count: {response.VoiceGeneration?.Count ?? 0}");
        }

        [Fact]
        public async Task Voice_Delete_Success()
        {
            if (!File.Exists(_voiceIdFile))
            {
                Console.WriteLine($"No VoiceId file found at {_voiceIdFile}. Run Voice_Design_Create first.");
                Assert.Fail("VoiceId file not found. Run Voice_Design_Create first.");
                return;
            }

            var voiceId = (await File.ReadAllTextAsync(_voiceIdFile)).Trim();
            Assert.NotNull(voiceId);
            Console.WriteLine($"Deleting VoiceId: {voiceId}");

            var request = new DeleteVoiceReq
            {
                VoiceId = voiceId,
                VoiceType = DeleteVoiceReq_voice_type.Voice_generation
            };

            var response = await _client.V1.Delete_voice.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Delete voice failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.VoiceId);
            Console.WriteLine($"Voice deleted successfully: {response.VoiceId}");
        }

        private static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Array.Empty<byte>();
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
