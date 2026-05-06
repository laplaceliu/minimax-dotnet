using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Core;
using MiniMax.Models;
using MiniMax.Models.Voice;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class VoiceTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;
        private readonly string _voiceIdFile;

        public VoiceTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-voice-tests");
            Directory.CreateDirectory(_outputDir);
            _voiceIdFile = Path.Combine(_outputDir, "last_voice_id.txt");
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task Voice_Design_Create_Success()
        {
            var tokenKey = Environment.GetEnvironmentVariable("MINIMAX_TOKEN_KEY") ?? throw new InvalidOperationException("MINIMAX_TOKEN_KEY not set");
            using var tokenClient = new MiniMaxClient(tokenKey);

            var request = new VoiceDesignReq
            {
                Prompt = "一位温柔的女性声音，柔和亲切，适合讲述睡前故事",
                PreviewText = "夜深了，古屋里只有他一人。窗外传来若有若无的脚步声，他屏住呼吸，慢慢地，慢慢地，走向那扇吱呀作响的门"
            };

            var response = await tokenClient.DesignVoiceAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Voice design failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Assert.NotNull(response.VoiceId);
            Console.WriteLine($"Voice design task created!");
            Console.WriteLine($"VoiceId: {response.VoiceId}");

            await File.WriteAllTextAsync(_voiceIdFile, "test_voice_id_for_deletion");
            Console.WriteLine($"VoiceId written to file for delete test");
        }

        [Fact]
        public void ErrorCodes_Provides_Friendly_Messages()
        {
            var error1008 = ErrorCodes.GetErrorInfo(1008);
            Assert.NotNull(error1008);
            Assert.Equal("余额不足", error1008.Reason);
            Assert.Contains("账户余额", error1008.FriendlyMessage);

            var error2049 = ErrorCodes.GetErrorInfo(2049);
            Assert.NotNull(error2049);
            Assert.Equal("无效的API Key", error2049.Reason);

            var error1026 = ErrorCodes.GetErrorInfo(1026);
            Assert.NotNull(error1026);
            Assert.Equal("输入内容涉敏", error1026.Reason);

            var unknownError = ErrorCodes.GetErrorInfo(9999);
            Assert.Null(unknownError);

            Assert.Equal("请稍后再试或联系我们", ErrorCodes.GetFriendlyMessage(9999));
            Assert.Equal("请检查您的账户余额", ErrorCodes.GetFriendlyMessage(1008));
        }

        [Fact]
        public void MiniMaxException_Shows_Friendly_Message()
        {
            var ex = new MiniMaxException(1008, "insufficient balance");
            Assert.Equal(1008, ex.StatusCode);
            Assert.Equal("余额不足", ex.Reason);
            Assert.Contains("请检查您的账户余额", ex.Message);
            Assert.Contains("1008", ex.Message);

            var ex2 = new MiniMaxException(2049);
            Assert.Equal(2049, ex2.StatusCode);
            Assert.Contains("无效的API Key", ex2.Message);
        }

        [Fact]
        public async Task Voice_Get_All_Success()
        {
            var response = await _client.GetVoiceAsync();

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Get voice failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");

            Console.WriteLine("Voice query successful!");
            var totalVoices = (response.SystemVoice?.Count ?? 0) + (response.VoiceCloning?.Count ?? 0) + (response.VoiceGeneration?.Count ?? 0);
            Console.WriteLine($"Total Voices: {totalVoices}");
        }

        [Fact]
        public async Task Voice_Get_System_Success()
        {
            var response = await _client.GetVoiceAsync(VoiceType.System);

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
            var response = await _client.GetVoiceAsync(VoiceType.VoiceGeneration);

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
                VoiceType = VoiceType.VoiceGeneration
            };

            var response = await _client.DeleteVoiceAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"Delete voice failed: {response.BaseResp.StatusCode} - {response.BaseResp.StatusMsg}");
            Console.WriteLine($"Voice deleted successfully: {response.BaseResp.StatusMsg}");
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
