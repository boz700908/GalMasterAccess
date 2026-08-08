# GAL PRO MASTER - Game API Documentation

## Overview

- **Game:** GAL PRO MASTER (《难道你是GAL高手》)
- **Engine:** Unity 2022.3.62f3
- **Runtime:** MonoBleedingEdge / net35 loader runtime
- **Architecture:** 64-bit
- **Developer:** GAL高手制作组

The findings below are based on the decompiled Mono `Assembly-CSharp.dll`. Unity's serialized Input Manager asset is not exposed as readable source, so bindings observed in code are listed explicitly and any unobserved keys remain unassigned.

## 1. Singleton Access Points

- `Singleton<T>.m_Instence`: shared singleton backing field used by game systems.
- `UIHanderCenter.m_Instence`: central UI action coordinator.
- `DialogueHandle.m_Instence`: dialogue state, text and input handling.
- `SaveLoadManager.m_Instence`: save data and `CurrentSettings`.
- `SceneLoadManager.m_Instence`: scene transitions and return to title.
- `GameAudioManager.m_Instence`: music and SFX.
- `GameSetUp.m_Instence`: demo/test flags and setup state.

## 2. Game Key Bindings (Original)

### Keyboard

- Unity Input Manager axes: `Vertical` maps Up/Down and W/S; `Horizontal` maps Left/Right and A/D.
- Unity Input Manager submit: `Submit` maps Return/Enter and joystick button 0.
- Unity Input Manager cancel: `Cancel` maps Escape and joystick button 1.
- `Escape`: closes dialogue, gallery, history, save/load, settings and confirmation windows when those handlers are active.
- `Space`: advances dialogue; also resumes a hidden dialogue UI and participates in skip behavior.
- `Enter` / `KeypadEnter`: confirms the debug dialogue input and is recognized as dialogue confirmation.
- `` ` `` (`BackQuote`): opens the dialogue debug console when debug state allows it.
- `LeftControl` / `RightControl`: held-state check used by dialogue speed/skip logic.

### Mouse

- Left button: advances dialogue, skips allowed demo video input, and activates UI buttons.
- Right button: closes/back-outs of dialogue, history, save/load, settings, staff and confirmation windows.
- Middle button: recognized as an alternate dialogue input.
- `Mouse ScrollWheel`: scrolls the history log.
- `Mouse X` / `Mouse Y`: read by controller/input abstraction for pointer movement.

### Controller abstraction

- Buttons read by `Flan_BN.Controller.ControllerManager`: `A`, `B`, `X`, `Y`, `RB`, `LB`, `Option`, `Map`.
- Axes read: `LT-RT`, `LeftHandle`, `RightHandle`, `LeftHandleY`, `RightHandleY`.

### Unobserved bindings

- No direct `KeyCode` references for arrow keys, WASD, number keys or F-keys were found in `Assembly-CSharp.dll`. Unity EventSystem may still map them through serialized Input Manager settings; verify in-game before reserving them.

## 3. Safe Mod Keys

- `F1`-`F11`: no direct use found in decompiled code. Treat as provisionally safe for accessibility commands, then verify against the game's Input Manager/in-game behavior.
- `F12`: reserved for debug mode by this project; not claimed as game-safe until tested.
- Do not use `Escape`, `Space`, `Enter`, mouse buttons, or controller buttons for global mod commands because they are active game controls.

## 4. UI System

### UI base classes and coordinators

- `UIHanderCenter : Singleton<UIHanderCenter>` coordinates `MenuAction`, `SettingAction`, `SaveloadAction`, `HistoryAction`, `StaffAction`, `CGAction`, `LoadAction`, and `SelectionAction`.
- Feature windows use `InitAction()` and `UninitAction()` methods and toggle their root `GameObject` active state.
- Unity `EventSystem.current` is used for selection; `SettingAction` and `UIHanderCenter` clear the selected object during transitions.
- `UISfxTrigger.BindActiveSelectablesInChildren` binds active Unity `Selectable` components after windows initialize.
- Navigation mode is serialized in the Unity scene and is not visible in C# decompilation. `NavigationHelper` logs each active control's mode and changes only `Navigation.Mode.None` to `Automatic`; existing explicit navigation is preserved.
- Dialogue choice buttons are created by `SelectionAction.Open` and may be inactive outside a choice; active choices are inspected at runtime.
- Accessibility adds a non-clickable `DialogueTextFocus` selectable before the real `DialogueHandle.dialogueButtons`; Enter on that focus is left to the game's `DialogueHandle.InputLogic`, while Enter on a real button invokes its existing `Button.onClick`.
- `CGAction` creates gallery entries as `CGUnit` pointer-click handlers rather than `Selectable` buttons. Keyboard support must invoke the real `IPointerClickHandler` event.
- `SaveloadAction` quick slots are `QLSlot` pointer-click handlers; their visible label is `QLSlot.slotText` and activation is `QLSlot.SlotClick()`.
- `HistoryObj` exposes `historyNameTxt`, `historyText`, `historyVoiceBtn`, and `historyReturnBtn`; history scrolling is handled by `HistoryAction`'s `IScrollHandler.OnScroll(PointerEventData)` path. Keyboard Up/Down is translated into the same Unity scroll event so the game's own bounds, easing, and close-at-bottom behavior remain active.
- Settings navigation includes `SettingAction.Buttons`, `Sliders`, all ten public `SettingButton` fields, and the three save/reset/close buttons plus every public slider.
- Gallery navigation includes `CGAction.galleryListButtons`, `closeButton`, and runtime `CGUnit` entries. `CGUnit.OnPointerClick` remains the activation path.
- Save/load navigation includes slot/delete buttons, page toggles, quick-save toggle, close button, and runtime `QLSlot` pointer-click entries.
- Input ownership: `Main` dispatches only the highest active game UI each frame (additional pages, then title menu, then dialogue). This prevents an inactive lower layer from replacing the current `EventSystem` selection.
- Mouse parity: each active page additionally scans its visible `Selectable` controls and real `IPointerClickHandler`/`IPointerEnterHandler` components; focus selection dispatches the game's pointer-enter path and confirmation dispatches pointer down/click/up.
- `StaffAction` displays two full-screen images and advances through its real `ShowSecondImage()` path; it has no Unity selectable controls.
- `CGAction.formalGalleryScrollbar` is explicitly assigned `Navigation.Mode.None` in `Awake`/initialization and is deliberately excluded from automatic navigation.

### Text access

- Dialogue text: public `TextMeshProUGUI mainDiagueText` on `DialogueHandle`.
- Dialogue speaker: public `TextMeshProUGUI mainDialogueName` and `mainDialogueName_Speak`.
- Setting examples: public `TextMeshProUGUI DefaultTextInOpition` and `ShowTextInOpition`.
- Selection option labels: `SelectionAction` finds child `TMP_Text` components on each option `Button`.
- Global confirmation text: private `_loadTipText` in `ConfirmTipAction`, assigned from public `SaveloadAction.LoadTipTxt`; use the public `LoadTipTxt` path where possible.
- Many visual controls are public fields (`Button`, `Slider`, `Image`) on the action classes. Reflection is not required for the documented primary text fields; use `ReflectionHelper` only when a future scene object exposes private-only data.

### Main menu

- Class: `MenuAction`.
- Buttons in order: `startNewGameButton`, `loadGameButton`, `settingButton`, `cGButton`, `staffButton`, `exitButton`.
- `MenuButton.buttonIndex` maps to new game, load, settings, gallery, staff, and quit.
- AssetRipper UI sprites provide the real labels: `开始1`, `载入1`, `设置1`, `鉴赏1`, `鸣谢1`, and `退出1`.
- Buttons become interactable after the fade animation; `MenuButton.button` is the Unity `Button` component.

### Settings

- Class: `SettingAction`.
- Controls: volume sliders, text speed sliders, dialogue background alpha slider, fullscreen/windowed buttons, voice mute buttons, close/reset/save buttons.
- `SettingButton.SettingButtonID` maps settings behavior; IDs 6-9 mute character voices.

### Dialogue and other windows

- Dialogue: `DialogueHandle` with `mainDiagueText`, speaker labels, `dialogueButtons`, and `voiceButton`.
- History: `HistoryAction`; closes with Escape/right mouse and scrolls using `Mouse ScrollWheel`.
- Save/load: `SaveloadAction` and `SaveLoadSlot`; global confirmation uses `ConfirmTipAction`.
- Gallery: `CGAction`; closes with Escape/right mouse.
- Staff: `StaffAction`; closes with Escape/right mouse.

## 5. Game Mechanics

- Core progression is dialogue-driven. `DialogueHandle` owns current dialogue data/index and text progression.
- Menu actions are routed through `UIHanderCenter` to scene, save/load, settings, gallery and staff systems.

## 6. Status and Notifications

- Confirmation and load messages are shown through `UIHanderCenter.TryShowGlobalConfirmTip` and `SaveloadAction.ShowLoadTip`.
- No separate toast/notification manager was identified in the decompiled assembly.

## 7. Audio System

- Pending decompilation.

## 8. Save and Load

- Pending decompilation.

## 9. Harmony Patch Points

- UI initialization: `UIHanderCenter.OpenMenuAction`, `OpenLoadAction`, `OpenSettingAction`, `OpenCGAction`, `OpenStaffAction`.
- Dialogue initialization/progression: `DialogueHandle` methods around its dialogue UI setup and input handling.
- Menu selection/action: `MenuButton.OnButtonClick`.
- Settings changes: `SettingButton.OnPointerClick`, `SettingAction.AddActionInButtons`.
- Confirmation display: `UIHanderCenter.TryShowGlobalConfirmTip`, `ConfirmTipAction.Show`.
- Prefer postfixes on public action methods so announcements occur after the game updates visible state.

## 10. Localization

- Mod output language: Chinese only.
- Game localization system: pending analysis; only needed if reusing game language state or terms.

## 11. Code Examples

- Add examples after real classes and methods are identified.

## 12. Known Problems and Workarounds

- Unity 2022.3.62f3 is near the known 2022.3.62f2 MelonLoader compatibility warning; verify loader behavior in-game.

## 13. Not Yet Analyzed

- [ ] Namespaces, entry points, and singleton patterns
- [ ] Input system and original key bindings
- [ ] UI class hierarchy and text access
- [ ] Event and Harmony hook candidates

## Change History

- **2026-08-08:** Initialized project documentation from setup interview.
