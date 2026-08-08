using UnityEngine;
using UnityEngine.UI;

namespace GalMasterAccess
{
    /// <summary>
    /// Disables the game's built-in keyboard navigation for a control handled by this mod.
    /// Mouse hover and pointer-click behavior remain owned by the game.
    /// </summary>
    internal static class NavigationHelper
    {
        public static void EnsureSelectable(Selectable selectable, string source)
        {
            if (selectable == null || !selectable.gameObject.activeInHierarchy)
            {
                return;
            }

            Navigation navigation = selectable.navigation;
            if (navigation.mode == Navigation.Mode.None)
            {
                return;
            }
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;

        }
    }
}
