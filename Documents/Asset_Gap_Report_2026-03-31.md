# INTIFALL Asset Gap Report (2026-03-31)

## Scope
- Repository: `https://github.com/gwdwws7-afk/steal`
- Branch: `master` (HEAD `60fb064`)
- Local path: `C:\test\Steal`

## Summary
- Runtime code and EditMode tests are present.
- Scene and prefab content assets are effectively missing from git history.
- Current build settings reference scene paths that do not exist in the working tree.

## Evidence
1. Git history contains no tracked `.unity` scene files and no tracked `.prefab` files.
   - `git ls-tree -r --name-only HEAD` filtered by `*.unity` / `*.prefab` returns none.
   - Initial commit `9e84de2` includes only:
     - `Assets/Scenes/Cockpit.meta`
     - `Assets/Scenes/Garden.meta`
     - `Assets/Scenes/Oasis.meta`
     - `Assets/Scenes/Terminal.meta`
2. No submodule-based content source.
   - `.gitmodules` does not exist.
3. No Git LFS tracked asset files in this repository.
   - `git lfs ls-files` returns no entries.
4. Build settings reference non-existent scenes.
   - `ProjectSettings/EditorBuildSettings.asset` includes paths such as:
     - `Assets/Scenes/Level01_Qhapaq_Passage.unity`
     - `Assets/Scenes/Level02_Temple_Complex.unity`
   - These files are absent in local workspace.
5. Commit `60fb064` only modified `EditorBuildSettings.asset`.
   - It did not add corresponding scene files.

## Impact
- PlayMode scene flow cannot be validated end-to-end.
- Main menu / level transition may fail at runtime if target scenes are not available.
- Tool spawning that depends on prefab assets can fail in runtime even when code compiles.

## Recovery Options
1. Recover from original content source (recommended).
   - Re-import missing `.unity`, `.prefab`, and referenced art/audio assets from the author's source project.
2. If source is unavailable, create temporary placeholder scenes/prefabs.
   - Minimal scenes matching build settings names.
   - Minimal tool prefabs for smoke testing.
3. Add a validation step to CI/preflight.
   - Verify required scene names and tool prefabs exist before test/build.

## Immediate Mitigations Applied in Code
- Align runtime level scene names with current build settings naming scheme.
- Make tool instantiation support direct prefab references in `ToolData` first, with Resources fallback.

