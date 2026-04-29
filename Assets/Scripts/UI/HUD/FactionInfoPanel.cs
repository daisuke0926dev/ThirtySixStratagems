using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.UI.HUD
{
    /// <summary>
    /// 勢力情報パネル
    /// 勢力の詳細情報を表示
    /// </summary>
    public class FactionInfoPanel : MonoBehaviour
    {
        [Header("基本情報")]
        [SerializeField] private TextMeshProUGUI _factionNameText;
        [SerializeField] private TextMeshProUGUI _leaderNameText;
        [SerializeField] private Image _factionBannerImage;
        [SerializeField] private Image _leaderPortraitImage;

        [Header("リソース")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _foodText;
        [SerializeField] private TextMeshProUGUI _stratagemPointsText;
        [SerializeField] private Slider _goldSlider;
        [SerializeField] private Slider _foodSlider;

        [Header("統計")]
        [SerializeField] private TextMeshProUGUI _territoryCountText;
        [SerializeField] private TextMeshProUGUI _totalPopulationText;
        [SerializeField] private TextMeshProUGUI _totalSoldiersText;
        [SerializeField] private TextMeshProUGUI _characterCountText;
        [SerializeField] private TextMeshProUGUI _armyCountText;

        [Header("収支")]
        [SerializeField] private TextMeshProUGUI _incomeText;
        [SerializeField] private TextMeshProUGUI _expenseText;
        [SerializeField] private TextMeshProUGUI _balanceText;
        [SerializeField] private TextMeshProUGUI _foodProductionText;
        [SerializeField] private TextMeshProUGUI _foodConsumptionText;

        [Header("武将リスト")]
        [SerializeField] private Transform _characterListContent;
        [SerializeField] private GameObject _characterItemPrefab;

        [Header("領地リスト")]
        [SerializeField] private Transform _territoryListContent;
        [SerializeField] private GameObject _territoryItemPrefab;

        [Header("ボタン")]
        [SerializeField] private Button _closeButton;

        // 状態
        private string _displayedFactionId;
        private List<GameObject> _characterItems = new List<GameObject>();
        private List<GameObject> _territoryItems = new List<GameObject>();

        // イベント
        public event Action<string> OnCharacterSelected;
        public event Action<string> OnTerritorySelected;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            EventBus.OnResourceChanged += OnResourceChanged;
        }

        private void OnDisable()
        {
            EventBus.OnResourceChanged -= OnResourceChanged;
        }

        #region Display

        /// <summary>
        /// 勢力情報を表示
        /// </summary>
        public void ShowFaction(string factionId)
        {
            _displayedFactionId = factionId;
            gameObject.SetActive(true);

            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null)
            {
                Close();
                return;
            }

            UpdateBasicInfo(faction);
            UpdateResources(faction);
            UpdateStatistics(faction);
            UpdateFinances(faction);
            PopulateCharacterList(faction);
            PopulateTerritoryList(faction);
        }

        /// <summary>
        /// 基本情報を更新
        /// </summary>
        private void UpdateBasicInfo(Faction faction)
        {
            if (_factionNameText != null)
                _factionNameText.text = faction.Name;

            // リーダー名
            var leader = GameManager.Instance?.GetCharacter(faction.LeaderId);
            if (_leaderNameText != null)
                _leaderNameText.text = leader?.Name ?? "なし";

            // TODO: バナーとポートレート画像
        }

        /// <summary>
        /// リソースを更新
        /// </summary>
        private void UpdateResources(Faction faction)
        {
            if (_goldText != null)
                _goldText.text = $"{faction.Gold:N0}";

            if (_foodText != null)
                _foodText.text = $"{faction.Food:N0}";

            if (_stratagemPointsText != null)
                _stratagemPointsText.text = faction.StratagemPoints.ToString();

            // スライダー（将来の上限表示用）
            if (_goldSlider != null)
            {
                _goldSlider.maxValue = 100000;
                _goldSlider.value = Mathf.Min(faction.Gold, _goldSlider.maxValue);
            }

            if (_foodSlider != null)
            {
                _foodSlider.maxValue = 200000;
                _foodSlider.value = Mathf.Min(faction.Food, _foodSlider.maxValue);
            }
        }

        /// <summary>
        /// 統計を更新
        /// </summary>
        private void UpdateStatistics(Faction faction)
        {
            if (_territoryCountText != null)
                _territoryCountText.text = faction.TerritoryIds.Count.ToString();

            // 総人口
            int totalPopulation = 0;
            foreach (var territoryId in faction.TerritoryIds)
            {
                var territory = GameManager.Instance?.GetTerritory(territoryId);
                if (territory != null)
                {
                    totalPopulation += territory.Population;
                }
            }
            if (_totalPopulationText != null)
                _totalPopulationText.text = FormatLargeNumber(totalPopulation);

            // 総兵力
            int totalSoldiers = Battle.ArmyManager.Instance?.GetTotalSoldiers(faction.Id) ?? 0;
            if (_totalSoldiersText != null)
                _totalSoldiersText.text = FormatLargeNumber(totalSoldiers);

            // 武将数
            int characterCount = CountCharacters(faction.Id);
            if (_characterCountText != null)
                _characterCountText.text = characterCount.ToString();

            // 軍団数
            int armyCount = Battle.ArmyManager.Instance?.GetArmiesByFaction(faction.Id)?.Count ?? 0;
            if (_armyCountText != null)
                _armyCountText.text = armyCount.ToString();
        }

        /// <summary>
        /// 財政を更新
        /// </summary>
        private void UpdateFinances(Faction faction)
        {
            // 収入
            int income = ResourceManager.Instance?.CalculateIncome(faction.Id) ?? 0;
            if (_incomeText != null)
                _incomeText.text = $"+{income:N0}";

            // 支出
            int expense = ResourceManager.Instance?.CalculateExpense(faction.Id) ?? 0;
            if (_expenseText != null)
                _expenseText.text = $"-{expense:N0}";

            // 収支
            int balance = income - expense;
            if (_balanceText != null)
            {
                _balanceText.text = balance >= 0 ? $"+{balance:N0}" : $"{balance:N0}";
                _balanceText.color = balance >= 0 ? Color.green : Color.red;
            }

            // 食糧生産
            int foodProduction = ResourceManager.Instance?.CalculateFoodProduction(faction.Id) ?? 0;
            if (_foodProductionText != null)
                _foodProductionText.text = $"+{foodProduction:N0}";

            // 食糧消費
            int foodConsumption = ResourceManager.Instance?.CalculateFoodConsumption(faction.Id) ?? 0;
            if (_foodConsumptionText != null)
                _foodConsumptionText.text = $"-{foodConsumption:N0}";
        }

        #endregion

        #region Lists

        /// <summary>
        /// 武将リストを生成
        /// </summary>
        private void PopulateCharacterList(Faction faction)
        {
            ClearCharacterList();

            if (_characterListContent == null || _characterItemPrefab == null) return;
            if (GameManager.Instance?.GameData == null) return;

            foreach (var character in GameManager.Instance.GameData.Characters.Values)
            {
                if (character.FactionId != faction.Id) continue;

                var item = Instantiate(_characterItemPrefab, _characterListContent);
                SetupCharacterItem(item, character);
                _characterItems.Add(item);
            }
        }

        /// <summary>
        /// 武将アイテムをセットアップ
        /// </summary>
        private void SetupCharacterItem(GameObject item, Character character)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
                texts[0].text = character.Name;

            if (texts.Length > 1)
                texts[1].text = $"武{character.Strength} 知{character.Intelligence} 統{character.Leadership}";

            var button = item.GetComponent<Button>();
            if (button != null)
            {
                string charId = character.Id;
                button.onClick.AddListener(() => OnCharacterSelected?.Invoke(charId));
            }
        }

        /// <summary>
        /// 武将リストをクリア
        /// </summary>
        private void ClearCharacterList()
        {
            foreach (var item in _characterItems)
            {
                Destroy(item);
            }
            _characterItems.Clear();
        }

        /// <summary>
        /// 領地リストを生成
        /// </summary>
        private void PopulateTerritoryList(Faction faction)
        {
            ClearTerritoryList();

            if (_territoryListContent == null || _territoryItemPrefab == null) return;

            foreach (var territoryId in faction.TerritoryIds)
            {
                var territory = GameManager.Instance?.GetTerritory(territoryId);
                if (territory == null) continue;

                var item = Instantiate(_territoryItemPrefab, _territoryListContent);
                SetupTerritoryItem(item, territory);
                _territoryItems.Add(item);
            }
        }

        /// <summary>
        /// 領地アイテムをセットアップ
        /// </summary>
        private void SetupTerritoryItem(GameObject item, Territory territory)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
                texts[0].text = territory.Name;

            if (texts.Length > 1)
                texts[1].text = $"人口: {FormatLargeNumber(territory.Population)}";

            var button = item.GetComponent<Button>();
            if (button != null)
            {
                string terrId = territory.Id;
                button.onClick.AddListener(() => OnTerritorySelected?.Invoke(terrId));
            }
        }

        /// <summary>
        /// 領地リストをクリア
        /// </summary>
        private void ClearTerritoryList()
        {
            foreach (var item in _territoryItems)
            {
                Destroy(item);
            }
            _territoryItems.Clear();
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
        /// 武将数をカウント
        /// </summary>
        private int CountCharacters(string factionId)
        {
            if (GameManager.Instance?.GameData == null) return 0;

            int count = 0;
            foreach (var character in GameManager.Instance.GameData.Characters.Values)
            {
                if (character.FactionId == factionId)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 大きな数値をフォーマット
        /// </summary>
        private string FormatLargeNumber(int value)
        {
            if (value >= 10000)
                return $"{value / 10000f:F1}万";
            else if (value >= 1000)
                return $"{value:N0}";
            else
                return value.ToString();
        }

        /// <summary>
        /// リソース変更時
        /// </summary>
        private void OnResourceChanged(ResourceEventArgs args)
        {
            if (args.FactionId == _displayedFactionId && gameObject.activeSelf)
            {
                var faction = GameManager.Instance?.GetFaction(_displayedFactionId);
                if (faction != null)
                {
                    UpdateResources(faction);
                }
            }
        }

        #endregion
    }
}
