using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Stratagem;

namespace ThirtySixStratagems.Battle
{
    /// <summary>
    /// 戦闘管理システム
    /// 戦闘の開始、進行、終了を管理
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private int _maxBattleRounds = 10;

        // 現在の戦闘状態
        private BattleState _currentBattle;
        private bool _isBattleInProgress = false;

        // イベント
        public event Action<BattleState> OnBattleStarted;
        public event Action<BattleRoundResult> OnBattleRoundCompleted;
        public event Action<BattleResult> OnBattleEnded;
        public event Action<string> OnStratagemUsedInBattle;

        /// <summary>
        /// 現在の戦闘状態
        /// </summary>
        public BattleState CurrentBattle => _currentBattle;

        /// <summary>
        /// 戦闘中かどうか
        /// </summary>
        public bool IsBattleInProgress => _isBattleInProgress;

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

        #region Battle Initiation

        /// <summary>
        /// 戦闘を開始
        /// </summary>
        public BattleState StartBattle(string attackerArmyId, string defenderArmyId, string territoryId)
        {
            if (_isBattleInProgress)
            {
                Debug.LogWarning("Battle already in progress");
                return null;
            }

            var attackerArmy = GameManager.Instance?.GetArmy(attackerArmyId);
            var defenderArmy = GameManager.Instance?.GetArmy(defenderArmyId);
            var territory = GameManager.Instance?.GetTerritory(territoryId);

            if (attackerArmy == null || territory == null)
            {
                Debug.LogError("Invalid battle parameters");
                return null;
            }

            _currentBattle = new BattleState
            {
                BattleId = Guid.NewGuid().ToString(),
                TerritoryId = territoryId,
                TerritoryName = territory.Name,
                CurrentRound = 0,
                Phase = BattlePhase.Preparation,
                RoundResults = new List<BattleRoundResult>()
            };

            // 攻撃側の設定
            _currentBattle.Attacker = CreateBattleUnit(attackerArmy, false);

            // 攻撃側ユニットの作成に失敗した場合
            if (_currentBattle.Attacker == null)
            {
                Debug.LogError("Failed to create attacker unit");
                _currentBattle = null;
                return null;
            }

            // 防御側の設定
            if (defenderArmy != null)
            {
                _currentBattle.Defender = CreateBattleUnit(defenderArmy, true);
            }
            else
            {
                // 守備軍がいない場合は領地の防御力のみ
                _currentBattle.Defender = CreateTerritoryDefense(territory);
            }

            // 防御側ユニットの作成に失敗した場合
            if (_currentBattle.Defender == null)
            {
                Debug.LogError("Failed to create defender unit");
                _currentBattle = null;
                return null;
            }

            // 地形効果を適用
            ApplyTerrainEffects(territory);

            _isBattleInProgress = true;

            // GameManagerに通知
            GameManager.Instance?.StartBattle(attackerArmyId, defenderArmyId, territoryId);

            // EventBus通知
            EventBus.BattleStarted(new BattleEventArgs
            {
                AttackerArmyId = attackerArmyId,
                DefenderArmyId = defenderArmyId,
                TerritoryId = territoryId
            });

            OnBattleStarted?.Invoke(_currentBattle);

            Debug.Log($"Battle started at {territory.Name}: {_currentBattle.Attacker.ArmyName} vs {_currentBattle.Defender.ArmyName}");

            return _currentBattle;
        }

        /// <summary>
        /// 戦闘ユニットを作成
        /// </summary>
        private BattleUnit CreateBattleUnit(Army army, bool isDefender)
        {
            var faction = GameManager.Instance?.GetFaction(army.FactionId);
            var commander = GameManager.Instance?.GetCharacter(army.CommanderId);

            var unit = new BattleUnit
            {
                ArmyId = army.Id,
                ArmyName = army.Name,
                FactionId = army.FactionId,
                FactionName = faction?.Name ?? "不明",
                CommanderId = army.CommanderId,
                CommanderName = commander?.Name ?? "なし",
                IsDefender = isDefender,
                InitialSoldiers = army.SoldierCount,
                CurrentSoldiers = army.SoldierCount,
                Morale = army.Morale,
                BaseCombatPower = CalculateBaseCombatPower(army, commander),
                ActiveEffects = new List<BattleEffect>()
            };

            // 計略効果を反映
            ApplyStratagemEffects(unit);

            return unit;
        }

