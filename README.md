# GalMasterAccess

GAL PRO MASTER 的中文屏幕阅读器与键盘导航 Mod。

## 安装

1. 确认游戏已经安装 MelonLoader 0.7.1 Open-Beta。
2. 下载 Release 中的 `GalMasterAccess-v*.zip`。
3. 将压缩包内容解压到 `GAL PRO MASTER.exe` 所在的游戏目录，并允许覆盖同名 Mod 文件。
4. 确认游戏目录中存在 `Tolk.dll`、`nvdaControllerClient64.dll` 和 `Mods/GalMasterAccess.dll`。

压缩包不包含游戏本体或游戏资源；这些文件必须由 Steam 安装提供。`UserData` 会随包提供，因为游戏的隐藏控制台配置保存在其中。

## 按键

- 上下方向键：浏览当前页面控件
- 左右方向键：调整滑块或下拉框
- Enter：调用当前控件的游戏原有点击逻辑
- Escape：返回
- F12：切换调试日志（默认开启）

## 构建

使用 PowerShell 执行 `scripts/Build-Mod.ps1`。项目依赖游戏目录中的 Unity 和 MelonLoader 程序集。

## 许可证

本项目使用 MIT 许可证。详见 [LICENSE](LICENSE)。
