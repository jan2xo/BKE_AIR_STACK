using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BKE_Air_Stack.Licensing
{
    internal sealed class ModuleLaunchClient
    {
        internal const string RenderDockPolicyId = "air-stack-render-dock-v1";
        private const string Schema = "bke.module-ipc.v1";
        private const int MaxMessageBytes = 16 * 1024;

        internal async Task<ModuleLaunchResult> LaunchRenderDockAsync(
            CancellationToken cancellationToken = default)
        {
            if (!OperatingSystem.IsWindows())
            {
                return new ModuleLaunchResult(false, "Secure Render Dock launch is available only on Windows.");
            }

            try
            {
                using var pipe = new NamedPipeClientStream(
                    ".",
                    GetPipeName(),
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous);
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(5));
                await pipe.ConnectAsync(timeout.Token).ConfigureAwait(false);

                var requestId = Guid.NewGuid().ToString("N");
                var payload = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    schema = Schema,
                    operation = "launch",
                    request_id = requestId,
                    policy_id = RenderDockPolicyId,
                    installation_id = InstallationIdentity.GetOrCreate(),
                });
                if (payload.Length > MaxMessageBytes)
                {
                    return new ModuleLaunchResult(false, "Render Dock launch request is too large.");
                }

                var header = new byte[4];
                BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
                await pipe.WriteAsync(header, timeout.Token).ConfigureAwait(false);
                await pipe.WriteAsync(payload, timeout.Token).ConfigureAwait(false);
                await pipe.FlushAsync(timeout.Token).ConfigureAwait(false);

                await ReadExactlyAsync(pipe, header, timeout.Token).ConfigureAwait(false);
                var responseLength = BinaryPrimitives.ReadInt32BigEndian(header);
                if (responseLength < 2 || responseLength > MaxMessageBytes)
                {
                    return new ModuleLaunchResult(false, "The Licensing Agent returned an invalid module response.");
                }

                var response = new byte[responseLength];
                await ReadExactlyAsync(pipe, response, timeout.Token).ConfigureAwait(false);
                using var json = JsonDocument.Parse(response);
                var root = json.RootElement;
                if (!root.TryGetProperty("schema", out var responseSchema) || responseSchema.GetString() != Schema ||
                    !root.TryGetProperty("request_id", out var echoed) || echoed.GetString() != requestId ||
                    !root.TryGetProperty("ok", out var ok))
                {
                    return new ModuleLaunchResult(false, "The Licensing Agent returned malformed module data.");
                }

                if (!ok.GetBoolean())
                {
                    var reason = root.TryGetProperty("error", out var error)
                        ? error.GetString() ?? "denied"
                        : "denied";
                    return new ModuleLaunchResult(false, $"Render Dock launch denied: {reason}.");
                }

                if (!root.TryGetProperty("result", out var result) ||
                    !result.TryGetProperty("policy_id", out var policy) ||
                    !string.Equals(policy.GetString(), RenderDockPolicyId, StringComparison.Ordinal) ||
                    !result.TryGetProperty("child_pid", out var childPid) ||
                    childPid.GetInt32() <= 0)
                {
                    return new ModuleLaunchResult(false, "The Licensing Agent returned an invalid launch result.");
                }

                return new ModuleLaunchResult(true, "Render Dock launched.");
            }
            catch (OperationCanceledException)
            {
                return new ModuleLaunchResult(false, "The Licensing Agent module service did not respond in time.");
            }
            catch (IOException)
            {
                return new ModuleLaunchResult(false, "The Licensing Agent module service is unavailable.");
            }
            catch (UnauthorizedAccessException)
            {
                return new ModuleLaunchResult(false, "The Licensing Agent rejected this process connection.");
            }
            catch (JsonException)
            {
                return new ModuleLaunchResult(false, "The Licensing Agent returned malformed module data.");
            }
            catch (CryptographicException)
            {
                return new ModuleLaunchResult(false, "Windows user identity could not be resolved securely.");
            }
        }

        private static string GetPipeName()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var sid = identity.User?.Value ?? throw new InvalidOperationException("Windows user SID unavailable.");
            var digest = SHA256.HashData(Encoding.ASCII.GetBytes(sid));
            var suffix = Convert.ToHexString(digest).ToLowerInvariant().Substring(0, 16);
            return $"bke-licensing-agent-{suffix}-module-v1";
        }

        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }
                offset += read;
            }
        }
    }

    internal sealed class ModuleLaunchResult
    {
        internal ModuleLaunchResult(bool launched, string message)
        {
            Launched = launched;
            Message = message;
        }

        internal bool Launched { get; }
        internal string Message { get; }
    }
}
