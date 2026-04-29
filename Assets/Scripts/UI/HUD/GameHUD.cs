using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.UI.HUD
{
    /// <summary>
    /// ゲームHUD
    /// メインゲーム画面のヘッドアップディスプレイ
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [Header("リソース表示")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _foodText;
        [SerializeField] private TextMeshProUGUI _stratagemPointsText;
        [SerializeField] private TextMeshProUGUI _totalSoldiersText;

        [Header("ターン情報")]
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _yearText;
        [SerializeField] private TextMeshProUGUI _phaseText;
        [SerializeField] private TextMeshProUGUI _currentFactionText;
        [SerializeField] private Image _factionBannerImage;

        [Header("ミニマップ")]
        [SerializeField] private RawImage _minimapImage;
        [SerializeField] private Button _minimapZoomInButton;
        [SerializeField] private Button _minimapZoomOutButton;

        [Header("クイックアクション")]
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private Button _stratagemButton;
        [SerializeField] private Button _diplomacyButton;
        [SerializeField] private Button _menuButton;

        [Header("パネル参照")]
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _stratagemPanel;
        [SerializeField] private GameObject _diplomacyPanel;

        [Header("アニメーション")]
        [SerializeField] private Animator _resourceAnimator;
        [SerializeField] private float _resourceChangeHighlightDuration = 0.5f;

        // イベント
        public event Action OnEndTurnClicked;
        public event Action OnStratagemClicked;
        public event Action OnDiplomacyClicked;
        public event Action OnMenuClicked;

        // キャッシュ
        private string _currentFactionId;
        private int _lastGold;
        private int _lastFood;
        private int _lastSP;

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
            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

            if (_stratagemButton != null)
                _stratagemButton.onClick.AddListener(OnStratagemButtonClicked);

            if (_diplomacyButton != null)
                _diplomacyButton.onClick.AddListener(OnDiplomacyButtonClicked);

            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenuButtonClicked);

            if (_minimapZoomInButton != null)
                _minimapZoomInButton.onClick.AddListener(OnMinimapZoomIn);

            if (_minimapZoomOutButton != null)
                _minimapZoomOutButton.onClick.AddListener(OnMinimapZoomOut);
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnFactionTurnStarted += OnFactionTurnStarted;
            EventBus.OnTurnPhaseChanged += OnPhaseChanged;
            EventBus.OnResourceChanged += OnResourceChanged;
            EventBus.OnTurnEnded += OnTurnEnded;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnFactionTurnStarted -= OnFactionTurnStarted;
            EventBus.OnTurnPhaseChanged -= OnPhaseChanged;
            EventBus.OnResourceChanged -= OnResourceChanged;
            EventBus.OnTurnEnded -= OnTurnEnded;
        }

        #endregion

        #region Update Display

        /// <summary>
        /// HUD全体を更新
        /// </summary>
        public void RefreshAll()
        {
            UpdateResourceDisplay();
            UpdateTurnInfo();
        }

        /// <summary>
        /// リソース表示を更新
        /// </summary>
        public void UpdateResourceDisplay()
        {
            var faction = GetPlayerFaction();
            if (faction == null) return;

            // 金
            if (_goldText != null)
            {
                int gold = faction.Gold;
                _goldText.text = FormatNumber(gold);
                if (gold != _lastGold)
                {
                    HighlightResourceChange(_goldText, gold > _lastGold);
                    _lastGold = gold;
                }
            }

            // 食糧
            if (_foodText != null)
            {
                int food = faction.Food;
                _foodText.text = FormatNumber(food);
                if (food != _lastFood)
                {
                    HighlightResourceChange(_foodText, food > _lastFood);
                    _lastFood = food;
                }
            }

            // 計略ポイント
            if (_stratagemPointsText != null)
            {
                int sp = faction.StratagemPoints;
                _stratagemPointsText.text = sp.ToString();
                if (sp != _lastSP)
                {
                    HighlightResourceChange(_stratagemPointsText, sp > _lastSP);
                    _lastSP = sp;
                }
            }

            // 総兵力
            if (_totalSoldiersText != null)
            {
                int soldiers = Battle.ArmyManager.Instance?.GetTotalSoldiers(faction.Id) ?? 0;
                _totalSoldiersText.text = FormatNumber(soldiers);
            }
        }

        /// <summary>
        /// ターン情報を更新
        /// </summary>
        public void UpdateTurnInfo()
        {
            var turnManager = TurnManager.Instance;
            if (turnManager == null) return;

            if (_turnText != null)
                _turnText.text = $"ターン {turnManager.CurrentTurn}";

            if (_yearText != null)
            {
                int year = GameManager.Instance?.CurrentYear ?? 0;
                _yearText.text = $"{year}年";
            }

            if (_phaseText != null)
                _phaseText.text = GetPhaseText(turnManager.CurrentPhase);

            UpdateCurrentFaction();
        }

        /// <summary>
        /// 現在の勢力表示を更新
        /// </summary>
        private void UpdateCurrentFaction()
        {
            var currentFaction = TurnManager.Instance?.GetCurrentFaction();
            if (currentFaction == null) return;

            if (_currentFactionText != null)
                _currentFactionText.text = currentFaction.Name;

            // バナー画像更新（将来の実装用）
            // if (_factionBannerImage != null && currentFaction.BannerSprite != null)
            //     _factionBannerImage.sprite = currentFaction.BannerSprite;
        }

        /// <summary>
        /// フェーズテキストを取得
        /// </summary>
        private string GetPhaseText(TurnPhase phase)
        {
            return phase switch
            {
                TurnPhase.Internal => "内政フェーズ",
                TurnPhase.Diplomacy => "外交フェーズ",
                TurnPhase.Military => "軍事フェーズ",
                _ => "フェーズ"
            };
        }

        /// <summary>
        /// 数値をフォーマット
        /// </summary>
        private string FormatNumber(int value)
        {
            if (value >= 10000)
                return $"{value / 10000f:F1}万";
            else if (value >= 1000)
                return $"{value / 1000f:F1}千";
            else
                return value.ToString();
        }

        /// <summary>
        /// リソース変更をハイライト
        /// </summary>
        private void HighlightResourceChange(TextMeshProUGUI text, bool increased)
        {
            if (text == null) return;

            Color highlightColor = increased ? Color.green : Color.red;
            Color originalColor = text.color;

            text.color = highlightColor;

            // 元の色に戻す
            StartCoroutine(ResetColorAfterDelay(text, originalColor, _resourceChangeHighlightDuration));
        }

        private System.Collections.IEnumerator ResetColorAfterDelay(TextMeshProUGUI text, Color color, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (text != null)
                text.color = color;
        }

        #endregion

        #region Button Handlers

        private void OnEndTurnButtonClicked()
        {
            OnEndTurnClicked?.Invoke();
            TurnManager.Instance?.EndFactionTurn();
        }

        private void OnStratagemButtonClicked()
        {
            OnStratagemClicked?.Invoke();
            TogglePanel(_stratagemPanel);
        }

        private void OnDiplomacyButtonClicked()
        {
            OnDiplomacyClicked?.Invoke();
            TogglePanel(_diplomacyPanel);
        }

        private void OnMenuButtonClicked()
        {
            OnMenuClicked?.Invoke();
            TogglePauseMenu();
        }

        private void OnMinimapZoomIn()
        {
            // TODO: ミニマップズームイン実装
            Debug.Log("Minimap zoom in");
        }

        private void OnMinimapZoomOut()
        {
            // TODO: ミニマップズームアウト実装
            Debug.Log("Minimap zoom out");
        }

        /// <summary>
        /// パネルをトグル
        /// </summary>
        private void TogglePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }

        /// <summary>
        /// ポーズメニューをトグル
        /// </summary>
        public void TogglePauseMenu()
        {
            if (_pauseMenuPanel != null)
            {
                bool isActive = !_pauseMenuPanel.activeSelf;
                _pauseMenuPanel.SetActive(isActive);

                // ゲーム一時停止
                Time.timeScale = isActive ? 0f : 1f;
            }
        }

        #endregion

        #region Event Handlers

        private void OnFactionTurnStarted(string factionId)
        {
            _currentFactionId = factionId;
            UpdateTurnInfo();
            UpdateResourceDisplay();

            // プレイヤーのターンかどうかでボタンの有効/無効を切り替え
            var faction = GameManager.Instance?.GetFaction(factionId);
            bool isPlayerTurn = faction?.IsPlayer ?? false;

            if (_endTurnButton != null)
                _endTurnButton.interactable = isPlayerTurn;

            if (_stratagemButton != null)
                _stratagemButton.interactable = isPlayerTurn;

            if (_diplomacyButton != null)
                _diplomacyButton.interactable = isPlayerTurn;
        }

        private void OnPhaseChanged(TurnPhase phase)
        {
            UpdateTurnInfo();
        }

        private void OnResourceChanged(ResourceEventArgs args)
        {
            if (args.FactionId == _currentFactionId)
            {
                UpdateResourceDisplay();
            }
        }

        private void OnTurnEnded(int turnNumber)
        {
            UpdateTurnInfo();
        }

        #endregion

        #region Helper

        /// <summary>
        /// プレイヤー勢力を取得
        /// </summary>
        private Faction GetPlayerFaction()
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.IsPlayer)
                {
                    return faction;
                }
            }

            return null;
        }

        /// <summary>
        /// ターン終了ボタンの状態を設定
        /// </summary>
        public void SetEndTurnButtonEnabled(bool enabled)
        {
            if (_endTurnButton != null)
                _endTurnButton.interactable = enabled;
        }

        /// <summary>
        /// 計略ボタンの状態を設定
        /// </summary>
        public void SetStratagemButtonEnabled(bool enabled)
        {
            if (_stratagemButton != null)
                _stratagemButton.interactable = enabled;
        }

        #endregion
    }
}
