using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GalMasterAccess
{
    /// <summary>
    /// Routes accessibility input through the game's cursor and Unity event path.
    /// </summary>
    internal static class UiInteraction
    {
        public static void Select(Selectable selectable)
        {
            if (selectable == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            UICursorUtility.MoveCursorToSelectable(selectable);
        }

        public static void Click(GameObject target)
        {
            if (target == null || EventSystem.current == null)
            {
                return;
            }

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                pointerClick = target,
                pointerEnter = target,
                pointerPress = target,
                rawPointerPress = target
            };
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerUpHandler);
        }

        public static void AdjustSlider(Slider slider, float value)
        {
            if (slider == null)
            {
                return;
            }

            UICursorUtility.MoveCursorToSelectable(slider);
            slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
        }
    }
}
