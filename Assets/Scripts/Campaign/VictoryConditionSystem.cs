using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Campaign
{
    /// <summary>
    /// 勝利条件システム
    /// 勝利・敗北条件の詳細な判定を担当
    /// </summary>
    public class VictoryConditionSystem : MonoBehaviour
    {
        public static VictoryConditionSystem Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private bool _checkEveryTurn = true;
        [SerializeField] private float _checkInterval = 1f;

        // イベント
        public event Action<VictoryInfo> OnVictoryAchieved;
        public event Action<DefeatInfo> OnDefeatOccurred;
        public event Action<float> OnVictoryProgressChanged;

        // 状態
        private float _lastCheckTime;
        private Dictionary<string, IVictoryCondition> _customConditions = new Dictionary<string, IVictoryCondition>();

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
            EventBus.OnTurnEnded += OnTurnEnded;
        }

        private void OnDisable()
        {
            EventBus.OnTurnEnded -= OnTurnEnded;
        }

        #region Victory Checking

        /// <summary>
        /// 勝利条件をチェック
        /// </summary>
        public VictoryCheckResult CheckVictoryConditions(string factionId)
        {
            var campaign = CampaignManager.Instance?.CurrentCampaign;
            if (campaign == null || factionId != campaign.PlayerFactionId)
            {
                return new VictoryCheckResult { Status = VictoryStatus.InProgress };
            }

            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null)
            {
                return new VictoryCheckResult { Status = VictoryStatus.InProgress };
            }

            // 勝利条件チェック
            var victoryResult = CheckVictory(campaign, faction);
            if (victoryResult.IsAchieved)
            {
                OnVictoryAchieved?.Invoke(victoryResult);
                return new VictoryCheckResult
                {
                    Status = VictoryStatus.Victory,
                    VictoryInfo = victoryResult
                };
            }

            // 敗北条件チェック
            var defeatResult = CheckDefeat(campaign, faction);
            if (defeatResult.IsDefeated)
            {
                OnDefeatOccurred?.Invoke(defeatResult);
                return new VictoryCheckResult
                {
                    Status = VictoryStatus.Defeat,
                    DefeatInfo = defeatResult
                };
            }

            // 進捗更新
            float progress = CalculateVictoryProgress(campaign, faction);
            OnVictoryProgressChanged?.Invoke(progress);

            return new VictoryCheckResult
            {
                Status = VictoryStatus.InProgress,
                Progress = progress
            };
        }

        /// <summary>
        /// 勝利チェック
        /// </summary>
        private VictoryInfo CheckVictory(CampaignState campaign, Faction faction)
        {
            var info = new VictoryInfo();

            switch (campaign.VictoryCondition)
            {
                case VictoryConditionType.Conquest:
                    info = CheckConquestVictory(faction);
                    break;

                case VictoryConditionType.TerritoryCount:
                    info = CheckTerritoryCountVictory(faction, campaign.VictoryTargetValue);
                    break;

                case VictoryConditionType.SurviveYears:
                    info = CheckSurvivalVictory(campaign);
                    break;

                case VictoryConditionType.DefeatFaction:
                    info = CheckDefeatFactionVictory(faction);
                    break;

                case VictoryConditionType.Alliance:
                    info = CheckAllianceVictory(faction);
                    break;
            }

            // カスタム条件チェック
            foreach (var condition in _customConditions.Values)
            {
                if (condition.IsAchieved(faction))
                {
                    info.IsAchieved = true;
                    info.VictoryType = VictoryType.Special;
                    info.Description = condition.Description;
                    break;
                }
            }

            return info;
        }

        /// <summary>
        /// 敗北チェック
        /// </summary>
        private DefeatInfo CheckDefeat(CampaignState campaign, Faction faction)
        {
            var info = new DefeatInfo();

            // 領地全喪失
            if (faction.TerritoryIds.Count == 0)
            {
                info.IsDefeated = true;
                info.DefeatType = DefeatType.TerritoryLost;
                info.Description = "全ての領地を失いました";
                return info;
            }

            // リーダー死亡/捕縛
            var leader = GameManager.Instance?.GetCharacter(faction.LeaderId);
            if (leader == null || leader.FactionId != faction.Id)
            {
                info.IsDefeated = true;
                info.DefeatType = DefeatType.LeaderLost;
                info.Description = "君主を失いました";
                return info;
            }

            // タイムアウト（生存条件以外）
            if (campaign.VictoryCondition != VictoryConditionType.SurviveYears)
            {
                if (campaign.CurrentTurn > 100) // 最大ターン数
                {
                    var mainObjective = campaign.Objectives.Find(o => o.ObjectiveId == "main_victory");
                    if (mainObjective != null && mainObjective.Status != ObjectiveStatus.Completed)
                    {
                        info.IsDefeated = true;
                        info.DefeatType = DefeatType.Timeout;
                        info.Description = "目標を達成できませんでした";
                        return info;
                    }
                }
            }

            return info;
        }

        #endregion

        #region Specific Victory Checks

        private VictoryInfo CheckConquestVictory(Faction faction)
        {
            var info = new VictoryInfo();
            int totalTerritories = GameManager.Instance?.GameData?.Territories.Count ?? 0;

            if (faction.TerritoryIds.Count >= totalTerritories)
            {
                info.IsAchieved = true;
                info.VictoryType = VictoryType.Conquest;
                info.Description = "天下を統一しました！";
            }

            return info;
        }

        private VictoryInfo CheckTerritoryCountVictory(Faction faction, int targetCount)
        {
            var info = new VictoryInfo();

            if (faction.TerritoryIds.Count >= targetCount)
            {
                info.IsAchieved = true;
                info.VictoryType = VictoryType.Domination;
                info.Description = $"{targetCount}つの領地を支配しました！";
            }

            return info;
        }

        private VictoryInfo CheckSurvivalVictory(CampaignState campaign)
        {
            var info = new VictoryInfo();
            int yearsElapsed = GameManager.Instance?.CurrentYear - campaign.StartYear ?? 0;

            if (yearsElapsed >= campaign.VictoryTargetValue)
            {
                info.IsAchieved = true;
                info.VictoryType = VictoryType.Survival;
                info.Description = $"{campaign.VictoryTargetValue}年間生き延びました！";
            }

            return info;
        }

        private VictoryInfo CheckDefeatFactionVictory(Faction faction)
        {
            var info = new VictoryInfo();

            // 他の全勢力が滅亡したかチェック
            bool allDefeated = true;
            foreach (var f in GameManager.Instance?.GameData?.Factions.Values ?? new List<Faction>())
            {
                if (f.Id != faction.Id && f.TerritoryIds.Count > 0)
                {
                    allDefeated = false;
                    break;
                }
            }

            if (allDefeated)
            {
                info.IsAchieved = true;
                info.VictoryType = VictoryType.Conquest;
                info.Description = "全ての敵を打ち倒しました！";
            }

            return info;
        }

        private VictoryInfo CheckAllianceVictory(Faction faction)
        {
            var info = new VictoryInfo();

            // TODO: 同盟システムとの連携
            // 全勢力と同盟を結んでいるかチェック

            return info;
        }

        #endregion

        #region Progress Calculation

        /// <summary>
        /// 勝利進捗を計算
        /// </summary>
        private float CalculateVictoryProgress(CampaignState campaign, Faction faction)
        {
            switch (campaign.VictoryCondition)
            {
                case VictoryConditionType.Conquest:
                    int totalTerritories = GameManager.Instance?.GameData?.Territories.Count ?? 1;
                    return (float)faction.TerritoryIds.Count / totalTerritories;

                case VictoryConditionType.TerritoryCount:
                    return (float)faction.TerritoryIds.Count / campaign.VictoryTargetValue;

                case VictoryConditionType.SurviveYears:
                    int yearsElapsed = GameManager.Instance?.CurrentYear - campaign.StartYear ?? 0;
                    return (float)yearsElapsed / campaign.VictoryTargetValue;

                case VictoryConditionType.DefeatFaction:
                    int totalFactions = GameManager.Instance?.GameData?.Factions.Count ?? 1;
                    int defeatedFactions = 0;
                    foreach (var f in GameManager.Instance?.GameData?.Factions.Values ?? new List<Faction>())
                    {
                        if (f.Id != faction.Id && f.TerritoryIds.Count == 0)
                        {
                            defeatedFactions++;
                        }
                    }
                    return (float)defeatedFactions / (totalFactions - 1);

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 勝利に必要な残りを取得
        /// </summary>
        public string GetRemainingRequirement(string factionId)
        {
            var campaign = CampaignManager.Instance?.CurrentCampaign;
            var faction = GameManager.Instance?.GetFaction(factionId);

            if (campaign == null || faction == null)
                return "";

            switch (campaign.VictoryCondition)
            {
                case VictoryConditionType.Conquest:
                    int total = GameManager.Instance.GameData.Territories.Count;
                    int remaining = total - faction.TerritoryIds.Count;
                    return $"残り{remaining}領地";

                case VictoryConditionType.TerritoryCount:
                    int needed = campaign.VictoryTargetValue - faction.TerritoryIds.Count;
                    return needed > 0 ? $"残り{needed}領地" : "達成！";

                case VictoryConditionType.SurviveYears:
                    int yearsLeft = campaign.VictoryTargetValue - (GameManager.Instance.CurrentYear - campaign.StartYear);
                    return yearsLeft > 0 ? $"残り{yearsLeft}年" : "達成！";

                default:
                    return "";
            }
        }

        #endregion

        #region Custom Conditions

        /// <summary>
        /// カスタム勝利条件を登録
        /// </summary>
        public void RegisterCustomCondition(string id, IVictoryCondition condition)
        {
            _customConditions[id] = condition;
        }

        /// <summary>
        /// カスタム勝利条件を解除
        /// </summary>
        public void UnregisterCustomCondition(string id)
        {
            _customConditions.Remove(id);
        }

        #endregion

        #region Event Handlers

        private void OnTurnEnded(int turnNumber)
        {
            if (!_checkEveryTurn) return;

            var campaign = CampaignManager.Instance?.CurrentCampaign;
            if (campaign != null && campaign.Status == CampaignStatus.InProgress)
            {
                CheckVictoryConditions(campaign.PlayerFactionId);
            }
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// 勝利チェック結果
    /// </summary>
    public class VictoryCheckResult
    {
        public VictoryStatus Status;
        public float Progress;
        public VictoryInfo VictoryInfo;
        public DefeatInfo DefeatInfo;
    }

    /// <summary>
    /// 勝利ステータス
    /// </summary>
    public enum VictoryStatus
    {
        InProgress,
        Victory,
        Defeat
    }

    /// <summary>
    /// 勝利情報
    /// </summary>
    public class VictoryInfo
    {
        public bool IsAchieved;
        public VictoryType VictoryType;
        public string Description;
        public int FinalScore;
    }

    /// <summary>
    /// 勝利タイプ
    /// </summary>
    public enum VictoryType
    {
        Conquest,
        Domination,
        Survival,
        Diplomatic,
        Special
    }

    /// <summary>
    /// 敗北情報
    /// </summary>
    public class DefeatInfo
    {
        public bool IsDefeated;
        public DefeatType DefeatType;
        public string Description;
    }

    /// <summary>
    /// 敗北タイプ
    /// </summary>
    public enum DefeatType
    {
        TerritoryLost,
        LeaderLost,
        Timeout,
        Surrender
    }

    /// <summary>
    /// カスタム勝利条件インターフェース
    /// </summary>
    public interface IVictoryCondition
    {
        string Id { get; }
        string Description { get; }
        bool IsAchieved(Faction faction);
    }

    #endregion
}
