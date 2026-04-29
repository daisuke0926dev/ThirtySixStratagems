using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Core
{
    /// <summary>
    /// ターン管理システム
    /// ターンの進行、フェーズ遷移、勢力順序を管理
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        [Header("ターン設定")]
        [SerializeField] private int _maxTurns = 100;

        // 現在の状態
        private int _currentTurn = 1;
        private TurnPhase _currentPhase = TurnPhase.Internal;
        private int _currentFactionIndex = 0;
        private List<string> _factionOrder = new List<string>();
        private bool _isProcessingTurn = false;

        // イベント
        public event Action<int> OnTurnStarted;
        public event Action<int> OnTurnEnded;
        public event Action<TurnPhase> OnPhaseChanged;
        public event Action<string> OnFactionTurnStarted;
        public event Action<string> OnFactionTurnEnded;
        public event Action OnAllFactionsTurnCompleted;

        /// <summary>
        /// 現在のターン数
        /// </summary>
        public int CurrentTurn => _currentTurn;

        /// <summary>
        /// 現在のフェーズ
        /// </summary>
        public TurnPhase CurrentPhase => _currentPhase;

        /// <summary>
        /// 現在の勢力ID
        /// </summary>
        public string CurrentFactionId =>
            _factionOrder.Count > 0 && _currentFactionIndex < _factionOrder.Count
                ? _factionOrder[_currentFactionIndex]
                : null;

        /// <summary>
        /// 最大ターン数
        /// </summary>
        public int MaxTurns => _maxTurns;

        /// <summary>
        /// ターン処理中か
        /// </summary>
        public bool IsProcessingTurn => _isProcessingTurn;

        /// <summary>
        /// 現在がプレイヤーのターンか
        /// </summary>
        public bool IsPlayerTurn
        {
            get
            {
                if (GameManager.Instance == null || GameManager.Instance.GameData == null)
                    return false;
                return CurrentFactionId == GameManager.Instance.GameData.PlayerFactionId;
            }
        }

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
            // EventBusとの連携
            EventBus.OnGameStarted += OnGameStarted;
        }

        private void OnDisable()
        {
            EventBus.OnGameStarted -= OnGameStarted;
        }

        /// <summary>
        /// ゲーム開始時の初期化
        /// </summary>
        private void OnGameStarted()
        {
            InitializeTurnOrder();
            StartTurn();
        }

        /// <summary>
        /// ターンシステムを初期化
        /// </summary>
        public void Initialize(List<string> factionIds, int startTurn = 1)
        {
            _currentTurn = startTurn;
            _currentPhase = TurnPhase.Internal;
            _currentFactionIndex = 0;
            _factionOrder = new List<string>(factionIds);

            Debug.Log($"TurnManager initialized: {_factionOrder.Count} factions, starting turn {_currentTurn}");
        }

        /// <summary>
        /// 勢力の順序を初期化（GameManagerから取得）
        /// </summary>
        private void InitializeTurnOrder()
        {
            if (GameManager.Instance == null || GameManager.Instance.GameData == null)
            {
                Debug.LogWarning("GameManager or GameData is not available");
                return;
            }

            var gameData = GameManager.Instance.GameData;
            _currentTurn = gameData.CurrentTurn;

            // 生存している勢力のみをリストに追加
            _factionOrder.Clear();
            foreach (var faction in gameData.Factions.Values)
            {
                if (faction.TerritoryIds.Count > 0)
                {
                    _factionOrder.Add(faction.Id);
                }
            }

            // プレイヤー勢力を先頭に
            if (!string.IsNullOrEmpty(gameData.PlayerFactionId) &&
                _factionOrder.Contains(gameData.PlayerFactionId))
            {
                _factionOrder.Remove(gameData.PlayerFactionId);
                _factionOrder.Insert(0, gameData.PlayerFactionId);
            }

            _currentFactionIndex = 0;
            _currentPhase = TurnPhase.Internal;

            Debug.Log($"Turn order initialized: {string.Join(", ", _factionOrder)}");
        }

        /// <summary>
        /// ターンを開始
        /// </summary>
        public void StartTurn()
        {
            if (_isProcessingTurn)
            {
                Debug.LogWarning("Already processing turn");
                return;
            }

            _isProcessingTurn = true;
            _currentPhase = TurnPhase.Internal;
            _currentFactionIndex = 0;

            // GameDataを更新
            if (GameManager.Instance?.GameData != null)
            {
                GameManager.Instance.GameData.CurrentTurn = _currentTurn;
            }

            Debug.Log($"=== Turn {_currentTurn} Started ===");

            // イベント発火
            OnTurnStarted?.Invoke(_currentTurn);
            EventBus.TurnStarted(_currentTurn);

            // 最初の勢力のターンを開始
            StartFactionTurn();
        }

        /// <summary>
        /// 勢力のターンを開始
        /// </summary>
        private void StartFactionTurn()
        {
            if (_currentFactionIndex >= _factionOrder.Count)
            {
                // 全勢力のターンが終了
                EndCurrentPhase();
                return;
            }

            var factionId = _factionOrder[_currentFactionIndex];

            // GameDataを更新
            if (GameManager.Instance?.GameData != null)
            {
                GameManager.Instance.GameData.CurrentFactionId = factionId;
            }

            Debug.Log($"Faction turn started: {factionId} (Phase: {_currentPhase})");

            // イベント発火
            OnFactionTurnStarted?.Invoke(factionId);
            EventBus.FactionTurnStarted(factionId);
        }

        /// <summary>
        /// 現在の勢力のターンを終了
        /// </summary>
        public void EndFactionTurn()
        {
            if (_currentFactionIndex >= _factionOrder.Count)
            {
                return;
            }

            var factionId = _factionOrder[_currentFactionIndex];

            Debug.Log($"Faction turn ended: {factionId}");

            // イベント発火
            OnFactionTurnEnded?.Invoke(factionId);
            EventBus.FactionTurnEnded(factionId);

            // 次の勢力へ
            _currentFactionIndex++;

            if (_currentFactionIndex >= _factionOrder.Count)
            {
                // 全勢力のこのフェーズが終了
                OnAllFactionsTurnCompleted?.Invoke();
                EndCurrentPhase();
            }
            else
            {
                // 次の勢力のターンを開始
                StartFactionTurn();
            }
        }

        /// <summary>
        /// 現在のフェーズを終了し、次のフェーズへ
        /// </summary>
        private void EndCurrentPhase()
        {
            var previousPhase = _currentPhase;

            switch (_currentPhase)
            {
                case TurnPhase.Internal:
                    _currentPhase = TurnPhase.Diplomacy;
                    break;

                case TurnPhase.Diplomacy:
                    _currentPhase = TurnPhase.Military;
                    break;

                case TurnPhase.Military:
                    _currentPhase = TurnPhase.End;
                    break;

                case TurnPhase.End:
                    // ターン終了処理
                    EndTurn();
                    return;
            }

            Debug.Log($"Phase changed: {previousPhase} -> {_currentPhase}");

            // イベント発火
            OnPhaseChanged?.Invoke(_currentPhase);
            EventBus.PhaseChanged(_currentPhase);

            // 次のフェーズを開始
            if (_currentPhase == TurnPhase.End)
            {
                // 終了フェーズは自動で処理
                ProcessEndPhase();
            }
            else
            {
                // 勢力インデックスをリセットして次のフェーズを開始
                _currentFactionIndex = 0;
                StartFactionTurn();
            }
        }

        /// <summary>
        /// 終了フェーズの処理
        /// </summary>
        private void ProcessEndPhase()
        {
            Debug.Log("Processing end phase...");

            // ターン終了時の処理
            // - 士気回復
            // - 効果の持続ターン減少
            // - 勝敗判定
            ProcessTurnEndEffects();

            // 勝敗判定
            CheckVictoryConditions();

            // 次のターンへ
            EndTurn();
        }

        /// <summary>
        /// ターン終了時の効果処理
        /// </summary>
        private void ProcessTurnEndEffects()
        {
            if (GameManager.Instance?.GameData == null) return;

            var gameData = GameManager.Instance.GameData;

            // 各軍の士気回復と効果処理
            foreach (var army in gameData.Armies.Values)
            {
                // 士気回復
                army.RecoverMorale(Constants.Balance.MoraleRecoveryPerTurn);

                // 効果の持続ターン減少
                army.DecrementEffects();
            }

            // 各領地の処理
            foreach (var territory in gameData.Territories.Values)
            {
                // 必要に応じて領地の効果処理
            }
        }

        /// <summary>
        /// 勝敗条件を確認
        /// </summary>
        private void CheckVictoryConditions()
        {
            if (GameManager.Instance == null) return;

            var gameData = GameManager.Instance.GameData;
            if (gameData == null) return;

            // プレイヤーの敗北確認
            if (GameManager.Instance.CheckDefeatCondition(gameData.PlayerFactionId))
            {
                GameManager.Instance.EndGame(GameEndReason.Defeat);
                return;
            }

            // プレイヤーの勝利確認
            if (GameManager.Instance.CheckVictoryCondition(gameData.PlayerFactionId))
            {
                GameManager.Instance.EndGame(GameEndReason.Victory);
                return;
            }

            // 滅亡した勢力を除外
            UpdateAliveFactions();
        }

        /// <summary>
        /// 生存勢力リストを更新
        /// </summary>
        private void UpdateAliveFactions()
        {
            if (GameManager.Instance?.GameData == null) return;

            var eliminatedFactions = new List<string>();

            foreach (var factionId in _factionOrder)
            {
                var faction = GameManager.Instance.GetFaction(factionId);
                if (faction == null || faction.TerritoryIds.Count == 0)
                {
                    eliminatedFactions.Add(factionId);
                }
            }

            foreach (var factionId in eliminatedFactions)
            {
                _factionOrder.Remove(factionId);
                Debug.Log($"Faction eliminated: {factionId}");
            }
        }

        /// <summary>
        /// ターンを終了
        /// </summary>
        private void EndTurn()
        {
            Debug.Log($"=== Turn {_currentTurn} Ended ===");

            // イベント発火
            OnTurnEnded?.Invoke(_currentTurn);
            EventBus.TurnEnded(_currentTurn);

            _isProcessingTurn = false;

            // 次のターンへ
            _currentTurn++;

            // 最大ターン数チェック
            if (_currentTurn > _maxTurns)
            {
                // 生存勝利判定
                CheckSurvivalVictory();
                return;
            }

            // 次のターンを開始
            StartTurn();
        }

        /// <summary>
        /// 生存勝利を判定
        /// </summary>
        private void CheckSurvivalVictory()
        {
            if (GameManager.Instance == null) return;

            var playerFactionId = GameManager.Instance.GameData?.PlayerFactionId;
            if (string.IsNullOrEmpty(playerFactionId)) return;

            var playerFaction = GameManager.Instance.GetFaction(playerFactionId);
            if (playerFaction != null && playerFaction.TerritoryIds.Count > 0)
            {
                GameManager.Instance.EndGame(GameEndReason.Survival);
            }
            else
            {
                GameManager.Instance.EndGame(GameEndReason.Defeat);
            }
        }

        /// <summary>
        /// 現在のフェーズをスキップ（デバッグ用）
        /// </summary>
        public void SkipPhase()
        {
            Debug.Log($"Skipping phase: {_currentPhase}");
            EndCurrentPhase();
        }

        /// <summary>
        /// 現在のターンをスキップ（デバッグ用）
        /// </summary>
        public void SkipTurn()
        {
            Debug.Log($"Skipping turn: {_currentTurn}");
            _currentPhase = TurnPhase.End;
            ProcessEndPhase();
        }

        /// <summary>
        /// フェーズ名を取得
        /// </summary>
        public string GetPhaseName(TurnPhase phase)
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

        /// <summary>
        /// 勢力の順序を取得
        /// </summary>
        public IReadOnlyList<string> GetFactionOrder()
        {
            return _factionOrder.AsReadOnly();
        }

        /// <summary>
        /// 特定の勢力がまだこのフェーズで行動可能か
        /// </summary>
        public bool CanFactionAct(string factionId)
        {
            if (!_factionOrder.Contains(factionId))
                return false;

            int factionIndex = _factionOrder.IndexOf(factionId);
            return factionIndex >= _currentFactionIndex;
        }
    }
}