        /// <summary>
        /// 領地防御ユニットを作成（守備軍なし時）
        /// </summary>
        private BattleUnit CreateTerritoryDefense(Territory territory)
        {
            // null参照エラーを防ぐ
            if (territory == null)
            {
                Debug.LogError("CreateTerritoryDefense: Territory is null");
                return new BattleUnit
                {
                    ArmyId = null,
                    ArmyName = "守備隊",
                    FactionId = "",
                    FactionName = "不明",
                    IsDefender = true,
                    InitialSoldiers = 10,
                    CurrentSoldiers = 10,
                    Morale = 50,
                    BaseCombatPower = 1,
                    ActiveEffects = new List<BattleEffect>()
                };
            }

            return new BattleUnit
            {
                ArmyId = null,
                ArmyName = $"{territory.Name}守備隊",
                FactionId = territory.OwnerId ?? "",
                FactionName = GameManager.Instance?.GetFaction(territory.OwnerId)?.Name ?? "不明",
                IsDefender = true,
                InitialSoldiers = territory.Defense * 10, // 防御力から仮想兵力を計算
                CurrentSoldiers = territory.Defense * 10,
                Morale = 50,
                BaseCombatPower = Mathf.Max(1, territory.Defense),
                ActiveEffects = new List<BattleEffect>()
            };
        }

        /// <summary>
        /// 基本戦闘力を計算
        /// </summary>
        private int CalculateBaseCombatPower(Army army, Character commander)
        {
            int power = army.SoldierCount / 100; // 基本戦闘力

            if (commander != null)
            {
                // 指揮官の能力値を反映
                power += commander.Strength / 10;
                power += commander.Leadership / 5;
            }

            // 士気の影響
            float moraleModifier = 1f + (army.Morale - 50) * Constants.Balance.MoraleImpact / 100f;
            power = Mathf.RoundToInt(power * moraleModifier);

            return Mathf.Max(1, power);
        }

        /// <summary>
        /// 地形効果を適用
        /// </summary>
        private void ApplyTerrainEffects(Territory territory)
        {
            // 防御側に地形ボーナス
            if (_currentBattle.Defender != null)
            {
                _currentBattle.Defender.TerrainBonus = Mathf.RoundToInt(
                    territory.Defense * Constants.Balance.DefenseBonus * 0.1f);
            }
        }

        /// <summary>
        /// 計略効果を戦闘に反映
        /// </summary>
        private void ApplyStratagemEffects(BattleUnit unit)
        {
            if (StratagemEffectProcessor.Instance == null) return;

            // 攻撃力ブースト
            int attackBoost = StratagemEffectProcessor.Instance.GetEffectValue(
                unit.FactionId, StratagemEffectType.AttackBoost);
            if (attackBoost > 0)
            {
                unit.ActiveEffects.Add(new BattleEffect
                {
                    EffectName = "攻撃力上昇",
                    PowerModifier = attackBoost
                });
            }

            // 防御力ブースト
            int defenseBoost = StratagemEffectProcessor.Instance.GetEffectValue(
                unit.FactionId, StratagemEffectType.DefenseBoost);
            if (defenseBoost > 0 && unit.IsDefender)
            {
                unit.ActiveEffects.Add(new BattleEffect
                {
                    EffectName = "防御力上昇",
                    PowerModifier = defenseBoost
                });
            }

            // 奇襲効果
            if (StratagemEffectProcessor.Instance.HasActiveEffect(unit.ArmyId, StratagemEffectType.Ambush))
            {
                unit.ActiveEffects.Add(new BattleEffect
                {
                    EffectName = "奇襲",
                    PowerModifier = 30,
                    DurationRounds = 1
                });
            }
        }

        #endregion

        #region Battle Progression

