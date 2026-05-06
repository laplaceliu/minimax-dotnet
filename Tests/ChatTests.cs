using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models.Chat;

namespace Tests
{
    public class ChatTests : IDisposable
    {
        private readonly MiniMaxClient _client;

        public ChatTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task Chat_Completion_Returns_Success()
        {
            var request = new ChatCompletionReq
            {
                Model = "MiniMax-M2.7",
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = "Hello, who are you?"
                    }
                }
            };

            var response = await _client.ChatCompletionAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0 || response.BaseResp.StatusCode == 1000, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Choices);
            Assert.NotEmpty(response.Choices);
            var firstChoice = response.Choices[0];
            Assert.NotNull(firstChoice.Message);
            Assert.NotNull(firstChoice.Message.Content);
            Console.WriteLine("Response: " + firstChoice.Message.Content);
        }
    }
}
