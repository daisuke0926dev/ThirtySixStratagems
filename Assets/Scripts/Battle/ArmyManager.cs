using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Battle
{
    /// <summary>
    /// 軍管理システム
    /// 軍の作成、移動、編成を管理
    /// </summary>
    public class ArmyManager : MonoBehaviour
    {
        public static ArmyManager Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private int _maxArmiesPerFaction = 10;
        [SerializeField] private int _minSoldiersPerArmy = 100;

        // イベント
        public event Action<Army> OnArmyCreated;
        public event Action<Army> OnArmyDisbanded;
        public event Action<Army, string, string> OnArmyMoved;
        public event Action<Army, Army> OnArmiesMerged;

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
            EventBus.OnTurnStarted += ProcessArmyMovements;
        }

        private void OnDisable()
        {
            EventBus.OnTurnStarted -= ProcessArmyMovements;
        }

        #region Army Creation

        /// <summary>
        /// 新しい軍を作成
        /// </summary>
        public Army CreateArmy(string factionId, string territoryId, string commanderId,
            int soldierCount, string armyName = null)
        {
            // バリデーション
            if (!ValidateArmyCreation(factionId, territoryId, commanderId, soldierCount, out string error))
            {
                Debug.LogWarning($"Cannot create army: {error}");
                return null;
            }

            var faction = GameManager.Instance?.GetFaction(factionId);
            var commander = GameManager.Instance?.GetCharacter(commanderId);

            // 軍を作成
            var army = new Army
            {
                Id = GenerateArmyId(),
                Name = armyName ?? GenerateArmyName(faction, commander),
                FactionId = factionId,
                TerritoryId = territoryId,
                CommanderId = commanderId,
                SoldierCount = soldierCount,
                Morale = 70, // 初期士気
                IsMoving = false
            };

            // GameDataに追加
            if (GameManager.Instance?.GameData != null)
            {
                GameManager.Instance.GameData.Armies[army.Id] = army;
            }

            // 武将を軍に配属
            if (commander != null)
            {
                commander.ArmyId = army.Id;
            }

            // イベント発火
            EventBus.ArmyCreated(new ArmyEventArgs
            {
                ArmyId = army.Id,
                FactionId = factionId,
                TerritoryId = territoryId
            });

            OnArmyCreated?.Invoke(army);

            Debug.Log($"Army created: {army.Name} ({soldierCount} soldiers) at {territoryId}");

            return army;
        }

        /// <summary>
        /// 軍作成のバリデーション
        /// </summary>
        private bool ValidateArmyCreation(string factionId, string territoryId, string commanderId,
            int soldierCount, out string error)
        {
            error = null;

            // 勢力チェック
            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null)
            {
                error = "勢力が見つかりません";
                return false;
            }

            // 領地チェック
            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory == null)
            {
                error = "領地が見つかりません";
                return false;
            }

            if (territory.OwnerId != factionId)
            {
                error = "自領地でのみ軍を作成できます";
                return false;
            }

            // 最大軍数チェック
            int currentArmyCount = GetArmyCount(factionId);
            if (currentArmyCount >= _maxArmiesPerFaction)
            {
                error = $"軍の上限数（{_maxArmiesPerFaction}）に達しています";
                return false;
            }

            // 最小兵数チェック
            if (soldierCount < _minSoldiersPerArmy)
            {
                error = $"最低{_minSoldiersPerArmy}人の兵士が必要です";
                return false;
            }

            // 指揮官チェック
            if (!string.IsNullOrEmpty(commanderId))
            {
                var commander = GameManager.Instance?.GetCharacter(commanderId);
                if (commander == null)
                {
                    error = "指揮官が見つかりません";
                    return false;
                }

                if (commander.FactionId != factionId)
                {
                    error = "他勢力の武将は指揮官にできません";
                    return false;
                }

                if (!string.IsNullOrEmpty(commander.ArmyId))
                {
                    error = "この武将は既に他の軍を率いています";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 軍IDを生成
        /// </summary>
        private string GenerateArmyId()
        {
            return $"army_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        /// <summary>
        /// 軍名を生成
        /// </summary>
        private string GenerateArmyName(Faction faction, Character commander)
        {
            if (commander != null)
            {
                return $"{commander.Name}軍";
            }
            return $"{faction?.Name ?? ""}第{GetArmyCount(faction?.Id) + 1}軍";
        }

        #endregion

        #region Army Movement

        /// <summary>
        /// 軍を移動開始
        /// </summary>
        public bool StartArmyMovement(string armyId, string targetTerritoryId)
        {
            var army = GameManager.Instance?.GetArmy(armyId);
            if (army == null)
            {
                Debug.LogWarning("Army not found");
                return false;
            }

            // 移動可能かチェック
            if (!CanMoveArmy(army, targetTerritoryId, out string error))
            {
                Debug.LogWarning($"Cannot move army: {error}");
                return false;
            }

            // 移動開始
            army.IsMoving = true;
            army.TargetTerritoryId = targetTerritoryId;
            army.MovementProgress = 0f;

            // イベント発火
            EventBus.ArmyMoveStarted(new ArmyEventArgs
            {
                ArmyId = army.Id,
                FactionId = army.FactionId,
                TerritoryId = army.TerritoryId,
                TargetTerritoryId = targetTerritoryId
            });

            Debug.Log($"Army {army.Name} started moving to {targetTerritoryId}");

            return true;
        }

        /// <summary>
        /// 軍が移動可能かチェック
        /// </summary>
        public bool CanMoveArmy(Army army, string targetTerritoryId, out string error)
        {
            error = null;

            if (army.IsMoving)
            {
                error = "既に移動中です";
                return false;
            }

            if (army.SoldierCount <= 0)
            {
                error = "兵力がありません";
                return false;
            }

            var currentTerritory = GameManager.Instance?.GetTerritory(army.TerritoryId);
            var targetTerritory = GameManager.Instance?.GetTerritory(targetTerritoryId);

            if (currentTerritory == null || targetTerritory == null)
            {
                error = "領地が見つかりません";
                return false;
            }

            // 隣接チェック
            if (!currentTerritory.AdjacentTerritoryIds.Contains(targetTerritoryId))
            {
                error = "隣接する領地にのみ移動できます";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 軍の移動をキャンセル
        /// </summary>
        public bool CancelArmyMovement(string armyId)
        {
            var army = GameManager.Instance?.GetArmy(armyId);
            if (army == null || !army.IsMoving)
            {
                return false;
            }

            army.IsMoving = false;
            army.TargetTerritoryId = null;
            army.MovementProgress = 0f;

            Debug.Log($"Army {army.Name} movement cancelled");

            return true;
        }

        /// <summary>
        /// 軍の移動を完了
        /// </summary>
        private void CompleteArmyMovement(Army army)
        {
            string previousTerritory = army.TerritoryId;
            string newTerritory = army.TargetTerritoryId;

            army.TerritoryId = newTerritory;
            army.IsMoving = false;
            army.TargetTerritoryId = null;
            army.MovementProgress = 0f;

            // イベント発火
            EventBus.ArmyMoveCompleted(new ArmyEventArgs
            {
                ArmyId = army.Id,
                FactionId = army.FactionId,
                TerritoryId = newTerritory,
                TargetTerritoryId = previousTerritory
            });

            OnArmyMoved?.Invoke(army, previousTerritory, newTerritory);

            Debug.Log($"Army {army.Name} arrived at {newTerritory}");

            // 敵領地なら戦闘開始判定
            CheckForBattleAtTerritory(army, newTerritory);
        }

        /// <summary>
        /// ターン開始時の移動処理
        /// </summary>
        private void ProcessArmyMovements(int turn)
        {
            if (GameManager.Instance?.GameData == null) return;

            var movingArmies = GameManager.Instance.GameData.Armies.Values
                .Where(a => a.IsMoving)
                .ToList();

            foreach (var army in movingArmies)
            {
                // 移動進捗を更新（1ターンで完了）
                army.MovementProgress = 1f;

                if (army.MovementProgress >= 1f)
                {
                    CompleteArmyMovement(army);
                }
            }
        }

        /// <summary>
        /// 戦闘発生をチェック
        /// </summary>
        private void CheckForBattleAtTerritory(Army army, string territoryId)
        {
            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory == null) return;

            // 敵領地に到着した場合
            if (territory.OwnerId != army.FactionId)
            {
                // 防御軍を探す
                var defender = GetArmyAtTerritory(territoryId, territory.OwnerId);

                if (defender != null || territory.Defense > 0)
                {
                    // 戦闘開始
                    BattleManager.Instance?.StartBattle(army.Id, defender?.Id, territoryId);
                }
            }
        }

        #endregion

        #region Army Management

        /// <summary>
        /// 軍を解散
        /// </summary>
        public bool DisbandArmy(string armyId)
        {
            var army = GameManager.Instance?.GetArmy(armyId);
            if (army == null)
            {
                return false;
            }

            // 移動中は解散不可
            if (army.IsMoving)
            {
                Debug.LogWarning("Cannot disband moving army");
                return false;
            }

            // 指揮官を解放
            if (!string.IsNullOrEmpty(army.CommanderId))
            {
                var commander = GameManager.Instance?.GetCharacter(army.CommanderId);
                if (commander != null)
                {
                    commander.ArmyId = null;
                }
            }

            // 軍を削除
            GameManager.Instance.GameData.Armies.Remove(armyId);

            // イベント発火
            EventBus.ArmyDisbanded(new ArmyEventArgs
            {
                ArmyId = armyId,
                FactionId = army.FactionId,
                TerritoryId = army.TerritoryId
            });

            OnArmyDisbanded?.Invoke(army);

            Debug.Log($"Army disbanded: {army.Name}");

            return true;
        }

        /// <summary>
        /// 軍を合流
        /// </summary>
        public bool MergeArmies(string sourceArmyId, string targetArmyId)
        {
            var sourceArmy = GameManager.Instance?.GetArmy(sourceArmyId);
            var targetArmy = GameManager.Instance?.GetArmy(targetArmyId);

            if (sourceArmy == null || targetArmy == null)
            {
                return false;
            }

            // 同じ領地にいる必要がある
            if (sourceArmy.TerritoryId != targetArmy.TerritoryId)
            {
                Debug.LogWarning("Armies must be in the same territory to merge");
                return false;
            }

            // 同じ勢力である必要がある
            if (sourceArmy.FactionId != targetArmy.FactionId)
            {
                Debug.LogWarning("Armies must belong to the same faction to merge");
                return false;
            }

            // 移動中は合流不可
            if (sourceArmy.IsMoving || targetArmy.IsMoving)
            {
                Debug.LogWarning("Cannot merge moving armies");
                return false;
            }

            // 兵力を合算
            targetArmy.SoldierCount += sourceArmy.SoldierCount;

            // 士気は平均
            targetArmy.Morale = (sourceArmy.Morale + targetArmy.Morale) / 2;

            // 元の軍を解散
            DisbandArmy(sourceArmyId);

            OnArmiesMerged?.Invoke(sourceArmy, targetArmy);

            Debug.Log($"Armies merged: {sourceArmy.Name} -> {targetArmy.Name}");

            return true;
        }

        /// <summary>
        /// 軍を分割
        /// </summary>
        public Army SplitArmy(string armyId, int soldiersToSplit, string newCommanderId = null)
        {
            var sourceArmy = GameManager.Instance?.GetArmy(armyId);
            if (sourceArmy == null)
            {
                return null;
            }

            // 移動中は分割不可
            if (sourceArmy.IsMoving)
            {
                Debug.LogWarning("Cannot split moving army");
                return null;
            }

            // 分割後の兵数チェック
            if (sourceArmy.SoldierCount - soldiersToSplit < _minSoldiersPerArmy ||
                soldiersToSplit < _minSoldiersPerArmy)
            {
                Debug.LogWarning($"Both armies must have at least {_minSoldiersPerArmy} soldiers");
                return null;
            }

            // 元の軍から兵を減らす
            sourceArmy.SoldierCount -= soldiersToSplit;

            // 新しい軍を作成
            var newArmy = CreateArmy(
                sourceArmy.FactionId,
                sourceArmy.TerritoryId,
                newCommanderId,
                soldiersToSplit
            );

            if (newArmy != null)
            {
                // 士気を引き継ぐ
                newArmy.Morale = sourceArmy.Morale;
            }

            Debug.Log($"Army split: {sourceArmy.Name} -{soldiersToSplit} -> {newArmy?.Name}");

            return newArmy;
        }

        /// <summary>
        /// 指揮官を変更
        /// </summary>
        public bool ChangeCommander(string armyId, string newCommanderId)
        {
            var army = GameManager.Instance?.GetArmy(armyId);
            if (army == null)
            {
                return false;
            }

            // 現在の指揮官を解放
            if (!string.IsNullOrEmpty(army.CommanderId))
            {
                var oldCommander = GameManager.Instance?.GetCharacter(army.CommanderId);
                if (oldCommander != null)
                {
                    oldCommander.ArmyId = null;
                }
            }

            // 新しい指揮官を配属
            if (!string.IsNullOrEmpty(newCommanderId))
            {
                var newCommander = GameManager.Instance?.GetCharacter(newCommanderId);
                if (newCommander == null || newCommander.FactionId != army.FactionId)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(newCommander.ArmyId))
                {
                    Debug.LogWarning("New commander is already leading another army");
                    return false;
                }

                newCommander.ArmyId = armyId;
                army.Name = $"{newCommander.Name}軍";
            }

            army.CommanderId = newCommanderId;

            Debug.Log($"Commander changed for {army.Name}");

            return true;
        }

        /// <summary>
        /// 兵を補充
        /// </summary>
        public bool ReinforcArmy(string armyId, int soldierCount)
        {
            var army = GameManager.Instance?.GetArmy(armyId);
            if (army == null)
            {
                return false;
            }

            army.SoldierCount += soldierCount;

            Debug.Log($"Army reinforced: {army.Name} +{soldierCount}");

            return true;
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 勢力の軍数を取得
        /// </summary>
        public int GetArmyCount(string factionId)
        {
            if (GameManager.Instance?.GameData == null) return 0;

            return GameManager.Instance.GameData.Armies.Values
                .Count(a => a.FactionId == factionId);
        }

        /// <summary>
        /// 勢力の全軍を取得
        /// </summary>
        public List<Army> GetArmiesByFaction(string factionId)
        {
            if (GameManager.Instance?.GameData == null)
                return new List<Army>();

            return GameManager.Instance.GameData.Armies.Values
                .Where(a => a.FactionId == factionId)
                .ToList();
        }

        /// <summary>
        /// 領地にいる軍を取得
        /// </summary>
        public List<Army> GetArmiesAtTerritory(string territoryId)
        {
            if (GameManager.Instance?.GameData == null)
                return new List<Army>();

            return GameManager.Instance.GameData.Armies.Values
                .Where(a => a.TerritoryId == territoryId && !a.IsMoving)
                .ToList();
        }

        /// <summary>
        /// 領地にいる特定勢力の軍を取得
        /// </summary>
        public Army GetArmyAtTerritory(string territoryId, string factionId)
        {
            if (GameManager.Instance?.GameData == null)
                return null;

            return GameManager.Instance.GameData.Armies.Values
                .FirstOrDefault(a => a.TerritoryId == territoryId &&
                                     a.FactionId == factionId &&
                                     !a.IsMoving);
        }

        /// <summary>
        /// 勢力の総兵力を取得
        /// </summary>
        public int GetTotalSoldiers(string factionId)
        {
            if (GameManager.Instance?.GameData == null) return 0;

            return GameManager.Instance.GameData.Armies.Values
                .Where(a => a.FactionId == factionId)
                .Sum(a => a.SoldierCount);
        }

        /// <summary>
        /// 移動中の軍を取得
        /// </summary>
        public List<Army> GetMovingArmies(string factionId = null)
        {
            if (GameManager.Instance?.GameData == null)
                return new List<Army>();

            var query = GameManager.Instance.GameData.Armies.Values.Where(a => a.IsMoving);

            if (!string.IsNullOrEmpty(factionId))
            {
                query = query.Where(a => a.FactionId == factionId);
            }

            return query.ToList();
        }

        #endregion
    }
}
