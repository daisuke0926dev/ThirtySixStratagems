using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Stratagem
{
    /// <summary>
    /// 計略管理システム
    /// 計略の実行、成功判定、効果適用を管理
    /// </summary>
    public class StratagemManager : MonoBehaviour
    {
        public static StratagemManager Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private StratagemDatabase _stratagemDatabase;

        // イベント
        public event Action<StratagemExecutionContext> OnStratagemPrepared;
        public event Action<StratagemExecutionResult> OnStratagemExecuted;
        public event Action<StratagemExecutionResult> OnStratagemSucceeded;
        public event Action<StratagemExecutionResult> OnStratagemFailed;

        // 現在実行中の計略
        private StratagemExecutionContext _currentExecution;

        /// <summary>
        /// 計略データベース
        /// </summary>
        public StratagemDatabase Database => _stratagemDatabase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // データベースの取得
            if (_stratagemDatabase == null && GameManager.Instance != null)
            {
                _stratagemDatabase = GameManager.Instance.StratagemDatabase;
            }
        }

        #region Stratagem Execution

        /// <summary>
        /// 計略を準備（実行前確認）
        /// </summary>
        public StratagemPrepareResult PrepareStratagem(string stratagemId, string casterFactionId,
            string casterCharacterId, string targetId)
        {
            var result = new StratagemPrepareResult();

            // 計略データを取得
            var stratagemData = GetStratagemData(stratagemId);
            if (stratagemData == null)
            {
                result.CanExecute = false;
                result.FailureReason = "計略が見つかりません";
                return result;
            }

            // 発動者の確認
            var casterFaction = GameManager.Instance?.GetFaction(casterFactionId);
            if (casterFaction == null)
            {
                result.CanExecute = false;
                result.FailureReason = "発動勢力が見つかりません";
                return result;
            }

            var casterCharacter = GameManager.Instance?.GetCharacter(casterCharacterId);

            // コストチェック
            if (casterFaction.StratagemPoints < stratagemData.CostSP)
            {
                result.CanExecute = false;
                result.FailureReason = $"計略ポイントが不足しています（必要: {stratagemData.CostSP}）";
                return result;
            }

            if (casterFaction.Gold < stratagemData.CostGold)
            {
                result.CanExecute = false;
                result.FailureReason = $"金が不足しています（必要: {stratagemData.CostGold}）";
                return result;
            }

            // 対象の有効性チェック
            if (!ValidateTarget(stratagemData, casterFactionId, targetId, out string targetError))
            {
                result.CanExecute = false;
                result.FailureReason = targetError;
                return result;
            }

            // 条件チェック
            var context = CreateConditionContext(casterFaction, casterCharacter, targetId);
            if (!stratagemData.CheckConditions(context))
            {
                result.CanExecute = false;
                result.FailureReason = "発動条件を満たしていません";
                return result;
            }

            // 成功率計算
            int successRate = CalculateSuccessRate(stratagemData, casterCharacter, targetId);

            result.CanExecute = true;
            result.StratagemData = stratagemData;
            result.SuccessRate = successRate;
            result.CostSP = stratagemData.CostSP;
            result.CostGold = stratagemData.CostGold;

            // 実行コンテキストを保存
            _currentExecution = new StratagemExecutionContext
            {
                StratagemData = stratagemData,
                CasterFactionId = casterFactionId,
                CasterCharacterId = casterCharacterId,
                TargetId = targetId,
                SuccessRate = successRate
            };

            OnStratagemPrepared?.Invoke(_currentExecution);

            return result;
        }

        /// <summary>
        /// 計略を実行
        /// </summary>
        public StratagemExecutionResult ExecuteStratagem(string stratagemId, string casterFactionId,
            string casterCharacterId, string targetId)
        {
            // 準備確認
            var prepareResult = PrepareStratagem(stratagemId, casterFactionId, casterCharacterId, targetId);
            if (!prepareResult.CanExecute)
            {
                return new StratagemExecutionResult
                {
                    Success = false,
                    FailureReason = prepareResult.FailureReason
                };
            }

            return ExecutePreparedStratagem();
        }

        /// <summary>
        /// 準備済みの計略を実行
        /// </summary>
        public StratagemExecutionResult ExecutePreparedStratagem()
        {
            if (_currentExecution == null)
            {
                return new StratagemExecutionResult
                {
                    Success = false,
                    FailureReason = "計略が準備されていません"
                };
            }

            var execution = _currentExecution;
            _currentExecution = null;

            // コスト消費
            var casterFaction = GameManager.Instance?.GetFaction(execution.CasterFactionId);
            if (casterFaction != null)
            {
                ResourceManager.Instance?.ConsumeResource(
                    execution.CasterFactionId, ResourceType.StratagemPoints, execution.StratagemData.CostSP);

                if (execution.StratagemData.CostGold > 0)
                {
                    ResourceManager.Instance?.ConsumeResource(
                        execution.CasterFactionId, ResourceType.Gold, execution.StratagemData.CostGold);
                }
            }

            // 成功判定
            bool succeeded = RollSuccess(execution.SuccessRate);

            var result = new StratagemExecutionResult
            {
                StratagemData = execution.StratagemData,
                CasterFactionId = execution.CasterFactionId,
                CasterCharacterId = execution.CasterCharacterId,
                TargetId = execution.TargetId,
                Success = succeeded,
                SuccessRate = execution.SuccessRate
            };

            // EventBus通知
            var eventArgs = new StratagemEventArgs
            {
                StratagemId = execution.StratagemData.StratagemId,
                CasterFactionId = execution.CasterFactionId,
                CasterCharacterId = execution.CasterCharacterId,
                TargetId = execution.TargetId,
                Success = succeeded
            };
            EventBus.StratagemExecuted(eventArgs);

            if (succeeded)
            {
                // 効果適用
                result.EffectResults = ApplyStratagemEffect(execution);
                result.FailureReason = null;

                Debug.Log($"[Stratagem] {execution.StratagemData.NameJP} succeeded!");

                eventArgs.Success = true;
                EventBus.StratagemSucceeded(eventArgs);
                OnStratagemSucceeded?.Invoke(result);
            }
            else
            {
                result.FailureReason = "計略は失敗しました";

                Debug.Log($"[Stratagem] {execution.StratagemData.NameJP} failed.");

                eventArgs.Success = false;
                eventArgs.FailureReason = result.FailureReason;
                EventBus.StratagemFailed(eventArgs);
                OnStratagemFailed?.Invoke(result);
            }

            OnStratagemExecuted?.Invoke(result);

            return result;
        }

        /// <summary>
        /// 準備をキャンセル
        /// </summary>
        public void CancelPreparedStratagem()
        {
            _currentExecution = null;
        }

        #endregion

        #region Success Rate Calculation

        /// <summary>
        /// 成功率を計算
        /// </summary>
        public int CalculateSuccessRate(StratagemData stratagem, Character caster, string targetId)
        {
            int baseRate = stratagem.BaseSuccessRate;

            // 知力ボーナス
            int intelligenceBonus = 0;
            if (caster != null)
            {
                // 知力80以上で+10%、90以上で+20%、100で+30%
                if (caster.Intelligence >= 100)
                    intelligenceBonus = 30;
                else if (caster.Intelligence >= 90)
                    intelligenceBonus = 20;
                else if (caster.Intelligence >= 80)
                    intelligenceBonus = 10;
                else if (caster.Intelligence >= 70)
                    intelligenceBonus = 5;
            }

            // カテゴリ補正
            int categoryBonus = GetCategoryBonus(stratagem.Category, caster);

            // 対象による補正
            int targetPenalty = GetTargetPenalty(targetId);

            int finalRate = Mathf.Clamp(baseRate + intelligenceBonus + categoryBonus - targetPenalty, 5, 95);

            return finalRate;
        }

        /// <summary>
        /// カテゴリによるボーナスを取得
        /// </summary>
        private int GetCategoryBonus(StratagemCategory category, Character caster)
        {
            if (caster == null) return 0;

            // 武将の得意カテゴリによるボーナス
            // TODO: キャラクターに得意カテゴリを追加
            return 0;
        }

        /// <summary>
        /// 対象による成功率ペナルティ
        /// </summary>
        private int GetTargetPenalty(string targetId)
        {
            // 対象の知力が高い場合はペナルティ
            var targetCharacter = GameManager.Instance?.GetCharacter(targetId);
            if (targetCharacter != null)
            {
                if (targetCharacter.Intelligence >= 90)
                    return 20;
                else if (targetCharacter.Intelligence >= 80)
                    return 10;
            }

            return 0;
        }

        /// <summary>
        /// 成功判定ロール
        /// </summary>
        private bool RollSuccess(int successRate)
        {
            int roll = UnityEngine.Random.Range(1, 101);
            return roll <= successRate;
        }

        #endregion

        #region Effect Application

        /// <summary>
        /// 計略効果を適用
        /// </summary>
        private List<StratagemEffectResult> ApplyStratagemEffect(StratagemExecutionContext execution)
        {
            var results = new List<StratagemEffectResult>();

            var effectResult = new StratagemEffectResult
            {
                EffectType = execution.StratagemData.PrimaryEffect,
                TargetId = execution.TargetId,
                Applied = false
            };

            // StratagemEffectProcessorに委譲
            if (StratagemEffectProcessor.Instance != null)
            {
                effectResult = StratagemEffectProcessor.Instance.ProcessEffect(
                    execution.StratagemData.PrimaryEffect,
                    execution.StratagemData.EffectValue,
                    execution.StratagemData.Duration,
                    execution.CasterFactionId,
                    execution.TargetId
                );
            }
            else
            {
                // フォールバック: 基本的な効果適用
                effectResult = ApplyBasicEffect(execution);
            }

            results.Add(effectResult);

            return results;
        }

        /// <summary>
        /// 基本的な効果適用（フォールバック）
        /// </summary>
        private StratagemEffectResult ApplyBasicEffect(StratagemExecutionContext execution)
        {
            var result = new StratagemEffectResult
            {
                EffectType = execution.StratagemData.PrimaryEffect,
                TargetId = execution.TargetId,
                Applied = true
            };

            Debug.Log($"[Stratagem Effect] {execution.StratagemData.PrimaryEffect} applied to {execution.TargetId}");

            return result;
        }

        #endregion

        #region Target Validation

        /// <summary>
        /// 対象の有効性を確認
        /// </summary>
        private bool ValidateTarget(StratagemData stratagem, string casterFactionId, string targetId, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(targetId) && stratagem.TargetType != StratagemTarget.Self)
            {
                error = "対象が指定されていません";
                return false;
            }

            switch (stratagem.TargetType)
            {
                case StratagemTarget.Self:
                    // 自分自身が対象、常に有効
                    return true;

                case StratagemTarget.EnemyFaction:
                    var enemyFaction = GameManager.Instance?.GetFaction(targetId);
                    if (enemyFaction == null)
                    {
                        error = "対象勢力が見つかりません";
                        return false;
                    }
                    if (targetId == casterFactionId)
                    {
                        error = "自勢力を対象にできません";
                        return false;
                    }
                    return true;

                case StratagemTarget.EnemyArmy:
                    var army = GameManager.Instance?.GetArmy(targetId);
                    if (army == null)
                    {
                        error = "対象軍が見つかりません";
                        return false;
                    }
                    if (army.FactionId == casterFactionId)
                    {
                        error = "自軍を対象にできません";
                        return false;
                    }
                    return true;

                case StratagemTarget.EnemyCharacter:
                    var character = GameManager.Instance?.GetCharacter(targetId);
                    if (character == null)
                    {
                        error = "対象武将が見つかりません";
                        return false;
                    }
                    if (character.FactionId == casterFactionId)
                    {
                        error = "自勢力の武将を対象にできません";
                        return false;
                    }
                    return true;

                case StratagemTarget.EnemyTerritory:
                    var territory = GameManager.Instance?.GetTerritory(targetId);
                    if (territory == null)
                    {
                        error = "対象領地が見つかりません";
                        return false;
                    }
                    if (territory.OwnerId == casterFactionId)
                    {
                        error = "自領地を対象にできません";
                        return false;
                    }
                    return true;

                case StratagemTarget.Any:
                    return true;

                default:
                    return true;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 計略データを取得
        /// </summary>
        public StratagemData GetStratagemData(string stratagemId)
        {
            if (_stratagemDatabase != null)
            {
                return _stratagemDatabase.GetStratagem(stratagemId);
            }
            return null;
        }

        /// <summary>
        /// 計略データを番号で取得
        /// </summary>
        public StratagemData GetStratagemByNumber(int number)
        {
            if (_stratagemDatabase != null)
            {
                return _stratagemDatabase.GetStratagemByNumber(number);
            }
            return null;
        }

        /// <summary>
        /// カテゴリの計略一覧を取得
        /// </summary>
        public List<StratagemData> GetStratagemsByCategory(StratagemCategory category)
        {
            if (_stratagemDatabase != null)
            {
                return _stratagemDatabase.GetStratagemsByCategory(category);
            }
            return new List<StratagemData>();
        }

        /// <summary>
        /// 使用可能な計略一覧を取得
        /// </summary>
        public List<StratagemData> GetAvailableStratagems(string factionId, string characterId)
        {
            var available = new List<StratagemData>();

            if (_stratagemDatabase == null) return available;

            var faction = GameManager.Instance?.GetFaction(factionId);
            var character = GameManager.Instance?.GetCharacter(characterId);

            if (faction == null) return available;

            foreach (var stratagem in _stratagemDatabase.AllStratagems)
            {
                // コストチェック
                if (faction.StratagemPoints < stratagem.CostSP) continue;
                if (faction.Gold < stratagem.CostGold) continue;

                // 条件チェック
                var context = CreateConditionContext(faction, character, null);
                if (!stratagem.CheckConditions(context)) continue;

                available.Add(stratagem);
            }

            return available;
        }

        /// <summary>
        /// 条件判定コンテキストを作成
        /// </summary>
        private ConditionContext CreateConditionContext(Faction faction, Character caster, string targetId)
        {
            var context = new ConditionContext
            {
                FactionGold = faction?.Gold ?? 0,
                StratagemPoints = faction?.StratagemPoints ?? 0,
                TerritoryCount = faction?.TerritoryIds.Count ?? 0,
                CasterIntelligence = caster?.Intelligence ?? 0,
                HasAlliance = faction?.AllianceIds.Count > 0,
                IsAtWar = false // TODO: 戦争状態の判定
            };

            // 軍の兵力
            if (!string.IsNullOrEmpty(targetId))
            {
                var army = GameManager.Instance?.GetArmy(targetId);
                if (army != null)
                {
                    context.ArmySoldiers = army.SoldierCount;
                }
            }

            return context;
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// 計略実行コンテキスト
    /// </summary>
    public class StratagemExecutionContext
    {
        public StratagemData StratagemData;
        public string CasterFactionId;
        public string CasterCharacterId;
        public string TargetId;
        public int SuccessRate;
    }

    /// <summary>
    /// 計略準備結果
    /// </summary>
    public class StratagemPrepareResult
    {
        public bool CanExecute;
        public StratagemData StratagemData;
        public int SuccessRate;
        public int CostSP;
        public int CostGold;
        public string FailureReason;
    }

    /// <summary>
    /// 計略実行結果
    /// </summary>
    public class StratagemExecutionResult
    {
        public StratagemData StratagemData;
        public string CasterFactionId;
        public string CasterCharacterId;
        public string TargetId;
        public bool Success;
        public int SuccessRate;
        public string FailureReason;
        public List<StratagemEffectResult> EffectResults;
    }

    /// <summary>
    /// 計略効果結果
    /// </summary>
    public class StratagemEffectResult
    {
        public StratagemEffectType EffectType;
        public string TargetId;
        public bool Applied;
        public int ValueApplied;
        public string Description;
    }

    #endregion
}
