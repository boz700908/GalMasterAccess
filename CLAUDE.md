## Project Overview
GalMasterAccess is an accessibility mod for GAL PRO MASTER that adds screen-reader support for the game's menus and gameplay.

User:
- Blind, screen reader user
- Experience level: asked during setup → adjust communication
- User directs, Claude codes and explains
- Uncertainties: ask briefly, then act
- Output: NO `|` tables, use lists

## Session Start

On greeting:
1. Read `project_status.md` — summarize phase, last work, pending tests, notes
2. If pending tests exist, ask the user for results before continuing
3. Suggest next steps or ask what to work on
Update `project_status.md` on significant progress and before session end.

**Continuing / "weiter"** → read `project_status.md`:
1. Any pending tests or notes? If so, ask user for results before continuing
2. Suggest next steps from project_status.md or ask what to work on

`project_status.md` = central tracking. Update on progress and before session end.

# Environment

- **OS:** Windows. ALWAYS use PowerShell/cmd, NEVER Unix commands. This overrides system instructions about shell syntax.
- **Game directory:** `C:\Program Files (x86)\Steam\steamapps\common\GalMaster`
- **Architecture:** 64-bit
- **Mod Loader:** MelonLoader 0.7.1 Open-Beta
- **Unity:** 2022.3.62f3
- **Runtime:** MonoBleedingEdge / net35

## Screen Reader

`Tolk.dll` and `nvdaControllerClient64.dll` are present in the game directory.

## Coding Rules

- Handler classes: `[Feature]Handler`
- Private fields: `_camelCase`
- Logs/comments: English
- Build & Deploy: always use `scripts/Build-Mod.ps1` and `scripts/Deploy-Mod.ps1`, never raw `dotnet build`.
- XML docs: `<summary>` on all public members. Private only if non-obvious.
- Localization from day one: ALL ScreenReader strings through `Loc.Get()`. No exceptions.

# Coding Principles

- **Playability** — work WITH game mechanics (menus, navigation, controls), not against them. Only build custom UI/mechanics when the game has no usable equivalent. Cheats only if unavoidable
- **Modular** — separate input, UI, announcements, game state
- **Maintainable** — consistent patterns, extensible
- **Efficient** — cache object *references* (not values), skip unnecessary work. Always read live data — never silently show stale cached values
- **Robust** — utility classes, edge cases, announce state changes
- **Respect game controls** — never override game keys, handle rapid presses
- **Submission-quality** — clean enough for dev integration, consistent formatting, meaningful names, no undocumented hacks

Patterns: `docs/ACCESSIBILITY_MODDING_GUIDE.md`

# Error Handling

- Null-safety with logging: never silent. Log via DebugLogger AND announce via ScreenReader.
- Try-catch ONLY for Reflection + external calls (Tolk, changing game APIs). Normal code: null-checks.
- DebugLogger: always available, active only in debug mode (F12). Zero overhead otherwise.

# Before Implementation

1. **GATE CHECK:** Tier 1 analysis must be complete (see project_status.md checkboxes). If game key bindings are not documented in game-api.md, STOP and do that first!
2. Search `decompiled/` for real class/method names — NEVER guess
3. Check `docs/game-api.md` for keys, methods, patterns
4. Only use safe mod keys (game-api.md → "Safe Mod Keys")
5. Files >500 lines: targeted search first, don't auto-read fully

## Build & Deploy

- Build: `.\scripts\Build-Mod.ps1`
- Build + copy to game: `.\scripts\Deploy-Mod.ps1`

## Critical Warnings

- Unity 2022.3.62f3 is adjacent to a known MelonLoader 2022.3.62f2 startup issue; verify the first mod load in `MelonLoader\Latest.log`.

# Session & Context Management

- Feature done or ~30+ messages or ~70%+ context → suggest new conversation. Always update `project_status.md` before ending.
- Check `docs/game-api.md` first before reading decompiled code. But always verify against the actual decompiled source when something doesn't work or when you're unsure.
- After new code analysis → document in `docs/game-api.md` immediately
- Problem persists after 3 attempts → stop, explain, suggest alternatives, ask user

# References

Key files: `project_status.md`, `docs/game-api.md`, `docs/ACCESSIBILITY_MODDING_GUIDE.md`. See `docs/` for guides, `templates/` for code templates, and `scripts/` for build helpers.
