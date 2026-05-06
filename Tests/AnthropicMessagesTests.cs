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
    public class AnthropicMessagesTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;

        public AnthropicMessagesTests()
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
        public async Task Anthropic_Messages_Create_Success()
        {
            var request = new CreateMessageReq
            {
                Model = CreateMessageReq_model.MiniMaxM27,
                MaxTokens = 1024,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = Message_role.User,
                        Content = new Message.Message_content { String = "Hello, who are you?" }
                    }
                }
            };

            var response = await _client.Anthropic.V1.Messages.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            Assert.NotNull(response.Id);
            Console.WriteLine($"Response ID: {response.Id}");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"StopReason: {response.StopReason}");
            foreach (var block in response.Content)
            {
                Console.WriteLine($"Content: {block.Text}");
            }
        }

        [Fact]
        public async Task Anthropic_Messages_With_SystemPrompt_Success()
        {
            var request = new CreateMessageReq
            {
                Model = CreateMessageReq_model.MiniMaxM27,
                MaxTokens = 1024,
                Temperature = 0.7,
                System = new CreateMessageReq.CreateMessageReq_system { String = "You are a helpful assistant." },
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = Message_role.User,
                        Content = new Message.Message_content { String = "What is the capital of France?" }
                    }
                }
            };

            var response = await _client.Anthropic.V1.Messages.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            Console.WriteLine($"Response: {response.Content[0].Text}");
        }

        [Fact]
        public async Task Anthropic_Messages_With_M2_1_Model_Success()
        {
            var request = new CreateMessageReq
            {
                Model = CreateMessageReq_model.MiniMaxM21,
                MaxTokens = 512,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = Message_role.User,
                        Content = new Message.Message_content { String = "Give me a short answer: 2+2=?" }
                    }
                }
            };

            var response = await _client.Anthropic.V1.Messages.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            Assert.NotNull(response.Usage);
            Console.WriteLine($"Input Tokens: {response.Usage.InputTokens}");
            Console.WriteLine($"Output Tokens: {response.Usage.OutputTokens}");
            Console.WriteLine($"Response: {response.Content[0].Text}");
        }

        [Fact]
        public async Task Anthropic_Messages_Multi_Turn_Conversation_Success()
        {
            var request = new CreateMessageReq
            {
                Model = CreateMessageReq_model.MiniMaxM27,
                MaxTokens = 1024,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = Message_role.User,
                        Content = new Message.Message_content { String = "My name is Alice." }
                    },
                    new Message
                    {
                        Role = Message_role.Assistant,
                        Content = new Message.Message_content { String = "Hello Alice! How can I help you today?" }
                    },
                    new Message
                    {
                        Role = Message_role.User,
                        Content = new Message.Message_content { String = "What is my name?" }
                    }
                }
            };

            var response = await _client.Anthropic.V1.Messages.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            Console.WriteLine($"Response: {response.Content[0].Text}");
        }
    }
}