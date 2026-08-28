using BKE.Desktop.Client;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SdkAuthorizationStatus = BKE.Desktop.Client.AuthorizationStatus;
using SdkLicenseCenterStatus = BKE.Desktop.Client.LicenseCenterStatus;

namespace BKE_Air_Stack.Licensing
{
    internal sealed class AgentClient : IDisposable
    {
        private readonly BkeDesktopClient _client = BkeDesktopClient.Create();

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

            var result = await _client.AuthorizeAsync(
                manifest.ProductId,
                manifest.Version,
                installationId,
                cancellationToken).ConfigureAwait(false);

            return result.Status switch
            {
                SdkAuthorizationStatus.Authorized => new AuthorizationResult(
                    AuthorizationStatus.Allowed,
                    "Air Stack is authorized."),
                SdkAuthorizationStatus.ActivationRequired => new AuthorizationResult(
                    AuthorizationStatus.ActivationRequired,
                    "Air Stack requires activation."),
                SdkAuthorizationStatus.AgentUnavailable => new AuthorizationResult(
                    AuthorizationStatus.AgentUnavailable,
                    "The Licensing Agent is unavailable."),
                SdkAuthorizationStatus.Timeout => new AuthorizationResult(
                    AuthorizationStatus.AgentUnavailable,
                    "The Licensing Agent did not respond in time."),
                SdkAuthorizationStatus.Unsupported => new AuthorizationResult(
                    AuthorizationStatus.Unsupported,
                    "This Air Stack product or version is not supported."),
                SdkAuthorizationStatus.Denied => new AuthorizationResult(
                    AuthorizationStatus.Denied,
                    "The Licensing Agent denied Air Stack startup."),
                SdkAuthorizationStatus.ProtocolRejected => new AuthorizationResult(
                    AuthorizationStatus.InvalidResponse,
                    "The Licensing Agent rejected the authorization request."),
                _ => new AuthorizationResult(
                    AuthorizationStatus.InvalidResponse,
                    "Authorization could not be verified.")
            };
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

            var result = await _client.OpenLicenseCenterAsync(
                manifest.ProductId,
                manifest.Version,
                installationId,
                cancellationToken).ConfigureAwait(false);

            return result.Status switch
            {
                SdkLicenseCenterStatus.AuthorizationRefreshed => new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.AuthorizationRefreshed,
                    "Air Stack activation was refreshed."),
                SdkLicenseCenterStatus.Cancelled => new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.Cancelled,
                    "Activation was cancelled."),
                SdkLicenseCenterStatus.AgentUnavailable => new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.AgentUnavailable,
                    string.IsNullOrWhiteSpace(result.Reason)
                        ? "Native License Center is unavailable."
                        : result.Reason),
                SdkLicenseCenterStatus.Timeout => new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.AgentUnavailable,
                    "The native License Center did not complete in time."),
                _ => new NativeLicenseCenterResult(
                    NativeLicenseCenterStatus.Failed,
                    string.IsNullOrWhiteSpace(result.Reason)
                        ? "Activation was not completed."
                        : result.Reason)
            };
        }

        public void Dispose()
        {
            _client.Dispose();
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
