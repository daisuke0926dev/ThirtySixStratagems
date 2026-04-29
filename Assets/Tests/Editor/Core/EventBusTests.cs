using System;
using NUnit.Framework;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.Tests.Editor.Core
{
    /// <summary>
    /// EventBusのテスト
    /// </summary>
    [TestFixture]
    public class EventBusTests
    {
        [SetUp]
        public void SetUp()
        {
            // 各テスト前にイベントをクリア
            EventBus.ClearAllEvents();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAllEvents();
        }

        #region OnTurnStarted Tests

        [Test]
        public void OnTurnStarted_WhenInvoked_CallsSubscribedHandler()
        {
            // Arrange
            int receivedTurn = -1;
            EventBus.OnTurnStarted += turn => receivedTurn = turn;

            // Act
            EventBus.TurnStarted(5);

            // Assert
            Assert.AreEqual(5, receivedTurn);
        }

        [Test]
        public void OnTurnStarted_WhenNoSubscribers_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => EventBus.TurnStarted(1));
        }

        [Test]
        public void OnTurnStarted_MultipleSubscribers_AllReceiveEvent()
        {
            // Arrange
            int count = 0;
            EventBus.OnTurnStarted += _ => count++;
            EventBus.OnTurnStarted += _ => count++;
            EventBus.OnTurnStarted += _ => count++;

            // Act
            EventBus.TurnStarted(1);

            // Assert
            Assert.AreEqual(3, count);
        }

        #endregion

        #region OnTurnEnded Tests

        [Test]
        public void OnTurnEnded_WhenInvoked_CallsSubscribedHandler()
        {
            // Arrange
            int receivedTurn = -1;
            EventBus.OnTurnEnded += turn => receivedTurn = turn;

            // Act
            EventBus.TurnEnded(10);

            // Assert
            Assert.AreEqual(10, receivedTurn);
        }

        #endregion

        #region OnTerritorySelected Tests

        [Test]
        public void OnTerritorySelected_WhenInvoked_PassesCorrectTerritoryId()
        {
            // Arrange
            string receivedId = null;
            EventBus.OnTerritorySelected += id => receivedId = id;

            // Act
            EventBus.TerritorySelected("territory_123");

            // Assert
            Assert.AreEqual("territory_123", receivedId);
        }

        [Test]
        public void OnTerritorySelected_WithNullId_HandlesGracefully()
        {
            // Arrange
            string receivedId = "initial";
            EventBus.OnTerritorySelected += id => receivedId = id;

            // Act
            EventBus.TerritorySelected(null);

            // Assert
            Assert.IsNull(receivedId);
        }

        #endregion

        #region OnStratagemExecuted Tests

        [Test]
        public void OnStratagemExecuted_WhenSuccessful_PassesCorrectArgs()
        {
            // Arrange
            StratagemEventArgs receivedArgs = null;
            EventBus.OnStratagemExecuted += args => receivedArgs = args;

            var eventArgs = new StratagemEventArgs
            {
                StratagemId = "strat_001",
                CasterFactionId = "faction_1",
                TargetId = "faction_2",
                Success = true
            };

            // Act
            EventBus.StratagemExecuted(eventArgs);

            // Assert
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual("strat_001", receivedArgs.StratagemId);
            Assert.AreEqual("faction_1", receivedArgs.CasterFactionId);
            Assert.AreEqual("faction_2", receivedArgs.TargetId);
            Assert.IsTrue(receivedArgs.Success);
        }

        [Test]
        public void OnStratagemExecuted_WhenFailed_PassesCorrectArgs()
        {
            // Arrange
            StratagemEventArgs receivedArgs = null;
            EventBus.OnStratagemExecuted += args => receivedArgs = args;

            var eventArgs = new StratagemEventArgs
            {
                StratagemId = "strat_002",
                Success = false
            };

            // Act
            EventBus.StratagemExecuted(eventArgs);

            // Assert
            Assert.IsNotNull(receivedArgs);
            Assert.IsFalse(receivedArgs.Success);
        }

        #endregion

        #region OnBattleStarted Tests

        [Test]
        public void OnBattleStarted_WhenInvoked_PassesBattleEventArgs()
        {
            // Arrange
            BattleEventArgs receivedArgs = null;
            EventBus.OnBattleStarted += args => receivedArgs = args;

            var eventArgs = new BattleEventArgs
            {
                AttackerArmyId = "army_wei_main",
                DefenderArmyId = "army_shu_main",
                TerritoryId = "hanzhong"
            };

            // Act
            EventBus.BattleStarted(eventArgs);

            // Assert
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual("army_wei_main", receivedArgs.AttackerArmyId);
            Assert.AreEqual("army_shu_main", receivedArgs.DefenderArmyId);
            Assert.AreEqual("hanzhong", receivedArgs.TerritoryId);
        }

        #endregion

        #region OnBattleEnded Tests

        [Test]
        public void OnBattleEnded_WhenInvoked_PassesBattleResultEventArgs()
        {
            // Arrange
            BattleResultEventArgs receivedArgs = null;
            EventBus.OnBattleEnded += args => receivedArgs = args;

            var eventArgs = new BattleResultEventArgs
            {
                VictorFactionId = "wei",
                AttackerLosses = 1000,
                DefenderLosses = 2000,
                TerritoryConquered = true
            };

            // Act
            EventBus.BattleEnded(eventArgs);

            // Assert
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual("wei", receivedArgs.VictorFactionId);
            Assert.AreEqual(1000, receivedArgs.AttackerLosses);
            Assert.AreEqual(2000, receivedArgs.DefenderLosses);
            Assert.IsTrue(receivedArgs.TerritoryConquered);
        }

        #endregion

        #region OnTerritoryConquered Tests

        [Test]
        public void OnTerritoryConquered_WhenInvoked_PassesTerritoryConqueredEventArgs()
        {
            // Arrange
            TerritoryConqueredEventArgs receivedArgs = null;
            EventBus.OnTerritoryConquered += args => receivedArgs = args;

            var eventArgs = new TerritoryConqueredEventArgs
            {
                TerritoryId = "luoyang",
                PreviousOwnerId = "han",
                NewOwnerId = "wei"
            };

            // Act
            EventBus.TerritoryConquered(eventArgs);

            // Assert
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual("luoyang", receivedArgs.TerritoryId);
            Assert.AreEqual("han", receivedArgs.PreviousOwnerId);
            Assert.AreEqual("wei", receivedArgs.NewOwnerId);
        }

        #endregion

        #region Unsubscribe Tests

        [Test]
        public void Unsubscribe_AfterUnsubscribing_DoesNotReceiveEvents()
        {
            // Arrange
            int callCount = 0;
            Action<int> handler = _ => callCount++;
            EventBus.OnTurnStarted += handler;

            // Act
            EventBus.TurnStarted(1);
            EventBus.OnTurnStarted -= handler;
            EventBus.TurnStarted(2);

            // Assert
            Assert.AreEqual(1, callCount);
        }

        #endregion

        #region ClearAllEvents Tests

        [Test]
        public void ClearAllEvents_WhenCalled_RemovesAllSubscribers()
        {
            // Arrange
            int count = 0;
            EventBus.OnTurnStarted += _ => count++;
            EventBus.OnTurnEnded += _ => count++;
            EventBus.OnGameStarted += () => count++;

            // Act
            EventBus.ClearAllEvents();
            EventBus.TurnStarted(1);
            EventBus.TurnEnded(1);
            EventBus.GameStarted();

            // Assert
            Assert.AreEqual(0, count);
        }

        #endregion
    }
}
