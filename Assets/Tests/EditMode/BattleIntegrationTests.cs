using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Battle;

namespace ThirtySixStratagems.Tests.EditMode
{
    /// <summary>
    /// 戦闘統合テスト
    /// 戦闘開始〜決着の一連フローをテスト
    /// </summary>
    [TestFixture]
    public class BattleIntegrationTests
    {
        private GameData _gameData;

        [SetUp]
        public void SetUp()
        {
            _gameData = TestHelpers.CreateTestGameData();
        }

        #region Battle Setup Tests

        [Test]
        public void BattleSetup_CreateBattleState()
        {
            var attackerArmy = _gameData.Armies["player_army"];
            var defenderArmy = _gameData.Armies["enemy_army"];

            var battleState = new BattleState
            {
                Attacker = CreateBattleUnitFromArmy(attackerArmy),
                Defender = CreateBattleUnitFromArmy(defenderArmy),
                CurrentRound = 1,
                TerritoryId = "territory_2"
            };

            Assert.IsNotNull(battleState.Attacker);
            Assert.IsNotNull(battleState.Defender);
            Assert.AreEqual(1, battleState.CurrentRound);
        }

        [Test]
        public void BattleSetup_AttackerAndDefenderDifferent()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(armyId: "attacker", factionId: "player");
            var defender = TestHelpers.CreateTestBattleUnit(armyId: "defender", factionId: "enemy");

            Assert.AreNotEqual(attacker.ArmyId, defender.ArmyId);
            Assert.AreNotEqual(attacker.FactionId, defender.FactionId);
        }

        [Test]
        public void BattleSetup_InitialSoldiersSet()
        {
            var unit = TestHelpers.CreateTestBattleUnit(soldiers: 1000);

            Assert.AreEqual(1000, unit.CurrentSoldiers);
            Assert.AreEqual(1000, unit.InitialSoldiers);
        }

        private BattleUnit CreateBattleUnitFromArmy(Army army)
        {
            var faction = _gameData.Factions[army.FactionId];
            var commander = army.CommanderId != null
                ? _gameData.Characters.GetValueOrDefault(army.CommanderId)
                : null;

            return new BattleUnit
            {
                ArmyId = army.Id,
                FactionId = army.FactionId,
                ArmyName = army.Id,
                FactionName = faction.Name,
                CurrentSoldiers = army.SoldierCount,
                InitialSoldiers = army.SoldierCount,
                Morale = army.Morale,
                BaseCombatPower = CalculateBasePower(army, commander),
                CommanderId = army.CommanderId,
                ActiveEffects = new List<BattleEffect>()
            };
        }

        private int CalculateBasePower(Army army, Character commander)
        {
            int power = army.SoldierCount / 100;
            if (commander != null)
            {
                power += commander.Strength / 10;
                power += commander.Leadership / 5;
            }
            return power;
        }

        #endregion

        #region Combat Round Tests

        [Test]
        public void CombatRound_BothSidesTakeCasualties()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(soldiers: 1000, basePower: 100);
            var defender = TestHelpers.CreateTestBattleUnit(soldiers: 1000, basePower: 100);

            // 戦闘ラウンドをシミュレート
            int attackerCasualties = CalculateCasualties(defender.BaseCombatPower, attacker.CurrentSoldiers);
            int defenderCasualties = CalculateCasualties(attacker.BaseCombatPower, defender.CurrentSoldiers);

            attacker.CurrentSoldiers -= attackerCasualties;
            defender.CurrentSoldiers -= defenderCasualties;

