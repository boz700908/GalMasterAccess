using MelonLoader;
using UnityEngine;
using System.Collections;

// ============================================================================
// KRITISCH: Zugriff auf Spielcode
// ============================================================================
// Jeder Zugriff auf Spielklassen VOR dem vollständigen Laden crasht!
//
// VERBOTEN in OnInitializeMelon() oder früher:
//   - Spielmanager-Singletons (GameManager.i, AudioManager.instance, etc.)
//   - typeof(SpielKlasse) in Harmony-Attributen
//
// ERLAUBT erst ab OnSceneWasLoaded() / wenn CheckGameReady() true ist.
//
// Bei Crashes oder stillem Fehlschlagen:
//   Siehe docs/technical-reference.md Abschnitt "KRITISCH: Zugriff auf Spielcode"
// ============================================================================

[assembly: MelonInfo(typeof(GalMasterAccess.Main), "GalMasterAccess", "1.0.0", "GalMasterAccess")]
[assembly: MelonGame("GAL高手制作组", "GAL PRO MASTER")]

namespace GalMasterAccess
{
    /// <summary>
    /// Main mod entry point. Coordinates all handlers and processes global hotkeys.
    ///
    /// BEST PRACTICE: Keep this class SMALL!
    /// - Only lifecycle methods (OnInitializeMelon, OnUpdate, OnApplicationQuit)
    /// - Only global hotkey dispatch (F12 debug toggle)
    /// - Only handler instantiation and update calls
    ///
    /// Put ALL feature logic in separate Handler classes.
    /// This makes the code easier to maintain and test.
    /// </summary>
    public class Main : MelonMod
    {
        #region Fields

        private bool _gameReady = true;
        private MainMenuHandler _mainMenuHandler;
        private DialogueHandler _dialogueHandler;
        private AdditionalPagesHandler _additionalPagesHandler;

        /// <summary>
        /// Debug mode - when true, logs all screenreader output and detailed game state.
        /// Toggle with F12.
        /// </summary>
        public static bool DebugMode = true;

        // Handlers - one per feature/screen
        // private InventoryHandler _inventoryHandler;
        // private DialogHandler _dialogHandler;
        // private ShopHandler _shopHandler;

        #endregion

        #region Lifecycle

        public override void OnInitializeMelon()
        {
            ScreenReader.Initialize();
            InitializeHandlers();
        }

        private void InitializeHandlers()
        {
            _mainMenuHandler = new MainMenuHandler();
            _dialogueHandler = new DialogueHandler();
            _additionalPagesHandler = new AdditionalPagesHandler();
        }

        public override void OnUpdate()
        {
            // Wait for game to be ready
            if (!CheckGameReady()) return;

            // Process global hotkeys first
            if (ProcessHotkeys()) return;

            // Update all handlers
            UpdateHandlers();
        }

        private bool CheckGameReady()
        {
            if (_gameReady) return true;

            return _gameReady;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonLogger.Msg($"Scene loaded: {sceneName}");
            DebugLogger.LogState($"Scene changed to: {sceneName}");
            _gameReady = true;
        }

        public override void OnApplicationQuit()
        {
            ScreenReader.Shutdown();
        }

        #endregion

        #region Hotkeys

        /// <summary>
        /// Processes global hotkeys. Returns true if a key was handled.
        /// Only dispatch to handlers here - don't put logic in Main!
        /// </summary>
        private bool ProcessHotkeys()
        {
            // F12 = Toggle debug mode
            if (Input.GetKeyDown(KeyCode.F12))
            {
                DebugMode = !DebugMode;
                MelonLogger.Msg($"Debug mode {(DebugMode ? "enabled" : "disabled")}");
                return true;
            }

            return false;
        }

        #endregion

        #region Handler Updates

        private void UpdateHandlers()
        {
            // Only the topmost game UI may own the accessibility focus.
            if (_additionalPagesHandler != null && _additionalPagesHandler.HasActivePage())
            {
                _additionalPagesHandler.Update();
                return;
            }

            UIHanderCenter center = UIHanderCenter.m_Instence;
            if (center != null && center.menuAction != null && center.menuAction.gameObject.activeInHierarchy)
            {
                _mainMenuHandler?.Update();
                return;
            }

            _dialogueHandler?.Update();
        }

        #endregion

    }
}
