using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.Models
{
    /// <summary>
    /// 勢力モデル
    /// </summary>
    [Serializable]
    public class Faction
    {
        [Header("基本情報")]
        public string Id;
        public string Name;
        public Color FactionColor = Color.red;

        [Header("君主")]
        public string RulerId;

        [Header("リソース")]
        public int Gold = 1000;
        public int Food = 500;

        [Header("計略ポイント")]
        public int StratagemPoints = 5;
        public int MaxStratagemPoints = 10;

        [Header("所有")]
        public List<string> TerritoryIds = new List<string>();
        public List<string> CharacterIds = new List<string>();
        public List<string> ArmyIds = new List<string>();

        [Header("外交")]
        public List<Alliance> Alliances = new List<Alliance>();
        public Dictionary<string, int> Relations = new Dictionary<string, int>();

        [Header("解放済み計略")]
        public List<string> UnlockedStratagemIds = new List<string>();

        [Header("プレイヤー設定")]
        public bool IsPlayer;
        public AIPersonality AiPersonality = AIPersonality.Balanced;

        /// <summary>
        /// 勢力の総兵力を計算
        /// </summary>
        public int CalculateTotalSoldiers(List<Army> armies)
        {
            int total = 0;
            foreach (var army in armies)
            {
                if (army.OwnerId == Id)
                {
                    total += army.Soldiers;
                }
            }
            return total;
        }

        /// <summary>
        /// 計略ポイントを回復
        /// </summary>
        public void RecoverStratagemPoints(int amount)
        {
            StratagemPoints = Mathf.Min(StratagemPoints + amount, MaxStratagemPoints);
        }

        /// <summary>
        /// 計略ポイントを消費
        /// </summary>
        public bool ConsumeStratagemPoints(int amount)
        {
            if (StratagemPoints >= amount)
            {
                StratagemPoints -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 勢力間の関係値を取得
        /// </summary>
        public int GetRelation(string factionId)
        {
            if (Relations.TryGetValue(factionId, out int relation))
            {
                return relation;
            }
            return 0;
        }

        /// <summary>
        /// 勢力間の関係値を変更
        /// </summary>
        public void ModifyRelation(string factionId, int delta)
        {
            if (!Relations.ContainsKey(factionId))
            {
                Relations[factionId] = 0;
            }
            Relations[factionId] = Mathf.Clamp(Relations[factionId] + delta, -100, 100);
        }

        /// <summary>
        /// 外交状態を取得
        /// </summary>
        public DiplomaticStatus GetDiplomaticStatus(string factionId)
        {
            // 同盟チェック
            foreach (var alliance in Alliances)
            {
                if (alliance.FactionId == factionId && alliance.IsActive)
                {
                    return DiplomaticStatus.Alliance;
                }
            }

            // 関係値から判定
            int relation = GetRelation(factionId);
            if (relation <= -75) return DiplomaticStatus.War;
            if (relation <= -25) return DiplomaticStatus.Hostile;
            if (relation >= 75) return DiplomaticStatus.Friendly;
            return DiplomaticStatus.Neutral;
        }

        /// <summary>
        /// 計略が解放されているか確認
        /// </summary>
        public bool IsStratagemUnlocked(string stratagemId)
        {
            return UnlockedStratagemIds.Contains(stratagemId);
        }

        /// <summary>
        /// 計略を解放
        /// </summary>
        public void UnlockStratagem(string stratagemId)
        {
            if (!UnlockedStratagemIds.Contains(stratagemId))
            {
                UnlockedStratagemIds.Add(stratagemId);
            }
        }
    }

    /// <summary>
    /// 同盟
    /// </summary>
    [Serializable]
    public class Alliance
    {
        public string FactionId;
        public int FormedTurn;
        public int Duration; // 0 = 無期限
        public bool IsActive = true;

        /// <summary>
        /// 同盟が有効期限内か確認
        /// </summary>
        public bool IsValid(int currentTurn)
        {
            if (!IsActive) return false;
            if (Duration <= 0) return true;
            return currentTurn < FormedTurn + Duration;
        }
    }
}
