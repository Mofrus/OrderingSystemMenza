using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace UTB.Minute.CanteenClient.Services;

public class SseNotificationService : IAsyncDisposable
{
    private readonly HttpClient _http;
    private CancellationTokenSource? _cts;

    public event Action? OnNotificationReceived;

    public SseNotificationService(HttpClient http)
    {
        _http = http;
    }

    public async Task StartListeningAsync()
    {
        if (_cts != null) return;
        _cts = new CancellationTokenSource();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "notifications/stream");
            request.SetBrowserResponseStreamingEnabled(true);
            
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
            using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
            using var reader = new StreamReader(stream);

            while (!_cts.Token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (line == null) break; // Konec streamu
                
                if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("data: "))
                {
                    // var data = line.Substring(6);
                    // Pro jednoduchost jen oznámíme, že přišla změna
                    OnNotificationReceived?.Invoke();
                }
            }
        }
        catch (Exception)
        {
            // Ošetření odpojení
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