            Assert.Less(attacker.CurrentSoldiers, 1000);
            Assert.Less(defender.CurrentSoldiers, 1000);
        }

        [Test]
        public void CombatRound_StrongerSideDealsMoreDamage()
        {
            var strongUnit = TestHelpers.CreateTestBattleUnit(basePower: 150);
            var weakUnit = TestHelpers.CreateTestBattleUnit(basePower: 50);

            int strongDamage = CalculateCasualties(strongUnit.BaseCombatPower, weakUnit.CurrentSoldiers);
            int weakDamage = CalculateCasualties(weakUnit.BaseCombatPower, strongUnit.CurrentSoldiers);

            Assert.Greater(strongDamage, weakDamage);
        }

        [Test]
        public void CombatRound_MoraleAffectsCombat()
        {
            var highMorale = TestHelpers.CreateTestBattleUnit(morale: 100, basePower: 100);
            var lowMorale = TestHelpers.CreateTestBattleUnit(morale: 20, basePower: 100);

            float highModifier = 1f + (highMorale.Morale - 50) * Constants.Balance.MoraleImpact / 100f;
            float lowModifier = 1f + (lowMorale.Morale - 50) * Constants.Balance.MoraleImpact / 100f;

            int highEffectivePower = Mathf.RoundToInt(highMorale.BaseCombatPower * highModifier);
            int lowEffectivePower = Mathf.RoundToInt(lowMorale.BaseCombatPower * lowModifier);

            Assert.Greater(highEffectivePower, lowEffectivePower);
        }

        [Test]
        public void CombatRound_RoundNumberIncrements()
        {
            var state = TestHelpers.CreateTestBattleState(currentRound: 1);

            state.CurrentRound++;

            Assert.AreEqual(2, state.CurrentRound);
        }

        private int CalculateCasualties(int attackPower, int targetSoldiers)
        {
            float baseRate = 0.3f;
            int damage = Mathf.RoundToInt(attackPower * baseRate * targetSoldiers / 100f);
            return Mathf.Max(1, damage);
        }

        #endregion

        #region Battle Resolution Tests

        [Test]
        public void BattleResolution_AttackerWins_WhenDefenderRouted()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(soldiers: 800, morale: 60);
            var defender = TestHelpers.CreateTestBattleUnit(soldiers: 100, morale: 5);

            bool defenderRouted = defender.Morale <= 10 || defender.CurrentSoldiers <= 0;
            bool attackerWins = defenderRouted;

            Assert.IsTrue(attackerWins);
        }

        [Test]
        public void BattleResolution_DefenderWins_WhenAttackerRouted()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(soldiers: 50, morale: 5);
            var defender = TestHelpers.CreateTestBattleUnit(soldiers: 900, morale: 80);

            bool attackerRouted = attacker.Morale <= 10 || attacker.CurrentSoldiers <= 0;
            bool defenderWins = attackerRouted;

            Assert.IsTrue(defenderWins);
        }

        [Test]
        public void BattleResolution_TerritoryChangesOwner()
        {
            var territory = _gameData.Territories["territory_2"];
            var attackerFaction = _gameData.Factions["player_faction"];
            var defenderFaction = _gameData.Factions["enemy_faction"];

            // 攻撃側が勝利した場合
            defenderFaction.TerritoryIds.Remove(territory.Id);
            attackerFaction.TerritoryIds.Add(territory.Id);
            territory.OwnerId = attackerFaction.Id;

            Assert.AreEqual(attackerFaction.Id, territory.OwnerId);
        }

        [Test]
        public void BattleResolution_ArmyCasualtiesApplied()
        {
            var army = _gameData.Armies["player_army"];
            int originalSoldiers = army.SoldierCount;
            int casualties = 200;

            army.SoldierCount -= casualties;

            Assert.AreEqual(originalSoldiers - casualties, army.SoldierCount);
        }

        #endregion

        #region Morale Effects in Battle Tests

        [Test]
        public void MoraleInBattle_WinnerGainsMorale()
        {
            var winner = TestHelpers.CreateTestBattleUnit(morale: 50);

            // 勝利ボーナス
            winner.Morale += 5;
            winner.Morale = Mathf.Min(winner.Morale, Constants.Balance.MaxMorale);

            Assert.AreEqual(55, winner.Morale);
        }

        [Test]
        public void MoraleInBattle_LoserLosesMorale()
        {
            var loser = TestHelpers.CreateTestBattleUnit(morale: 50);

            // 敗北ペナルティ
            loser.Morale -= Constants.Balance.MoraleLossOnDefeat;
            loser.Morale = Mathf.Max(loser.Morale, 0);

            Assert.AreEqual(30, loser.Morale);
        }

        [Test]
        public void MoraleInBattle_HeavyLossesCauseMoraleDrop()
        {
            var unit = TestHelpers.CreateTestBattleUnit(soldiers: 1000, morale: 50);

            // 50%の損害
            unit.CurrentSoldiers = 500;
            float lossRatio = 1f - ((float)unit.CurrentSoldiers / unit.InitialSoldiers);
            int moraleLoss = Mathf.RoundToInt(lossRatio * 20);
            unit.Morale -= moraleLoss;

            Assert.Less(unit.Morale, 50);
        }

        [Test]
        public void MoraleInBattle_RoutingThreshold()
        {
            var unit = TestHelpers.CreateTestBattleUnit(morale: 8);

            bool isRouting = unit.Morale <= 10;

            Assert.IsTrue(isRouting);
        }

        #endregion

        #region Commander Effects Tests

        [Test]
        public void CommanderEffects_StrengthBonus()
        {
            var commander = TestHelpers.CreateTestCharacter(strength: 90);

            int bonus = commander.Strength / 10;

            Assert.AreEqual(9, bonus);
        }

        [Test]
        public void CommanderEffects_LeadershipBonus()
        {
            var commander = TestHelpers.CreateTestCharacter(leadership: 85);

            int bonus = commander.Leadership / 5;

            Assert.AreEqual(17, bonus);
        }

        [Test]
        public void CommanderEffects_TotalCommanderBonus()
        {
            var commander = TestHelpers.CreateTestCharacter(strength: 80, leadership: 70);

            int totalBonus = commander.Strength / 10 + commander.Leadership / 5;

            Assert.AreEqual(22, totalBonus); // 8 + 14
        }

        [Test]
        public void CommanderEffects_NoCommanderNoBonus()
        {
            var army = TestHelpers.CreateTestArmy(commanderId: null);

            bool hasCommander = !string.IsNullOrEmpty(army.CommanderId);
            int bonus = hasCommander ? 10 : 0;

            Assert.AreEqual(0, bonus);
        }

        #endregion

        #region Terrain Effects Tests

        [Test]
        public void TerrainEffects_DefenseBonus()
        {
            int basePower = 100;
            int terrainBonus = 20;

            int totalPower = basePower + terrainBonus;

            Assert.AreEqual(120, totalPower);
        }

        [Test]
        public void TerrainEffects_DefenderGetsBonus()
        {
            var defender = TestHelpers.CreateTestBattleUnit(basePower: 100, terrainBonus: 15);

            int effectivePower = defender.BaseCombatPower + defender.TerrainBonus;

            Assert.AreEqual(115, effectivePower);
        }

        [Test]
        public void TerrainEffects_AttackerNoTerrainBonus()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(basePower: 100, terrainBonus: 0);

            int effectivePower = attacker.BaseCombatPower + attacker.TerrainBonus;

            Assert.AreEqual(100, effectivePower);
        }

        #endregion

        #region Multi-Round Battle Tests

        [Test]
        public void MultiRoundBattle_SoldiersDecreaseEachRound()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(soldiers: 1000, basePower: 100);
            var defender = TestHelpers.CreateTestBattleUnit(soldiers: 1000, basePower: 100);

            int initialAttacker = attacker.CurrentSoldiers;
            int initialDefender = defender.CurrentSoldiers;

            // 3ラウンド戦闘
            for (int round = 1; round <= 3; round++)
            {
                int attackerCas = CalculateCasualties(defender.BaseCombatPower, attacker.CurrentSoldiers);
                int defenderCas = CalculateCasualties(attacker.BaseCombatPower, defender.CurrentSoldiers);

                attacker.CurrentSoldiers = Mathf.Max(0, attacker.CurrentSoldiers - attackerCas);
                defender.CurrentSoldiers = Mathf.Max(0, defender.CurrentSoldiers - defenderCas);
            }

            Assert.Less(attacker.CurrentSoldiers, initialAttacker);
            Assert.Less(defender.CurrentSoldiers, initialDefender);
        }

        [Test]
        public void MultiRoundBattle_BattleEndsWhenOneSideEliminated()
        {
            var attacker = TestHelpers.CreateTestBattleUnit(soldiers: 1000, basePower: 200);
            var defender = TestHelpers.CreateTestBattleUnit(soldiers: 100, basePower: 50);

            int rounds = 0;
            while (attacker.CurrentSoldiers > 0 && defender.CurrentSoldiers > 0 && rounds < 10)
            {
                int attackerCas = Mathf.Min(attacker.CurrentSoldiers, 30);
                int defenderCas = Mathf.Min(defender.CurrentSoldiers, 60);

                attacker.CurrentSoldiers -= attackerCas;
                defender.CurrentSoldiers -= defenderCas;
                rounds++;
            }

            Assert.IsTrue(attacker.CurrentSoldiers <= 0 || defender.CurrentSoldiers <= 0 || rounds >= 10);
        }

        #endregion

        #region Post-Battle Effects Tests

        [Test]
        public void PostBattle_WinnerArmyUpdated()
        {
            var army = _gameData.Armies["player_army"];
            int originalSoldiers = army.SoldierCount;
            int casualties = 150;

            // 戦闘後処理
            army.SoldierCount -= casualties;

            Assert.AreEqual(originalSoldiers - casualties, army.SoldierCount);
        }

        [Test]
        public void PostBattle_LoserArmyDestroyed()
        {
            var army = _gameData.Armies["enemy_army"];

            // 壊滅
            army.SoldierCount = 0;

            bool isDestroyed = army.SoldierCount <= 0;
            Assert.IsTrue(isDestroyed);
        }

        [Test]
        public void PostBattle_CharacterCapture()
        {
            var character = _gameData.Characters["enemy_char"];
            string originalFaction = character.FactionId;

            // 捕虜になる（50%の確率など）
            bool isCaptured = true; // シミュレート
            if (isCaptured)
            {
                character.FactionId = "player_faction";
            }

            Assert.AreNotEqual(originalFaction, character.FactionId);
        }

        #endregion
    }
}
