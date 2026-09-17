using Xunit;

namespace AirStack.Tests;

public sealed class StartupParityContractTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void NotificationPreflightRunsBeforeGraceAndLicensing()
    {
        var program = Read("BKE AirStack", "Program.cs");

        var preflight = program.IndexOf(
            "NotificationCoordinator.ShowBeforeLicensing()",
            StringComparison.Ordinal);
        var grace = program.IndexOf("GracePeriodClient", StringComparison.Ordinal);
        var authorize = program.IndexOf("EnsureAuthorizedAsync", StringComparison.Ordinal);

        Assert.True(preflight >= 0, "Notification preflight is not composed.");
        Assert.True(grace > preflight, "Notification preflight must run before grace evaluation.");
        Assert.True(authorize > preflight, "Notification preflight must run before Agent authorization.");
    }

    [Fact]
    public void LicensingUsesModernSdkAndContainsCustomerFacingFailureText()
    {
        var project = Read("BKE AirStack", "BKE AirStack.csproj");
        var program = Read("BKE AirStack", "Program.cs");
        var agent = Read("BKE AirStack", "Licensing", "AgentClient.cs");

        Assert.Contains("<TargetFramework>net10.0-windows</TargetFramework>", project);
        Assert.Contains("BKE.Desktop.Licensing\" Version=\"2.0.0", project);
        Assert.DoesNotContain("BKE.Desktop.Client", project, StringComparison.Ordinal);
        Assert.Contains("BkeLicensingClient.Create", agent);
        Assert.Contains("EnsureAuthorizedAsync", agent);
        Assert.Contains("OpenLicenseCenterAsync", agent);
        Assert.Contains("AuthorizeAsync", agent);
        Assert.DoesNotContain("HttpClient", agent, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1:43873", agent, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/authorize", agent, StringComparison.Ordinal);
        Assert.DoesNotContain("authorization.Message", program, StringComparison.Ordinal);
        Assert.DoesNotContain("center.Message", program, StringComparison.Ordinal);
        Assert.Contains("Use BKE License Center to activate or repair licensing.", program);
        Assert.Contains("AuthorizationStatus.AgentUnavailable or AuthorizationStatus.Timeout", program);
        Assert.Contains("AuthorizationStatus.ActivationCancelled", program);
        Assert.Contains("AuthorizationStatus.Authorized", program);
    }

    [Fact]
    public void LegacyDuplicateContractsAndRawUpdaterTransportRemainRemoved()
    {
        Assert.False(File.Exists(Path.Combine(
            RepositoryRoot, "BKE AirStack", "Licensing", "AuthorizationResult.cs")));
        Assert.False(File.Exists(Path.Combine(
            RepositoryRoot, "BKE AirStack", "Licensing", "AgentUpdateClient.cs")));
        Assert.False(File.Exists(Path.Combine(
            RepositoryRoot, "BKE AirStack", "Licensing", "AgentUpdateCoordinator.cs")));

        var updater = Read("BKE AirStack", "Updates", "UpdateCoordinator.cs");
        Assert.Contains("BkeUpdaterClient.Create", updater);
        Assert.Contains("UpdateCheckStatus.UpdateAvailable", updater);
        Assert.DoesNotContain("HttpClient", updater, StringComparison.Ordinal);
        Assert.DoesNotContain("127.0.0.1:43873", updater, StringComparison.Ordinal);
        Assert.DoesNotContain("/v1/updates", updater, StringComparison.Ordinal);
    }

    private static string Read(params string[] path) =>
        File.ReadAllText(Path.Combine(new[] { RepositoryRoot }.Concat(path).ToArray()));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BKE_AIR_STACK.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Air Stack repository root.");
    }
}
