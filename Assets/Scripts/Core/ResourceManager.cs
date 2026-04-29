using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Core
{
    /// <summary>
    /// リソース管理システム
    /// 金、食料、計略ポイント、兵力を管理
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        // イベント
        public event Action<string, ResourceType, int, int> OnResourceChanged;
        public event Action<string> OnInsufficientResources;

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
        }

        private void OnEnable()
        {
            // ターン開始時の収入処理を登録
            EventBus.OnTurnStarted += ProcessTurnIncome;
            EventBus.OnTurnEnded += ProcessTurnExpenses;
        }

        private void OnDisable()
        {
            EventBus.OnTurnStarted -= ProcessTurnIncome;
            EventBus.OnTurnEnded -= ProcessTurnExpenses;
        }

        #region Resource Operations

        /// <summary>
        /// リソースを追加
        /// </summary>
        public bool AddResource(string factionId, ResourceType type, int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot add negative resource amount: {amount}");
                return false;
            }

            var faction = GetFaction(factionId);
            if (faction == null) return false;

            int previousValue = GetResourceValue(faction, type);
            SetResourceValue(faction, type, previousValue + amount);
            int newValue = GetResourceValue(faction, type);

            NotifyResourceChange(factionId, type, previousValue, newValue);

            Debug.Log($"[Resource] {factionId}: {type} +{amount} ({previousValue} -> {newValue})");
            return true;
        }

        /// <summary>
        /// リソースを消費
        /// </summary>
        public bool ConsumeResource(string factionId, ResourceType type, int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"Cannot consume negative resource amount: {amount}");
                return false;
            }

            var faction = GetFaction(factionId);
            if (faction == null) return false;

            int currentValue = GetResourceValue(faction, type);

            if (currentValue < amount)
            {
                Debug.Log($"[Resource] Insufficient {type}: {currentValue} < {amount}");
                OnInsufficientResources?.Invoke(factionId);
                return false;
            }

            int newValue = currentValue - amount;
            SetResourceValue(faction, type, newValue);

            NotifyResourceChange(factionId, type, currentValue, newValue);

            Debug.Log($"[Resource] {factionId}: {type} -{amount} ({currentValue} -> {newValue})");
            return true;
        }

        /// <summary>
        /// リソースが足りるか確認
        /// </summary>
        public bool HasEnoughResource(string factionId, ResourceType type, int amount)
        {
            var faction = GetFaction(factionId);
            if (faction == null) return false;

            return GetResourceValue(faction, type) >= amount;
        }

        /// <summary>
        /// 複数リソースが足りるか確認
        /// </summary>
        public bool HasEnoughResources(string factionId, Dictionary<ResourceType, int> costs)
        {
            foreach (var cost in costs)
            {
                if (!HasEnoughResource(factionId, cost.Key, cost.Value))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 複数リソースを一括消費
        /// </summary>
        public bool ConsumeResources(string factionId, Dictionary<ResourceType, int> costs)
        {
            // まず全てのリソースが足りるか確認
            if (!HasEnoughResources(factionId, costs))
            {
                return false;
            }

            // 一括消費
            foreach (var cost in costs)
            {
                ConsumeResource(factionId, cost.Key, cost.Value);
            }

            return true;
        }

        /// <summary>
        /// リソース量を取得
        /// </summary>
        public int GetResource(string factionId, ResourceType type)
        {
            var faction = GetFaction(factionId);
            if (faction == null) return 0;

            return GetResourceValue(faction, type);
        }

        /// <summary>
        /// リソースを設定（直接設定、主にロード用）
        /// </summary>
        public void SetResource(string factionId, ResourceType type, int value)
        {
            var faction = GetFaction(factionId);
            if (faction == null) return;

            int previousValue = GetResourceValue(faction, type);
            SetResourceValue(faction, type, Math.Max(0, value));
            int newValue = GetResourceValue(faction, type);

            if (previousValue != newValue)
            {
                NotifyResourceChange(factionId, type, previousValue, newValue);
            }
        }

        #endregion

        #region Income/Expense Processing

        /// <summary>
        /// ターン収入を処理
        /// </summary>
        private void ProcessTurnIncome(int turn)
        {
            if (GameManager.Instance?.GameData == null) return;

            Debug.Log($"=== Processing turn {turn} income ===");

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                ProcessFactionIncome(faction);
            }
        }

        /// <summary>
        /// 勢力の収入を処理
        /// </summary>
        private void ProcessFactionIncome(Faction faction)
        {
            if (faction == null) return;

            int goldIncome = CalculateGoldIncome(faction);
            int foodIncome = CalculateFoodIncome(faction);
            int spRecovery = CalculateSPRecovery(faction);

            // 収入を追加
            AddResource(faction.Id, ResourceType.Gold, goldIncome);
            AddResource(faction.Id, ResourceType.Food, foodIncome);

            // 計略ポイントは上限あり
            int currentSP = faction.StratagemPoints;
            int maxSP = Constants.Balance.DefaultMaxStratagemPoints;
            int newSP = Math.Min(currentSP + spRecovery, maxSP);
            if (newSP > currentSP)
            {
                AddResource(faction.Id, ResourceType.StratagemPoints, newSP - currentSP);
            }

            Debug.Log($"[Income] {faction.Id}: Gold +{goldIncome}, Food +{foodIncome}, SP +{spRecovery}");
        }

        /// <summary>
        /// ターン支出を処理
        /// </summary>
        private void ProcessTurnExpenses(int turn)
        {
            if (GameManager.Instance?.GameData == null) return;

            Debug.Log($"=== Processing turn {turn} expenses ===");

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                ProcessFactionExpenses(faction);
            }
        }

        /// <summary>
        /// 勢力の支出を処理
        /// </summary>
        private void ProcessFactionExpenses(Faction faction)
        {
            if (faction == null) return;

            // 兵糧消費
            int foodConsumption = CalculateFoodConsumption(faction);

            if (!ConsumeResource(faction.Id, ResourceType.Food, foodConsumption))
            {
                // 食料不足時のペナルティ
                ProcessFoodShortage(faction);
            }
        }

        /// <summary>
        /// 食料不足時のペナルティ処理
        /// </summary>
        private void ProcessFoodShortage(Faction faction)
        {
            Debug.LogWarning($"[Resource] {faction.Id}: Food shortage!");

            // 全軍の士気低下
            if (GameManager.Instance?.GameData == null) return;

            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId == faction.Id)
                {
                    army.ReduceMorale(Constants.Balance.MoraleLossNoSupply);
                }
            }

            // 食料を0に
            SetResource(faction.Id, ResourceType.Food, 0);
        }

        #endregion

        #region Income Calculations

        /// <summary>
        /// 金収入を計算
        /// </summary>
        public int CalculateGoldIncome(Faction faction)
        {
            if (faction == null || GameManager.Instance?.GameData == null) return 0;

            int totalIncome = 0;

            foreach (var territoryId in faction.TerritoryIds)
            {
                if (GameManager.Instance.GameData.Territories.TryGetValue(territoryId, out var territory))
                {
                    totalIncome += CalculateTerritoryGoldIncome(territory);
                }
            }

            return totalIncome;
        }

        /// <summary>
        /// 領地の金収入を計算
        /// </summary>
        public int CalculateTerritoryGoldIncome(Territory territory)
        {
            if (territory == null) return 0;

            // 基本収入 = 経済力 × 基本係数
            int baseIncome = territory.Economy * Constants.Balance.BaseIncomePerEconomy;

            // 人口ボーナス
            int populationBonus = territory.Population / Constants.Balance.PopulationIncomeBonus;

            return baseIncome + populationBonus;
        }

        /// <summary>
        /// 食料収入を計算
        /// </summary>
        public int CalculateFoodIncome(Faction faction)
        {
            if (faction == null || GameManager.Instance?.GameData == null) return 0;

            int totalFood = 0;

            foreach (var territoryId in faction.TerritoryIds)
            {
                if (GameManager.Instance.GameData.Territories.TryGetValue(territoryId, out var territory))
                {
                    totalFood += CalculateTerritoryFoodProduction(territory);
                }
            }

            return totalFood;
        }

        /// <summary>
        /// 領地の食料生産を計算
        /// </summary>
        public int CalculateTerritoryFoodProduction(Territory territory)
        {
            if (territory == null) return 0;

            // 農業建物などがあれば加算
            // 基本は人口に比例
            return territory.Population / 100;
        }

        /// <summary>
        /// 計略ポイント回復量を計算
        /// </summary>
        public int CalculateSPRecovery(Faction faction)
        {
            if (faction == null) return 0;

            int baseSP = Constants.Balance.StratagemPointRecoveryBase;

            // 高知力武将がいればボーナス
            int intelligenceBonus = CalculateIntelligenceBonus(faction);

            return baseSP + intelligenceBonus;
        }

        /// <summary>
        /// 知力ボーナスを計算
        /// </summary>
        private int CalculateIntelligenceBonus(Faction faction)
        {
            if (GameManager.Instance?.GameData == null) return 0;

            int maxIntelligence = 0;

            foreach (var character in GameManager.Instance.GameData.Characters.Values)
            {
                if (character.FactionId == faction.Id && character.Intelligence > maxIntelligence)
                {
                    maxIntelligence = character.Intelligence;
                }
            }

            // 知力90以上で+1、100で+2
            if (maxIntelligence >= 100) return 2;
            if (maxIntelligence >= 90) return 1;
            return 0;
        }

        /// <summary>
        /// 兵糧消費を計算
        /// </summary>
        public int CalculateFoodConsumption(Faction faction)
        {
            if (faction == null || GameManager.Instance?.GameData == null) return 0;

            int totalSoldiers = 0;

            // 全軍の兵力を合計
            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId == faction.Id)
                {
                    totalSoldiers += army.SoldierCount;
                }
            }

            return totalSoldiers * Constants.Balance.FoodConsumptionPerSoldier;
        }

        #endregion

        #region Recruitment

        /// <summary>
        /// 徴兵可能な兵数を計算
        /// </summary>
        public int CalculateMaxRecruitment(string territoryId)
        {
            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory == null) return 0;

            // 人口の一定割合まで徴兵可能
            return territory.Population / Constants.Balance.RecruitmentPopulationRatio;
        }

        /// <summary>
        /// 徴兵コストを計算
        /// </summary>
        public int CalculateRecruitmentCost(int soldierCount)
        {
            return soldierCount * Constants.Balance.RecruitmentCostPerSoldier;
        }

        /// <summary>
        /// 徴兵を実行
        /// </summary>
        public bool Recruit(string factionId, string territoryId, int soldierCount)
        {
            if (soldierCount <= 0) return false;

            // 最大徴兵数チェック
            int maxRecruitment = CalculateMaxRecruitment(territoryId);
            if (soldierCount > maxRecruitment)
            {
                Debug.Log($"Cannot recruit {soldierCount} soldiers. Max: {maxRecruitment}");
                return false;
            }

            // コスト計算
            int cost = CalculateRecruitmentCost(soldierCount);

            // 金が足りるか
            if (!HasEnoughResource(factionId, ResourceType.Gold, cost))
            {
                Debug.Log($"Not enough gold to recruit. Cost: {cost}");
                return false;
            }

            // 金を消費
            if (!ConsumeResource(factionId, ResourceType.Gold, cost))
            {
                return false;
            }

            // 領地の人口を減少
            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory != null)
            {
                territory.Population -= soldierCount;
            }

            // 兵力を追加（実際の軍への追加は呼び出し側で行う）
            Debug.Log($"[Recruit] {factionId} recruited {soldierCount} soldiers from {territoryId}");

            return true;
        }

        #endregion

        #region Transfer

        /// <summary>
        /// 勢力間でリソースを譲渡
        /// </summary>
        public bool TransferResource(string fromFactionId, string toFactionId, ResourceType type, int amount)
        {
            if (amount <= 0) return false;

            if (!HasEnoughResource(fromFactionId, type, amount))
            {
                return false;
            }

            if (ConsumeResource(fromFactionId, type, amount))
            {
                AddResource(toFactionId, type, amount);
                Debug.Log($"[Transfer] {fromFactionId} -> {toFactionId}: {type} {amount}");
                return true;
            }

            return false;
        }

        #endregion

        #region Helper Methods

        private Faction GetFaction(string factionId)
        {
            return GameManager.Instance?.GetFaction(factionId);
        }

        private int GetResourceValue(Faction faction, ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Gold:
                    return faction.Gold;
                case ResourceType.Food:
                    return faction.Food;
                case ResourceType.StratagemPoints:
                    return faction.StratagemPoints;
                case ResourceType.Soldiers:
                    return GetTotalSoldiers(faction.Id);
                default:
                    return 0;
            }
        }

        private void SetResourceValue(Faction faction, ResourceType type, int value)
        {
            switch (type)
            {
                case ResourceType.Gold:
                    faction.Gold = value;
                    break;
                case ResourceType.Food:
                    faction.Food = value;
                    break;
                case ResourceType.StratagemPoints:
                    faction.StratagemPoints = Math.Min(value, Constants.Balance.DefaultMaxStratagemPoints);
                    break;
                case ResourceType.Soldiers:
                    // 兵力は直接設定不可
                    Debug.LogWarning("Cannot directly set soldier count");
                    break;
            }
        }

        private int GetTotalSoldiers(string factionId)
        {
            if (GameManager.Instance?.GameData == null) return 0;

            int total = 0;
            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId == factionId)
                {
                    total += army.SoldierCount;
                }
            }
            return total;
        }

        private void NotifyResourceChange(string factionId, ResourceType type, int previousValue, int newValue)
        {
            OnResourceChanged?.Invoke(factionId, type, previousValue, newValue);

            var args = new ResourceEventArgs
            {
                FactionId = factionId,
                ResourceType = type,
                PreviousValue = previousValue,
                NewValue = newValue,
                Delta = newValue - previousValue
            };
            EventBus.ResourceChanged(args);
        }

        #endregion
    }
}
