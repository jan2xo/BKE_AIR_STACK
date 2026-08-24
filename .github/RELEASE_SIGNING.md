# Air Stack GitHub Release Signing Configuration

Production Windows signing should be performed by GitHub Actions with a managed signing service, not from a developer certificate store and not from a committed PFX.

## Preferred model

GitHub Actions (`windows-latest`) -> OIDC/cloud authentication -> Microsoft Artifact Signing -> signature verification -> package/hash -> GitHub Release.

Reference implementation patterns:

- https://github.com/ErsatzTV/legacy/blob/main/.github/workflows/artifacts.yml
- https://github.com/Azure/artifact-signing-action

## Repository configuration placeholders

The signing workflow may consume repository/environment variables such as:

- `AZURE_SIGNING_ENDPOINT`
- `AZURE_SIGNING_ACCOUNT_NAME`
- `AZURE_SIGNING_CERTIFICATE_PROFILE_NAME`

OIDC/cloud authentication configuration may require secrets or protected environment values such as:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

Do not put actual secret values in this file or in workflow YAML.

## Release gate

When managed signing is enabled, the production release workflow should:

1. build the Windows artifact;
2. sign the public EXE/installer with SHA-256 and RFC3161 timestamping;
3. verify the Authenticode signature;
4. fail closed if verification fails;
5. package and compute SHA-256;
6. publish the verified artifact to GitHub Releases.

The current hotfix release intentionally disables only the legacy ClickOnce/PFX manifest-signing step during `dotnet publish`; it does not delete the Windows application manifest and does not change the product version.
