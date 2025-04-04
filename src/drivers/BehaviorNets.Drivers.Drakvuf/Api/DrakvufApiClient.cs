using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using BehaviorNets.Drivers.Drakvuf.Reporting;

namespace BehaviorNets.Drivers.Drakvuf.Api;

public class DrakvufApiClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly Uri _uploadUri;
    private readonly Uri _statusUri;
    private readonly Uri _logsUri;

    public DrakvufApiClient(Uri baseUri)
    {
        _client = new HttpClient();
        _uploadUri = new Uri(baseUri, "/upload");
        _statusUri = new Uri(baseUri, "/status/");
        _logsUri = new Uri(baseUri, "/logs/");
    }

    public int RetryDelay
    {
        get;
        set;
    } = 1000;

    public int MaxRetryAttempts
    {
        get;
        set;
    } = 5;

    public async Task<string> CreateTaskAsync(TaskCreateParameters parameters)
    {
        using var form = new MultipartFormDataContent();

        // Add file to form.
        using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(parameters.FilePath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "file", Path.GetFileName(parameters.FilePath));

        // Specify timeout to run on.
        form.Add(new StringContent((parameters.Timeout * 60).ToString()), "timeout");

        // Add start command if specified
        if (parameters.StartCommand is not null)
            form.Add(new StringContent(parameters.StartCommand), "start_command");

        // Specify plugins to use.
        form.Add(new StringContent(JsonSerializer.Serialize(parameters.EnabledPlugins)), "plugins");

        // Post, with 5 retry attempts.
        var response = await RetryOnFailure(async () =>
        {
            var response = await _client.PostAsync(_uploadUri, form);
            response.EnsureSuccessStatusCode();
            return response;
        });

        // Translate result.
        string jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TaskCreateResult>(jsonResponse, JsonDefaults.ApiSerializerOptions);

        if (result is null)
            throw new DrakvufApiException("Invalid null response after creating task.");

        return result.TaskUid;
    }

    public async Task<DrakRunReport> GetDrakrunReport(string taskUid) => await RetryOnFailure(async () =>
    {
        var response = await _client.GetAsync(new Uri(_logsUri, taskUid + "/drakrun"));
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        if (reader.Peek() != '{')
            return null;

        return DrakRunReport.FromReader(reader);
    });

    public async Task<ApiMonReport> GetApiMonReport(string taskUid) => await RetryOnFailure(async () =>
    {
        var response = await _client.GetAsync(new Uri(_logsUri, taskUid + "/apimon"));
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        if (reader.Peek() != '{')
            return null;

        return ApiMonReport.FromReader(reader);
    });

    public async Task<SysCallReport> GetSysCallReport(string taskUid) => await RetryOnFailure(async () =>
    {
        var response = await _client.GetAsync(new Uri(_logsUri, taskUid + "/syscall"));
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        if (reader.Peek() != '{')
            return null;

        return SysCallReport.FromReader(reader);
    });

    public async Task<RegMonReport> GetRegMonReport(string taskUid) => await RetryOnFailure(async () =>
    {
        var response = await _client.GetAsync(new Uri(_logsUri, taskUid + "/regmon"));
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        if (reader.Peek() != '{')
            return null;

        return RegMonReport.FromReader(reader);
    });

    public async Task<LibraryMonReport> GetLibraryMonReport(string taskUid) => await RetryOnFailure(async () =>
    {
        var response = await _client.GetAsync(new Uri(_logsUri, taskUid + "/librarymon"));
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        if (reader.Peek() != '{')
            return null;

        return LibraryMonReport.FromReader(reader);
    });

    public async Task<TaskStatus> GetTaskStatusAsync(string taskUid) => await RetryOnFailure(async () =>
    {
        var response = await _client.GetAsync(new Uri(_statusUri, taskUid));
        if (!response.IsSuccessStatusCode)
            return null;

        return JsonSerializer.Deserialize<TaskStatus>(
            await response.Content.ReadAsStringAsync(),
            JsonDefaults.ApiSerializerOptions);
    });

    /// <inheritdoc />
    public void Dispose() => _client?.Dispose();

    private async Task<T> RetryOnFailure<T>(Func<Task<T>> action)
    {
        var exceptions = new List<Exception>(MaxRetryAttempts);
            
        for (int i = 0; i < MaxRetryAttempts; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                await Task.Delay(RetryDelay);
            }
        }

        throw new DrakvufApiException($"Failed to perform task after {MaxRetryAttempts} attempts.",
            new AggregateException(exceptions));
    }
}