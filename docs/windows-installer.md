# Air Stack Windows installer

Air Stack is packaged as a self-contained Windows x64 application and installed to
`C:\Program Files\BKE AirStack`. The installer preserves the canonical executable
name (`BKE AirStack.exe`), product identity (`bke-air-stack`), and adjacent
`bke.manifest.json` used by the existing Licensing Agent authorization boundary.

## Build

On disposable Windows compute with the .NET 6 SDK and Inno Setup 6:

```powershell
./scripts/build-windows-installer.ps1
```

The script validates project, assembly, manifest, installer, and filename version
alignment; publishes `win-x64` self-contained; stages the runtime-required
`default.vmix`; rejects developer or secret-bearing files; and produces:

```text
artifacts/installer/Air-Stack-1.0.0-Windows-x64.exe
artifacts/installer/Air-Stack-installer.json
```

The staged template replaces two obsolete developer-machine paths from the source
template with portable values. The source template is not modified.

## Installed-runtime verification

```powershell
./scripts/verify-windows-installer.ps1 `
  -InstallerPath ./artifacts/installer/Air-Stack-1.0.0-Windows-x64.exe
```

The disposable Windows check installs under Program Files, blocks the production
grace host, exercises the current localhost Licensing Agent contract with a local
boundary fake, verifies allowed and blocked startup paths, and uninstalls. It also
checks that Air Stack user data, its stable installation ID, and independently
owned shared Licensing Agent state survive uninstall.

The installer does not install, update, or remove the shared Licensing Agent. It
does not contain licensing policy, activation logic, updater behavior, secrets,
source code, tests, or the repository's stale prebuilt tools executable.

Code signing, release publication, Digital Solutions artifact registration, and
the pre-existing mismatch between the Tools button and the repository's legacy
tools executable are outside this packaging change.
