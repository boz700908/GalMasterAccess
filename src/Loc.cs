using System.Collections.Generic;

namespace GalMasterAccess
{
    /// <summary>
    /// Zentrale Lokalisierung für den Accessibility-Mod.
    /// Erkennt Spielsprache automatisch.
    ///
    /// Verwendung:
    ///   Loc.Get("key")              - String abrufen
    ///   Loc.Get("key", arg1, arg2)  - String mit Platzhaltern {0}, {1}
    /// </summary>
    public static class Loc
    {
        #region Fields

        private static bool _initialized = false;
        private static string _currentLang = "zh";

        // Dictionaries für jede unterstützte Sprache
        private static readonly Dictionary<string, string> _chinese = new Dictionary<string, string>();
        // Weitere Sprachen nach Bedarf hinzufügen:
        // private static readonly Dictionary<string, string> _spanish = new();
        // private static readonly Dictionary<string, string> _french = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialisiert die Lokalisierung. Einmal beim Mod-Start aufrufen.
        /// </summary>
        public static void Initialize()
        {
            InitializeStrings();
            RefreshLanguage();
            _initialized = true;
        }

        /// <summary>
        /// Aktualisiert die Sprache basierend auf Spieleinstellung.
        /// Aufrufen wenn Spieler Sprache ändert.
        /// </summary>
        public static void RefreshLanguage()
        {
            string gameLang = GetGameLanguage();

            switch (gameLang)
            {
                case "de":
                    _currentLang = "zh";
                    break;
                // Weitere Sprachen hier:
                // case "es":
                //     _currentLang = "es";
                //     break;
                default:
                    _currentLang = "zh";
                    break;
            }
        }

        /// <summary>
        /// Holt einen lokalisierten String.
        /// </summary>
        public static string Get(string key)
        {
            if (!_initialized) Initialize();

            var dict = GetCurrentDictionary();

            // Versuche aktuelle Sprache
            if (dict.TryGetValue(key, out string value))
                return value;

            // Fallback: Englisch
            // Letzter Fallback: Key selbst (hilft beim Debugging)
            return key;
        }

        /// <summary>
        /// Holt einen lokalisierten String mit Platzhaltern.
        /// Nutzt {0}, {1}, {2} etc. als Platzhalter.
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// === DIESE METHODE FÜR DEIN SPIEL ANPASSEN ===
        /// Liest die aktuelle Spielsprache aus.
        /// </summary>
        private static string GetGameLanguage()
        {
            // TODO: An das Spiel anpassen!
            // Suche im dekompilierten Code nach: Language, Localization, I18n, getAlias()

            // UNITY BEISPIELE:
            // return Language.getAlias();
            // return PlayerPrefs.GetString("language", "en");

            // FALLBACK (einsprachig):
            return "en";
        }

        private static Dictionary<string, string> GetCurrentDictionary()
        {
            switch (_currentLang)
            {
                default: return _chinese;
            }
        }

        /// <summary>
        /// Hilfsmethode: Fügt einen String in alle Sprachen ein.
        /// Bei mehr Sprachen: Parameter erweitern!
        /// </summary>
        private static void Add(string key, string chinese)
        {
            _chinese[key] = chinese;
        }

        /// <summary>
        /// Alle Übersetzungen hier definieren.
        /// </summary>
        private static void InitializeStrings()
        {
            // All announcements use text supplied by the game itself.
        }

        #endregion
    }
}
