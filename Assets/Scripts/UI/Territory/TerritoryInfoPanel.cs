using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.UI.Territory
{
    /// <summary>
    /// 領地情報パネル
    /// 選択した領地の詳細情報を表示
    /// </summary>
    public class TerritoryInfoPanel : MonoBehaviour
    {
        public static TerritoryInfoPanel Instance { get; private set; }

        [Header("基本情報")]
        [SerializeField] private TextMeshProUGUI _territoryNameText;
        [SerializeField] private TextMeshProUGUI _ownerNameText;
        [SerializeField] private Image _ownerBannerImage;

        [Header("ステータス")]
        [SerializeField] private TextMeshProUGUI _populationText;
        [SerializeField] private TextMeshProUGUI _economyText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private Slider _populationSlider;
        [SerializeField] private Slider _economySlider;
        [SerializeField] private Slider _defenseSlider;

        [Header("生産")]
        [SerializeField] private TextMeshProUGUI _goldProductionText;
        [SerializeField] private TextMeshProUGUI _foodProductionText;
        [SerializeField] private TextMeshProUGUI _recruitableText;

        [Header("軍事")]
        [SerializeField] private Transform _armyListContent;
        [SerializeField] private GameObject _armyItemPrefab;
        [SerializeField] private TextMeshProUGUI _totalSoldiersText;

        [Header("隣接領地")]
        [SerializeField] private Transform _adjacentListContent;
        [SerializeField] private GameObject _adjacentItemPrefab;

        [Header("アクションボタン")]
        [SerializeField] private Button _developButton;
        [SerializeField] private Button _recruitButton;
        [SerializeField] private Button _fortifyButton;
        [SerializeField] private Button _moveArmyButton;
        [SerializeField] private TextMeshProUGUI _developCostText;
        [SerializeField] private TextMeshProUGUI _recruitCostText;

        [Header("パネル")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        // 状態
        private string _selectedTerritoryId;
        private List<GameObject> _armyItems = new List<GameObject>();
        private List<GameObject> _adjacentItems = new List<GameObject>();

        // イベント
        public event Action<string> OnTerritorySelected;
        public event Action<string> OnArmySelected;
        public event Action OnDevelopRequested;
        public event Action OnRecruitRequested;
        public event Action OnFortifyRequested;

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
            EventBus.OnTerritoryChanged += OnTerritoryChanged;
        }

        private void OnDisable()
        {
            EventBus.OnTerritoryChanged -= OnTerritoryChanged;
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            if (_developButton != null)
                _developButton.onClick.AddListener(OnDevelopClicked);

            if (_recruitButton != null)
                _recruitButton.onClick.AddListener(OnRecruitClicked);

            if (_fortifyButton != null)
                _fortifyButton.onClick.AddListener(OnFortifyClicked);

            if (_moveArmyButton != null)
                _moveArmyButton.onClick.AddListener(OnMoveArmyClicked);
        }

        #endregion

        #region Display

        /// <summary>
        /// 領地情報を表示
        /// </summary>
        public void ShowTerritory(string territoryId)
        {
            _selectedTerritoryId = territoryId;
            gameObject.SetActive(true);

            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory == null)
            {
                Close();
                return;
            }

            UpdateBasicInfo(territory);
            UpdateStats(territory);
            UpdateProduction(territory);
            UpdateArmyList(territory);
            UpdateAdjacentList(territory);
            UpdateActionButtons(territory);

            OnTerritorySelected?.Invoke(territoryId);
        }

        /// <summary>
        /// 基本情報を更新
        /// </summary>
        private void UpdateBasicInfo(Data.Models.Territory territory)
        {
            if (_territoryNameText != null)
                _territoryNameText.text = territory.Name;

            var owner = GameManager.Instance?.GetFaction(territory.OwnerId);
            if (_ownerNameText != null)
                _ownerNameText.text = owner?.Name ?? "無所属";

            // TODO: バナー画像
        }

        /// <summary>
        /// ステータスを更新
        /// </summary>
        private void UpdateStats(Data.Models.Territory territory)
        {
            // 人口
            if (_populationText != null)
                _populationText.text = $"{territory.Population:N0}";
            if (_populationSlider != null)
            {
                _populationSlider.maxValue = 100000;
                _populationSlider.value = territory.Population;
            }

            // 経済
            if (_economyText != null)
                _economyText.text = territory.Economy.ToString();
            if (_economySlider != null)
            {
                _economySlider.maxValue = 100;
                _economySlider.value = territory.Economy;
            }

            // 防御
            if (_defenseText != null)
                _defenseText.text = territory.Defense.ToString();
            if (_defenseSlider != null)
            {
                _defenseSlider.maxValue = 100;
                _defenseSlider.value = territory.Defense;
            }
        }

        /// <summary>
        /// 生産情報を更新
        /// </summary>
        private void UpdateProduction(Data.Models.Territory territory)
        {
            // 金収入
            int goldProduction = Mathf.RoundToInt(territory.Economy * territory.Population / 1000f);
            if (_goldProductionText != null)
                _goldProductionText.text = $"+{goldProduction}/ターン";

            // 食糧生産
            int foodProduction = Mathf.RoundToInt(territory.Population * 0.5f);
            if (_foodProductionText != null)
                _foodProductionText.text = $"+{foodProduction}/ターン";

            // 徴兵可能数
            int recruitable = ResourceManager.Instance?.CalculateMaxRecruitment(territory.Id) ?? 0;
            if (_recruitableText != null)
                _recruitableText.text = $"最大 {recruitable} 人";
        }

        /// <summary>
        /// 軍リストを更新
        /// </summary>
        private void UpdateArmyList(Data.Models.Territory territory)
        {
            ClearArmyList();

            if (_armyListContent == null || _armyItemPrefab == null) return;

            var armies = ArmyManager.Instance?.GetArmiesAtTerritory(territory.Id);
            if (armies == null) return;

            int totalSoldiers = 0;

            foreach (var army in armies)
            {
                var item = Instantiate(_armyItemPrefab, _armyListContent);
                SetupArmyItem(item, army);
                _armyItems.Add(item);
                totalSoldiers += army.SoldierCount;
            }

            if (_totalSoldiersText != null)
                _totalSoldiersText.text = $"駐留兵力: {totalSoldiers:N0}";
        }

        /// <summary>
        /// 軍アイテムをセットアップ
        /// </summary>
        private void SetupArmyItem(GameObject item, Army army)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
                texts[0].text = army.Name;

            if (texts.Length > 1)
                texts[1].text = $"兵力: {army.SoldierCount:N0}";

            if (texts.Length > 2)
                texts[2].text = $"士気: {army.Morale}";

            var button = item.GetComponent<Button>();
            if (button != null)
            {
                string armyId = army.Id;
                button.onClick.AddListener(() => OnArmySelected?.Invoke(armyId));
            }
        }

        /// <summary>
        /// 軍リストをクリア
        /// </summary>
        private void ClearArmyList()
        {
            foreach (var item in _armyItems)
            {
                Destroy(item);
            }
            _armyItems.Clear();
        }

        /// <summary>
        /// 隣接領地リストを更新
        /// </summary>
        private void UpdateAdjacentList(Data.Models.Territory territory)
        {
            ClearAdjacentList();

            if (_adjacentListContent == null || _adjacentItemPrefab == null) return;

            foreach (var adjId in territory.AdjacentTerritoryIds)
            {
                var adjTerritory = GameManager.Instance?.GetTerritory(adjId);
                if (adjTerritory == null) continue;

                var item = Instantiate(_adjacentItemPrefab, _adjacentListContent);
                SetupAdjacentItem(item, adjTerritory);
                _adjacentItems.Add(item);
            }
        }

        /// <summary>
        /// 隣接領地アイテムをセットアップ
        /// </summary>
        private void SetupAdjacentItem(GameObject item, Data.Models.Territory territory)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
                texts[0].text = territory.Name;

            var owner = GameManager.Instance?.GetFaction(territory.OwnerId);
            if (texts.Length > 1)
                texts[1].text = owner?.Name ?? "無所属";

            // 色分け
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                var playerFaction = GetPlayerFaction();
                if (playerFaction != null)
                {
                    if (territory.OwnerId == playerFaction.Id)
                        image.color = new Color(0.5f, 0.8f, 0.5f, 0.5f); // 緑（自領）
                    else
                        image.color = new Color(0.8f, 0.5f, 0.5f, 0.5f); // 赤（敵領）
                }
            }

            var button = item.GetComponent<Button>();
            if (button != null)
            {
                string terrId = territory.Id;
                button.onClick.AddListener(() => ShowTerritory(terrId));
            }
        }

        /// <summary>
        /// 隣接リストをクリア
        /// </summary>
        private void ClearAdjacentList()
        {
            foreach (var item in _adjacentItems)
            {
                Destroy(item);
            }
            _adjacentItems.Clear();
        }

        /// <summary>
        /// アクションボタンを更新
        /// </summary>
        private void UpdateActionButtons(Data.Models.Territory territory)
        {
            var playerFaction = GetPlayerFaction();
            bool isOwner = playerFaction != null && territory.OwnerId == playerFaction.Id;

            // 自領地でない場合はボタンを無効化
            if (_developButton != null)
                _developButton.interactable = isOwner;

            if (_recruitButton != null)
                _recruitButton.interactable = isOwner;

            if (_fortifyButton != null)
                _fortifyButton.interactable = isOwner;

            if (_moveArmyButton != null)
            {
                var armies = ArmyManager.Instance?.GetArmiesAtTerritory(territory.Id);
                _moveArmyButton.interactable = isOwner && armies != null && armies.Count > 0;
            }

            // コスト表示
            if (_developCostText != null)
                _developCostText.text = $"費用: {CalculateDevelopCost(territory)}";

            if (_recruitCostText != null)
            {
                int maxRecruit = ResourceManager.Instance?.CalculateMaxRecruitment(territory.Id) ?? 0;
                int cost = ResourceManager.Instance?.CalculateRecruitmentCost(maxRecruit) ?? 0;
                _recruitCostText.text = $"費用: {cost}";
            }
        }

        /// <summary>
        /// 開発コストを計算
        /// </summary>
        private int CalculateDevelopCost(Data.Models.Territory territory)
        {
            return 500 + (territory.Economy * 10);
        }

        #endregion

        #region Button Handlers

        private void OnDevelopClicked()
        {
            OnDevelopRequested?.Invoke();

            var territory = GameManager.Instance?.GetTerritory(_selectedTerritoryId);
            if (territory == null) return;

            int cost = CalculateDevelopCost(territory);
            var playerFaction = GetPlayerFaction();

            if (playerFaction != null && playerFaction.Gold >= cost)
            {
                // 開発実行
                ResourceManager.Instance?.SpendGold(playerFaction.Id, cost);
                territory.Economy = Mathf.Min(100, territory.Economy + 5);

                // 表示更新
                ShowTerritory(_selectedTerritoryId);
            }
        }

        private void OnRecruitClicked()
        {
            OnRecruitRequested?.Invoke();
            // 徴兵パネルを開く処理は別途実装
        }

        private void OnFortifyClicked()
        {
            OnFortifyRequested?.Invoke();

            var territory = GameManager.Instance?.GetTerritory(_selectedTerritoryId);
            if (territory == null) return;

            int cost = 300 + (territory.Defense * 5);
            var playerFaction = GetPlayerFaction();

            if (playerFaction != null && playerFaction.Gold >= cost)
            {
                ResourceManager.Instance?.SpendGold(playerFaction.Id, cost);
                territory.Defense = Mathf.Min(100, territory.Defense + 5);

                ShowTerritory(_selectedTerritoryId);
            }
        }

        private void OnMoveArmyClicked()
        {
            // 軍移動モードを開始
            Debug.Log("Move army mode started");
        }

        #endregion

        #region Helper

        /// <summary>
        /// 閉じる
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// プレイヤー勢力を取得
        /// </summary>
        private Faction GetPlayerFaction()
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.IsPlayer)
                    return faction;
            }
            return null;
        }

        /// <summary>
        /// 領地変更イベント
        /// </summary>
        private void OnTerritoryChanged(string territoryId)
        {
            if (territoryId == _selectedTerritoryId && gameObject.activeSelf)
            {
                ShowTerritory(_selectedTerritoryId);
            }
        }

        /// <summary>
        /// 選択中の領地ID
        /// </summary>
        public string SelectedTerritoryId => _selectedTerritoryId;

        #endregion
    }
}
