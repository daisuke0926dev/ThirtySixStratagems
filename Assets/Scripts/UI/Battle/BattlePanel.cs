using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.UI.Battle
{
    /// <summary>
    /// 戦闘パネル
    /// 戦闘状況の表示と操作を提供
    /// </summary>
    public class BattlePanel : MonoBehaviour
    {
        [Header("ヘッダー")]
        [SerializeField] private TextMeshProUGUI _battleTitleText;
        [SerializeField] private TextMeshProUGUI _roundText;
        [SerializeField] private TextMeshProUGUI _phaseText;

        [Header("攻撃側")]
        [SerializeField] private BattleUnitDisplay _attackerDisplay;
        [SerializeField] private Image _attackerFactionIcon;

        [Header("防御側")]
        [SerializeField] private BattleUnitDisplay _defenderDisplay;
        [SerializeField] private Image _defenderFactionIcon;

        [Header("戦闘ログ")]
        [SerializeField] private Transform _logContainer;
        [SerializeField] private GameObject _logEntryPrefab;
        [SerializeField] private ScrollRect _logScrollRect;
        [SerializeField] private int _maxLogEntries = 50;

        [Header("ボタン")]
        [SerializeField] private Button _executeRoundButton;
        [SerializeField] private Button _autoResolveButton;
        [SerializeField] private Button _useStratagemButton;
        [SerializeField] private Button _retreatButton;

        [Header("結果表示")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultTitleText;
        [SerializeField] private TextMeshProUGUI _resultDetailsText;
        [SerializeField] private Button _resultCloseButton;

        private List<GameObject> _logEntries = new List<GameObject>();

        // イベント
        public event Action OnRoundExecuteRequested;
        public event Action OnAutoResolveRequested;
        public event Action OnStratagemRequested;
        public event Action<bool> OnRetreatRequested;
        public event Action OnResultClosed;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            SubscribeToBattleEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromBattleEvents();
        }

        #region Setup

        private void SetupButtons()
        {
            if (_executeRoundButton != null)
            {
                _executeRoundButton.onClick.AddListener(OnExecuteRoundClicked);
            }

            if (_autoResolveButton != null)
            {
                _autoResolveButton.onClick.AddListener(OnAutoResolveClicked);
            }

            if (_useStratagemButton != null)
            {
                _useStratagemButton.onClick.AddListener(OnUseStratagemClicked);
            }

            if (_retreatButton != null)
            {
                _retreatButton.onClick.AddListener(OnRetreatClicked);
            }

            if (_resultCloseButton != null)
            {
                _resultCloseButton.onClick.AddListener(OnResultCloseClicked);
            }
        }

        private void SubscribeToBattleEvents()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStarted += OnBattleStarted;
                BattleManager.Instance.OnBattleRoundCompleted += OnBattleRoundCompleted;
                BattleManager.Instance.OnBattleEnded += OnBattleEnded;
            }
        }

        private void UnsubscribeFromBattleEvents()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStarted -= OnBattleStarted;
                BattleManager.Instance.OnBattleRoundCompleted -= OnBattleRoundCompleted;
                BattleManager.Instance.OnBattleEnded -= OnBattleEnded;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// パネルを表示
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ClearLog();

            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }

            if (BattleManager.Instance?.CurrentBattle != null)
            {
                UpdateDisplay(BattleManager.Instance.CurrentBattle);
            }
        }

        /// <summary>
        /// パネルを非表示
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        public void UpdateDisplay(BattleState battle)
        {
            if (battle == null) return;

            // ヘッダー
            if (_battleTitleText != null)
            {
                _battleTitleText.text = $"{battle.TerritoryName}の戦い";
            }

            if (_roundText != null)
            {
                _roundText.text = $"第{battle.CurrentRound}ラウンド";
            }

            if (_phaseText != null)
            {
                _phaseText.text = GetPhaseText(battle.Phase);
            }

            // ユニット表示
            if (_attackerDisplay != null)
            {
                _attackerDisplay.UpdateDisplay(battle.Attacker);
            }

            if (_defenderDisplay != null)
            {
                _defenderDisplay.UpdateDisplay(battle.Defender);
            }

            // ボタン状態
            UpdateButtonStates(battle);
        }

        /// <summary>
        /// ログを追加
        /// </summary>
        public void AddLogEntry(string message, LogEntryType type = LogEntryType.Normal)
        {
            if (_logContainer == null || _logEntryPrefab == null) return;

            // 最大数を超えたら古いものを削除
            while (_logEntries.Count >= _maxLogEntries)
            {
                var oldest = _logEntries[0];
                _logEntries.RemoveAt(0);
                Destroy(oldest);
            }

            var entryObj = Instantiate(_logEntryPrefab, _logContainer);
            var entryText = entryObj.GetComponent<TextMeshProUGUI>();

            if (entryText != null)
            {
                entryText.text = message;
                entryText.color = GetLogColor(type);
            }

            _logEntries.Add(entryObj);

            // スクロールを下に
            if (_logScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _logScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// 結果を表示
        /// </summary>
        public void ShowResult(BattleResult result)
        {
            if (_resultPanel == null) return;

            _resultPanel.SetActive(true);

            if (_resultTitleText != null)
            {
                _resultTitleText.text = result.AttackerVictory ? "勝利！" : "敗北...";
                _resultTitleText.color = result.AttackerVictory ? Color.green : Color.red;
            }

            if (_resultDetailsText != null)
            {
                string details = $"総ラウンド数: {result.TotalRounds}\n\n";
                details += $"攻撃側損害: {result.AttackerTotalCasualties}\n";
                details += $"攻撃側生存: {result.AttackerSurvivors}\n\n";
                details += $"防御側損害: {result.DefenderTotalCasualties}\n";
                details += $"防御側生存: {result.DefenderSurvivors}";

                if (result.TerritoryConquered)
                {
                    details += "\n\n領地を占領しました！";
                }

                _resultDetailsText.text = details;
            }
        }

        #endregion

        #region Event Handlers

        private void OnBattleStarted(BattleState battle)
        {
            Show();
            UpdateDisplay(battle);
            AddLogEntry($"戦闘開始: {battle.TerritoryName}", LogEntryType.Important);
            AddLogEntry($"攻撃側: {battle.Attacker.ArmyName} ({battle.Attacker.FactionName})", LogEntryType.Normal);
            AddLogEntry($"防御側: {battle.Defender.ArmyName} ({battle.Defender.FactionName})", LogEntryType.Normal);
        }

        private void OnBattleRoundCompleted(BattleRoundResult round)
        {
            var battle = BattleManager.Instance?.CurrentBattle;
            if (battle != null)
            {
                UpdateDisplay(battle);
            }

            // ログ追加
            AddLogEntry($"--- 第{round.RoundNumber}ラウンド ---", LogEntryType.Header);
            AddLogEntry($"攻撃側戦闘力: {round.AttackerPower} / 防御側戦闘力: {round.DefenderPower}");
            AddLogEntry($"攻撃側損害: {round.AttackerCasualties} / 防御側損害: {round.DefenderCasualties}",
                round.AttackerCasualties > round.DefenderCasualties ? LogEntryType.Bad : LogEntryType.Good);

            if (!string.IsNullOrEmpty(round.SpecialEvent))
            {
                AddLogEntry(round.SpecialEvent, LogEntryType.Important);
            }
        }

        private void OnBattleEnded(BattleResult result)
        {
            AddLogEntry("=== 戦闘終了 ===", LogEntryType.Header);
            AddLogEntry(result.AttackerVictory ? "攻撃側の勝利！" : "防御側の勝利！",
                result.AttackerVictory ? LogEntryType.Good : LogEntryType.Bad);

            ShowResult(result);

            // ボタンを無効化
            SetButtonsEnabled(false);
        }

        private void OnExecuteRoundClicked()
        {
            OnRoundExecuteRequested?.Invoke();

            if (BattleManager.Instance?.IsBattleInProgress == true)
            {
                BattleManager.Instance.ExecuteRound();
            }
        }

        private void OnAutoResolveClicked()
        {
            OnAutoResolveRequested?.Invoke();

            if (BattleManager.Instance?.IsBattleInProgress == true)
            {
                SetButtonsEnabled(false);
                BattleManager.Instance.AutoResolveBattle();
            }
        }

        private void OnUseStratagemClicked()
        {
            OnStratagemRequested?.Invoke();
        }

        private void OnRetreatClicked()
        {
            OnRetreatRequested?.Invoke(true); // 攻撃側の撤退
        }

        private void OnResultCloseClicked()
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }

            OnResultClosed?.Invoke();
            Hide();
        }

        #endregion

        #region Helper Methods

        private void ClearLog()
        {
            foreach (var entry in _logEntries)
            {
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
            _logEntries.Clear();
        }

        private void UpdateButtonStates(BattleState battle)
        {
            bool canAct = battle.Phase == BattlePhase.Combat ||
                          battle.Phase == BattlePhase.Preparation;

            if (_executeRoundButton != null)
            {
                _executeRoundButton.interactable = canAct;
            }

            if (_autoResolveButton != null)
            {
                _autoResolveButton.interactable = canAct;
            }

            if (_useStratagemButton != null)
            {
                _useStratagemButton.interactable = canAct;
            }

            if (_retreatButton != null)
            {
                _retreatButton.interactable = canAct && battle.CurrentRound > 0;
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            if (_executeRoundButton != null) _executeRoundButton.interactable = enabled;
            if (_autoResolveButton != null) _autoResolveButton.interactable = enabled;
            if (_useStratagemButton != null) _useStratagemButton.interactable = enabled;
            if (_retreatButton != null) _retreatButton.interactable = enabled;
        }

        private string GetPhaseText(BattlePhase phase)
        {
            switch (phase)
            {
                case BattlePhase.Preparation:
                    return "準備中";
                case BattlePhase.Combat:
                    return "戦闘中";
                case BattlePhase.Result:
                    return "結果";
                default:
                    return "";
            }
        }

        private Color GetLogColor(LogEntryType type)
        {
            switch (type)
            {
                case LogEntryType.Good:
                    return new Color(0.2f, 0.8f, 0.2f);
                case LogEntryType.Bad:
                    return new Color(0.8f, 0.2f, 0.2f);
                case LogEntryType.Important:
                    return new Color(1f, 0.8f, 0.2f);
                case LogEntryType.Header:
                    return new Color(0.6f, 0.8f, 1f);
                default:
                    return Color.white;
            }
        }

        #endregion
    }

    /// <summary>
    /// ログエントリタイプ
    /// </summary>
    public enum LogEntryType
    {
        Normal,
        Good,
        Bad,
        Important,
        Header
    }
}
