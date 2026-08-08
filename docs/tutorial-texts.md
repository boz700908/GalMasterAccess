# 教程与帮助文本提取

说明：以下内容来自《难道你是GAL高手》`Assembly-CSharp.dll` 的反编译结果。当前程序集没有发现名为 `Tutorial`、`Guide` 或 `HelpText` 的专用教程类，也没有独立的 JSON/XML/CSV/TXT 教程文件；大部分游戏说明直接配置在 Unity 场景或资源包中，因此本文件记录代码中能确认的提示和操作行为。

## 已发现的提示文本

### 设置界面

来源：`decompiled/SettingAction.cs`

- `这是一个显示默认速度的示例>3<.`：默认文本速度示例。
- `这是一个显示设置速度的示例>3<.`：当前文本速度示例。
- `是否恢复默认设置`：恢复默认设置前的确认提示。

### 存档/确认提示

来源：`decompiled/SaveloadAction.cs`、`decompiled/UIHanderCenter.cs`

- 存档和读档使用全局确认提示窗口，提示文本通过 `LoadTipTxt.text` 设置。
- 全局确认窗口提供“确定”和“取消”按钮，并将焦点移到确定按钮。
- 关闭游戏时会调用全局确认提示，文本为 `是否关闭游戏`（来源：`UIHanderCenter.cs`）。
- Demo 结束时存在 `是否跳过` 确认提示（来源：`UIHanderCenter.cs`）。

## 可确认的操作行为（不是教程文本）

来源：`decompiled/DialogueHandle.cs`、`decompiled/CGAction.cs`、`decompiled/HistoryAction.cs`、`decompiled/SettingAction.cs`

- `Space`：推进对话；对话 UI 隐藏时也可用于继续。
- `Enter` / 小键盘 `Enter`：在调试对话输入框中确认。
- `Escape`：关闭对话、鉴赏、历史记录、设置或确认提示等当前窗口（具体行为取决于当前界面）。
- 鼠标左键：推进对话或激活 UI。
- 鼠标右键：返回/关闭当前窗口或取消确认。
- 鼠标滚轮：在历史记录中滚动。
- 反引号（`` ` ``）：打开对话调试控制台，仅在调试相关状态下生效。

## 未发现或无法从代码确认的内容

- 没有独立的教程管理器或帮助页面类。
- Unity AssetBundle 中可能包含场景文本，但需要 Unity 资源查看工具才能按场景提取，当前命令行扫描未得到可读文本文件。
- 键盘方向键、数字键和菜单快捷键尚未完成全量分析，不能据此决定安全 Mod 按键。

## 对无障碍功能规划的提示

1. 对话系统（`DialogueHandle`）是最先应分析的功能：它集中处理文本显示、推进、跳过、历史记录和返回。
2. 菜单和设置系统（`MenuAction`、`MenuButton`、`SettingAction`、`SettingButton`）包含可读的 `TextMeshProUGUI` 组件，适合建立首批焦点朗读功能。
3. 确认提示（`ConfirmTipAction`）是通用弹窗入口，应在 Tier 1 UI 分析中记录其按钮和文本字段。
