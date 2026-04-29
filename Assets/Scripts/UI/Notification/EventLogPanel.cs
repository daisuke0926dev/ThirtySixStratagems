using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.UI.Notification
{
    /// <summary>
    /// イベントログパネル
    /// ゲーム内イベントの履歴を表示
    /// </summary>
    public class EventLogPanel : MonoBehaviour
    {
        public static EventLogPanel Instance { get; private set; }

        [Header("UI要素")]
        [SerializeField] private Transform _logContent;
        [SerializeField] private GameObject _logEntryPrefab;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _toggleButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private GameObject _panelContent;

        [Header("フィルター")]
        [SerializeField] private Toggle _showBattleToggle;
        [SerializeField] private Toggle _showDiplomacyToggle;
        [SerializeField] private Toggle _showStratagemToggle;
        [SerializeField] private Toggle _showSystemToggle;

        [Header("設定")]
        [SerializeField] private int _maxLogEntries = 100;
        [SerializeField] private bool _autoScroll = true;

        // ログデータ
        private List<EventLogEntry> _allLogs = new List<EventLogEntry>();
        private List<GameObject> _logItems = new List<GameObject>();
        private bool _isPanelOpen = false;

        // フィルター状態
        private bool _showBattle = true;
        private bool _showDiplomacy = true;
        private bool _showStratagem = true;
        private bool _showSystem = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            SetupButtons();
            SetupToggles();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.AddListener(TogglePanel);

            if (_clearButton != null)
                _clearButton.onClick.AddListener(ClearLog);
        }

        /// <summary>
        /// トグルの設定
        /// </summary>
        private void SetupToggles()
        {
            if (_showBattleToggle != null)
            {
                _showBattleToggle.isOn = _showBattle;
                _showBattleToggle.onValueChanged.AddListener(v => { _showBattle = v; RefreshDisplay(); });
            }

            if (_showDiplomacyToggle != null)
            {
                _showDiplomacyToggle.isOn = _showDiplomacy;
                _showDiplomacyToggle.onValueChanged.AddListener(v => { _showDiplomacy = v; RefreshDisplay(); });
            }

            if (_showStratagemToggle != null)
            {
                _showStratagemToggle.isOn = _showStratagem;
                _showStratagemToggle.onValueChanged.AddListener(v => { _showStratagem = v; RefreshDisplay(); });
            }

            if (_showSystemToggle != null)
            {
                _showSystemToggle.isOn = _showSystem;
                _showSystemToggle.onValueChanged.AddListener(v => { _showSystem = v; RefreshDisplay(); });
            }
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnBattleStarted += OnBattleStarted;
            EventBus.OnBattleEnded += OnBattleEnded;
            EventBus.OnTerritoryConquered += OnTerritoryConquered;
            EventBus.OnStratagemExecuted += OnStratagemExecuted;
            EventBus.OnFactionTurnStarted += OnFactionTurnStarted;
            EventBus.OnTurnEnded += OnTurnEnded;
            EventBus.OnArmyDisbanded += OnArmyDisbanded;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnBattleStarted -= OnBattleStarted;
            EventBus.OnBattleEnded -= OnBattleEnded;
            EventBus.OnTerritoryConquered -= OnTerritoryConquered;
            EventBus.OnStratagemExecuted -= OnStratagemExecuted;
            EventBus.OnFactionTurnStarted -= OnFactionTurnStarted;
            EventBus.OnTurnEnded -= OnTurnEnded;
            EventBus.OnArmyDisbanded -= OnArmyDisbanded;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ログを追加
        /// </summary>
        public void AddLog(string message, EventLogCategory category, string details = null)
        {
            var entry = new EventLogEntry
            {
                Message = message,
                Details = details,
                Category = category,
                Turn = TurnManager.Instance?.CurrentTurn ?? 0,
                Year = GameManager.Instance?.CurrentYear ?? 0,
                Timestamp = DateTime.Now
            };

            _allLogs.Add(entry);

            // 上限超過時は古いログを削除
            while (_allLogs.Count > _maxLogEntries)
            {
                _allLogs.RemoveAt(0);
            }

            // 表示に追加
            if (ShouldShowEntry(entry))
            {
                AddLogItem(entry);
            }
        }

        /// <summary>
        /// パネルの開閉
        /// </summary>
        public void TogglePanel()
        {
            _isPanelOpen = !_isPanelOpen;
            if (_panelContent != null)
                _panelContent.SetActive(_isPanelOpen);
        }

        /// <summary>
        /// ログをクリア
        /// </summary>
        public void ClearLog()
        {
            _allLogs.Clear();
            ClearDisplayedLogs();
        }

        #endregion

        #region Display

        /// <summary>
        /// 表示を更新
        /// </summary>
        private void RefreshDisplay()
        {
            ClearDisplayedLogs();

            foreach (var entry in _allLogs)
            {
                if (ShouldShowEntry(entry))
                {
                    AddLogItem(entry);
                }
            }
        }

        /// <summary>
        /// ログアイテムを追加
        /// </summary>
        private void AddLogItem(EventLogEntry entry)
        {
            if (_logContent == null || _logEntryPrefab == null) return;

            var item = Instantiate(_logEntryPrefab, _logContent);
            SetupLogItem(item, entry);
            _logItems.Add(item);

            // 自動スクロール
            if (_autoScroll && _scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// ログアイテムをセットアップ
        /// </summary>
        private void SetupLogItem(GameObject item, EventLogEntry entry)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            // タイムスタンプ
            if (texts.Length > 0)
                texts[0].text = $"[T{entry.Turn}]";

            // メッセージ
            if (texts.Length > 1)
                texts[1].text = entry.Message;

            // 詳細
            if (texts.Length > 2 && !string.IsNullOrEmpty(entry.Details))
                texts[2].text = entry.Details;

            // カテゴリに応じた色
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetCategoryColor(entry.Category);
            }
        }

        /// <summary>
        /// 表示されているログをクリア
        /// </summary>
        private void ClearDisplayedLogs()
        {
            foreach (var item in _logItems)
            {
                Destroy(item);
            }
            _logItems.Clear();
        }

        /// <summary>
        /// エントリを表示すべきか判定
        /// </summary>
        private bool ShouldShowEntry(EventLogEntry entry)
        {
            return entry.Category switch
            {
                EventLogCategory.Battle => _showBattle,
                EventLogCategory.Diplomacy => _showDiplomacy,
                EventLogCategory.Stratagem => _showStratagem,
                EventLogCategory.System => _showSystem,
                _ => true
            };
        }

        /// <summary>
        /// カテゴリの色を取得
        /// </summary>
        private Color GetCategoryColor(EventLogCategory category)
        {
            return category switch
            {
                EventLogCategory.Battle => new Color(0.9f, 0.3f, 0.3f, 0.2f),
                EventLogCategory.Diplomacy => new Color(0.5f, 0.3f, 0.8f, 0.2f),
                EventLogCategory.Stratagem => new Color(0.2f, 0.6f, 0.8f, 0.2f),
                EventLogCategory.System => new Color(0.5f, 0.5f, 0.5f, 0.2f),
                _ => new Color(1f, 1f, 1f, 0.1f)
            };
        }

        #endregion

        #region Event Handlers

        private void OnBattleStarted(BattleEventArgs args)
        {
            var territory = GameManager.Instance?.GetTerritory(args.TerritoryId);
            AddLog($"{territory?.Name ?? "不明"}で戦闘開始", EventLogCategory.Battle);
        }

        private void OnBattleEnded(BattleResultEventArgs args)
        {
            var victor = GameManager.Instance?.GetFaction(args.VictorFactionId);
            var territory = GameManager.Instance?.GetTerritory(args.TerritoryId);
            string details = $"損害 - 攻撃側: {args.AttackerLosses}, 防御側: {args.DefenderLosses}";
            AddLog($"{territory?.Name}の戦い: {victor?.Name}勝利", EventLogCategory.Battle, details);
        }

        private void OnTerritoryConquered(TerritoryConqueredEventArgs args)
        {
            var territory = GameManager.Instance?.GetTerritory(args.TerritoryId);
            var prev = GameManager.Instance?.GetFaction(args.PreviousOwnerId);
            var newOwner = GameManager.Instance?.GetFaction(args.NewOwnerId);
            AddLog($"{territory?.Name}が{newOwner?.Name}の領地に", EventLogCategory.Battle,
                $"旧支配: {prev?.Name}");
        }

        private void OnStratagemExecuted(StratagemEventArgs args)
        {
            string result = args.Success ? "成功" : "失敗";
            var caster = GameManager.Instance?.GetFaction(args.CasterFactionId);
            AddLog($"{caster?.Name}の計略「{args.StratagemName}」{result}", EventLogCategory.Stratagem);
        }

        private void OnFactionTurnStarted(string factionId)
        {
            var faction = GameManager.Instance?.GetFaction(factionId);
            AddLog($"{faction?.Name}のターン開始", EventLogCategory.System);
        }

        private void OnTurnEnded(int turnNumber)
        {
            AddLog($"ターン{turnNumber}終了", EventLogCategory.System);
        }

        private void OnArmyDisbanded(ArmyEventArgs args)
        {
            AddLog($"軍が解散しました", EventLogCategory.Battle);
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// イベントログカテゴリ
    /// </summary>
    public enum EventLogCategory
    {
        System,
        Battle,
        Diplomacy,
        Stratagem
    }

    /// <summary>
    /// イベントログエントリ
    /// </summary>
    public class EventLogEntry
    {
        public string Message;
        public string Details;
        public EventLogCategory Category;
        public int Turn;
        public int Year;
        public DateTime Timestamp;
    }

    #endregion
}
