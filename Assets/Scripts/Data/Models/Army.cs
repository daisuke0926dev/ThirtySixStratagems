using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Data.Models
{
    /// <summary>
    /// 軍隊モデル
    /// </summary>
    [Serializable]
    public class Army
    {
        [Header("基本情報")]
        public string Id;
        public string OwnerId;

        [Header("兵力")]
        [Min(0)]
        public int Soldiers;

        [Header("指揮官")]
        public string CommanderId;
        public List<string> OfficerIds = new List<string>();

        [Header("位置")]
        public string LocationTerritoryId;
        public string TargetTerritoryId;

        [Header("状態")]
        public ArmyState State = ArmyState.Idle;

        [Range(0, 100)]
        public int Morale = 100;

        [Min(0)]
        public int Supplies = 100;

        [Header("移動")]
        public List<string> MovementPath = new List<string>();
        public int MovementProgress;

        [Header("効果")]
        public List<ActiveEffect> ActiveEffects = new List<ActiveEffect>();

        /// <summary>
        /// 戦闘力を計算
        /// </summary>
        public int CalculateCombatPower(Character commander)
        {
            float commanderBonus = commander != null ? commander.CalculateCombatBonus() : 1.0f;
            float moraleBonus = Morale / 100f;
            float supplyPenalty = Supplies > 0 ? 1.0f : 0.5f;

            int basePower = Soldiers;
            return Mathf.RoundToInt(basePower * commanderBonus * moraleBonus * supplyPenalty);
        }

        /// <summary>
        /// 兵力を増減
        /// </summary>
        public void ModifySoldiers(int delta)
        {
            Soldiers = Mathf.Max(0, Soldiers + delta);
        }

        /// <summary>
        /// 士気を変化
        /// </summary>
        public void ModifyMorale(int delta)
        {
            Morale = Mathf.Clamp(Morale + delta, 0, 100);
        }

        /// <summary>
        /// 兵糧を消費
        /// </summary>
        public void ConsumeSupplies(int amount)
        {
            Supplies = Mathf.Max(0, Supplies - amount);

            // 兵糧切れで士気低下
            if (Supplies <= 0)
            {
                ModifyMorale(-10);
            }
        }

        /// <summary>
        /// 兵糧を補給
        /// </summary>
        public void Resupply(int amount)
        {
            Supplies += amount;
        }

        /// <summary>
        /// 移動を開始
        /// </summary>
        public void StartMovement(List<string> path, string target)
        {
            MovementPath = new List<string>(path);
            TargetTerritoryId = target;
            MovementProgress = 0;
            State = ArmyState.Moving;
        }

        /// <summary>
        /// 移動を進める
        /// </summary>
        public bool AdvanceMovement()
        {
            if (State != ArmyState.Moving || MovementPath.Count == 0)
            {
                return false;
            }

            MovementProgress++;
            if (MovementProgress >= MovementPath.Count)
            {
                LocationTerritoryId = TargetTerritoryId;
                MovementPath.Clear();
                TargetTerritoryId = null;
                MovementProgress = 0;
                State = ArmyState.Idle;
                return true;
            }

            LocationTerritoryId = MovementPath[MovementProgress];
            return false;
        }

        /// <summary>
        /// 撤退
        /// </summary>
        public void Retreat(string retreatToTerritoryId)
        {
            State = ArmyState.Retreating;
            LocationTerritoryId = retreatToTerritoryId;
            MovementPath.Clear();
            TargetTerritoryId = null;
            ModifyMorale(-20);
        }

        /// <summary>
        /// アクティブな効果を追加
        /// </summary>
        public void AddEffect(ActiveEffect effect)
        {
            ActiveEffects.Add(effect);
        }

        /// <summary>
        /// 効果を更新（ターン終了時）
        /// </summary>
        public void UpdateEffects()
        {
            for (int i = ActiveEffects.Count - 1; i >= 0; i--)
            {
                ActiveEffects[i].RemainingTurns--;
                if (ActiveEffects[i].RemainingTurns <= 0)
                {
                    ActiveEffects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 特定の効果が有効か確認
        /// </summary>
        public bool HasEffect(StratagemEffectType effectType)
        {
            foreach (var effect in ActiveEffects)
            {
                if (effect.EffectType == effectType && effect.RemainingTurns > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 軍を合流
        /// </summary>
        public void Merge(Army other)
        {
            Soldiers += other.Soldiers;
            Supplies += other.Supplies;

            // 士気は平均
            Morale = (Morale + other.Morale) / 2;

            // 副将を追加
            foreach (var officerId in other.OfficerIds)
            {
                if (!OfficerIds.Contains(officerId))
                {
                    OfficerIds.Add(officerId);
                }
            }
        }

        /// <summary>
        /// 軍を分割
        /// </summary>
        public Army Split(int soldierCount, string newArmyId)
        {
            if (soldierCount >= Soldiers || soldierCount <= 0)
            {
                return null;
            }

            Soldiers -= soldierCount;

            var newArmy = new Army
            {
                Id = newArmyId,
                OwnerId = OwnerId,
                Soldiers = soldierCount,
                LocationTerritoryId = LocationTerritoryId,
                Morale = Morale,
                Supplies = Supplies / 2,
                State = ArmyState.Idle
            };

            Supplies = Supplies / 2;

            return newArmy;
        }
    }

    /// <summary>
    /// アクティブな効果
    /// </summary>
    [Serializable]
    public class ActiveEffect
    {
        public string SourceStratagemId;
        public StratagemEffectType EffectType;
        public int EffectValue;
        public int RemainingTurns;
    }
}
