using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ローカライゼーションシステム
    /// 多言語対応とテキスト管理
    /// </summary>
    public class LocalizationSystem : MonoBehaviour
    {
        public static LocalizationSystem Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private SystemLanguage _defaultLanguage = SystemLanguage.Japanese;
        [SerializeField] private bool _useSystemLanguage = true;

        [Header("言語データ")]
        [SerializeField] private List<LocalizationData> _localizationData = new List<LocalizationData>();

        // 現在の言語
        private SystemLanguage _currentLanguage;

        // 言語テーブル
        private Dictionary<SystemLanguage, Dictionary<string, string>> _stringTables = new Dictionary<SystemLanguage, Dictionary<string, string>>();

        // イベント
        public event Action<SystemLanguage> OnLanguageChanged;

        /// <summary>
        /// 現在の言語
        /// </summary>
        public SystemLanguage CurrentLanguage => _currentLanguage;

        /// <summary>
        /// サポート言語リスト
        /// </summary>
        public IReadOnlyList<SystemLanguage> SupportedLanguages => new SystemLanguage[]
        {
            SystemLanguage.Japanese,
            SystemLanguage.English,
            SystemLanguage.ChineseSimplified,
            SystemLanguage.ChineseTraditional,
            SystemLanguage.Korean
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Initialization

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            LoadStringTables();
            InitializeDefaultStrings();

            // 言語設定
            if (_useSystemLanguage)
            {
                SetLanguage(GetSystemLanguage());
            }
            else
            {
                SetLanguage(LoadSavedLanguage());
            }
        }

        /// <summary>
        /// 文字列テーブルを読み込み
        /// </summary>
        private void LoadStringTables()
        {
            foreach (var data in _localizationData)
            {
                if (data == null) continue;

                if (!_stringTables.ContainsKey(data.Language))
                {
                    _stringTables[data.Language] = new Dictionary<string, string>();
                }

                foreach (var entry in data.Entries)
                {
                    _stringTables[data.Language][entry.Key] = entry.Value;
                }
            }
        }

        /// <summary>
        /// デフォルト文字列を初期化
        /// </summary>
        private void InitializeDefaultStrings()
        {
            // 日本語（デフォルト）
            var japanese = GetOrCreateTable(SystemLanguage.Japanese);
            InitializeJapaneseStrings(japanese);

            // 英語
            var english = GetOrCreateTable(SystemLanguage.English);
            InitializeEnglishStrings(english);

            // 中国語（簡体字）
            var chineseSimplified = GetOrCreateTable(SystemLanguage.ChineseSimplified);
            InitializeChineseSimplifiedStrings(chineseSimplified);
        }

        /// <summary>
        /// テーブルを取得または作成
        /// </summary>
        private Dictionary<string, string> GetOrCreateTable(SystemLanguage language)
        {
            if (!_stringTables.ContainsKey(language))
            {
                _stringTables[language] = new Dictionary<string, string>();
            }
            return _stringTables[language];
        }

        /// <summary>
        /// 日本語文字列を初期化
        /// </summary>
        private void InitializeJapaneseStrings(Dictionary<string, string> table)
        {
            // UI共通
            table.TryAdd("ui.confirm", "確認");
            table.TryAdd("ui.cancel", "キャンセル");
            table.TryAdd("ui.ok", "OK");
            table.TryAdd("ui.yes", "はい");
            table.TryAdd("ui.no", "いいえ");
            table.TryAdd("ui.back", "戻る");
            table.TryAdd("ui.next", "次へ");
            table.TryAdd("ui.close", "閉じる");
            table.TryAdd("ui.save", "保存");
            table.TryAdd("ui.load", "読込");
            table.TryAdd("ui.settings", "設定");

            // メニュー
            table.TryAdd("menu.title", "三十六計");
            table.TryAdd("menu.newgame", "新規ゲーム");
            table.TryAdd("menu.continue", "続きから");
            table.TryAdd("menu.scenario", "シナリオ選択");
            table.TryAdd("menu.exit", "終了");

            // ゲーム
            table.TryAdd("game.turn", "ターン");
            table.TryAdd("game.year", "年");
            table.TryAdd("game.gold", "金");
            table.TryAdd("game.food", "兵糧");
            table.TryAdd("game.soldiers", "兵力");
            table.TryAdd("game.morale", "士気");
            table.TryAdd("game.territory", "領地");
            table.TryAdd("game.faction", "勢力");

            // 計略カテゴリ
            table.TryAdd("stratagem.category.winning", "勝戦計");
            table.TryAdd("stratagem.category.enemy", "敵戦計");
            table.TryAdd("stratagem.category.attack", "攻戦計");
            table.TryAdd("stratagem.category.chaos", "混戦計");
            table.TryAdd("stratagem.category.equal", "並戦計");
            table.TryAdd("stratagem.category.losing", "敗戦計");

            // 戦闘
            table.TryAdd("battle.start", "戦闘開始");
            table.TryAdd("battle.victory", "勝利");
            table.TryAdd("battle.defeat", "敗北");
            table.TryAdd("battle.draw", "引き分け");
            table.TryAdd("battle.retreat", "撤退");
            table.TryAdd("battle.attack", "攻撃");
            table.TryAdd("battle.defend", "防御");

            // 通知
            table.TryAdd("notification.turn_start", "{0}ターン開始");
            table.TryAdd("notification.stratagem_success", "計略「{0}」が成功しました");
            table.TryAdd("notification.stratagem_fail", "計略「{0}」が失敗しました");
            table.TryAdd("notification.territory_conquered", "{0}を占領しました");
            table.TryAdd("notification.territory_lost", "{0}を失いました");

            // 設定
            table.TryAdd("settings.language", "言語");
            table.TryAdd("settings.audio", "音声");
            table.TryAdd("settings.master_volume", "マスター音量");
            table.TryAdd("settings.bgm_volume", "BGM音量");
            table.TryAdd("settings.se_volume", "SE音量");
            table.TryAdd("settings.voice_volume", "ボイス音量");

            // 勝利条件
            table.TryAdd("victory.conquest", "天下統一");
            table.TryAdd("victory.territory_count", "{0}領地占領");
            table.TryAdd("victory.elimination", "敵勢力全滅");
            table.TryAdd("victory.survive", "{0}ターン生存");
        }

        /// <summary>
        /// 英語文字列を初期化
        /// </summary>
        private void InitializeEnglishStrings(Dictionary<string, string> table)
        {
            // UI共通
            table.TryAdd("ui.confirm", "Confirm");
            table.TryAdd("ui.cancel", "Cancel");
            table.TryAdd("ui.ok", "OK");
            table.TryAdd("ui.yes", "Yes");
            table.TryAdd("ui.no", "No");
            table.TryAdd("ui.back", "Back");
            table.TryAdd("ui.next", "Next");
            table.TryAdd("ui.close", "Close");
            table.TryAdd("ui.save", "Save");
            table.TryAdd("ui.load", "Load");
            table.TryAdd("ui.settings", "Settings");

            // メニュー
            table.TryAdd("menu.title", "36 Stratagems");
            table.TryAdd("menu.newgame", "New Game");
            table.TryAdd("menu.continue", "Continue");
            table.TryAdd("menu.scenario", "Select Scenario");
            table.TryAdd("menu.exit", "Exit");

            // ゲーム
            table.TryAdd("game.turn", "Turn");
            table.TryAdd("game.year", "Year");
            table.TryAdd("game.gold", "Gold");
            table.TryAdd("game.food", "Food");
            table.TryAdd("game.soldiers", "Soldiers");
            table.TryAdd("game.morale", "Morale");
            table.TryAdd("game.territory", "Territory");
            table.TryAdd("game.faction", "Faction");

            // 計略カテゴリ
            table.TryAdd("stratagem.category.winning", "Winning Stratagems");
            table.TryAdd("stratagem.category.enemy", "Enemy Stratagems");
            table.TryAdd("stratagem.category.attack", "Attack Stratagems");
            table.TryAdd("stratagem.category.chaos", "Chaos Stratagems");
            table.TryAdd("stratagem.category.equal", "Equal Stratagems");
            table.TryAdd("stratagem.category.losing", "Losing Stratagems");

            // 戦闘
            table.TryAdd("battle.start", "Battle Start");
            table.TryAdd("battle.victory", "Victory");
            table.TryAdd("battle.defeat", "Defeat");
            table.TryAdd("battle.draw", "Draw");
            table.TryAdd("battle.retreat", "Retreat");
            table.TryAdd("battle.attack", "Attack");
            table.TryAdd("battle.defend", "Defend");

            // 通知
            table.TryAdd("notification.turn_start", "Turn {0} Started");
            table.TryAdd("notification.stratagem_success", "Stratagem \"{0}\" succeeded");
            table.TryAdd("notification.stratagem_fail", "Stratagem \"{0}\" failed");
            table.TryAdd("notification.territory_conquered", "Conquered {0}");
            table.TryAdd("notification.territory_lost", "Lost {0}");

            // 設定
            table.TryAdd("settings.language", "Language");
            table.TryAdd("settings.audio", "Audio");
            table.TryAdd("settings.master_volume", "Master Volume");
            table.TryAdd("settings.bgm_volume", "BGM Volume");
            table.TryAdd("settings.se_volume", "SE Volume");
            table.TryAdd("settings.voice_volume", "Voice Volume");

            // 勝利条件
            table.TryAdd("victory.conquest", "World Conquest");
            table.TryAdd("victory.territory_count", "Conquer {0} Territories");
            table.TryAdd("victory.elimination", "Eliminate All Enemies");
            table.TryAdd("victory.survive", "Survive {0} Turns");
        }

        /// <summary>
        /// 中国語（簡体字）文字列を初期化
        /// </summary>
        private void InitializeChineseSimplifiedStrings(Dictionary<string, string> table)
        {
            // UI共通
            table.TryAdd("ui.confirm", "确认");
            table.TryAdd("ui.cancel", "取消");
            table.TryAdd("ui.ok", "确定");
            table.TryAdd("ui.yes", "是");
            table.TryAdd("ui.no", "否");
            table.TryAdd("ui.back", "返回");
            table.TryAdd("ui.next", "下一步");
            table.TryAdd("ui.close", "关闭");
            table.TryAdd("ui.save", "保存");
            table.TryAdd("ui.load", "加载");
            table.TryAdd("ui.settings", "设置");

            // メニュー
            table.TryAdd("menu.title", "三十六计");
            table.TryAdd("menu.newgame", "新游戏");
            table.TryAdd("menu.continue", "继续");
            table.TryAdd("menu.scenario", "选择剧本");
            table.TryAdd("menu.exit", "退出");

            // ゲーム
            table.TryAdd("game.turn", "回合");
            table.TryAdd("game.year", "年");
            table.TryAdd("game.gold", "金钱");
            table.TryAdd("game.food", "粮草");
            table.TryAdd("game.soldiers", "兵力");
            table.TryAdd("game.morale", "士气");
            table.TryAdd("game.territory", "领地");
            table.TryAdd("game.faction", "势力");

            // 計略カテゴリ
            table.TryAdd("stratagem.category.winning", "胜战计");
            table.TryAdd("stratagem.category.enemy", "敌战计");
            table.TryAdd("stratagem.category.attack", "攻战计");
            table.TryAdd("stratagem.category.chaos", "混战计");
            table.TryAdd("stratagem.category.equal", "并战计");
            table.TryAdd("stratagem.category.losing", "败战计");

            // 戦闘
            table.TryAdd("battle.start", "战斗开始");
            table.TryAdd("battle.victory", "胜利");
            table.TryAdd("battle.defeat", "失败");
            table.TryAdd("battle.draw", "平局");
            table.TryAdd("battle.retreat", "撤退");
            table.TryAdd("battle.attack", "攻击");
            table.TryAdd("battle.defend", "防御");

            // 通知
            table.TryAdd("notification.turn_start", "第{0}回合开始");
            table.TryAdd("notification.stratagem_success", "计策「{0}」成功");
            table.TryAdd("notification.stratagem_fail", "计策「{0}」失败");
            table.TryAdd("notification.territory_conquered", "占领了{0}");
            table.TryAdd("notification.territory_lost", "失去了{0}");

            // 設定
            table.TryAdd("settings.language", "语言");
            table.TryAdd("settings.audio", "音频");
            table.TryAdd("settings.master_volume", "主音量");
            table.TryAdd("settings.bgm_volume", "背景音乐音量");
            table.TryAdd("settings.se_volume", "音效音量");
            table.TryAdd("settings.voice_volume", "语音音量");

            // 勝利条件
            table.TryAdd("victory.conquest", "统一天下");
            table.TryAdd("victory.territory_count", "占领{0}个领地");
            table.TryAdd("victory.elimination", "消灭所有敌人");
            table.TryAdd("victory.survive", "生存{0}回合");
        }

        #endregion

        #region Language Management

        /// <summary>
        /// 言語を設定
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            // サポートされていない言語は英語にフォールバック
            if (!_stringTables.ContainsKey(language))
            {
                language = SystemLanguage.English;
            }

            if (_currentLanguage != language)
            {
                _currentLanguage = language;
                SaveLanguageSetting(language);
                OnLanguageChanged?.Invoke(language);

                Debug.Log($"Language changed to: {language}");
            }
        }

        /// <summary>
        /// システム言語を取得
        /// </summary>
        private SystemLanguage GetSystemLanguage()
        {
            var systemLang = Application.systemLanguage;

            // サポートされている言語かチェック
            if (_stringTables.ContainsKey(systemLang))
            {
                return systemLang;
            }

            // 中国語の場合
            if (systemLang == SystemLanguage.Chinese)
            {
                return SystemLanguage.ChineseSimplified;
            }

            return _defaultLanguage;
        }

        /// <summary>
        /// 保存された言語設定を読み込み
        /// </summary>
        private SystemLanguage LoadSavedLanguage()
        {
            int savedLang = PlayerPrefs.GetInt("Language", (int)_defaultLanguage);
            return (SystemLanguage)savedLang;
        }

        /// <summary>
        /// 言語設定を保存
        /// </summary>
        private void SaveLanguageSetting(SystemLanguage language)
        {
            PlayerPrefs.SetInt("Language", (int)language);
            PlayerPrefs.Save();
        }

        #endregion

        #region Localization

        /// <summary>
        /// ローカライズされた文字列を取得
        /// </summary>
        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            // 現在の言語で検索
            if (_stringTables.TryGetValue(_currentLanguage, out var table))
            {
                if (table.TryGetValue(key, out var value))
                {
                    return value;
                }
            }

            // フォールバック（日本語）
            if (_stringTables.TryGetValue(SystemLanguage.Japanese, out var fallbackTable))
            {
                if (fallbackTable.TryGetValue(key, out var fallbackValue))
                {
                    return fallbackValue;
                }
            }

            // キーをそのまま返す
            Debug.LogWarning($"Localization key not found: {key}");
            return key;
        }

        /// <summary>
        /// フォーマット付きローカライズ文字列を取得
        /// </summary>
        public string GetString(string key, params object[] args)
        {
            string format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                Debug.LogWarning($"Format error for key: {key}");
                return format;
            }
        }

        /// <summary>
        /// 言語名を取得
        /// </summary>
        public string GetLanguageName(SystemLanguage language)
        {
            return language switch
            {
                SystemLanguage.Japanese => "日本語",
                SystemLanguage.English => "English",
                SystemLanguage.ChineseSimplified => "简体中文",
                SystemLanguage.ChineseTraditional => "繁體中文",
                SystemLanguage.Korean => "한국어",
                _ => language.ToString()
            };
        }

        /// <summary>
        /// 短縮キーでローカライズ文字列を取得
        /// </summary>
        public static string L(string key)
        {
            return Instance?.GetString(key) ?? key;
        }

        /// <summary>
        /// フォーマット付き短縮キーでローカライズ文字列を取得
        /// </summary>
        public static string L(string key, params object[] args)
        {
            return Instance?.GetString(key, args) ?? key;
        }

        #endregion

        #region Dynamic String Registration

        /// <summary>
        /// 文字列を登録
        /// </summary>
        public void RegisterString(string key, string value, SystemLanguage language)
        {
            var table = GetOrCreateTable(language);
            table[key] = value;
        }

        /// <summary>
        /// 複数言語で文字列を登録
        /// </summary>
        public void RegisterStrings(string key, Dictionary<SystemLanguage, string> values)
        {
            foreach (var kvp in values)
            {
                RegisterString(key, kvp.Value, kvp.Key);
            }
        }

        #endregion
    }

    /// <summary>
    /// ローカライゼーションデータ
    /// </summary>
    [Serializable]
    public class LocalizationData : ScriptableObject
    {
        [SerializeField] private SystemLanguage _language;
        [SerializeField] private List<LocalizationEntry> _entries = new List<LocalizationEntry>();

        public SystemLanguage Language => _language;
        public IReadOnlyList<LocalizationEntry> Entries => _entries;
    }

    /// <summary>
    /// ローカライゼーションエントリ
    /// </summary>
    [Serializable]
    public class LocalizationEntry
    {
        [SerializeField] private string _key;
        [SerializeField] private string _value;

        public string Key => _key;
        public string Value => _value;
    }
}