        /// <summary>
        /// 戦闘ラウンドを実行
        /// </summary>
        public BattleRoundResult ExecuteRound()
        {
            if (!_isBattleInProgress || _currentBattle == null)
            {
                return null;
            }

            _currentBattle.CurrentRound++;
            _currentBattle.Phase = BattlePhase.Combat;

            var result = BattleCalculator.Instance?.CalculateRound(_currentBattle);

            if (result == null)
            {
                // フォールバック計算
                result = CalculateRoundFallback();
            }

            // 結果を適用
            ApplyRoundResult(result);

            _currentBattle.RoundResults.Add(result);

            OnBattleRoundCompleted?.Invoke(result);

            Debug.Log($"Round {_currentBattle.CurrentRound}: Attacker lost {result.AttackerCasualties}, Defender lost {result.DefenderCasualties}");

            // 戦闘終了判定
            if (CheckBattleEnd())
            {
                EndBattle();
            }

            return result;
        }

        /// <summary>
        /// フォールバックのラウンド計算
        /// </summary>
        private BattleRoundResult CalculateRoundFallback()
        {
            var attacker = _currentBattle.Attacker;
            var defender = _currentBattle.Defender;

            // 戦闘力計算
            int attackerPower = CalculateEffectivePower(attacker, false);
            int defenderPower = CalculateEffectivePower(defender, true);

            // 損害計算
            float powerRatio = (float)attackerPower / Mathf.Max(1, defenderPower);

            int attackerCasualties = Mathf.RoundToInt(
                attacker.CurrentSoldiers * Constants.Balance.WinnerLossRate / powerRatio);
            int defenderCasualties = Mathf.RoundToInt(
                defender.CurrentSoldiers * Constants.Balance.LoserLossRate * powerRatio);

            // 最低でも1の損害
            attackerCasualties = Mathf.Max(1, Mathf.Min(attackerCasualties, attacker.CurrentSoldiers));
            defenderCasualties = Mathf.Max(1, Mathf.Min(defenderCasualties, defender.CurrentSoldiers));

            return new BattleRoundResult
            {
                RoundNumber = _currentBattle.CurrentRound,
                AttackerPower = attackerPower,
                DefenderPower = defenderPower,
                AttackerCasualties = attackerCasualties,
                DefenderCasualties = defenderCasualties,
                AttackerMoraleChange = powerRatio > 1 ? 2 : -3,
                DefenderMoraleChange = powerRatio < 1 ? 2 : -3
            };
        }

        /// <summary>
        /// 有効戦闘力を計算
        /// </summary>
        private int CalculateEffectivePower(BattleUnit unit, bool isDefending)
        {
            int power = unit.BaseCombatPower;

            // 地形ボーナス
            if (isDefending)
            {
                power += unit.TerrainBonus;
            }

            // 効果による修正
            foreach (var effect in unit.ActiveEffects)
            {
                power = Mathf.RoundToInt(power * (1f + effect.PowerModifier / 100f));
            }

            // 兵力による補正
            float soldierRatio = (float)unit.CurrentSoldiers / Mathf.Max(1, unit.InitialSoldiers);
            power = Mathf.RoundToInt(power * soldierRatio);

            return Mathf.Max(1, power);
        }

        /// <summary>
        /// ラウンド結果を適用
        /// </summary>
        private void ApplyRoundResult(BattleRoundResult result)
        {
            _currentBattle.Attacker.CurrentSoldiers -= result.AttackerCasualties;
            _currentBattle.Defender.CurrentSoldiers -= result.DefenderCasualties;

            _currentBattle.Attacker.Morale = Mathf.Clamp(
                _currentBattle.Attacker.Morale + result.AttackerMoraleChange, 0, 100);
            _currentBattle.Defender.Morale = Mathf.Clamp(
                _currentBattle.Defender.Morale + result.DefenderMoraleChange, 0, 100);

            // 一時効果のターン減少
            DecrementBattleEffects(_currentBattle.Attacker);
            DecrementBattleEffects(_currentBattle.Defender);
        }

