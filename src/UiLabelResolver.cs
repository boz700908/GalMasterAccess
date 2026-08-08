using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace GalMasterAccess
{
    /// <summary>
    /// Resolves labels from the actual UI Sprite names exported from the game.
    /// </summary>
    internal static class UiLabelResolver
    {
        public static string Resolve(Selectable selectable)
        {
            if (selectable == null)
            {
                return string.Empty;
            }

            SaveLoadSlot saveSlot = selectable.GetComponentInParent<SaveLoadSlot>();
            if (saveSlot != null)
            {
                if (selectable == saveSlot.deleteButton) return "删除存档";
                if (selectable == saveSlot.slotButton)
                {
                    string name = saveSlot.saveDataNameText != null ? saveSlot.saveDataNameText.text.Trim() : string.Empty;
                    string time = saveSlot.saveTimeText != null ? saveSlot.saveTimeText.text.Trim() : string.Empty;
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(time)) return "无存档";
                    if (string.IsNullOrEmpty(name)) return time;
                    if (string.IsNullOrEmpty(time)) return name;
                    return name + "，" + time;
                }
            }

            HistoryObj historyItem = selectable.GetComponentInParent<HistoryObj>();
            if (historyItem != null)
            {
                if (selectable == historyItem.historyVoiceBtn) return "重播语音";
                if (selectable == historyItem.historyReturnBtn) return "返回";
            }

            SaveloadAction saveLoad = selectable.GetComponentInParent<SaveloadAction>();
            if (saveLoad != null)
            {
                if (selectable == saveLoad.SureBtn) return "确认";
                if (selectable == saveLoad.CancelBtn) return "取消";
            }

            CGUnit cgUnit = selectable.GetComponent<CGUnit>();
            if (cgUnit != null)
            {
                return string.IsNullOrWhiteSpace(cgUnit.GroupKey) ? "鉴赏项目" : "鉴赏 " + cgUnit.GroupKey;
            }

            CGAction gallery = selectable.GetComponentInParent<CGAction>();
            string galleryLabel = ResolveGalleryButton(gallery, selectable.gameObject);
            if (!string.IsNullOrEmpty(galleryLabel)) return galleryLabel;

            SettingAction settings = UIHanderCenter.m_Instence != null ? UIHanderCenter.m_Instence.settingAction : null;
            if (settings != null)
            {
                if (selectable == settings.ResolutionDropdown) return "分辨率";
                if (selectable == settings.CloseActionButton) return "返回";
                if (selectable == settings.ResetAllSettingButton) return "恢复默认设置";
                if (selectable == settings.SaveSettingButton) return "保存设置";
            }

            SettingButton settingButton = selectable.GetComponent<SettingButton>();
            if (settingButton != null)
            {
                switch (settingButton.SettingButtonID)
                {
                    case 0: return "全屏模式";
                    case 1: return "窗口模式";
                    case 2: return "播放时降低BGM";
                    case 3: return "切页停止播放语音";
                    case 4: return "已读跳过";
                    case 5: return "已读变色";
                    case 6: return "MM语音静音";
                    case 7: return "MCL语音静音";
                    case 8: return "HXC语音静音";
                    case 9: return "BXY语音静音";
                }
            }

            Image image = selectable.GetComponentInChildren<Image>(true);
            string spriteName = image != null && image.sprite != null ? image.sprite.name : string.Empty;
            string label = ResolveSprite(spriteName);
            return string.IsNullOrEmpty(label) ? selectable.gameObject.name : label;
        }

        private static string ResolveGalleryButton(CGAction gallery, GameObject target)
        {
            if (gallery == null || target == null) return string.Empty;
            FieldInfo buttonsField = typeof(CGAction).GetField("galleryListButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo poolsField = typeof(CGAction).GetField("galleryPools", BindingFlags.Instance | BindingFlags.NonPublic);
            IList buttons = buttonsField != null ? buttonsField.GetValue(gallery) as IList : null;
            IList pools = poolsField != null ? poolsField.GetValue(gallery) as IList : null;
            if (buttons == null || pools == null) return string.Empty;
            for (int i = 0; i < buttons.Count && i < pools.Count; i++)
            {
                Button button = buttons[i] as Button;
                if (button == null || (button.gameObject != target && !target.transform.IsChildOf(button.transform))) continue;
                object pool = pools[i];
                FieldInfo nameField = pool != null ? pool.GetType().GetField("PoolName") : null;
                return nameField != null ? nameField.GetValue(pool) as string : string.Empty;
            }
            return string.Empty;
        }

        public static string ResolveSlider(Slider slider)
        {
            if (slider == null) return string.Empty;
            SettingAction settings = UIHanderCenter.m_Instence != null ? UIHanderCenter.m_Instence.settingAction : null;
            if (settings != null)
            {
                if (slider == settings.MasterVolumeSlider) return "全音量";
                if (slider == settings.VoiceVolumeSlider) return "语音音量";
                if (slider == settings.BGMVolumeSlider) return "背景音乐音量";
                if (slider == settings.SEVolumeSlider) return "音效音量";
                if (slider == settings.TextSpeedSlider) return "速度";
                if (slider == settings.TextSpeedSliderInAutoMode) return "自动模式";
                if (slider == settings.DialogueBkAlphaSlider) return "对话框背景透明度";
            }
            return Resolve(slider);
        }

        public static string ResolveSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return string.Empty;
            }

            switch (spriteName)
            {
                case "开始1": return "开始";
                case "载入1": return "载入";
                case "设置1": return "设置";
                case "鉴赏1": return "鉴赏";
                case "鸣谢1": return "鸣谢";
                case "退出1": return "退出";
                case "config_menu_reset_normal":
                case "config_menu_reset_click": return "恢复默认设置";
                case "config_resolution_submenu_bg_collapse":
                case "config_resolution_submenu_bg_expand": return "分辨率选项";
                case "config_bo1_normal":
                case "config_bo1_confirm":
                case "config_bo1_confirm_click":
                case "config_bo1_click": return "设置选项";
                case "dialog_menu_bo_auto_normal":
                case "dialog_menu_bo_auto_confirm":
                case "dialog_menu_bo_auto_click": return "自动播放";
                case "dialog_menu_bo_hide_normal":
                case "dialog_menu_bo_hide_confirm":
                case "dialog_menu_bo_hide_click": return "隐藏对话框";
                case "dialog_menu_bo_history_normal":
                case "dialog_menu_bo_history_confirm":
                case "dialog_menu_bo_history_click": return "历史记录";
                case "dialog_menu_bo_load_normal":
                case "dialog_menu_bo_load_confirm":
                case "dialog_menu_bo_load_click": return "载入游戏";
                case "dialog_menu_bo_ql_normal":
                case "dialog_menu_bo_ql_confirm":
                case "dialog_menu_bo_ql_click": return "快速载入";
                case "dialog_menu_bo_qs_normal":
                case "dialog_menu_bo_qs_confirm":
                case "dialog_menu_bo_qs_click": return "快速保存";
                case "dialog_menu_bo_save_normal":
                case "dialog_menu_bo_save_confirm":
                case "dialog_menu_bo_save_click": return "保存游戏";
                case "dialog_menu_bo_skip_normal":
                case "dialog_menu_bo_skip_confirm":
                case "dialog_menu_bo_skip_click": return "跳过已读内容";
                case "dialog_menu_bo_system_normal":
                case "dialog_menu_bo_system_confirm":
                case "dialog_menu_bo_system_click": return "系统设置";
                case "sl_menu_auto_normal":
                case "sl_menu_auto_confirm":
                case "sl_menu_auto_click": return "自动存档页";
                case "sl_menu_quick_normal":
                case "sl_menu_quick_confirm":
                case "sl_menu_quick_click": return "快速存档页";
                case "general_menu1_back_normal":
                case "general_menu1_back_click": return "返回";
                case "general_menu2_bo_revoice_normal":
                case "general_menu2_bo_revoice_confirm":
                case "general_menu2_bo_revoice_click": return "重播语音";
                case "history_menu_bo_jump_normal":
                case "history_menu_bo_jump_confirm":
                case "history_menu_bo_jump_click": return "跳转到当前记录";
                case "Staff00": return "鸣谢第一页";
                case "Staff01": return "鸣谢第二页";
            }

            if (spriteName.StartsWith("general_menu1_", System.StringComparison.Ordinal))
            {
                string suffix = spriteName.Substring("general_menu1_".Length);
                if (int.TryParse(suffix.Split('_')[0], out int slot))
                {
                    return $"存档槽 {slot}";
                }
            }

            return spriteName;
        }
    }
}
