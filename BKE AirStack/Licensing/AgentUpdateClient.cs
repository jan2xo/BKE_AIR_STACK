using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BKE_Air_Stack.Licensing
{
    internal sealed class AgentUpdateClient : IDisposable
    {
        private static readonly Uri AgentBase = new Uri("http://127.0.0.1:43873/", UriKind.Absolute);
        private readonly HttpClient _http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        internal async Task<UpdateStatus?> StatusAsync(string productId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(2));
                var path = $"v1/updates/status?product_id={Uri.EscapeDataString(productId)}";
                using var response = await _http.GetAsync(new Uri(AgentBase, path), timeout.Token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var value = JsonSerializer.Deserialize<UpdateStatus>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                return value != null && value.ProductId == productId && !string.IsNullOrWhiteSpace(value.State) ? value : null;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException || ex is JsonException)
            {
                return null;
            }
        }

        internal async Task QueueRefreshAsync(UpdateStatus status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(status.CurrentVersion)) return;
            await PostAsync("v1/updates/refresh", new { product_id = status.ProductId, version = status.CurrentVersion }, TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
        }

        internal async Task DismissAsync(UpdateStatus status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(status.CurrentVersion) || string.IsNullOrWhiteSpace(status.LatestVersion)) return;
            await PostAsync("v1/updates/dismiss", new
            {
                product_id = status.ProductId,
                version = status.CurrentVersion,
                latest_version = status.LatestVersion
            }, TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
        }

        internal async Task<string> OpenUpdateCenterAsync(UpdateStatus status, CancellationToken cancellationToken = default)
        {
            var correlation = Guid.NewGuid().ToString("N");
            var json = await PostAsync("v1/update-center/open", new { product_id = status.ProductId, version = status.CurrentVersion, correlation_id = correlation }, TimeSpan.FromMinutes(15), cancellationToken).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<UpdateCenterResponse>(json);
            return result != null && result.CorrelationId == correlation
                ? (string.IsNullOrWhiteSpace(result.Reason) ? result.Outcome : result.Reason)
                : "The Licensing Agent returned an invalid Update Center result.";
        }

        private async Task<string> PostAsync(string path, object body, TimeSpan timeoutValue, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(timeoutValue);
            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var response = await _http.PostAsync(new Uri(AgentBase, path), content, timeout.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public void Dispose() => _http.Dispose();
    }

    internal sealed class UpdateStatus
    {
        [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
        [JsonPropertyName("product_id")] public string ProductId { get; set; } = string.Empty;
        [JsonPropertyName("current_version")] public string CurrentVersion { get; set; } = string.Empty;
        [JsonPropertyName("latest_version")] public string LatestVersion { get; set; } = string.Empty;
        internal bool Available => State == "update_available" || State == "stale_update";
    }

    internal sealed class UpdateCenterResponse
    {
        [JsonPropertyName("outcome")] public string Outcome { get; set; } = string.Empty;
        [JsonPropertyName("reason")] public string Reason { get; set; } = string.Empty;
        [JsonPropertyName("correlation_id")] public string CorrelationId { get; set; } = string.Empty;
    }
}
