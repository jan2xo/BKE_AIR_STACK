# BKE AirStack — CLI (Windows)

> Fast, script‑friendly control for **BKE AirStack**. All commands return exit codes and write human‑readable output to stdout (JSON optional in future).

---

## Quick usage
**PowerShell**
```powershell
# Run app normally (UI/host if applicable)
& ".\BKE AirStack.exe"

# See available commands/flags
& ".\BKE AirStack.exe" --help
```

**CMD**
```cmd
"BKE AirStack.exe" --help
```

---

## Expiry controls

> AirStack stores its expiry data in **%LOCALAPPDATA%\bas.dat** (machine+user scoped).

**PowerShell**
```powershell
# Set a specific expiry (UTC ISO-8601, Zulu)
& ".\BKE AirStack.exe" --set-expiry=2025-12-31T23:59:59Z

# Show current expiry and remaining time
& ".\BKE AirStack.exe" --show-expiry

# Extend the current expiry by N days (negative shortens)
& ".\BKE AirStack.exe" --extend-days=7
& ".\BKE AirStack.exe" --extend-days=-3
```

**CMD**
```cmd
"BKE AirStack.exe" --set-expiry=2025-12-31T23:59:59Z
"BKE AirStack.exe" --show-expiry
"BKE AirStack.exe" --extend-days=7
"BKE AirStack.exe" --extend-days=-3
```

---

## Examples (copy/paste)

**Set expiry to end of year**
```powershell
& ".\BKE AirStack.exe" --set-expiry=2025-12-31T23:59:59Z
```

**Extend licence by 30 days**
```powershell
& ".\BKE AirStack.exe" --extend-days=30
```

**Check status (prints expiry + remaining time)**
```powershell
& ".\BKE AirStack.exe" --show-expiry
```

---

## Notes
- Time format must be **UTC Z** (e.g., `2025-12-31T23:59:59Z`).
- For unattended scripts, prefer **CMD** form or PowerShell with `Start-Process -Wait` and check `$LASTEXITCODE`.
- The data file name for AirStack is **bas.dat** (Render Dock uses **brd.dat**). No other behavior is shared by default.
