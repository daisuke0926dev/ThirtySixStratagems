using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Stratagem
{
    /// <summary>
    /// 計略効果処理システム
    /// 各計略効果の具体的な処理を担当
    /// </summary>
    public class StratagemEffectProcessor : MonoBehaviour
    {
        public static StratagemEffectProcessor Instance { get; private set; }

        // アクティブな効果のリスト
        private List<ActiveStratagemEffect> _activeEffects = new List<ActiveStratagemEffect>();

        // イベント
        public event Action<ActiveStratagemEffect> OnEffectApplied;
        public event Action<ActiveStratagemEffect> OnEffectExpired;

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
            EventBus.OnTurnEnded += ProcessTurnEndEffects;
        }

        private void OnDisable()
        {
            EventBus.OnTurnEnded -= ProcessTurnEndEffects;
        }

        #region Effect Processing

        /// <summary>
        /// 効果を処理
        /// </summary>
        public StratagemEffectResult ProcessEffect(StratagemEffectType effectType, int value,
            int duration, string casterFactionId, string targetId)
        {
            var result = new StratagemEffectResult
            {
                EffectType = effectType,
                TargetId = targetId,
                Applied = false
            };

            switch (effectType)
            {
                case StratagemEffectType.StealthMovement:
                    result = ApplyStealthMovement(casterFactionId, duration);
                    break;

                case StratagemEffectType.ForceRetreat:
                    result = ApplyForceRetreat(targetId, value);
                    break;

                case StratagemEffectType.FactionConflict:
                    result = ApplyFactionConflict(targetId, casterFactionId, duration);
                    break;

                case StratagemEffectType.DefenseBoost:
                    result = ApplyDefenseBoost(casterFactionId, value, duration);
                    break;

                case StratagemEffectType.AttackBoost:
                    result = ApplyAttackBoost(casterFactionId, value, duration);
                    break;

                case StratagemEffectType.Ambush:
                    result = ApplyAmbush(targetId, value);
                    break;

                case StratagemEffectType.Disinformation:
                    result = ApplyDisinformation(targetId, duration);
                    break;

                case StratagemEffectType.Diplomacy:
                    result = ApplyDiplomacy(casterFactionId, targetId, value);
                    break;

                case StratagemEffectType.ResourcePlunder:
                    result = ApplyResourcePlunder(casterFactionId, targetId, value);
                    break;

                case StratagemEffectType.Reconnaissance:
                    result = ApplyReconnaissance(targetId, duration);
                    break;

                case StratagemEffectType.TerritoryControl:
                    result = ApplyTerritoryControl(targetId, value, duration);
                    break;

                case StratagemEffectType.LoyaltyReduce:
                    result = ApplyLoyaltyReduce(targetId, value);
                    break;

                case StratagemEffectType.CharacterCapture:
                    result = ApplyCharacterCapture(casterFactionId, targetId, value);
                    break;

                case StratagemEffectType.SupplyDisrupt:
                    result = ApplySupplyDisrupt(targetId, duration);
                    break;

                case StratagemEffectType.Escape:
                    result = ApplyEscape(casterFactionId);
                    break;

                case StratagemEffectType.InternalStrife:
                    result = ApplyInternalStrife(targetId, value, duration);
                    break;

                case StratagemEffectType.MoraleReduce:
                    result = ApplyMoraleReduce(targetId, value);
                    break;

                case StratagemEffectType.Composite:
                    result = ApplyCompositeEffect(casterFactionId, targetId, value, duration);
                    break;

                default:
                    Debug.LogWarning($"Unhandled effect type: {effectType}");
                    break;
            }

            return result;
        }

        #endregion

        #region Individual Effect Implementations

        /// <summary>
        /// 隠密移動：軍の移動を敵に察知されない
        /// </summary>
        private StratagemEffectResult ApplyStealthMovement(string factionId, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.StealthMovement,
                TargetId = factionId,
                RemainingTurns = duration,
                EffectValue = 1
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.StealthMovement,
                TargetId = factionId,
                Applied = true,
                Description = $"{duration}ターンの間、軍の移動が敵に察知されません"
            };
        }

        /// <summary>
        /// 強制撤退：敵軍を撤退させる
        /// </summary>
        private StratagemEffectResult ApplyForceRetreat(string armyId, int successRate)
        {
            var army = GameManager.Instance?.GetArmy(armyId);
            if (army == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.ForceRetreat,
                    TargetId = armyId,
                    Applied = false,
                    Description = "対象の軍が見つかりません"
                };
            }

            // 成功判定
            bool success = UnityEngine.Random.Range(1, 101) <= successRate;

            if (success)
            {
                // 軍を撤退させる（元の領地に戻す）
                army.IsMoving = false;
                army.TargetTerritoryId = null;
                army.MovementProgress = 0f;

                // 士気も低下
                army.ReduceMorale(10);

                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.ForceRetreat,
                    TargetId = armyId,
                    Applied = true,
                    Description = "敵軍を撤退させました"
                };
            }

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.ForceRetreat,
                TargetId = armyId,
                Applied = false,
                Description = "撤退させることができませんでした"
            };
        }

        /// <summary>
        /// 敵勢力間の対立を煽る
        /// </summary>
        private StratagemEffectResult ApplyFactionConflict(string targetFactionId1, string targetFactionId2, int duration)
        {
            // 関係悪化効果を追加
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.FactionConflict,
                TargetId = $"{targetFactionId1}_{targetFactionId2}",
                RemainingTurns = duration,
                EffectValue = -30 // 関係値低下
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.FactionConflict,
                TargetId = targetFactionId1,
                Applied = true,
                Description = "敵勢力間の関係を悪化させました"
            };
        }

        /// <summary>
        /// 防御力ブースト
        /// </summary>
        private StratagemEffectResult ApplyDefenseBoost(string factionId, int value, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.DefenseBoost,
                TargetId = factionId,
                RemainingTurns = duration,
                EffectValue = value
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.DefenseBoost,
                TargetId = factionId,
                Applied = true,
                ValueApplied = value,
                Description = $"防御力が{value}%上昇しました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 攻撃力ブースト
        /// </summary>
        private StratagemEffectResult ApplyAttackBoost(string factionId, int value, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.AttackBoost,
                TargetId = factionId,
                RemainingTurns = duration,
                EffectValue = value
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.AttackBoost,
                TargetId = factionId,
                Applied = true,
                ValueApplied = value,
                Description = $"攻撃力が{value}%上昇しました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 奇襲攻撃：次の攻撃ダメージ増加
        /// </summary>
        private StratagemEffectResult ApplyAmbush(string targetArmyId, int damageBonus)
        {
            var army = GameManager.Instance?.GetArmy(targetArmyId);
            if (army == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.Ambush,
                    TargetId = targetArmyId,
                    Applied = false
                };
            }

            // 敵軍に混乱状態を付与
            army.ReduceMorale(15);

            // ダメージボーナス効果を付与
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.Ambush,
                TargetId = targetArmyId,
                RemainingTurns = 1,
                EffectValue = damageBonus
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.Ambush,
                TargetId = targetArmyId,
                Applied = true,
                ValueApplied = damageBonus,
                Description = $"奇襲により敵軍の士気が低下、次の攻撃で{damageBonus}%の追加ダメージ"
            };
        }

        /// <summary>
        /// 偽情報を流す
        /// </summary>
        private StratagemEffectResult ApplyDisinformation(string targetFactionId, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.Disinformation,
                TargetId = targetFactionId,
                RemainingTurns = duration,
                EffectValue = 1
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.Disinformation,
                TargetId = targetFactionId,
                Applied = true,
                Description = $"偽情報を流しました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 外交操作
        /// </summary>
        private StratagemEffectResult ApplyDiplomacy(string casterFactionId, string targetFactionId, int relationChange)
        {
            var casterFaction = GameManager.Instance?.GetFaction(casterFactionId);
            var targetFaction = GameManager.Instance?.GetFaction(targetFactionId);

            if (casterFaction == null || targetFaction == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.Diplomacy,
                    TargetId = targetFactionId,
                    Applied = false
                };
            }

            // 外交関係を改善
            // TODO: DiplomacyManagerとの連携

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.Diplomacy,
                TargetId = targetFactionId,
                Applied = true,
                ValueApplied = relationChange,
                Description = $"外交関係が{relationChange}改善しました"
            };
        }

        /// <summary>
        /// 資源略奪
        /// </summary>
        private StratagemEffectResult ApplyResourcePlunder(string casterFactionId, string targetId, int percentage)
        {
            // 対象が領地か勢力かを判定
            var territory = GameManager.Instance?.GetTerritory(targetId);
            var targetFaction = territory != null
                ? GameManager.Instance?.GetFaction(territory.OwnerId)
                : GameManager.Instance?.GetFaction(targetId);

            if (targetFaction == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.ResourcePlunder,
                    TargetId = targetId,
                    Applied = false
                };
            }

            // 金の略奪
            int goldToPlunder = targetFaction.Gold * percentage / 100;
            if (goldToPlunder > 0)
            {
                ResourceManager.Instance?.ConsumeResource(targetFaction.Id, ResourceType.Gold, goldToPlunder);
                ResourceManager.Instance?.AddResource(casterFactionId, ResourceType.Gold, goldToPlunder);
            }

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.ResourcePlunder,
                TargetId = targetId,
                Applied = true,
                ValueApplied = goldToPlunder,
                Description = $"{goldToPlunder}の金を略奪しました"
            };
        }

        /// <summary>
        /// 偵察：敵情報を得る
        /// </summary>
        private StratagemEffectResult ApplyReconnaissance(string targetId, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.Reconnaissance,
                TargetId = targetId,
                RemainingTurns = duration,
                EffectValue = 1
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.Reconnaissance,
                TargetId = targetId,
                Applied = true,
                Description = $"敵の情報を探りました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 領地支配操作
        /// </summary>
        private StratagemEffectResult ApplyTerritoryControl(string territoryId, int value, int duration)
        {
            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.TerritoryControl,
                    TargetId = territoryId,
                    Applied = false
                };
            }

            // 防御力低下効果
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.TerritoryControl,
                TargetId = territoryId,
                RemainingTurns = duration,
                EffectValue = -value
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.TerritoryControl,
                TargetId = territoryId,
                Applied = true,
                Description = $"領地の防御力が{value}%低下しました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 忠誠度低下
        /// </summary>
        private StratagemEffectResult ApplyLoyaltyReduce(string characterId, int value)
        {
            var character = GameManager.Instance?.GetCharacter(characterId);
            if (character == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.LoyaltyReduce,
                    TargetId = characterId,
                    Applied = false
                };
            }

            character.Loyalty = Mathf.Max(0, character.Loyalty - value);

            // 忠誠度が閾値以下なら離反の可能性
            if (character.Loyalty <= Constants.Balance.LoyaltyThresholdForDefection)
            {
                Debug.Log($"[Stratagem] {character.Name} の忠誠度が危険水準に！");
            }

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.LoyaltyReduce,
                TargetId = characterId,
                Applied = true,
                ValueApplied = value,
                Description = $"忠誠度が{value}低下しました"
            };
        }

        /// <summary>
        /// 武将捕獲
        /// </summary>
        private StratagemEffectResult ApplyCharacterCapture(string casterFactionId, string characterId, int successRate)
        {
            var character = GameManager.Instance?.GetCharacter(characterId);
            if (character == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.CharacterCapture,
                    TargetId = characterId,
                    Applied = false
                };
            }

            // 捕獲成功判定
            bool success = UnityEngine.Random.Range(1, 101) <= successRate;

            if (success)
            {
                string previousFaction = character.FactionId;
                character.FactionId = casterFactionId;
                character.Loyalty = 30; // 初期忠誠度は低い

                // イベント発火
                EventBus.CharacterCaptured(new CharacterEventArgs
                {
                    CharacterId = characterId,
                    PreviousFactionId = previousFaction,
                    NewFactionId = casterFactionId
                });

                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.CharacterCapture,
                    TargetId = characterId,
                    Applied = true,
                    Description = $"{character.Name}を捕獲しました"
                };
            }

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.CharacterCapture,
                TargetId = characterId,
                Applied = false,
                Description = "捕獲に失敗しました"
            };
        }

        /// <summary>
        /// 補給線遮断
        /// </summary>
        private StratagemEffectResult ApplySupplyDisrupt(string targetFactionId, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.SupplyDisrupt,
                TargetId = targetFactionId,
                RemainingTurns = duration,
                EffectValue = 50 // 補給50%減
            };

            AddActiveEffect(effect);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.SupplyDisrupt,
                TargetId = targetFactionId,
                Applied = true,
                Description = $"補給線を遮断しました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 撤退：安全に退却
        /// </summary>
        private StratagemEffectResult ApplyEscape(string factionId)
        {
            // 全軍に撤退効果を付与
            if (GameManager.Instance?.GameData == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.Escape,
                    TargetId = factionId,
                    Applied = false
                };
            }

            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId == factionId && army.IsMoving)
                {
                    // 移動中の軍を即座に帰還
                    army.IsMoving = false;
                    army.TargetTerritoryId = null;
                    army.MovementProgress = 0f;
                }
            }

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.Escape,
                TargetId = factionId,
                Applied = true,
                Description = "全軍が安全に撤退しました"
            };
        }

        /// <summary>
        /// 内部混乱
        /// </summary>
        private StratagemEffectResult ApplyInternalStrife(string targetFactionId, int value, int duration)
        {
            var effect = new ActiveStratagemEffect
            {
                EffectType = StratagemEffectType.InternalStrife,
                TargetId = targetFactionId,
                RemainingTurns = duration,
                EffectValue = value
            };

            AddActiveEffect(effect);

            // 全武将の忠誠度低下
            if (GameManager.Instance?.GameData != null)
            {
                foreach (var character in GameManager.Instance.GameData.Characters.Values)
                {
                    if (character.FactionId == targetFactionId)
                    {
                        character.Loyalty = Mathf.Max(0, character.Loyalty - value / 2);
                    }
                }
            }

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.InternalStrife,
                TargetId = targetFactionId,
                Applied = true,
                Description = $"内部混乱を引き起こしました（{duration}ターン）"
            };
        }

        /// <summary>
        /// 士気低下
        /// </summary>
        private StratagemEffectResult ApplyMoraleReduce(string targetArmyId, int value)
        {
            var army = GameManager.Instance?.GetArmy(targetArmyId);
            if (army == null)
            {
                return new StratagemEffectResult
                {
                    EffectType = StratagemEffectType.MoraleReduce,
                    TargetId = targetArmyId,
                    Applied = false
                };
            }

            army.ReduceMorale(value);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.MoraleReduce,
                TargetId = targetArmyId,
                Applied = true,
                ValueApplied = value,
                Description = $"士気が{value}低下しました"
            };
        }

        /// <summary>
        /// 複合効果
        /// </summary>
        private StratagemEffectResult ApplyCompositeEffect(string casterFactionId, string targetId, int value, int duration)
        {
            // 複数の効果を適用
            ApplyAttackBoost(casterFactionId, value / 2, duration);
            ApplyDefenseBoost(casterFactionId, value / 2, duration);

            return new StratagemEffectResult
            {
                EffectType = StratagemEffectType.Composite,
                TargetId = targetId,
                Applied = true,
                Description = "複合効果を発揮しました"
            };
        }

        #endregion

        #region Active Effect Management

        /// <summary>
        /// アクティブ効果を追加
        /// </summary>
        private void AddActiveEffect(ActiveStratagemEffect effect)
        {
            _activeEffects.Add(effect);
            OnEffectApplied?.Invoke(effect);
            Debug.Log($"[Effect] Added: {effect.EffectType} on {effect.TargetId} for {effect.RemainingTurns} turns");
        }

        /// <summary>
        /// ターン終了時の効果処理
        /// </summary>
        private void ProcessTurnEndEffects(int turn)
        {
            var expiredEffects = new List<ActiveStratagemEffect>();

            foreach (var effect in _activeEffects)
            {
                effect.RemainingTurns--;

                if (effect.RemainingTurns <= 0)
                {
                    expiredEffects.Add(effect);
                }
            }

            // 期限切れ効果を除去
            foreach (var effect in expiredEffects)
            {
                _activeEffects.Remove(effect);
                OnEffectExpired?.Invoke(effect);
                Debug.Log($"[Effect] Expired: {effect.EffectType} on {effect.TargetId}");
            }
        }

        /// <summary>
        /// 対象に適用されている効果を取得
        /// </summary>
        public List<ActiveStratagemEffect> GetActiveEffects(string targetId)
        {
            return _activeEffects.FindAll(e => e.TargetId == targetId);
        }

        /// <summary>
        /// 特定タイプの効果が適用されているか
        /// </summary>
        public bool HasActiveEffect(string targetId, StratagemEffectType effectType)
        {
            return _activeEffects.Exists(e => e.TargetId == targetId && e.EffectType == effectType);
        }

        /// <summary>
        /// 効果の合計値を取得
        /// </summary>
        public int GetEffectValue(string targetId, StratagemEffectType effectType)
        {
            int total = 0;
            foreach (var effect in _activeEffects)
            {
                if (effect.TargetId == targetId && effect.EffectType == effectType)
                {
                    total += effect.EffectValue;
                }
            }
            return total;
        }

        /// <summary>
        /// 全てのアクティブ効果をクリア
        /// </summary>
        public void ClearAllEffects()
        {
            _activeEffects.Clear();
        }

        #endregion
    }

    /// <summary>
    /// アクティブな計略効果
    /// </summary>
    [Serializable]
    public class ActiveStratagemEffect
    {
        public StratagemEffectType EffectType;
        public string TargetId;
        public int RemainingTurns;
        public int EffectValue;
        public string SourceStratagemId;
        public string CasterFactionId;
    }
}
