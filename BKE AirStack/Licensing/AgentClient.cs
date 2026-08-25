using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BKE_Air_Stack.Licensing
{
    internal sealed class AgentClient : IDisposable
    {
        private static readonly Uri AuthorizationEndpoint =
            new Uri("http://127.0.0.1:43873/v1/authorize", UriKind.Absolute);
        private static readonly Uri NativeLicenseCenterEndpoint =
            new Uri("http://127.0.0.1:43873/v1/license-center/open", UriKind.Absolute);

        private readonly HttpClient _httpClient;

        internal AgentClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        internal async Task<AuthorizationResult> AuthorizeAsync(
            CancellationToken cancellationToken = default)
        {
            ProductManifest manifest;
            string installationId;
            try
            {
                manifest = LoadManifest();
                installationId = InstallationIdentity.GetOrCreate();
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException ||
                ex is InvalidDataException)
            {
                return new AuthorizationResult(
                    AuthorizationStatus.InvalidResponse,
                    "Air Stack product or installation identity is missing or invalid.");
            }

            var request = new AuthorizationRequest
            {
                ProductId = manifest.ProductId,
                Version = manifest.Version,
                InstallationId = installationId
            };

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(3));
                var responseJson = await PostJsonAsync(
                    AuthorizationEndpoint, request, timeout.Token).ConfigureAwait(false);
                var decision = JsonSerializer.Deserialize<AuthorizationResponse>(responseJson);

                if (decision == null || decision.Authorized == null ||
                    string.IsNullOrWhiteSpace(decision.Reason))
                {
                    return new AuthorizationResult(
                        AuthorizationStatus.InvalidResponse,
                        "The Licensing Agent returned an invalid authorization response.");
                }

                if (decision.Authorized.Value)
                {
                    return new AuthorizationResult(
                        AuthorizationStatus.Allowed,
                        "Air Stack is authorized.");
                }

                return MapDenial(decision.Reason);
            }
            catch (OperationCanceledException)
            {
                return new AuthorizationResult(
                    AuthorizationStatus.AgentUnavailable,
                    "The Licensing Agent did not respond in time.");
            }
            catch (HttpRequestException)
            {
                return new AuthorizationResult(
                    AuthorizationStatus.AgentUnavailable,
                    "The Licensing Agent is unavailable.");
            }
            catch (JsonException)
            {
                return new AuthorizationResult(
                    AuthorizationStatus.InvalidResponse,
                    "The Licensing Agent returned malformed data.");
            }
            catch (Exception)
            {
                return new AuthorizationResult(
                    AuthorizationStatus.InvalidResponse,
                    "Authorization could not be verified.");
            }
        }

        internal async Task<NativeLicenseCenterResult> OpenNativeLicenseCenterAsync(
            CancellationToken cancellationToken = default)
        {
            ProductManifest manifest;
            string installationId;
            try
            {
                manifest = LoadManifest();
                installationId = InstallationIdentity.GetOrCreate();
            }
            catch (Exception)
            {
                return new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.Failed,
                    "Air Stack product context is invalid.");
            }

            var correlationId = Guid.NewGuid().ToString("N");
            var request = new NativeLicenseCenterRequest
            {
                ProductId = manifest.ProductId,
                Version = manifest.Version,
                InstallationId = installationId,
                CorrelationId = correlationId
            };

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromMinutes(15));
                var responseJson = await PostJsonAsync(
                    NativeLicenseCenterEndpoint, request, timeout.Token).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<NativeLicenseCenterResponse>(responseJson);
                if (result == null ||
                    !string.Equals(result.CorrelationId, correlationId, StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(result.Outcome))
                {
                    return new NativeLicenseCenterResult(
                        NativeLicenseCenterStatus.Failed,
                        "The Licensing Agent returned an invalid License Center result.");
                }

                return result.Outcome switch
                {
                    "authorization_refreshed" => new NativeLicenseCenterResult(
                        NativeLicenseCenterStatus.AuthorizationRefreshed,
                        "Air Stack activation was refreshed."),
                    "cancelled" => new NativeLicenseCenterResult(
                        NativeLicenseCenterStatus.Cancelled,
                        "Activation was cancelled."),
                    "agent_unavailable" => new NativeLicenseCenterResult(
                        NativeLicenseCenterStatus.AgentUnavailable,
                        string.IsNullOrWhiteSpace(result.Reason) ? "Native License Center is unavailable." : result.Reason),
                    _ => new NativeLicenseCenterResult(
                        NativeLicenseCenterStatus.Failed,
                        string.IsNullOrWhiteSpace(result.Reason) ? "Activation was not completed." : result.Reason),
                };
            }
            catch (OperationCanceledException)
            {
                return new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.AgentUnavailable,
                    "The native License Center did not complete in time.");
            }
            catch (HttpRequestException)
            {
                return new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.AgentUnavailable,
                    "The Licensing Agent is unavailable.");
            }
            catch (JsonException)
            {
                return new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.Failed,
                    "The Licensing Agent returned malformed License Center data.");
            }
        }

        private async Task<string> PostJsonAsync(
            Uri endpoint, object request, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(
                endpoint, content, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("The Licensing Agent returned a non-success status.");
            }
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private static AuthorizationResult MapDenial(string reason)
        {
            if (string.Equals(reason, "activation_required", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthorizationResult(
                    AuthorizationStatus.ActivationRequired,
                    "Air Stack requires activation.");
            }

            if (string.Equals(reason, "unsupported", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(reason, "unsupported_product", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(reason, "unsupported_version", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthorizationResult(
                    AuthorizationStatus.Unsupported,
                    "This Air Stack product or version is not supported.");
            }

            return new AuthorizationResult(
                AuthorizationStatus.Denied,
                "The Licensing Agent denied Air Stack startup.");
        }

        private static ProductManifest LoadManifest()
        {
            var manifestPath = Path.Combine(AppContext.BaseDirectory, "bke.manifest.json");
            var json = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<ProductManifest>(json);

            if (manifest == null ||
                manifest.SchemaVersion != 1 ||
                !string.Equals(manifest.ProductId, "bke-air-stack", StringComparison.Ordinal) ||
                !string.Equals(manifest.DisplayName, "Air Stack", StringComparison.Ordinal) ||
                !string.Equals(manifest.EntryPoint, "BKE AirStack.exe", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(manifest.Version))
            {
                throw new InvalidDataException("Invalid Air Stack manifest.");
            }

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var canonicalVersion = assemblyVersion == null
                ? string.Empty
                : $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

            if (!string.Equals(manifest.Version, canonicalVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Manifest version does not match Air Stack.");
            }

            return manifest;
        }

        private sealed class ProductManifest
        {
            [JsonPropertyName("schemaVersion")]
            public int SchemaVersion { get; set; }

            [JsonPropertyName("productId")]
            public string ProductId { get; set; } = string.Empty;

            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            [JsonPropertyName("entryPoint")]
            public string EntryPoint { get; set; } = string.Empty;
        }

        private sealed class AuthorizationRequest
        {
            [JsonPropertyName("product_id")]
            public string ProductId { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            [JsonPropertyName("installation_id")]
            public string InstallationId { get; set; } = string.Empty;
        }

        private sealed class AuthorizationResponse
        {
            [JsonPropertyName("authorized")]
            public bool? Authorized { get; set; }

            [JsonPropertyName("reason")]
            public string Reason { get; set; } = string.Empty;
        }

        private sealed class NativeLicenseCenterRequest
        {
            [JsonPropertyName("product_id")]
            public string ProductId { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            [JsonPropertyName("installation_id")]
            public string InstallationId { get; set; } = string.Empty;

            [JsonPropertyName("correlation_id")]
            public string CorrelationId { get; set; } = string.Empty;
        }

        private sealed class NativeLicenseCenterResponse
        {
            [JsonPropertyName("outcome")]
            public string Outcome { get; set; } = string.Empty;

            [JsonPropertyName("reason")]
            public string Reason { get; set; } = string.Empty;

            [JsonPropertyName("authorization_changed")]
            public bool AuthorizationChanged { get; set; }

            [JsonPropertyName("correlation_id")]
            public string CorrelationId { get; set; } = string.Empty;
        }
    }

    internal enum NativeLicenseCenterStatus
    {
        AuthorizationRefreshed,
        Cancelled,
        AgentUnavailable,
        Failed
    }

    internal sealed class NativeLicenseCenterResult
    {
        internal NativeLicenseCenterResult(NativeLicenseCenterStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        internal NativeLicenseCenterStatus Status { get; }
        internal string Message { get; }
    }
}
