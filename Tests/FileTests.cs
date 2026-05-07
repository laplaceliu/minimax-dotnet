using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Files;

namespace Tests
{
    public class FileTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public FileTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-file-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task File_Upload_Text_Success()
        {
            var testFilePath = Path.Combine(_outputDir, "test_input.txt");
            var testContent = "This is a test file for async TTS synthesis. Hello world!";
            await File.WriteAllTextAsync(testFilePath, testContent);

            var fileBytes = await File.ReadAllBytesAsync(testFilePath);
            var response = await _client.UploadFileAsync("t2a_async_input", fileBytes, "test_input.txt");

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Console.WriteLine($"Upload StatusCode: {response.BaseResp.StatusCode}, StatusMsg: {response.BaseResp.StatusMsg}");
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.True(response.File!.FileId > 0, "FileId should be positive");
            Console.WriteLine($"Uploaded FileId: {response.File.FileId}, Filename: {response.File.Filename}");

            File.Delete(testFilePath);
        }

        [Fact]
        public async Task File_List_Success()
        {
            var response = await _client.ListFilesAsync();

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Console.WriteLine($"List StatusCode: {response.BaseResp.StatusCode}, StatusMsg: {response.BaseResp.StatusMsg}");
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");

            if (response.Files != null)
            {
                Console.WriteLine($"Found {response.Files.Count} files");
                foreach (var file in response.Files)
                {
                    Console.WriteLine($"  FileId: {file.FileId}, Filename: {file.Filename}, Bytes: {file.Bytes}");
                }
            }
        }

        [Fact]
        public async Task File_Retrieve_Success()
        {
            var listResponse = await _client.ListFilesAsync();

            Assert.NotNull(listResponse);
            Assert.NotNull(listResponse.Files);
            Assert.True(listResponse.Files.Count > 0, "No files found to retrieve");

            var firstFileId = listResponse.Files[0].FileId;
            Console.WriteLine($"Retrieving file_id: {firstFileId}");

            var retrieveResponse = await _client.RetrieveFileAsync(firstFileId);

            Assert.NotNull(retrieveResponse);
            Assert.NotNull(retrieveResponse.BaseResp);
            Console.WriteLine($"Retrieve StatusCode: {retrieveResponse.BaseResp.StatusCode}, StatusMsg: {retrieveResponse.BaseResp.StatusMsg}");
            Assert.True(retrieveResponse.BaseResp.StatusCode == 0, $"StatusCode: {retrieveResponse.BaseResp.StatusCode}");
            Assert.NotNull(retrieveResponse.File);
            Console.WriteLine($"Retrieved FileId: {retrieveResponse.File?.FileId}, Filename: {retrieveResponse.File?.Filename}, Bytes: {retrieveResponse.File?.Bytes}");
        }

        [Fact]
        public async Task File_Delete_Success()
        {
            var testFilePath = Path.Combine(_outputDir, "test_delete.txt");
            var testContent = "This is a test file for deletion test.";
            await File.WriteAllTextAsync(testFilePath, testContent);

            var fileBytes = await File.ReadAllBytesAsync(testFilePath);
            var uploadResponse = await _client.UploadFileAsync("t2a_async_input", fileBytes, "test_delete.txt");

            Assert.NotNull(uploadResponse);
            Assert.NotNull(uploadResponse.BaseResp);
            Assert.True(uploadResponse.BaseResp.StatusCode == 0, "Upload failed");
            var fileId = uploadResponse.File!.FileId;
            Console.WriteLine($"Uploaded file for deletion: {fileId}");

            var deleteRequest = new DeleteFileReq { FileId = fileId, Purpose = "t2a_async_input" };
            var deleteResponse = await _client.DeleteFileAsync(deleteRequest);

            Assert.NotNull(deleteResponse);
            Assert.NotNull(deleteResponse.BaseResp);
            Console.WriteLine($"Delete StatusCode: {deleteResponse.BaseResp.StatusCode}, StatusMsg: {deleteResponse.BaseResp.StatusMsg}");
            Assert.True(deleteResponse.BaseResp.StatusCode == 0, $"Delete failed with status: {deleteResponse.BaseResp.StatusCode}");

            File.Delete(testFilePath);
        }

        [Fact]
        public async Task File_Upload_And_Download()
        {
            var testContent = "This is a test file for upload and download test. Hello, MiniMax!";
            var testFilePath = Path.Combine(_outputDir, "test_upload_download.txt");
            await File.WriteAllTextAsync(testFilePath, testContent);

            var fileBytes = await File.ReadAllBytesAsync(testFilePath);
            var uploadResponse = await _client.UploadFileAsync("t2a_async_input", fileBytes, "test_upload_download.txt");

            Assert.NotNull(uploadResponse);
            Assert.NotNull(uploadResponse.BaseResp);
            Assert.True(uploadResponse.BaseResp.StatusCode == 0, "Upload failed");
            var fileId = uploadResponse.File!.FileId;
            Console.WriteLine($"Uploaded file_id: {fileId}");

            var retrieveResponse = await _client.RetrieveFileAsync(fileId);

            Assert.NotNull(retrieveResponse);
            Assert.NotNull(retrieveResponse.BaseResp);
            Assert.True(retrieveResponse.BaseResp.StatusCode == 0, "Retrieve failed");
            Assert.NotNull(retrieveResponse.File);
            Console.WriteLine($"Retrieved File - FileId: {retrieveResponse.File?.FileId}, Filename: {retrieveResponse.File?.Filename}");

            var fileStream = await _client.RetrieveFileContentAsync(fileId);
            Assert.NotNull(fileStream);

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var downloadedBytes = memoryStream.ToArray();
            Assert.True(downloadedBytes.Length > 0, "Downloaded file is empty");
            Console.WriteLine($"Downloaded file size: {downloadedBytes.Length} bytes");

            File.Delete(testFilePath);
        }
    }
}
