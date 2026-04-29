using System.Collections.Generic;
using NUnit.Framework;
using ThirtySixStratagems.Stratagem;

namespace ThirtySixStratagems.Tests.Editor.Stratagem
{
    /// <summary>
    /// StratagemCheckResultのテスト
    /// </summary>
    [TestFixture]
    public class StratagemCheckResultTests
    {
        #region CanUse Tests

        [Test]
        public void CanUse_WhenNoFailedConditions_ReturnsTrue()
        {
            // Arrange
            var result = new StratagemCheckResult
            {
                StratagemId = "test_stratagem",
                CanUse = true,
                FailedConditions = new List<ConditionCheckResult>()
            };

            // Assert
            Assert.IsTrue(result.CanUse);
            Assert.AreEqual(0, result.FailedConditions.Count);
        }

        [Test]
        public void CanUse_WhenHasFailedConditions_ReturnsFalse()
        {
            // Arrange
            var result = new StratagemCheckResult
            {
                StratagemId = "test_stratagem",
                CanUse = false,
                FailedConditions = new List<ConditionCheckResult>
                {
                    new ConditionCheckResult
                    {
                        ConditionType = "StratagemPoints",
                        IsMet = false,
                        Description = "計略ポイント不足"
                    }
                }
            };

            // Assert
            Assert.IsFalse(result.CanUse);
            Assert.AreEqual(1, result.FailedConditions.Count);
        }

        #endregion

        #region GetFailureReasons Tests

        [Test]
        public void GetFailureReasons_WhenNoFailedConditions_ReturnsNull()
        {
            // Arrange
            var result = new StratagemCheckResult
            {
                FailedConditions = new List<ConditionCheckResult>()
            };

            // Act
            var reasons = result.GetFailureReasons();

            // Assert
            Assert.IsNull(reasons);
        }

        [Test]
        public void GetFailureReasons_WhenNullFailedConditions_ReturnsNull()
        {
            // Arrange
            var result = new StratagemCheckResult
            {
                FailedConditions = null
            };

            // Act
            var reasons = result.GetFailureReasons();

            // Assert
            Assert.IsNull(reasons);
        }

        [Test]
        public void GetFailureReasons_WhenSingleFailedCondition_ReturnsSingleReason()
        {
            // Arrange
            var result = new StratagemCheckResult
            {
                FailedConditions = new List<ConditionCheckResult>
                {
                    new ConditionCheckResult
                    {
                        ConditionType = "Gold",
                        Description = "金不足"
                    }
                }
            };

            // Act
            var reasons = result.GetFailureReasons();

            // Assert
            Assert.AreEqual("金不足", reasons);
        }

        [Test]
        public void GetFailureReasons_WhenMultipleFailedConditions_ReturnsJoinedReasons()
        {
            // Arrange
            var result = new StratagemCheckResult
            {
                FailedConditions = new List<ConditionCheckResult>
                {
                    new ConditionCheckResult { Description = "金不足" },
                    new ConditionCheckResult { Description = "計略ポイント不足" },
                    new ConditionCheckResult { Description = "対象が無効" }
                }
            };

            // Act
            var reasons = result.GetFailureReasons();

            // Assert
            Assert.IsTrue(reasons.Contains("金不足"));
            Assert.IsTrue(reasons.Contains("計略ポイント不足"));
            Assert.IsTrue(reasons.Contains("対象が無効"));
        }

        #endregion

        #region ConditionCheckResult Tests

        [Test]
        public void ConditionCheckResult_WhenMet_IsMettIsTrue()
        {
            // Arrange
            var condition = new ConditionCheckResult
            {
                ConditionType = "Gold",
                IsMet = true,
                CurrentValue = 5000,
                RequiredValue = 1000
            };

            // Assert
            Assert.IsTrue(condition.IsMet);
            Assert.Greater(condition.CurrentValue, condition.RequiredValue);
        }

        [Test]
        public void ConditionCheckResult_WhenNotMet_HasCorrectValues()
        {
            // Arrange
            var condition = new ConditionCheckResult
            {
                ConditionType = "StratagemPoints",
                IsMet = false,
                CurrentValue = 3,
                RequiredValue = 5,
                Description = "計略ポイント不足（現在: 3 / 必要: 5）"
            };

            // Assert
            Assert.IsFalse(condition.IsMet);
            Assert.Less(condition.CurrentValue, condition.RequiredValue);
            Assert.IsNotNull(condition.Description);
        }

        #endregion

        #region StratagemAvailability Tests

        [Test]
        public void StratagemAvailability_WhenAvailable_IsAvailableTrue()
        {
            // Arrange
            var availability = new StratagemAvailability
            {
                IsAvailable = true,
                CheckResult = new StratagemCheckResult
                {
                    CanUse = true,
                    FailedConditions = new List<ConditionCheckResult>()
                }
            };

            // Assert
            Assert.IsTrue(availability.IsAvailable);
            Assert.IsTrue(availability.CheckResult.CanUse);
        }

        [Test]
        public void StratagemAvailability_WhenNotAvailable_IsAvailableFalse()
        {
            // Arrange
            var availability = new StratagemAvailability
            {
                IsAvailable = false,
                CheckResult = new StratagemCheckResult
                {
                    CanUse = false,
                    FailedConditions = new List<ConditionCheckResult>
                    {
                        new ConditionCheckResult
                        {
                            ConditionType = "Phase",
                            IsMet = false
                        }
                    }
                }
            };

            // Assert
            Assert.IsFalse(availability.IsAvailable);
            Assert.IsFalse(availability.CheckResult.CanUse);
        }

        #endregion
    }
}
