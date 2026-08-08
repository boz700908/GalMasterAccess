using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GalMasterAccess
{
    /// <summary>
    /// Announces the image-based main menu buttons while preserving Unity's navigation.
    /// </summary>
    public sealed class MainMenuHandler
    {
        private readonly List<MenuButton> _buttons = new List<MenuButton>();
        private GameObject _lastSelected;
        private bool _wasOpen;

        /// <summary>
        /// Checks the title menu and announces selection changes.
        /// </summary>
        public void Update()
        {
            UIHanderCenter center = UIHanderCenter.m_Instence;
            MenuAction menu = center != null ? center.menuAction : null;
            if (menu == null || !menu.gameObject.activeInHierarchy)
            {
                if (_wasOpen)
                {
                    _wasOpen = false;
                    _lastSelected = null;
                    AccessStateManager.Exit(AccessStateManager.State.MainMenu);
                }
                return;
            }

            if (!_wasOpen)
            {
                _wasOpen = true;
                AccessStateManager.TryEnter(AccessStateManager.State.MainMenu);
                RefreshButtons(menu);
                SelectFirstInteractable();
            }

            else if (_buttons.Count == 0 || (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null))
            {
                // MenuAction keeps buttons inactive until its fade-in completes.
                RefreshButtons(menu);
                SelectFirstInteractable();
            }

            // MenuAction may restore its Navigation settings during fade-in or focus changes.
            // Re-apply None to every real menu button before processing this frame's keys.
            DisableNativeNavigation();

            HandleKeyboardNavigation();

            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;
            if (selected == null || selected == _lastSelected)
            {
                return;
            }

            MenuButton button = selected.GetComponentInParent<MenuButton>();
            if (button == null || button.button == null || !button.button.interactable)
            {
                return;
            }

            _lastSelected = selected;
            ScreenReader.Say(GetVisualName(button));
            DebugLogger.LogState($"Main menu selection: {button.buttonIndex}");
        }

        private void RefreshButtons(MenuAction menu)
        {
            _buttons.Clear();
            AddButton(menu.startNewGameButton);
            AddButton(menu.loadGameButton);
            AddButton(menu.settingButton);
            AddButton(menu.cGButton);
            AddButton(menu.staffButton);
            AddButton(menu.exitButton);
        }

        private void DisableNativeNavigation()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i] != null && _buttons[i].button != null)
                {
                    NavigationHelper.EnsureSelectable(_buttons[i].button, "main-menu");
                }
            }
        }

        private void AddButton(MenuButton button)
        {
            if (button != null && button.button != null)
            {
                _buttons.Add(button);
            }
        }

        private void SelectFirstInteractable()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            foreach (MenuButton button in _buttons)
            {
                NavigationHelper.EnsureSelectable(button.button, "main-menu");
                if (button.button.gameObject.activeInHierarchy && button.button.interactable)
                {
                    UiInteraction.Select(button.button);
                    return;
                }
            }
        }

        private void HandleKeyboardNavigation()
        {
            if (EventSystem.current == null || _buttons.Count == 0)
            {
                return;
            }

            int current = GetCurrentButtonIndex();
            int direction = 0;
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = 1;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = -1;
            }

            if (direction != 0)
            {
                SelectButtonAt(WrapIndex(current + direction));
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Button button = _buttons[WrapIndex(current)].button;
                if (button != null && button.interactable)
                {
                    UiInteraction.Click(button.gameObject);
                }
            }
        }

        private int GetCurrentButtonIndex()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                MenuButton selectedButton = selected.GetComponentInParent<MenuButton>();
                if (selectedButton != null)
                {
                    for (int i = 0; i < _buttons.Count; i++)
                    {
                        if (_buttons[i] == selectedButton)
                        {
                            return i;
                        }
                    }
                }
            }

            return 0;
        }

        private int WrapIndex(int index)
        {
            int count = _buttons.Count;
            index %= count;
            return index < 0 ? index + count : index;
        }

        private void SelectButtonAt(int index)
        {
            for (int offset = 0; offset < _buttons.Count; offset++)
            {
                MenuButton button = _buttons[WrapIndex(index + offset)];
                if (button.button != null && button.button.gameObject.activeInHierarchy && button.button.interactable)
                {
                    UiInteraction.Select(button.button);
                    return;
                }
            }
        }

        private string GetVisualName(MenuButton button)
        {
            if (button.linkImage != null && button.linkImage.sprite != null)
            {
                return UiLabelResolver.ResolveSprite(button.linkImage.sprite.name);
            }

            return button.gameObject.name;
        }
    }
}
