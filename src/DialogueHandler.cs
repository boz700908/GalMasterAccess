using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GalMasterAccess
{
    /// <summary>
    /// Announces dialogue lines, active choices, and the navigation state of dialogue controls.
    /// </summary>
    public sealed class DialogueHandler
    {
        private string _lastLine;
        private string _lastDialogueText;
        private GameObject _lastSelected;
        private bool _wasOpen;
        private Selectable _textFocus;
        private bool _lineAnnouncedThisFrame;

        /// <summary>
        /// Updates dialogue announcements and active-control navigation.
        /// </summary>
        public void Update()
        {
            DialogueHandle dialogue = DialogueHandle.m_Instence;
            if (dialogue == null || dialogue.dialogueObj == null || !dialogue.dialogueObj.activeInHierarchy)
            {
                Reset();
                return;
            }

            if (!_wasOpen)
            {
                _wasOpen = true;
                AccessStateManager.TryEnter(AccessStateManager.State.Dialogue);
            }

            EnsureDialogueNavigation(dialogue);
            EnsureTextFocus(dialogue);
            _lineAnnouncedThisFrame = false;
            HandleChoiceKeyboardNavigation();
            HandleDialogueKeyboardNavigation(dialogue);
            ApplyDialogueAdvanceFocusGuard(dialogue);
            AnnounceLine(dialogue);
            AnnounceSelectedControl();
        }

        private void ApplyDialogueAdvanceFocusGuard(DialogueHandle dialogue)
        {
            UIHanderCenter center = UIHanderCenter.m_Instence;
            SelectionAction selection = center != null ? center.selectionAction : null;
            if (selection != null && selection.IsOpen)
            {
                return;
            }

            if (EventSystem.current == null || _textFocus == null)
            {
                return;
            }

            dialogue.hadPauseDialogueInput = EventSystem.current.currentSelectedGameObject != _textFocus.gameObject;
        }

        private void EnsureDialogueNavigation(DialogueHandle dialogue)
        {
            NavigationHelper.EnsureSelectable(_textFocus, "dialogue-text");
            if (dialogue.dialogueButtons != null)
            {
                foreach (Button button in dialogue.dialogueButtons)
                {
                    NavigationHelper.EnsureSelectable(button, "dialogue");
                }
            }
            NavigationHelper.EnsureSelectable(dialogue.voiceButton, "dialogue-voice");

            UIHanderCenter center = UIHanderCenter.m_Instence;
            SelectionAction selection = center != null ? center.selectionAction : null;
            if (selection != null && selection.IsOpen)
            {
                foreach (Selectable selectable in selection.GetComponentsInChildren<Selectable>(true))
                {
                    NavigationHelper.EnsureSelectable(selectable, "choice");
                }
            }
        }

        private void EnsureTextFocus(DialogueHandle dialogue)
        {
            if (_textFocus != null || dialogue.dialogueObj == null)
            {
                return;
            }

            GameObject focusObject = new GameObject("DialogueTextFocus", typeof(RectTransform), typeof(Selectable));
            focusObject.transform.SetParent(dialogue.dialogueObj.transform, false);
            focusObject.transform.SetAsFirstSibling();
            _textFocus = focusObject.GetComponent<Selectable>();
            _textFocus.interactable = true;
            Navigation navigation = _textFocus.navigation;
            navigation.mode = Navigation.Mode.None;
            _textFocus.navigation = navigation;
        }

        private void HandleDialogueKeyboardNavigation(DialogueHandle dialogue)
        {
            UIHanderCenter center = UIHanderCenter.m_Instence;
            SelectionAction selection = center != null ? center.selectionAction : null;
            if (selection != null && selection.IsOpen || EventSystem.current == null || _textFocus == null)
            {
                return;
            }

            List<Selectable> controls = new List<Selectable> { _textFocus };
            if (dialogue.dialogueButtons != null)
            {
                foreach (Button button in dialogue.dialogueButtons)
                {
                    if (button != null && button.gameObject.activeInHierarchy && button.interactable)
                    {
                        controls.Add(button);
                    }
                }
            }
            if (dialogue.voiceButton != null && dialogue.voiceButton.gameObject.activeInHierarchy && dialogue.voiceButton.interactable)
            {
                controls.Add(dialogue.voiceButton);
            }

            Selectable selected = EventSystem.current.currentSelectedGameObject != null
                ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()
                : null;
            int current = controls.IndexOf(selected);
            if (current < 0)
            {
                current = 0;
                UiInteraction.Select(_textFocus);
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

            if (direction != 0 && controls.Count > 1)
            {
                int next = (current + direction + controls.Count) % controls.Count;
                UiInteraction.Select(controls[next]);
            }
            else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && selected is Button button)
            {
                dialogue.hadPauseDialogueInput = false;
                UiInteraction.Click(button.gameObject);
                dialogue.hadPauseDialogueInput = true;
            }
        }

        private void AnnounceLine(DialogueHandle dialogue)
        {
            TextMeshProUGUI textComponent = dialogue.mainDiagueText;
            if (textComponent == null || string.IsNullOrWhiteSpace(textComponent.text))
            {
                if (textComponent != null && string.IsNullOrWhiteSpace(textComponent.text))
                {
                    _lastLine = null;
                }
                return;
            }

            string line = textComponent.text;
            if (line == _lastLine)
            {
                return;
            }

            _lastLine = line;
            string speaker = dialogue.mainDialogueName != null
                ? dialogue.mainDialogueName.text
                : string.Empty;
            if (string.IsNullOrWhiteSpace(speaker) && dialogue.mainDialogueName_Speak != null)
            {
                speaker = dialogue.mainDialogueName_Speak.text;
            }
            _lastDialogueText = string.IsNullOrWhiteSpace(speaker) ? line : speaker + "\n" + line;
            ScreenReader.Say(_lastDialogueText);
            _lineAnnouncedThisFrame = true;
        }

        private void AnnounceSelectedControl()
        {
            EventSystem eventSystem = EventSystem.current;
            GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
            if (selected == null || selected == _lastSelected)
            {
                return;
            }

            _lastSelected = selected;
            if (_textFocus != null && selected == _textFocus.gameObject)
            {
                if (!_lineAnnouncedThisFrame)
                {
                    ScreenReader.Say(_lastDialogueText ?? _lastLine ?? string.Empty);
                }
                return;
            }

            Selectable selectedControl = selected.GetComponent<Selectable>();
            TMP_Text text = selected.GetComponentInChildren<TMP_Text>(true);
            string value = ResolveDialogueControlLabel(selectedControl, text);
            ScreenReader.Say(value);
        }

        private static string ResolveDialogueControlLabel(Selectable control, TMP_Text childText)
        {
            if (control == null) return string.Empty;
            SelectionAction selection = control.GetComponentInParent<SelectionAction>();
            if (selection != null && childText != null && !string.IsNullOrWhiteSpace(childText.text))
            {
                return childText.text.Trim();
            }
            string resolved = UiLabelResolver.Resolve(control);
            if (!string.IsNullOrWhiteSpace(resolved) && resolved != control.gameObject.name)
            {
                return resolved;
            }
            return childText != null && !string.IsNullOrWhiteSpace(childText.text)
                ? childText.text.Trim()
                : resolved;
        }

        private void HandleChoiceKeyboardNavigation()
        {
            UIHanderCenter center = UIHanderCenter.m_Instence;
            SelectionAction selection = center != null ? center.selectionAction : null;
            if (selection == null || !selection.IsOpen || EventSystem.current == null)
            {
                return;
            }

            List<Selectable> controls = new List<Selectable>();
            foreach (Selectable selectable in selection.GetComponentsInChildren<Selectable>(true))
            {
                if (selectable.gameObject.activeInHierarchy && selectable.interactable)
                {
                    controls.Add(selectable);
                }
            }

            if (controls.Count == 0)
            {
                return;
            }

            int current = controls.IndexOf(EventSystem.current.currentSelectedGameObject != null
                ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()
                : null);
            if (current < 0)
            {
                current = 0;
                UiInteraction.Select(controls[0]);
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

            if (direction != 0)
            {
                int next = (current + direction + controls.Count) % controls.Count;
                UiInteraction.Select(controls[next]);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Button button = controls[current] as Button;
                if (button != null) UiInteraction.Click(button.gameObject);
            }
        }

        private void Reset()
        {
            if (_wasOpen)
            {
                AccessStateManager.Exit(AccessStateManager.State.Dialogue);
            }

            _wasOpen = false;
            _lastLine = null;
            _lastDialogueText = null;
            _lastSelected = null;
            _lineAnnouncedThisFrame = false;
            if (_textFocus != null)
            {
                Object.Destroy(_textFocus.gameObject);
            }
            _textFocus = null;
        }
    }
}
