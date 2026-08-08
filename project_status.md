# Project Status: GalMasterAccess

## Project Info

- **Game:** GAL PRO MASTER (《难道你是GAL高手》)
- **Engine:** Unity 2022.3.62f3
- **Architecture:** 64-bit
- **Mod Loader:** MelonLoader 0.7.1 Open-Beta
- **Runtime:** net35 (project target: net472 per loader helper)
- **Game directory:** C:\Program Files (x86)\Steam\steamapps\common\GalMaster
- **User experience level:** Some experience
- **User game familiarity:** Somewhat
- **Mod languages:** Chinese only

## Setup Progress

- [x] Experience level determined
- [x] Game name and path confirmed
- [x] Game familiarity assessed
- [x] Game directory auto-check completed
- [x] Mod loader selected and installed (MelonLoader)
- [x] Tolk DLLs in place (Tolk.dll + nvdaControllerClient64.dll)
- [x] .NET SDK available
- [x] Decompiler tool ready (ilspycmd 9.1.0.7988)
- [x] Game code decompiled to `decompiled/`
- [x] Tutorial texts extracted (code-visible prompts documented in `docs/tutorial-texts.md`)
- [x] Multilingual support decided (Chinese only)
- [x] Project directory set up (csproj, Main.cs, screen reader, localization, state manager)
- [x] CLAUDE.md updated with project-specific values
- [x] First build successful
- [x] "Mod loaded" announcement working in game

## Current Phase

**Phase:** Framework
**Currently working on:** Full UI control audit and game-event navigation
**Blocked by:** Nothing

## Codebase Analysis Progress

### GATE: Tier 1 MUST be complete before Phase 2 (Framework)!

- [x] 1.1 Structure overview (namespaces, singletons) -> documented in game-api.md
- [x] 1.2 Input system - observed game key bindings documented in game-api.md; serialized Input Manager still needs runtime verification
- [x] 1.2 Input system - provisional safe mod keys identified and listed in game-api.md
- [x] 1.3 UI system (base classes, text access patterns, Reflection needed?)
- [x] 1.4 State management decision -> documented below
- [x] 1.5 Localization: Chinese-only mod; game localization analysis deferred unless needed

## Game Key Bindings (Original)

- Observed keyboard, mouse, and controller bindings are documented in `docs/game-api.md`; serialized Unity Input Manager bindings still need runtime verification.

## Implemented Features

- Basic MelonLoader framework compiled and copied to `Mods/GalMasterAccess.dll`.
- Main menu and UI labels are resolved from AssetRipper-exported sprite names and real game text fields.
- Dialogue handler: announces real speaker/text values and inspects/enables navigation for active dialogue and choice controls when the scene reports `Navigation.Mode.None`.
- Dialogue speaker fix: prefer the game's `characterName` field; use `characterName_Speak` only as fallback because it can contain English pronunciation names.
- Additional page support: settings, save/load, history, gallery, staff and confirmation pages are discovered from `UIHanderCenter`; `CGUnit` and `QLSlot` pointer handlers receive keyboard activation without inventing labels.
- Navigation audit: the first generic implementation incorrectly included visual `Selectable` objects such as `CardBg`; page-specific control lists now use the game's public fields and real pointer-handler classes.
- Dialogue context: a non-clickable text focus is inserted before real dialogue buttons; text focus preserves the game's own Enter-to-advance path.
- History text browsing: Up/Down now dispatch Unity `IScrollHandler` scroll events to the real `HistoryAction`, and announce the currently visible `HistoryObj` name/text.
- Focus ownership: only the topmost active game UI handler now updates keyboard focus, preventing lower pages from taking focus through an open overlay.
- Mouse parity: visible real Unity selectables and pointer handlers on active pages are included in keyboard scanning; Enter dispatches Unity pointer down/click/up.

## Pending Tests

- Main menu: verify the first active button announces its actual Sprite name.
- Dialogue: verify a line is announced once, choices are announced, and keyboard navigation works for active controls.
- Keyboard fallback: test Up/Down/Left/Right, Tab, and Enter in the main menu and choice panel.
- Additional pages: test settings sliders/buttons, save/load slots and tabs, history Up/Down text browsing, gallery `CGUnit` entries, staff image advance, and confirmation buttons.
- Restart the game after the latest deployment; confirm `Latest.log` no longer floods with per-frame navigation entries.
- Main menu regression fix: collect `MenuButton` references before fade-in and select after they become active.
- Main menu: verify disabled buttons (for example locked gallery/staff entries) are skipped.
- Settings: controls now come from SettingAction's real sliders/buttons/dropdowns; selection announces label plus live value/state, and slider arrows announce value only.

## Known Issues

- Unity 2022.3.62f3 is adjacent to a known MelonLoader issue listed for 2022.3.62f2; monitor `MelonLoader/Latest.log` during first mod load.

## Architecture Decisions

- Decompile the Mono `Assembly-CSharp.dll` with `ilspycmd` before writing feature code.
- Use `AccessStateManager`: at least dialogue, settings, save/load, history, gallery, staff, and confirmation windows share Escape/right mouse and need exclusive input ownership.

## Key Bindings (Mod)

- Up/Down: browse the active page's controls; on History, scroll the real history content
- Left/Right: adjust the selected slider only
- Enter: dispatch the selected control's real pointer click
- Escape: leave to the game's own Escape handling
- F12: debug mode toggle; enabled by default

## Notes for Next Session

- Main menu handler is implemented; next test should verify focus movement and Chinese button announcements.
- Latest build deployed after settings value/name and menu native-navigation suppression changes. Runtime verification still requires opening the settings and main-menu pages in-game.
- Native navigation suppression is now conditional: controls are only rewritten when the game changes their mode away from None; this applies to all handlers and the dialogue text focus.
