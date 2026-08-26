# Changelog

## Unreleased

- Unified runtime utilities under the `CDTU.Utils` namespace.
- Removed duplicate BFS implementations and added consistent validation and custom comparers.
- Removed optional UniTask and DOTween dependencies from the core utilities.
- Decoupled pointer conversion from a specific Unity input package.
- Replaced the unsafe cross-thread object pool with an explicit main-thread ownership model.
- Consolidated scene and persistent singleton lifecycle behavior.
- Made the read-only Inspector drawer self-contained inside the Utils folder.
