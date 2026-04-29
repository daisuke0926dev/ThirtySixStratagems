using System;
using UnityEngine;

namespace ThirtySixStratagems.Data.Models
{
    /// <summary>
    /// キャラクター（武将）モデル
    /// </summary>
    [Serializable]
    public class Character
    {
        [Header("基本情報")]
        public string Id;
        public string Name;
        public CharacterType Type = CharacterType.General;

        [Header("能力値")]
        [Range(1, 100)]
        public int Strength = 50;      // 武力

        [Range(1, 100)]
        public int Intelligence = 50;   // 知力

        [Range(1, 100)]
        public int Leadership = 50;     // 統率

        [Range(1, 100)]
        public int Politics = 50;       // 政治

        [Range(1, 100)]
        public int Charisma = 50;       // 魅力

        [Header("状態")]
        [Range(0, 100)]
        public int Loyalty = 100;       // 忠誠度

        [Range(0, 100)]
        public int Health = 100;        // 体力

        public bool IsAlive = true;
        public bool IsCaptured;

        [Header("所属")]
        public string FactionId;
        public string LocationTerritoryId;
        public string AssignedArmyId;

        [Header("得意計略")]
        public StratagemCategory SpecialtyCategory = StratagemCategory.Winning;

        [Header("ビジュアル")]
        public string PortraitId;

        /// <summary>
        /// 指揮可能兵力上限
        /// </summary>
        public int MaxCommandableArmy => Leadership * 100;

        /// <summary>
        /// 計略ポイント回復量
        /// </summary>
        public int StratagemPointRecovery => Intelligence / 20;

        /// <summary>
        /// 戦闘力ボーナスを計算
        /// </summary>
        public float CalculateCombatBonus()
        {
            return 1.0f + (Strength / 100f);
        }

        /// <summary>
        /// 計略成功率ボーナスを計算
        /// </summary>
        public int CalculateStratagemBonus(StratagemCategory category)
        {
            int baseBonus = Intelligence / 5;

            // 得意カテゴリなら追加ボーナス
            if (category == SpecialtyCategory)
            {
                baseBonus += 20;
            }

            return baseBonus;
        }

        /// <summary>
        /// 内政効率を計算
        /// </summary>
        public float CalculatePoliticsEfficiency()
        {
            return 1.0f + (Politics / 100f);
        }

        /// <summary>
        /// 登用成功率を計算
        /// </summary>
        public int CalculateRecruitmentChance(Character target)
        {
            int baseChance = 30;
            int charismaBonus = (Charisma - target.Charisma) / 2;
            int loyaltyPenalty = target.Loyalty / 2;

            return Mathf.Clamp(baseChance + charismaBonus - loyaltyPenalty, 5, 95);
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int amount)
        {
            Health = Mathf.Max(0, Health - amount);
            if (Health <= 0)
            {
                IsAlive = false;
            }
        }

        /// <summary>
        /// 回復
        /// </summary>
        public void Heal(int amount)
        {
            if (IsAlive)
            {
                Health = Mathf.Min(100, Health + amount);
            }
        }

        /// <summary>
        /// 忠誠度を変化
        /// </summary>
        public void ModifyLoyalty(int delta)
        {
            Loyalty = Mathf.Clamp(Loyalty + delta, 0, 100);
        }

        /// <summary>
        /// 捕虜状態を設定
        /// </summary>
        public void Capture(string captureFactionId)
        {
            IsCaptured = true;
            FactionId = captureFactionId;
            Loyalty = 0;
            AssignedArmyId = null;
        }

        /// <summary>
        /// 解放
        /// </summary>
        public void Release(string newFactionId)
        {
            IsCaptured = false;
            FactionId = newFactionId;
        }
    }
}
