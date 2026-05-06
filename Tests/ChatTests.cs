using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using Xunit;
using MiniMax.Models;
using MiniMaxClient = MiniMax.MiniMaxClient;

namespace Tests
{
    public class ChatTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;

        public ChatTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var authHandler = new AuthHandler(new HttpClientHandler(), apiKey);
            _httpClient = new HttpClient(authHandler);
            var adapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, _httpClient, null);
            _client = new MiniMaxClient(adapter);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public async Task Chat_Completion_Returns_Success()
        {
            var request = new ChatCompletionReq
            {
                Model = ChatCompletionReq_model.MiniMaxM27,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = Message_role.User,
                        Content = new Message.Message_content { String = "Hello, who are you?" }
                    }
                }
            };

            var response = await _client.V1.Chat.Completions.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0 || response.BaseResp.StatusCode == 1000, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Choices);
            Assert.NotEmpty(response.Choices);
            Assert.NotNull(response.Choices[0].Message);
            Assert.NotNull(response.Choices[0].Message.Content);
            Console.WriteLine("Response: " + response.Choices[0].Message.Content);
        }
    }
}