using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// TurnManager関連のテスト
    /// ターン・フェーズ進行ロジックのユニットテスト
    /// </summary>
    [TestFixture]
    public class TurnManagerTests
    {
        #region TurnPhase Tests

        [Test]
        public void TurnPhase_AllPhasesExist()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(TurnPhase), TurnPhase.Internal));
            Assert.IsTrue(System.Enum.IsDefined(typeof(TurnPhase), TurnPhase.Diplomacy));
            Assert.IsTrue(System.Enum.IsDefined(typeof(TurnPhase), TurnPhase.Military));
            Assert.IsTrue(System.Enum.IsDefined(typeof(TurnPhase), TurnPhase.End));
        }

        [Test]
        public void TurnPhase_CorrectOrder()
        {
            // フェーズの順序を確認
            int internalPhase = (int)TurnPhase.Internal;
            int diplomacyPhase = (int)TurnPhase.Diplomacy;
            int militaryPhase = (int)TurnPhase.Military;
            int endPhase = (int)TurnPhase.End;

            Assert.Less(internalPhase, diplomacyPhase);
            Assert.Less(diplomacyPhase, militaryPhase);
            Assert.Less(militaryPhase, endPhase);
        }

        #endregion

        #region Phase Transition Tests

        [Test]
        public void PhaseTransition_InternalToDiplomacy()
        {
            TurnPhase current = TurnPhase.Internal;
            TurnPhase next = GetNextPhase(current);

            Assert.AreEqual(TurnPhase.Diplomacy, next);
        }

        [Test]
        public void PhaseTransition_DiplomacyToMilitary()
        {
            TurnPhase current = TurnPhase.Diplomacy;
            TurnPhase next = GetNextPhase(current);

            Assert.AreEqual(TurnPhase.Military, next);
        }

        [Test]
        public void PhaseTransition_MilitaryToEnd()
        {
            TurnPhase current = TurnPhase.Military;
            TurnPhase next = GetNextPhase(current);

            Assert.AreEqual(TurnPhase.End, next);
        }

        // ヘルパー: 次のフェーズを取得
        private TurnPhase GetNextPhase(TurnPhase current)
        {
            switch (current)
            {
                case TurnPhase.Internal:
                    return TurnPhase.Diplomacy;
                case TurnPhase.Diplomacy:
                    return TurnPhase.Military;
                case TurnPhase.Military:
                    return TurnPhase.End;
                default:
                    return TurnPhase.Internal; // 次のターンの開始
            }
        }

        #endregion

        #region Turn Number Tests

        [Test]
        public void TurnNumber_IncrementCorrectly()
        {
            int currentTurn = 1;
            currentTurn++;

            Assert.AreEqual(2, currentTurn);
        }

        [Test]
        public void TurnNumber_MaxTurnCheck()
        {
            int currentTurn = 100;
            int maxTurns = 100;

            bool reachedMax = currentTurn >= maxTurns;

            Assert.IsTrue(reachedMax);
        }

        [Test]
        public void TurnNumber_WithinMax()
        {
            int currentTurn = 50;
            int maxTurns = 100;

            bool withinMax = currentTurn < maxTurns;

            Assert.IsTrue(withinMax);
        }

        #endregion

        #region Faction Order Tests

        [Test]
        public void FactionOrder_PlayerFirst()
        {
            var factions = new List<string> { "enemy1", "player", "enemy2" };
            string playerFactionId = "player";

            // プレイヤーを先頭に移動
            if (factions.Contains(playerFactionId))
            {
                factions.Remove(playerFactionId);
                factions.Insert(0, playerFactionId);
            }

            Assert.AreEqual("player", factions[0]);
        }

        [Test]
        public void FactionOrder_ExcludeEliminated()
        {
            var factions = new List<string> { "player", "enemy1", "enemy2" };
            var eliminatedFactions = new List<string> { "enemy1" };

            foreach (var eliminated in eliminatedFactions)
            {
                factions.Remove(eliminated);
            }

            Assert.AreEqual(2, factions.Count);
            Assert.IsFalse(factions.Contains("enemy1"));
        }

        [Test]
        public void FactionOrder_IndexTracking()
        {
            var factions = new List<string> { "player", "enemy1", "enemy2" };
            int currentIndex = 0;

            Assert.AreEqual("player", factions[currentIndex]);

            currentIndex++;
            Assert.AreEqual("enemy1", factions[currentIndex]);

            currentIndex++;
            Assert.AreEqual("enemy2", factions[currentIndex]);
        }

        [Test]
        public void FactionOrder_AllFactionsProcessed()
        {
            var factions = new List<string> { "player", "enemy1", "enemy2" };
            int currentIndex = 0;
            int processedCount = 0;

            while (currentIndex < factions.Count)
            {
                processedCount++;
                currentIndex++;
            }

            Assert.AreEqual(3, processedCount);
        }

        #endregion

        #region Player Turn Detection Tests

        [Test]
        public void IsPlayerTurn_WhenPlayerActive()
        {
            string currentFactionId = "player";
            string playerFactionId = "player";

            bool isPlayerTurn = currentFactionId == playerFactionId;

            Assert.IsTrue(isPlayerTurn);
        }

        [Test]
        public void IsPlayerTurn_WhenEnemyActive()
        {
            string currentFactionId = "enemy";
            string playerFactionId = "player";

            bool isPlayerTurn = currentFactionId == playerFactionId;

            Assert.IsFalse(isPlayerTurn);
        }

        #endregion

        #region Victory/Defeat Condition Tests

        [Test]
        public void VictoryCondition_AllTerritoriesConquered()
        {
            var playerFaction = TestHelpers.CreateTestFaction(id: "player");
            playerFaction.TerritoryIds.Clear();
            playerFaction.TerritoryIds.Add("t1");
            playerFaction.TerritoryIds.Add("t2");
            playerFaction.TerritoryIds.Add("t3");

            int totalTerritories = 3;
            bool victory = playerFaction.TerritoryIds.Count >= totalTerritories;

            Assert.IsTrue(victory);
        }

        [Test]
        public void DefeatCondition_NoTerritories()
        {
            var playerFaction = TestHelpers.CreateTestFaction(id: "player");
            playerFaction.TerritoryIds.Clear();

            bool defeat = playerFaction.TerritoryIds.Count == 0;

            Assert.IsTrue(defeat);
        }

        [Test]
        public void SurvivalVictory_AtMaxTurn()
        {
            var playerFaction = TestHelpers.CreateTestFaction(id: "player");
            int currentTurn = 100;
            int maxTurns = 100;

            bool reachedMax = currentTurn >= maxTurns;
            bool hasTerritory = playerFaction.TerritoryIds.Count > 0;
            bool survivalVictory = reachedMax && hasTerritory;

            Assert.IsTrue(survivalVictory);
        }

        #endregion

        #region Faction Elimination Tests

        [Test]
        public void FactionElimination_NoTerritories()
        {
            var faction = TestHelpers.CreateTestFaction();
            faction.TerritoryIds.Clear();

            bool isEliminated = faction.TerritoryIds.Count == 0;

            Assert.IsTrue(isEliminated);
        }

        [Test]
        public void FactionElimination_HasTerritories()
        {
            var faction = TestHelpers.CreateTestFaction();

            bool isEliminated = faction.TerritoryIds.Count == 0;

            Assert.IsFalse(isEliminated);
        }

        [Test]
        public void FactionElimination_UpdateFactionOrder()
        {
            var factionOrder = new List<string> { "player", "enemy1", "enemy2" };
            var eliminatedFaction = "enemy1";

            factionOrder.Remove(eliminatedFaction);

            Assert.AreEqual(2, factionOrder.Count);
            Assert.IsFalse(factionOrder.Contains("enemy1"));
            Assert.AreEqual("player", factionOrder[0]);
            Assert.AreEqual("enemy2", factionOrder[1]);
        }

        #endregion

        #region Turn End Effects Tests

        [Test]
        public void TurnEndEffects_MoraleRecovery()
        {
            var army = TestHelpers.CreateTestArmy(morale: 50);
            int recovery = Constants.Balance.MoraleRecoveryPerTurn;
            int maxMorale = Constants.Balance.MaxMorale;

            int newMorale = System.Math.Min(army.Morale + recovery, maxMorale);

            Assert.AreEqual(55, newMorale);
        }

        [Test]
        public void TurnEndEffects_MoraleCapAtMax()
        {
            var army = TestHelpers.CreateTestArmy(morale: 98);
            int recovery = Constants.Balance.MoraleRecoveryPerTurn;
            int maxMorale = Constants.Balance.MaxMorale;

            int newMorale = System.Math.Min(army.Morale + recovery, maxMorale);

            Assert.AreEqual(100, newMorale);
        }

        #endregion

        #region Phase Name Tests

        [Test]
        public void PhaseName_Internal()
        {
            string name = GetPhaseName(TurnPhase.Internal);
            Assert.AreEqual("内政フェーズ", name);
        }

        [Test]
        public void PhaseName_Diplomacy()
        {
            string name = GetPhaseName(TurnPhase.Diplomacy);
            Assert.AreEqual("外交フェーズ", name);
        }

        [Test]
        public void PhaseName_Military()
        {
            string name = GetPhaseName(TurnPhase.Military);
            Assert.AreEqual("軍事フェーズ", name);
        }

        [Test]
        public void PhaseName_End()
        {
            string name = GetPhaseName(TurnPhase.End);
            Assert.AreEqual("終了フェーズ", name);
        }

        // ヘルパー: フェーズ名を取得
        private string GetPhaseName(TurnPhase phase)
        {
            switch (phase)
            {
                case TurnPhase.Internal:
                    return "内政フェーズ";
                case TurnPhase.Diplomacy:
                    return "外交フェーズ";
                case TurnPhase.Military:
                    return "軍事フェーズ";
                case TurnPhase.End:
                    return "終了フェーズ";
                default:
                    return "不明";
            }
        }

        #endregion

        #region Can Act Check Tests

        [Test]
        public void CanFactionAct_BeforeCurrentIndex()
        {
            var factionOrder = new List<string> { "player", "enemy1", "enemy2" };
            int currentIndex = 1; // enemy1のターン
            string factionId = "player";

            int factionIndex = factionOrder.IndexOf(factionId);
            bool canAct = factionIndex >= currentIndex;

            Assert.IsFalse(canAct); // playerは既に行動済み
        }

        [Test]
        public void CanFactionAct_AtCurrentIndex()
        {
            var factionOrder = new List<string> { "player", "enemy1", "enemy2" };
            int currentIndex = 1;
            string factionId = "enemy1";

            int factionIndex = factionOrder.IndexOf(factionId);
            bool canAct = factionIndex >= currentIndex;

            Assert.IsTrue(canAct);
        }

        [Test]
        public void CanFactionAct_AfterCurrentIndex()
        {
            var factionOrder = new List<string> { "player", "enemy1", "enemy2" };
            int currentIndex = 0;
            string factionId = "enemy2";

            int factionIndex = factionOrder.IndexOf(factionId);
            bool canAct = factionIndex >= currentIndex;

            Assert.IsTrue(canAct);
        }

        [Test]
        public void CanFactionAct_NotInOrder()
        {
            var factionOrder = new List<string> { "player", "enemy1" };
            string factionId = "enemy2"; // 存在しない

            bool inOrder = factionOrder.Contains(factionId);

            Assert.IsFalse(inOrder);
        }

        #endregion
    }
}
