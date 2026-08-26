# Air Stack

## Update notifications

After the authorized WinForms UI is shown, Air Stack reads cached update state from the local Licensing Agent for `bke-air-stack` and the included `bke-render-dock` module. The query is non-blocking and never contacts Digital Solutions directly. Digital or Agent unavailability does not change startup authorization. Update actions remain Agent-owned; Air Stack never receives artifact URLs or invokes the updater helper.

## Product identity convention

- `displayName` is the human-readable customer-facing product brand: `Air Stack`.
- `productId` is the machine/licensing identity: `bke-air-stack`.
- New products use `bke-<normalized-product-name>`; for example, `Render Dock` uses `bke-render-dock`.
- Treat `productId` as immutable after a real commercial lifecycle begins.
- Executable, repository, project, and other internal names do not need to match `displayName` exactly.
