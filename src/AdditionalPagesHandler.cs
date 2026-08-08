using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GalMasterAccess
{
    /// <summary>
    /// Provides navigation and announcements for the game's non-dialogue UI pages.
    /// </summary>
    public sealed class AdditionalPagesHandler
    {
        private readonly List<Selectable> _controls = new List<Selectable>();
        private GameObject _lastSelected;
        private Selectable _confirmationTextFocus;
        private Selectable _historyTextFocus;
        private string _activePage;
        private AccessStateManager.State _activeState;
        private int _lastRefreshFrame = -100;
        private int _lastControlCount = -1;
        private bool _settingAdjustment;

        /// <summary>
        /// Returns whether a game page above dialogue or the title menu is currently open.
        /// </summary>
        public bool HasActivePage()
        {
            string page;
            return FindActivePage(out page) != null;
        }

        /// <summary>
        /// Updates the currently active page using the game's actual UI hierarchy.
        /// </summary>
        public void Update()
        {
            string page;
            GameObject root = FindActivePage(out page);
            if (root == null)
            {
                Reset();
                return;
            }

            if (_activePage != page)
            {
                if (_confirmationTextFocus != null)
                {
                    Object.Destroy(_confirmationTextFocus.gameObject);
                    _confirmationTextFocus = null;
                }
                if (_historyTextFocus != null)
                {
                    Object.Destroy(_historyTextFocus.gameObject);
                    _historyTextFocus = null;
                }
                _activePage = page;
                _lastSelected = null;
                _controls.Clear();
                _lastControlCount = -1;
                _activeState = GetState(page);
                AccessStateManager.TryEnter(_activeState);
            }

            if (Time.frameCount - _lastRefreshFrame >= 5)
            {
                RefreshControls(root);
                RefreshSpecialControls(root);
                _lastRefreshFrame = Time.frameCount;
            }
            EnsureSelection();
            EnsureCurrentNavigation();
            if (!HandleHistoryKeyboardInput())
            {
                HandleKeyboardInput();
                HandleSpecialKeyboardInput();
            }
            AnnounceSelection();
        }

        private GameObject FindActivePage(out string page)
        {
            page = null;
            UIHanderCenter center = UIHanderCenter.m_Instence;
            if (center == null)
            {
                return null;
            }

            foreach (ConfirmTipAction confirmation in Object.FindObjectsOfType<ConfirmTipAction>())
            {
                if (confirmation != null && confirmation.IsOpen && confirmation.gameObject.activeInHierarchy)
                {
                    page = "confirmation";
                    return confirmation.gameObject;
                }
            }

            if (center.saveloadAction != null && center.saveloadAction.LoadTip != null &&
                center.saveloadAction.LoadTip.activeInHierarchy)
            {
                page = "save-confirmation";
                return center.saveloadAction.LoadTip;
            }

            if (center.settingAction != null && center.settingAction.gameObject.activeInHierarchy)
            {
                page = "settings";
                return center.settingAction.gameObject;
            }
            if (center.saveloadAction != null && center.saveloadAction.gameObject.activeInHierarchy)
            {
                page = "save-load";
                return center.saveloadAction.gameObject;
            }
            if (center.historyAction != null && center.historyAction.gameObject.activeInHierarchy)
            {
                page = "history";
                return center.historyAction.gameObject;
            }
            if (center.cgAction != null && center.cgAction.gameObject.activeInHierarchy)
            {
                page = "gallery";
                return center.cgAction.gameObject;
            }
            if (center.staffAction != null && center.staffAction.gameObject.activeInHierarchy)
            {
                page = "staff";
                return center.staffAction.gameObject;
            }

            return null;
        }

        private static AccessStateManager.State GetState(string page)
        {
            switch (page)
            {
                case "save-load": return AccessStateManager.State.SaveLoad;
                case "history": return AccessStateManager.State.History;
                case "gallery": return AccessStateManager.State.Gallery;
                case "staff": return AccessStateManager.State.Staff;
                case "confirmation": return AccessStateManager.State.Confirmation;
                case "save-confirmation": return AccessStateManager.State.Confirmation;
                default: return AccessStateManager.State.Settings;
            }
        }

        private void RefreshControls(GameObject root)
        {
            _controls.Clear();
            if (IsConfirmationPage())
            {
                EnsureConfirmationTextFocus(root);
                AddControl(_confirmationTextFocus);
            }
            if (_activePage == "history")
            {
                EnsureHistoryTextFocus(root);
                AddControl(_historyTextFocus);
            }

            AddMouseReachableControls(root);
            if (_controls.Count != _lastControlCount)
            {
                _lastControlCount = _controls.Count;
                DebugLogger.LogState($"Accessibility page {_activePage}: {_controls.Count} mouse-reachable controls");
            }
        }

        private void AddMouseReachableControls(GameObject root)
        {
            if (_activePage == "settings")
            {
                SettingAction settings = UIHanderCenter.m_Instence != null ? UIHanderCenter.m_Instence.settingAction : null;
                if (settings != null)
                {
                    if (settings.Sliders != null) foreach (Slider slider in settings.Sliders) AddControl(slider);
                    AddControl(settings.MasterVolumeSlider); AddControl(settings.VoiceVolumeSlider);
                    AddControl(settings.BGMVolumeSlider); AddControl(settings.SEVolumeSlider);
                    AddControl(settings.TextSpeedSlider); AddControl(settings.TextSpeedSliderInAutoMode);
                    AddControl(settings.DialogueBkAlphaSlider);
                    if (settings.Buttons != null) foreach (Button button in settings.Buttons) AddControl(button);
                    // SettingAction.dropdown is an unused legacy template field; ResolutionDropdown
                    // is the only dropdown wired to the game's settings logic.
                    AddControl(settings.ResolutionDropdown);
                    AddProxyControl(settings.FullScreenButton != null ? settings.FullScreenButton.gameObject : null);
                    AddProxyControl(settings.WindowedButton != null ? settings.WindowedButton.gameObject : null);
                    AddProxyControl(settings.PlayTextSetDownBGMVolumeButton != null ? settings.PlayTextSetDownBGMVolumeButton.gameObject : null);
                    AddProxyControl(settings.StopVoiceInDialogueEndButton != null ? settings.StopVoiceInDialogueEndButton.gameObject : null);
                    AddProxyControl(settings.NoReadJumpButton != null ? settings.NoReadJumpButton.gameObject : null);
                    AddProxyControl(settings.ReadedColorButton != null ? settings.ReadedColorButton.gameObject : null);
                    AddProxyControl(settings.MMVoiceMuteButton != null ? settings.MMVoiceMuteButton.gameObject : null);
                    AddProxyControl(settings.MCLVoiceMuteButton != null ? settings.MCLVoiceMuteButton.gameObject : null);
                    AddProxyControl(settings.HXCVoiceMuteButton != null ? settings.HXCVoiceMuteButton.gameObject : null);
                    AddProxyControl(settings.BXYVoiceMuteButton != null ? settings.BXYVoiceMuteButton.gameObject : null);
                    AddControl(settings.CloseActionButton); AddControl(settings.ResetAllSettingButton); AddControl(settings.SaveSettingButton);
                }
                return;
            }

            foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true)) AddControl(selectable);

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || behaviour is UISfxTrigger)
                {
                    continue;
                }

                if (behaviour is IPointerClickHandler || behaviour is IPointerEnterHandler)
                {
                    AddProxyControl(behaviour.gameObject);
                }
            }
        }

        private void AddControl(Selectable selectable)
        {
            bool specialFocus = selectable == _confirmationTextFocus || selectable == _historyTextFocus;
            bool genericProxy = selectable != null && selectable.GetType() == typeof(Selectable) &&
                (selectable.GetComponent<IPointerClickHandler>() != null || selectable.GetComponent<IPointerEnterHandler>() != null);
            if (selectable == null || selectable is Scrollbar || (selectable.GetType() == typeof(Selectable) && !genericProxy && !specialFocus) || !selectable.gameObject.activeInHierarchy || !selectable.interactable || !IsMouseReachable(selectable.gameObject) || _controls.Contains(selectable))
            {
                return;
            }

            _controls.Add(selectable);
            NavigationHelper.EnsureSelectable(selectable, _activePage);
        }

        private void AddProxyControl(GameObject target)
        {
            if (target == null || !target.activeInHierarchy || !IsMouseReachable(target))
            {
                return;
            }

            Selectable proxy = target.GetComponent<Selectable>();
            if (proxy == null)
            {
                proxy = target.AddComponent<Selectable>();
            }
            proxy.interactable = true;
            AddControl(proxy);
        }

        private static bool IsMouseReachable(GameObject target)
        {
            for (Transform current = target.transform; current != null; current = current.parent)
            {
                CanvasGroup canvasGroup = current.GetComponent<CanvasGroup>();
                if (canvasGroup != null && (!canvasGroup.interactable || !canvasGroup.blocksRaycasts))
                {
                    return false;
                }
            }
            return true;
        }

        private void EnsureConfirmationTextFocus(GameObject root)
        {
            if (_confirmationTextFocus != null || root == null)
            {
                return;
            }

            GameObject focusObject = new GameObject("ConfirmationTextFocus", typeof(RectTransform), typeof(Selectable));
            focusObject.transform.SetParent(root.transform, false);
            focusObject.transform.SetAsFirstSibling();
            _confirmationTextFocus = focusObject.GetComponent<Selectable>();
            _confirmationTextFocus.interactable = true;
        }

        private void EnsureHistoryTextFocus(GameObject root)
        {
            if (_historyTextFocus != null || root == null) return;
            GameObject focusObject = new GameObject("HistoryTextFocus", typeof(RectTransform), typeof(Selectable));
            focusObject.transform.SetParent(root.transform, false);
            focusObject.transform.SetAsFirstSibling();
            _historyTextFocus = focusObject.GetComponent<Selectable>();
            _historyTextFocus.interactable = true;
            NavigationHelper.EnsureSelectable(_historyTextFocus, "history-text");
        }

        private bool IsConfirmationPage()
        {
            return _activePage == "confirmation" || _activePage == "save-confirmation";
        }

        private void EnsureSelection()
        {
            if (EventSystem.current == null || _controls.Count == 0)
            {
                return;
            }

            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.GetComponent<Selectable>() != null && _controls.Contains(selected.GetComponent<Selectable>()))
            {
                return;
            }

            UiInteraction.Select(_controls[0]);
        }

        private void RefreshSpecialControls(GameObject root)
        {
        }

        private void EnsureCurrentNavigation()
        {
            for (int i = 0; i < _controls.Count; i++)
            {
                NavigationHelper.EnsureSelectable(_controls[i], _activePage);
            }
        }

        private void HandleKeyboardInput()
        {
            if (EventSystem.current == null || _controls.Count == 0)
            {
                return;
            }

            Selectable selected = EventSystem.current.currentSelectedGameObject != null
                ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()
                : null;
            int current = _controls.IndexOf(selected);
            if (current < 0)
            {
                current = 0;
            }

            int direction = 0;
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = 1;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = -1;
            }

            Slider slider = _controls[current] as Slider;
            TMP_Dropdown dropdown = _controls[current] as TMP_Dropdown;
            if (dropdown != null && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
            {
                int valueDirection = Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 1;
                if (dropdown.options != null && dropdown.options.Count > 0)
                {
                    int next = Mathf.Clamp(dropdown.value + valueDirection, 0, dropdown.options.Count - 1);
                    if (next != dropdown.value)
                    {
                        dropdown.value = next;
                        dropdown.RefreshShownValue();
                    }
                    ScreenReader.Say(GetDropdownValue(dropdown));
                }
                return;
            }
            float delta = slider != null
                ? (slider.wholeNumbers ? 1f : (slider.maxValue - slider.minValue) / 20f)
                : 0f;
            if (slider != null && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                UiInteraction.AdjustSlider(slider, slider.value - delta);
                _settingAdjustment = true;
                ScreenReader.Say(FormatSliderValue(slider));
                return;
            }
            if (slider != null && Input.GetKeyDown(KeyCode.RightArrow))
            {
                UiInteraction.AdjustSlider(slider, slider.value + delta);
                _settingAdjustment = true;
                ScreenReader.Say(FormatSliderValue(slider));
                return;
            }

            if (direction != 0)
            {
                int next = (current + direction + _controls.Count) % _controls.Count;
                UiInteraction.Select(_controls[next]);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Selectable control = _controls[current];
                if (control == _confirmationTextFocus || control is Slider)
                {
                    return;
                }

                UiInteraction.Click(control.gameObject);
            }
        }

        private bool HandleHistoryKeyboardInput()
        {
            if (_activePage != "history" || !Input.anyKeyDown)
            {
                return false;
            }

            float scroll = 0f;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                scroll = 1f;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                scroll = -1f;
            }

            if (scroll == 0f)
            {
                return false;
            }

            UIHanderCenter center = UIHanderCenter.m_Instence;
            HistoryAction history = center != null ? center.historyAction : null;
            if (history == null || EventSystem.current == null)
            {
                return false;
            }

            // HistoryAction intentionally closes itself when its mouse wheel is moved down
            // at the newest entry. Keyboard Down is reserved for reading history, so retain
            // the page at that boundary instead of dispatching the close-producing scroll.
            if (scroll < 0f && IsAtHistoryNewestEntry(history))
            {
                AnnounceHistoryText(history);
                return true;
            }

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                scrollDelta = new Vector2(0f, scroll),
                pointerEnter = history.gameObject
            };
            ExecuteEvents.Execute<IScrollHandler>(history.gameObject, eventData, ExecuteEvents.scrollHandler);
            AnnounceHistoryText(history);
            return true;
        }

        private static bool IsAtHistoryNewestEntry(HistoryAction history)
        {
            FieldInfo targetField = history.GetType().GetField("_targetScrollY", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo maximumField = history.GetType().GetField("_maxScrollY", BindingFlags.Instance | BindingFlags.NonPublic);
            if (targetField == null || maximumField == null) return false;
            float target = (float)targetField.GetValue(history);
            float maximum = (float)maximumField.GetValue(history);
            return Mathf.Abs(target - maximum) < 0.5f;
        }

        private static void AnnounceHistoryText(HistoryAction history)
        {
            if (history == null || history.historyRecordPool == null)
            {
                return;
            }

            HistoryObj candidate = null;
            float bestY = float.MaxValue;
            foreach (HistoryObj item in history.historyRecordPool)
            {
                if (item == null || !item.gameObject.activeInHierarchy || item.historyText == null || string.IsNullOrWhiteSpace(item.historyText.text))
                {
                    continue;
                }

                float y = Mathf.Abs(item.transform.localPosition.y);
                if (candidate == null || y < bestY)
                {
                    candidate = item;
                    bestY = y;
                }
            }

            if (candidate == null)
            {
                return;
            }

            string text = candidate.historyText.text.Trim();
            string name = candidate.historyNameTxt != null ? candidate.historyNameTxt.text.Trim() : string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                text = name + "\n" + text;
            }
            ScreenReader.Say(text);
        }

        private void HandleSpecialKeyboardInput()
        {
            if (_activePage == "staff" && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                UIHanderCenter.m_Instence.staffAction.SendMessage("ShowSecondImage", SendMessageOptions.DontRequireReceiver);
            }
        }

        private void AnnounceSelection()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected == null || selected == _lastSelected)
            {
                return;
            }

            Selectable selectable = selected.GetComponent<Selectable>();
            if (selectable == null || !_controls.Contains(selectable))
            {
                return;
            }

            _lastSelected = selected;
            if (selectable == _confirmationTextFocus)
            {
                ConfirmTipAction confirmation = selected.GetComponentInParent<ConfirmTipAction>();
                if (confirmation != null)
                {
                    foreach (TMP_Text confirmationText in confirmation.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (confirmationText != null && !string.IsNullOrWhiteSpace(confirmationText.text))
                        {
                            ScreenReader.Say(confirmationText.text);
                            break;
                        }
                    }
                }
                else
                {
                    TMP_Text confirmationText = selected.transform.parent != null
                        ? selected.transform.parent.GetComponentInChildren<TMP_Text>(true)
                        : null;
                    if (confirmationText != null && !string.IsNullOrWhiteSpace(confirmationText.text))
                    {
                        ScreenReader.Say(confirmationText.text.Trim());
                    }
                }
                return;
            }
            if (selectable == _historyTextFocus)
            {
                AnnounceHistoryText(UIHanderCenter.m_Instence != null ? UIHanderCenter.m_Instence.historyAction : null);
                return;
            }
            TMP_Text text = selected.GetComponentInChildren<TMP_Text>(true);
            if (_settingAdjustment)
            {
                _settingAdjustment = false;
                return;
            }
            ScreenReader.Say(DescribeControl(selectable, text));
        }

        private string DescribeControl(Selectable selectable, TMP_Text text)
        {
            HistoryObj historyItem = selectable != null ? selectable.GetComponentInParent<HistoryObj>() : null;
            if (historyItem != null)
            {
                if (selectable == historyItem.historyVoiceBtn) return "重播语音";
                if (selectable == historyItem.historyReturnBtn) return "返回";
            }
            Slider slider = selectable as Slider;
            if (slider != null) return UiLabelResolver.ResolveSlider(slider) + "，" + FormatSliderValue(slider);
            string label = UiLabelResolver.Resolve(selectable);
            SettingButton setting = selectable.GetComponent<SettingButton>();
            if (setting != null) return label + "，" + ResolveSettingState(setting.SettingButtonID);
            TMP_Dropdown dropdown = selectable as TMP_Dropdown;
            if (dropdown != null) return label + "，" + (dropdown.captionText != null ? dropdown.captionText.text : dropdown.value.ToString());
            return text != null && !string.IsNullOrWhiteSpace(text.text) ? text.text.Trim() : label;
        }

        private string FormatSliderValue(Slider slider)
        {
            return slider.wholeNumbers ? ((int)slider.value).ToString() : slider.value.ToString("0.##");
        }

        private static string GetDropdownValue(TMP_Dropdown dropdown)
        {
            if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0) return string.Empty;
            int index = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
            return dropdown.options[index].text;
        }

        private string ResolveSettingState(int id)
        {
            GameSettings s = Singleton<SaveLoadManager>.m_Instence != null ? Singleton<SaveLoadManager>.m_Instence.CurrentSettings : null;
            if (s == null) return string.Empty;
            switch (id)
            {
                case 0: return s.isWindowedFullScreen ? "开启" : "关闭";
                case 1: return s.isWindowed ? "开启" : "关闭";
                case 2: return s.playTextSetDownBGMVolume ? "开启" : "关闭";
                case 3: return s.StopVoiceInDialogueEnd ? "开启" : "关闭";
                case 4: return s.noReadJump ? "开启" : "关闭";
                case 5: return s.readedColor ? "开启" : "关闭";
                case 6: return s.muteMMVoice ? "开启" : "关闭";
                case 7: return s.muteMCLVoice ? "开启" : "关闭";
                case 8: return s.muteHXCVoice ? "开启" : "关闭";
                case 9: return s.muteBXYVoice ? "开启" : "关闭";
                default: return string.Empty;
            }
        }

        private void Reset()
        {
            if (_activePage != null)
            {
                AccessStateManager.Exit(_activeState);
            }

            _activePage = null;
            _activeState = AccessStateManager.State.None;
            _lastRefreshFrame = -100;
            _lastSelected = null;
            _lastControlCount = -1;
            _controls.Clear();
            if (_confirmationTextFocus != null)
            {
                Object.Destroy(_confirmationTextFocus.gameObject);
            }
            _confirmationTextFocus = null;
            if (_historyTextFocus != null)
            {
                Object.Destroy(_historyTextFocus.gameObject);
            }
            _historyTextFocus = null;
        }
    }
}
