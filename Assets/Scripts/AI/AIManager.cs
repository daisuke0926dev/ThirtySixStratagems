using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.AI
{
    /// <summary>
    /// AI管理システム
    /// 全AIの統括と実行を管理
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private float _actionDelay = 0.5f;
        [SerializeField] private bool _enableAI = true;
        [SerializeField] private AIProfile _defaultProfile;

        [Header("デバッグ")]
        [SerializeField] private bool _logAIDecisions = true;

        // AI実行状態
        private bool _isProcessingAI = false;
        private Queue<AIAction> _pendingActions = new Queue<AIAction>();
        private Dictionary<string, FactionAI> _factionAIs = new Dictionary<string, FactionAI>();

        // イベント
        public event Action<string> OnAITurnStarted;
        public event Action<string> OnAITurnEnded;
        public event Action<AIAction> OnAIActionExecuted;

        /// <summary>
        /// AIが有効かどうか
        /// </summary>
        public bool IsAIEnabled
        {
            get => _enableAI;
            set => _enableAI = value;
        }

        /// <summary>
        /// AI処理中かどうか
        /// </summary>
        public bool IsProcessingAI => _isProcessingAI;

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
            EventBus.OnFactionTurnStarted += OnFactionTurnStarted;
            EventBus.OnFactionTurnEnded += OnFactionTurnEnded;
            EventBus.OnGameStarted += InitializeFactionAIs;
        }

        private void OnDisable()
        {
            EventBus.OnFactionTurnStarted -= OnFactionTurnStarted;
            EventBus.OnFactionTurnEnded -= OnFactionTurnEnded;
            EventBus.OnGameStarted -= InitializeFactionAIs;
        }

        #region Initialization

        /// <summary>
        /// 勢力AIを初期化
        /// </summary>
        private void InitializeFactionAIs()
        {
            _factionAIs.Clear();

            if (GameManager.Instance?.GameData == null) return;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (!faction.IsPlayer)
                {
                    var factionAI = CreateFactionAI(faction);
                    _factionAIs[faction.Id] = factionAI;
                }
            }

            LogAI($"Initialized AI for {_factionAIs.Count} factions");
        }

        /// <summary>
        /// 勢力AIを作成
        /// </summary>
        private FactionAI CreateFactionAI(Faction faction)
        {
            var profile = _defaultProfile ?? AIProfile.CreateDefault();
            return new FactionAI(faction.Id, profile);
        }

        /// <summary>
        /// AIプロファイルを設定
        /// </summary>
        public void SetAIProfile(string factionId, AIProfile profile)
        {
            if (_factionAIs.TryGetValue(factionId, out var factionAI))
            {
                factionAI.SetProfile(profile);
            }
        }

        #endregion

        #region Turn Processing

        /// <summary>
        /// 勢力ターン開始時の処理
        /// </summary>
        private void OnFactionTurnStarted(string factionId)
        {
            if (!_enableAI) return;

            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null || faction.IsPlayer) return;

            // AI勢力のターン
            OnAITurnStarted?.Invoke(factionId);
            StartCoroutine(ProcessAITurn(factionId));
        }

        /// <summary>
        /// 勢力ターン終了時の処理
        /// </summary>
        private void OnFactionTurnEnded(string factionId)
        {
            // 必要に応じてクリーンアップ
        }

        /// <summary>
        /// AIターンを処理
        /// </summary>
        private IEnumerator ProcessAITurn(string factionId)
        {
            _isProcessingAI = true;

            LogAI($"[{factionId}] AI turn started");

            if (!_factionAIs.TryGetValue(factionId, out var factionAI))
            {
                LogAI($"[{factionId}] No AI found, skipping");
                _isProcessingAI = false;
                EndAITurn(factionId);
                yield break;
            }

            // 現在のフェーズに応じた行動を決定
            var currentPhase = TurnManager.Instance?.CurrentPhase ?? TurnPhase.Internal;
            var actions = factionAI.DecideActions(currentPhase);

            LogAI($"[{factionId}] Decided {actions.Count} actions for {currentPhase}");

            // アクションを順番に実行
            foreach (var action in actions)
            {
                yield return new WaitForSeconds(_actionDelay);

                ExecuteAction(action);
                OnAIActionExecuted?.Invoke(action);
            }

            yield return new WaitForSeconds(_actionDelay);

            _isProcessingAI = false;

            LogAI($"[{factionId}] AI turn ended");
            OnAITurnEnded?.Invoke(factionId);

            // ターン終了を通知
            EndAITurn(factionId);
        }

        /// <summary>
        /// AIターンを終了
        /// </summary>
        private void EndAITurn(string factionId)
        {
            TurnManager.Instance?.EndFactionTurn();
        }

        #endregion

        #region Action Execution

        /// <summary>
        /// AIアクションを実行
        /// </summary>
        private void ExecuteAction(AIAction action)
        {
            LogAI($"Executing action: {action.Type} - {action.Description}");

            switch (action.Type)
            {
                case AIActionType.MoveArmy:
                    ExecuteMoveArmy(action);
                    break;

                case AIActionType.Attack:
                    ExecuteAttack(action);
                    break;

                case AIActionType.UseStratagem:
                    ExecuteStratagem(action);
                    break;

                case AIActionType.Recruit:
                    ExecuteRecruit(action);
                    break;

                case AIActionType.Diplomacy:
                    ExecuteDiplomacy(action);
                    break;

                case AIActionType.Defend:
                    ExecuteDefend(action);
                    break;

                case AIActionType.Wait:
                    // 何もしない
                    break;
            }
        }

        private void ExecuteMoveArmy(AIAction action)
        {
            Battle.ArmyManager.Instance?.StartArmyMovement(action.SourceId, action.TargetId);
        }

        private void ExecuteAttack(AIAction action)
        {
            // 移動と攻撃は同じ処理（移動先に敵がいれば自動的に戦闘）
            Battle.ArmyManager.Instance?.StartArmyMovement(action.SourceId, action.TargetId);
        }

        private void ExecuteStratagem(AIAction action)
        {
            var factionAI = _factionAIs.GetValueOrDefault(action.FactionId);
            string casterId = factionAI?.GetBestStratagemCaster() ?? "";

            Stratagem.StratagemManager.Instance?.ExecuteStratagem(
                action.StratagemId, action.FactionId, casterId, action.TargetId);
        }

        private void ExecuteRecruit(AIAction action)
        {
            int amount = action.Value;
            ResourceManager.Instance?.Recruit(action.FactionId, action.SourceId, amount);
        }

        private void ExecuteDiplomacy(AIAction action)
        {
            // TODO: 外交システムとの連携
            LogAI($"Diplomacy action: {action.Description}");
        }

        private void ExecuteDefend(AIAction action)
        {
            // 防御態勢（移動キャンセル等）
            var armies = Battle.ArmyManager.Instance?.GetArmiesByFaction(action.FactionId);
            if (armies != null)
            {
                foreach (var army in armies)
                {
                    if (army.IsMoving)
                    {
                        Battle.ArmyManager.Instance?.CancelArmyMovement(army.Id);
                    }
                }
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 勢力AIを取得
        /// </summary>
        public FactionAI GetFactionAI(string factionId)
        {
            return _factionAIs.GetValueOrDefault(factionId);
        }

        /// <summary>
        /// AI勢力かどうか
        /// </summary>
        public bool IsAIFaction(string factionId)
        {
            return _factionAIs.ContainsKey(factionId);
        }

        #endregion

        #region Debug

        private void LogAI(string message)
        {
            if (_logAIDecisions)
            {
                Debug.Log($"[AI] {message}");
            }
        }

        /// <summary>
        /// AI思考をログ出力
        /// </summary>
        public void LogAIThought(string factionId, string thought)
        {
            if (_logAIDecisions)
            {
                Debug.Log($"[AI:{factionId}] {thought}");
            }
        }

        #endregion
    }

    #region AI Data Classes

    /// <summary>
    /// AIアクション
    /// </summary>
    [Serializable]
    public class AIAction
    {
        public AIActionType Type;
        public string FactionId;
        public string SourceId;
        public string TargetId;
        public string StratagemId;
        public int Value;
        public int Priority;
        public string Description;

        public static AIAction CreateMoveAction(string factionId, string armyId, string targetTerritory, int priority = 50)
        {
            return new AIAction
            {
                Type = AIActionType.MoveArmy,
                FactionId = factionId,
                SourceId = armyId,
                TargetId = targetTerritory,
                Priority = priority,
                Description = $"Move army to {targetTerritory}"
            };
        }

        public static AIAction CreateAttackAction(string factionId, string armyId, string targetTerritory, int priority = 70)
        {
            return new AIAction
            {
                Type = AIActionType.Attack,
                FactionId = factionId,
                SourceId = armyId,
                TargetId = targetTerritory,
                Priority = priority,
                Description = $"Attack {targetTerritory}"
            };
        }

        public static AIAction CreateStratagemAction(string factionId, string stratagemId, string targetId, int priority = 60)
        {
            return new AIAction
            {
                Type = AIActionType.UseStratagem,
                FactionId = factionId,
                StratagemId = stratagemId,
                TargetId = targetId,
                Priority = priority,
                Description = $"Use stratagem on {targetId}"
            };
        }

        public static AIAction CreateRecruitAction(string factionId, string territoryId, int amount, int priority = 40)
        {
            return new AIAction
            {
                Type = AIActionType.Recruit,
                FactionId = factionId,
                SourceId = territoryId,
                Value = amount,
                Priority = priority,
                Description = $"Recruit {amount} soldiers at {territoryId}"
            };
        }

        public static AIAction CreateWaitAction(string factionId)
        {
            return new AIAction
            {
                Type = AIActionType.Wait,
                FactionId = factionId,
                Priority = 0,
                Description = "Wait"
            };
        }
    }

    /// <summary>
    /// AIアクションタイプ
    /// </summary>
    public enum AIActionType
    {
        Wait,
        MoveArmy,
        Attack,
        UseStratagem,
        Recruit,
        Diplomacy,
        Defend
    }

    /// <summary>
    /// AIプロファイル（性格設定）
    /// </summary>
    [Serializable]
    public class AIProfile
    {
        [Range(0, 100)]
        public int Aggression = 50;      // 攻撃性

        [Range(0, 100)]
        public int Caution = 50;         // 慎重さ

        [Range(0, 100)]
        public int Expansion = 50;       // 拡張志向

        [Range(0, 100)]
        public int StratagemUse = 50;    // 計略使用頻度

        [Range(0, 100)]
        public int Diplomacy = 50;       // 外交志向

        [Range(0, 100)]
        public int Economy = 50;         // 経済重視

        public static AIProfile CreateDefault()
        {
            return new AIProfile();
        }

        public static AIProfile CreateAggressive()
        {
            return new AIProfile
            {
                Aggression = 80,
                Caution = 20,
                Expansion = 70,
                StratagemUse = 60,
                Diplomacy = 30,
                Economy = 40
            };
        }

        public static AIProfile CreateDefensive()
        {
            return new AIProfile
            {
                Aggression = 30,
                Caution = 80,
                Expansion = 30,
                StratagemUse = 50,
                Diplomacy = 60,
                Economy = 70
            };
        }

        public static AIProfile CreateStrategist()
        {
            return new AIProfile
            {
                Aggression = 50,
                Caution = 60,
                Expansion = 50,
                StratagemUse = 90,
                Diplomacy = 50,
                Economy = 50
            };
        }
    }

    #endregion
}
