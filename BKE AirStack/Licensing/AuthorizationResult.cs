namespace BKE_Air_Stack.Licensing
{
    internal enum AuthorizationStatus
    {
        Allowed,
        Denied,
        Cancelled,
        AgentUnavailable,
        Unsupported,
        InvalidResponse
    }

    internal sealed class AuthorizationResult
    {
        internal AuthorizationResult(AuthorizationStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        internal AuthorizationStatus Status { get; }

        internal string Message { get; }
    }
}