        /// <summary>
        /// 戦闘効果の持続ターンを減少
        /// </summary>
        private void DecrementBattleEffects(BattleUnit unit)
        {
            for (int i = unit.ActiveEffects.Count - 1; i >= 0; i--)
            {
                if (unit.ActiveEffects[i].DurationRounds > 0)
                {
                    unit.ActiveEffects[i].DurationRounds--;
                    if (unit.ActiveEffects[i].DurationRounds <= 0)
                    {
                        unit.ActiveEffects.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 戦闘終了判定
        /// </summary>
        private bool CheckBattleEnd()
        {
            // 攻撃側全滅
            if (_currentBattle.Attacker.CurrentSoldiers <= 0)
            {
                return true;
            }

            // 防御側全滅
            if (_currentBattle.Defender.CurrentSoldiers <= 0)
            {
                return true;
            }

            // 士気崩壊
            if (_currentBattle.Attacker.Morale <= 10 || _currentBattle.Defender.Morale <= 10)
            {
                return true;
            }

            // 最大ラウンド到達
            if (_currentBattle.CurrentRound >= _maxBattleRounds)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Battle End

        /// <summary>
        /// 戦闘を終了
        /// </summary>
        public BattleResult EndBattle()
        {
            if (!_isBattleInProgress || _currentBattle == null)
            {
                return null;
            }

            _currentBattle.Phase = BattlePhase.Result;

            var result = CreateBattleResult();

            // 実際の軍データを更新
            ApplyBattleResultToArmies(result);

            // 領地占領判定
            if (result.AttackerVictory)
            {
                ProcessTerritoryConquest(result);
            }

            _isBattleInProgress = false;

            // GameManagerに通知
            GameManager.Instance?.EndBattle();

            // EventBus通知
            EventBus.BattleEnded(new BattleResultEventArgs
            {
                AttackerArmyId = _currentBattle.Attacker.ArmyId,
                DefenderArmyId = _currentBattle.Defender.ArmyId,
                TerritoryId = _currentBattle.TerritoryId,
                VictorFactionId = result.VictorFactionId,
                AttackerLosses = result.AttackerTotalCasualties,
                DefenderLosses = result.DefenderTotalCasualties,
                TerritoryConquered = result.TerritoryConquered
            });

            OnBattleEnded?.Invoke(result);

            Debug.Log($"Battle ended: {(result.AttackerVictory ? "Attacker" : "Defender")} victory!");

            var completedBattle = _currentBattle;
            _currentBattle = null;

            return result;
        }

        /// <summary>
        /// 戦闘結果を作成
        /// </summary>
        private BattleResult CreateBattleResult()
        {
            var attacker = _currentBattle.Attacker;
            var defender = _currentBattle.Defender;

            // 勝敗判定
            bool attackerVictory = false;

            if (defender.CurrentSoldiers <= 0)
            {
                attackerVictory = true;
            }
            else if (attacker.CurrentSoldiers <= 0)
            {
                attackerVictory = false;
            }
            else if (defender.Morale <= 10 && attacker.Morale > 10)
            {
                attackerVictory = true;
            }
            else if (attacker.Morale <= 10 && defender.Morale > 10)
            {
                attackerVictory = false;
            }
            else
            {
                // 最大ラウンド到達時は残存兵力で判定
                attackerVictory = attacker.CurrentSoldiers > defender.CurrentSoldiers;
            }

            int attackerCasualties = attacker.InitialSoldiers - attacker.CurrentSoldiers;
            int defenderCasualties = defender.InitialSoldiers - defender.CurrentSoldiers;

            return new BattleResult
            {
                BattleId = _currentBattle.BattleId,
                TerritoryId = _currentBattle.TerritoryId,
                AttackerVictory = attackerVictory,
                VictorFactionId = attackerVictory ? attacker.FactionId : defender.FactionId,
                AttackerFactionId = attacker.FactionId,
                DefenderFactionId = defender.FactionId,
                AttackerTotalCasualties = attackerCasualties,
                DefenderTotalCasualties = defenderCasualties,
                AttackerSurvivors = Mathf.Max(0, attacker.CurrentSoldiers),
                DefenderSurvivors = Mathf.Max(0, defender.CurrentSoldiers),
                TotalRounds = _currentBattle.CurrentRound,
                TerritoryConquered = attackerVictory && defender.CurrentSoldiers <= 0
            };
        }

        /// <summary>
        /// 戦闘結果を軍データに反映
        /// </summary>
        private void ApplyBattleResultToArmies(BattleResult result)
        {
            var attacker = _currentBattle.Attacker;
            var defender = _currentBattle.Defender;

            // 攻撃側軍の更新
            if (!string.IsNullOrEmpty(attacker.ArmyId))
            {
                var attackerArmy = GameManager.Instance?.GetArmy(attacker.ArmyId);
                if (attackerArmy != null)
                {
                    attackerArmy.SoldierCount = result.AttackerSurvivors;
                    attackerArmy.Morale = attacker.Morale;

                    if (attackerArmy.SoldierCount <= 0)
                    {
                        // 軍が全滅
                        DisbandArmy(attackerArmy);
                    }
                    else if (!result.AttackerVictory)
                    {
                        // 敗北時の士気低下
                        attackerArmy.ReduceMorale(Constants.Balance.MoraleLossOnDefeat);
                    }
                }
            }

            // 防御側軍の更新
            if (!string.IsNullOrEmpty(defender.ArmyId))
            {
                var defenderArmy = GameManager.Instance?.GetArmy(defender.ArmyId);
                if (defenderArmy != null)
                {
                    defenderArmy.SoldierCount = result.DefenderSurvivors;
                    defenderArmy.Morale = defender.Morale;

                    if (defenderArmy.SoldierCount <= 0)
                    {
                        DisbandArmy(defenderArmy);
                    }
                    else if (result.AttackerVictory)
                    {
                        defenderArmy.ReduceMorale(Constants.Balance.MoraleLossOnDefeat);
                    }
                }
            }
        }

        /// <summary>
        /// 軍を解散
        /// </summary>
        private void DisbandArmy(Army army)
        {
            if (GameManager.Instance?.GameData == null) return;

            GameManager.Instance.GameData.Armies.Remove(army.Id);

            EventBus.ArmyDisbanded(new ArmyEventArgs
            {
                ArmyId = army.Id,
                FactionId = army.FactionId,
                TerritoryId = army.TerritoryId
            });

            Debug.Log($"Army disbanded: {army.Name}");
        }

        /// <summary>
        /// 領地占領を処理
        /// </summary>
        private void ProcessTerritoryConquest(BattleResult result)
        {
            if (!result.TerritoryConquered) return;

            var territory = GameManager.Instance?.GetTerritory(result.TerritoryId);
            if (territory == null) return;

            string previousOwner = territory.OwnerId;
            string newOwner = _currentBattle.Attacker.FactionId;

            // 領地の所有者を変更
            territory.OwnerId = newOwner;

            // 勢力の領地リストを更新
            var previousFaction = GameManager.Instance?.GetFaction(previousOwner);
            var newFaction = GameManager.Instance?.GetFaction(newOwner);

            if (previousFaction != null)
            {
                previousFaction.TerritoryIds.Remove(territory.Id);
            }

            if (newFaction != null && !newFaction.TerritoryIds.Contains(territory.Id))
            {
                newFaction.TerritoryIds.Add(territory.Id);
            }

            // EventBus通知
            EventBus.TerritoryConquered(new TerritoryConqueredEventArgs
            {
                TerritoryId = territory.Id,
                PreviousOwnerId = previousOwner,
                NewOwnerId = newOwner
            });

            Debug.Log($"Territory conquered: {territory.Name} ({previousOwner} -> {newOwner})");
        }

        #endregion

        #region Battle Actions

        /// <summary>
        /// 戦闘中に計略を使用
        /// </summary>
        public bool UseStratagemInBattle(string stratagemId, string targetId)
        {
            if (!_isBattleInProgress) return false;

            // 計略実行
            var result = StratagemManager.Instance?.ExecuteStratagem(
                stratagemId,
                _currentBattle.Attacker.FactionId,
                _currentBattle.Attacker.CommanderId,
                targetId);

            if (result != null && result.Success)
            {
                OnStratagemUsedInBattle?.Invoke(stratagemId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 撤退を試みる
        /// </summary>
        public bool AttemptRetreat(bool isAttacker)
        {
            if (!_isBattleInProgress) return false;

            var unit = isAttacker ? _currentBattle.Attacker : _currentBattle.Defender;

            // 撤退成功率 = 士気 / 2 + 指揮官の知力 / 5
            int successRate = unit.Morale / 2;
            var commander = GameManager.Instance?.GetCharacter(unit.CommanderId);
            if (commander != null)
            {
                successRate += commander.Intelligence / 5;
            }

            bool success = UnityEngine.Random.Range(1, 101) <= successRate;

            if (success)
            {
                // 撤退時の追加損害
                int retreatCasualties = Mathf.RoundToInt(unit.CurrentSoldiers * 0.1f);
                unit.CurrentSoldiers -= retreatCasualties;

                Debug.Log($"{unit.ArmyName} retreated with {retreatCasualties} additional casualties");

                // 戦闘終了（撤退側の敗北）
                _currentBattle.Phase = BattlePhase.Result;
                if (isAttacker)
                {
                    _currentBattle.Attacker.CurrentSoldiers = 0; // 敗北扱い
                }
                else
                {
                    _currentBattle.Defender.CurrentSoldiers = 0;
                }

                EndBattle();
            }

            return success;
        }

        /// <summary>
        /// 戦闘を自動進行
        /// </summary>
        public BattleResult AutoResolveBattle()
        {
            if (!_isBattleInProgress) return null;

            while (_isBattleInProgress && _currentBattle.CurrentRound < _maxBattleRounds)
            {
                ExecuteRound();
            }

            if (_isBattleInProgress)
            {
                return EndBattle();
            }

            return null;
        }

        #endregion
    }

    #region Battle Data Classes

    /// <summary>
    /// 戦闘状態
    /// </summary>
    [Serializable]
    public class BattleState
    {
        public string BattleId;
        public string TerritoryId;
        public string TerritoryName;
        public int CurrentRound;
        public BattlePhase Phase;
        public BattleUnit Attacker;
        public BattleUnit Defender;
        public List<BattleRoundResult> RoundResults;
    }

    /// <summary>
    /// 戦闘ユニット
    /// </summary>
    [Serializable]
    public class BattleUnit
    {
        public string ArmyId;
        public string ArmyName;
        public string FactionId;
        public string FactionName;
        public string CommanderId;
        public string CommanderName;
        public bool IsDefender;
        public int InitialSoldiers;
        public int CurrentSoldiers;
        public int Morale;
        public int BaseCombatPower;
        public int TerrainBonus;
        public List<BattleEffect> ActiveEffects;
    }

    /// <summary>
    /// 戦闘効果
    /// </summary>
    [Serializable]
    public class BattleEffect
    {
        public string EffectName;
        public int PowerModifier;
        public int DurationRounds;

        /// <summary>
        /// DurationRoundsのエイリアス（テスト互換性用）
        /// </summary>
        public int Duration
        {
            get => DurationRounds;
            set => DurationRounds = value;
        }
    }

    /// <summary>
    /// ラウンド結果
    /// </summary>
    [Serializable]
    public class BattleRoundResult
    {
        public int RoundNumber;
        public int AttackerPower;
        public int DefenderPower;
        public int AttackerCasualties;
        public int DefenderCasualties;
        public int AttackerMoraleChange;
        public int DefenderMoraleChange;
        public string SpecialEvent;
    }

    /// <summary>
    /// 戦闘結果
    /// </summary>
    [Serializable]
    public class BattleResult
    {
        public string BattleId;
        public string TerritoryId;
        public bool AttackerVictory;
        public string VictorFactionId;
        public string AttackerFactionId;
        public string DefenderFactionId;
        public int AttackerTotalCasualties;
        public int DefenderTotalCasualties;
        public int AttackerSurvivors;
        public int DefenderSurvivors;
        public int DefenderRemainingeSoldiers => DefenderSurvivors;
        public int TotalRounds;
        public bool TerritoryConquered;

        /// <summary>
        /// 指定勢力が勝利したかどうか
        /// </summary>
        public bool IsVictory(string factionId)
        {
            return VictorFactionId == factionId;
        }
    }

    /// <summary>
    /// 戦闘フェーズ
    /// </summary>
    public enum BattlePhase
    {
        Preparation,    // 準備
        Combat,         // 戦闘中
        Result          // 結果
    }

    #endregion
}
