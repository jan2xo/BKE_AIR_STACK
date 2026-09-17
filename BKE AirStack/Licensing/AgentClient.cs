using BKE.Desktop.Licensing;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BKE_Air_Stack.Licensing
{
    internal sealed class AgentClient : IDisposable
    {
        private readonly BkeLicensingClient _client = BkeLicensingClient.Create();

        internal async Task<AuthorizationResult> EnsureAuthorizedAsync(
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

            var authorization = await _client.EnsureAuthorizedAsync(
                manifest.ProductId,
                manifest.Version,
                installationId,
                new LicensingFlowOptions
                {
                    ActivationInteraction = ActivationInteraction.NativeDesktop
                },
                cancellationToken).ConfigureAwait(false);

            if (authorization.Status != AuthorizationStatus.Denied)
            {
                return authorization;
            }

            // A denied local authorization can represent a stale or otherwise
            // unverifiable persisted lease. Do not expose the Agent's internal
            // denial reason to the user; let the Agent own recovery presentation.
            var center = await _client.OpenLicenseCenterAsync(
                manifest.ProductId,
                manifest.Version,
                installationId,
                cancellationToken).ConfigureAwait(false);

            switch (center.Status)
            {
                case LicenseCenterStatus.AuthorizationRefreshed:
                case LicenseCenterStatus.Completed:
                    return await _client.AuthorizeAsync(
                        manifest.ProductId,
                        manifest.Version,
                        installationId,
                        cancellationToken).ConfigureAwait(false);

                case LicenseCenterStatus.Cancelled:
                    return new AuthorizationResult(
                        AuthorizationStatus.ActivationCancelled,
                        "activation_cancelled");

                case LicenseCenterStatus.AgentUnavailable:
                    return new AuthorizationResult(AuthorizationStatus.AgentUnavailable, center.Reason);

                case LicenseCenterStatus.Timeout:
                    return new AuthorizationResult(AuthorizationStatus.Timeout, center.Reason);

                case LicenseCenterStatus.ProtocolRejected:
                    return new AuthorizationResult(AuthorizationStatus.ProtocolRejected, center.Reason);

                case LicenseCenterStatus.InvalidRequest:
                    return new AuthorizationResult(AuthorizationStatus.InvalidRequest, center.Reason);

                case LicenseCenterStatus.InvalidResponse:
                    return new AuthorizationResult(AuthorizationStatus.InvalidResponse, center.Reason);

                case LicenseCenterStatus.InvalidProductContext:
                case LicenseCenterStatus.IncompatibleProductVersion:
                case LicenseCenterStatus.Unsupported:
                    return new AuthorizationResult(AuthorizationStatus.Unsupported, center.Reason);

                case LicenseCenterStatus.ActivationFailed:
                case LicenseCenterStatus.Failed:
                default:
                    return new AuthorizationResult(
                        AuthorizationStatus.Denied,
                        "license_center_recovery_failed");
            }
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
}
