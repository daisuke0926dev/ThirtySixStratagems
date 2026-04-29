using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.UI.Menu
{
    /// <summary>
    /// シナリオ選択パネル
    /// 新規ゲーム開始時のシナリオ選択を管理
    /// </summary>
    public class ScenarioSelectPanel : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private MainMenuUI _mainMenu;
        [SerializeField] private ScenarioDatabase _scenarioDatabase;

        [Header("リスト")]
        [SerializeField] private Transform _scenarioListContent;
        [SerializeField] private GameObject _scenarioItemPrefab;

        [Header("詳細表示")]
        [SerializeField] private GameObject _detailsPanel;
        [SerializeField] private TextMeshProUGUI _scenarioNameText;
        [SerializeField] private TextMeshProUGUI _scenarioDescriptionText;
        [SerializeField] private TextMeshProUGUI _yearText;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        [SerializeField] private Transform _factionListContent;
        [SerializeField] private GameObject _factionItemPrefab;

        [Header("勢力選択")]
        [SerializeField] private TextMeshProUGUI _selectedFactionText;
        [SerializeField] private Image _factionBannerImage;

        [Header("ボタン")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;

        // 状態
        private ScenarioData _selectedScenario;
        private string _selectedFactionId;
        private List<GameObject> _scenarioItems = new List<GameObject>();
        private List<GameObject> _factionItems = new List<GameObject>();

        // イベント
        public event Action<string, string> OnGameStartRequested;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            PopulateScenarioList();
            ClearDetails();
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
                _startButton.interactable = false;
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// シナリオリストを生成
        /// </summary>
        private void PopulateScenarioList()
        {
            ClearScenarioList();

            if (_scenarioDatabase == null || _scenarioItemPrefab == null || _scenarioListContent == null)
                return;

            foreach (var scenario in _scenarioDatabase.Scenarios)
            {
                var item = Instantiate(_scenarioItemPrefab, _scenarioListContent);
                var scenarioItem = item.GetComponent<ScenarioListItem>();

                if (scenarioItem != null)
                {
                    scenarioItem.Setup(scenario, OnScenarioSelected);
                }
                else
                {
                    // 簡易セットアップ
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = scenario.ScenarioName;
                    }

                    var button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        var scenarioRef = scenario;
                        button.onClick.AddListener(() => OnScenarioSelected(scenarioRef));
                    }
                }

                _scenarioItems.Add(item);
            }
        }

        /// <summary>
        /// シナリオリストをクリア
        /// </summary>
        private void ClearScenarioList()
        {
            foreach (var item in _scenarioItems)
            {
                Destroy(item);
            }
            _scenarioItems.Clear();
        }

        #endregion

        #region Scenario Selection

        /// <summary>
        /// シナリオが選択された
        /// </summary>
        private void OnScenarioSelected(ScenarioData scenario)
        {
            _selectedScenario = scenario;
            _selectedFactionId = null;
            UpdateStartButton();

            ShowScenarioDetails(scenario);
            PopulateFactionList(scenario);
        }

        /// <summary>
        /// シナリオ詳細を表示
        /// </summary>
        private void ShowScenarioDetails(ScenarioData scenario)
        {
            if (_detailsPanel != null)
                _detailsPanel.SetActive(true);

            if (_scenarioNameText != null)
                _scenarioNameText.text = scenario.ScenarioName;

            if (_scenarioDescriptionText != null)
                _scenarioDescriptionText.text = scenario.Description;

            if (_yearText != null)
                _yearText.text = $"年代: {scenario.Year}年";

            if (_difficultyText != null)
                _difficultyText.text = $"難易度: {GetDifficultyText(scenario.Difficulty)}";
        }

        /// <summary>
        /// 難易度テキストを取得
        /// </summary>
        private string GetDifficultyText(int difficulty)
        {
            return difficulty switch
            {
                1 => "易 ★☆☆☆☆",
                2 => "普通 ★★☆☆☆",
                3 => "難 ★★★☆☆",
                4 => "超難 ★★★★☆",
                5 => "極難 ★★★★★",
                _ => "不明"
            };
        }

        /// <summary>
        /// 詳細をクリア
        /// </summary>
        private void ClearDetails()
        {
            _selectedScenario = null;
            _selectedFactionId = null;

            if (_detailsPanel != null)
                _detailsPanel.SetActive(false);

            if (_selectedFactionText != null)
                _selectedFactionText.text = "勢力を選択してください";

            UpdateStartButton();
        }

        #endregion

        #region Faction Selection

        /// <summary>
        /// 勢力リストを生成
        /// </summary>
        private void PopulateFactionList(ScenarioData scenario)
        {
            ClearFactionList();

            if (_factionListContent == null || _factionItemPrefab == null)
                return;

            foreach (var faction in scenario.Factions)
            {
                if (!faction.IsPlayable) continue;

                var item = Instantiate(_factionItemPrefab, _factionListContent);
                var factionItem = item.GetComponent<FactionListItem>();

                if (factionItem != null)
                {
                    factionItem.Setup(faction, OnFactionSelected);
                }
                else
                {
                    // 簡易セットアップ
                    var text = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = faction.FactionName;
                    }

                    var button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        var factionRef = faction;
                        button.onClick.AddListener(() => OnFactionSelected(factionRef));
                    }
                }

                _factionItems.Add(item);
            }
        }

        /// <summary>
        /// 勢力リストをクリア
        /// </summary>
        private void ClearFactionList()
        {
            foreach (var item in _factionItems)
            {
                Destroy(item);
            }
            _factionItems.Clear();
        }

        /// <summary>
        /// 勢力が選択された
        /// </summary>
        private void OnFactionSelected(ScenarioFactionData faction)
        {
            _selectedFactionId = faction.FactionId;

            if (_selectedFactionText != null)
                _selectedFactionText.text = $"選択中: {faction.FactionName}";

            if (_factionBannerImage != null && faction.BannerSprite != null)
                _factionBannerImage.sprite = faction.BannerSprite;

            UpdateStartButton();
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 開始ボタン更新
        /// </summary>
        private void UpdateStartButton()
        {
            if (_startButton != null)
            {
                _startButton.interactable = _selectedScenario != null && !string.IsNullOrEmpty(_selectedFactionId);
            }
        }

        /// <summary>
        /// 開始ボタンクリック
        /// </summary>
        private void OnStartClicked()
        {
            if (_selectedScenario == null || string.IsNullOrEmpty(_selectedFactionId))
                return;

            Debug.Log($"Starting game: Scenario={_selectedScenario.ScenarioId}, Faction={_selectedFactionId}");

            OnGameStartRequested?.Invoke(_selectedScenario.ScenarioId, _selectedFactionId);

            _mainMenu?.StartNewGame(_selectedScenario.ScenarioId);
        }

        /// <summary>
        /// 戻るボタンクリック
        /// </summary>
        private void OnBackClicked()
        {
            _mainMenu?.ShowMainPanel();
        }

        #endregion
    }

    /// <summary>
    /// シナリオリストアイテム
    /// </summary>
    public class ScenarioListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _yearText;
        [SerializeField] private Image _thumbnail;
        [SerializeField] private Button _button;

        private ScenarioData _scenario;
        private Action<ScenarioData> _onSelected;

        public void Setup(ScenarioData scenario, Action<ScenarioData> onSelected)
        {
            _scenario = scenario;
            _onSelected = onSelected;

            if (_nameText != null)
                _nameText.text = scenario.ScenarioName;

            if (_yearText != null)
                _yearText.text = $"{scenario.Year}年";

            if (_thumbnail != null && scenario.Thumbnail != null)
                _thumbnail.sprite = scenario.Thumbnail;

            if (_button != null)
                _button.onClick.AddListener(() => _onSelected?.Invoke(_scenario));
        }
    }

    /// <summary>
    /// 勢力リストアイテム
    /// </summary>
    public class FactionListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _leaderText;
        [SerializeField] private Image _bannerImage;
        [SerializeField] private Button _button;

        private ScenarioFactionData _faction;
        private Action<ScenarioFactionData> _onSelected;

        public void Setup(ScenarioFactionData faction, Action<ScenarioFactionData> onSelected)
        {
            _faction = faction;
            _onSelected = onSelected;

            if (_nameText != null)
                _nameText.text = faction.FactionName;

            if (_leaderText != null)
                _leaderText.text = $"君主: {faction.LeaderName}";

            if (_bannerImage != null && faction.BannerSprite != null)
                _bannerImage.sprite = faction.BannerSprite;

            if (_button != null)
                _button.onClick.AddListener(() => _onSelected?.Invoke(_faction));
        }
    }
}
