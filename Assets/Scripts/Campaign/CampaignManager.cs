using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.Campaign
{
    /// <summary>
    /// キャンペーン管理システム
    /// ゲームのキャンペーン進行を統括
    /// </summary>
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        [Header("データベース")]
        [SerializeField] private ScenarioDatabase _scenarioDatabase;

        [Header("設定")]
        [SerializeField] private int _maxTurns = 100;
        [SerializeField] private bool _enableAutosave = true;
        [SerializeField] private int _autosaveInterval = 5;

        // キャンペーン状態
        private CampaignState _currentCampaign;
        private bool _isCampaignActive = false;

        // イベント
        public event Action<CampaignState> OnCampaignStarted;
        public event Action<CampaignState> OnCampaignEnded;
        public event Action<CampaignState> OnCampaignStateChanged;
        public event Action<string> OnObjectiveCompleted;
        public event Action<string> OnObjectiveFailed;

        /// <summary>
        /// 現在のキャンペーン状態
        /// </summary>
        public CampaignState CurrentCampaign => _currentCampaign;

        /// <summary>
        /// キャンペーンがアクティブか
        /// </summary>
        public bool IsCampaignActive => _isCampaignActive;

        /// <summary>
        /// シナリオデータベース
        /// </summary>
        public ScenarioDatabase ScenarioDatabase => _scenarioDatabase;

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
            EventBus.OnTurnEnded += OnTurnEnded;
            EventBus.OnTerritoryConquered += OnTerritoryConquered;
            EventBus.OnBattleEnded += OnBattleEnded;
        }

        private void OnDisable()
        {
            EventBus.OnTurnEnded -= OnTurnEnded;
            EventBus.OnTerritoryConquered -= OnTerritoryConquered;
            EventBus.OnBattleEnded -= OnBattleEnded;
        }

        #region Campaign Lifecycle

        /// <summary>
        /// 新規キャンペーンを開始
        /// </summary>
        public bool StartNewCampaign(string scenarioId, string playerFactionId)
        {
            if (_isCampaignActive)
            {
                Debug.LogWarning("Campaign already active. End current campaign first.");
                return false;
            }

            var scenario = _scenarioDatabase?.GetScenario(scenarioId);
            if (scenario == null)
            {
                Debug.LogError($"Scenario not found: {scenarioId}");
                return false;
            }

            // キャンペーン状態を初期化
            _currentCampaign = new CampaignState
            {
                CampaignId = Guid.NewGuid().ToString(),
                ScenarioId = scenarioId,
                ScenarioName = scenario.ScenarioName,
                PlayerFactionId = playerFactionId,
                StartYear = scenario.Year,
                CurrentTurn = 1,
                Difficulty = scenario.Difficulty,
                VictoryCondition = scenario.VictoryCondition,
                VictoryTargetValue = scenario.VictoryTargetValue,
                Status = CampaignStatus.InProgress,
                Objectives = new List<CampaignObjective>(),
                Statistics = new CampaignStatistics(),
                StartTime = DateTime.Now
            };

            // 目標を設定
            InitializeObjectives(scenario);

            _isCampaignActive = true;

            Debug.Log($"Campaign started: {scenario.ScenarioName} as {playerFactionId}");

            OnCampaignStarted?.Invoke(_currentCampaign);

            return true;
        }

        /// <summary>
        /// キャンペーンを再開
        /// </summary>
        public bool ResumeCampaign(CampaignState savedState)
        {
            if (_isCampaignActive)
            {
                Debug.LogWarning("Campaign already active");
                return false;
            }

            _currentCampaign = savedState;
            _isCampaignActive = true;

            Debug.Log($"Campaign resumed: {savedState.ScenarioName}");

            OnCampaignStarted?.Invoke(_currentCampaign);

            return true;
        }

        /// <summary>
        /// キャンペーンを終了
        /// </summary>
        public void EndCampaign(CampaignEndReason reason)
        {
            if (!_isCampaignActive) return;

            _currentCampaign.EndTime = DateTime.Now;
            _currentCampaign.EndReason = reason;

            switch (reason)
            {
                case CampaignEndReason.Victory:
                    _currentCampaign.Status = CampaignStatus.Victory;
                    break;
                case CampaignEndReason.Defeat:
                    _currentCampaign.Status = CampaignStatus.Defeat;
                    break;
                case CampaignEndReason.Abandoned:
                    _currentCampaign.Status = CampaignStatus.Abandoned;
                    break;
            }

            _isCampaignActive = false;

            Debug.Log($"Campaign ended: {reason}");

            OnCampaignEnded?.Invoke(_currentCampaign);
        }

        #endregion

        #region Objectives

        /// <summary>
        /// 目標を初期化
        /// </summary>
        private void InitializeObjectives(ScenarioData scenario)
        {
            _currentCampaign.Objectives.Clear();

            // メイン目標
            var mainObjective = new CampaignObjective
            {
                ObjectiveId = "main_victory",
                Title = GetVictoryConditionTitle(scenario.VictoryCondition),
                Description = GetVictoryConditionDescription(scenario.VictoryCondition, scenario.VictoryTargetValue),
                Type = ObjectiveType.Main,
                TargetValue = scenario.VictoryTargetValue,
                CurrentValue = 0,
                Status = ObjectiveStatus.InProgress
            };
            _currentCampaign.Objectives.Add(mainObjective);

            // サブ目標（シナリオに応じて追加）
            AddSubObjectives(scenario);
        }

        /// <summary>
        /// サブ目標を追加
        /// </summary>
        private void AddSubObjectives(ScenarioData scenario)
        {
            // 計略マスター目標
            _currentCampaign.Objectives.Add(new CampaignObjective
            {
                ObjectiveId = "stratagem_master",
                Title = "計略の達人",
                Description = "10種類の計略を成功させる",
                Type = ObjectiveType.Side,
                TargetValue = 10,
                CurrentValue = 0,
                Status = ObjectiveStatus.InProgress
            });

            // 連勝目標
            _currentCampaign.Objectives.Add(new CampaignObjective
            {
                ObjectiveId = "winning_streak",
                Title = "連戦連勝",
                Description = "5連勝を達成する",
                Type = ObjectiveType.Side,
                TargetValue = 5,
                CurrentValue = 0,
                Status = ObjectiveStatus.InProgress
            });
        }

        /// <summary>
        /// 目標の進捗を更新
        /// </summary>
        public void UpdateObjectiveProgress(string objectiveId, int progress)
        {
            var objective = _currentCampaign.Objectives.Find(o => o.ObjectiveId == objectiveId);
            if (objective == null) return;

            objective.CurrentValue = progress;

            if (objective.CurrentValue >= objective.TargetValue && objective.Status == ObjectiveStatus.InProgress)
            {
                objective.Status = ObjectiveStatus.Completed;
                objective.CompletedTurn = _currentCampaign.CurrentTurn;

                OnObjectiveCompleted?.Invoke(objectiveId);
            }

            OnCampaignStateChanged?.Invoke(_currentCampaign);
        }

        /// <summary>
        /// 目標を失敗にする
        /// </summary>
        public void FailObjective(string objectiveId)
        {
            var objective = _currentCampaign.Objectives.Find(o => o.ObjectiveId == objectiveId);
            if (objective == null) return;

            objective.Status = ObjectiveStatus.Failed;

            OnObjectiveFailed?.Invoke(objectiveId);
            OnCampaignStateChanged?.Invoke(_currentCampaign);
        }

        #endregion

        #region Victory Condition Helpers

        private string GetVictoryConditionTitle(VictoryConditionType type)
        {
            return type switch
            {
                VictoryConditionType.Conquest => "天下統一",
                VictoryConditionType.TerritoryCount => "領土拡大",
                VictoryConditionType.SurviveYears => "存続",
                VictoryConditionType.DefeatFaction => "宿敵打倒",
                VictoryConditionType.Alliance => "同盟統一",
                _ => "勝利"
            };
        }

        private string GetVictoryConditionDescription(VictoryConditionType type, int targetValue)
        {
            return type switch
            {
                VictoryConditionType.Conquest => "全ての領地を制圧せよ",
                VictoryConditionType.TerritoryCount => $"{targetValue}つの領地を支配せよ",
                VictoryConditionType.SurviveYears => $"{targetValue}年間生き残れ",
                VictoryConditionType.DefeatFaction => "敵対勢力を滅ぼせ",
                VictoryConditionType.Alliance => "同盟による統一を達成せよ",
                _ => "目標を達成せよ"
            };
        }

        #endregion

        #region Statistics

        /// <summary>
        /// 統計を更新
        /// </summary>
        public void UpdateStatistics(Action<CampaignStatistics> updateAction)
        {
            updateAction?.Invoke(_currentCampaign.Statistics);
            OnCampaignStateChanged?.Invoke(_currentCampaign);
        }

        /// <summary>
        /// 戦闘統計を記録
        /// </summary>
        public void RecordBattle(bool isVictory, int casualties, int enemyCasualties)
        {
            _currentCampaign.Statistics.TotalBattles++;

            if (isVictory)
            {
                _currentCampaign.Statistics.BattlesWon++;
                _currentCampaign.Statistics.CurrentWinStreak++;

                if (_currentCampaign.Statistics.CurrentWinStreak > _currentCampaign.Statistics.MaxWinStreak)
                {
                    _currentCampaign.Statistics.MaxWinStreak = _currentCampaign.Statistics.CurrentWinStreak;
                }

                // 連勝目標の更新
                UpdateObjectiveProgress("winning_streak", _currentCampaign.Statistics.CurrentWinStreak);
            }
            else
            {
                _currentCampaign.Statistics.BattlesLost++;
                _currentCampaign.Statistics.CurrentWinStreak = 0;
            }

            _currentCampaign.Statistics.TotalCasualties += casualties;
            _currentCampaign.Statistics.TotalEnemyCasualties += enemyCasualties;
        }

        /// <summary>
        /// 計略統計を記録
        /// </summary>
        public void RecordStratagem(string stratagemId, bool success)
        {
            _currentCampaign.Statistics.StratagemsUsed++;

            if (success)
            {
                _currentCampaign.Statistics.StratagemsSucceeded++;

                // ユニーク計略をカウント
                if (!_currentCampaign.Statistics.UniqueStratagemsUsed.Contains(stratagemId))
                {
                    _currentCampaign.Statistics.UniqueStratagemsUsed.Add(stratagemId);
                    UpdateObjectiveProgress("stratagem_master", _currentCampaign.Statistics.UniqueStratagemsUsed.Count);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnTurnEnded(int turnNumber)
        {
            if (!_isCampaignActive) return;

            _currentCampaign.CurrentTurn = turnNumber + 1;
            _currentCampaign.Statistics.TurnsPlayed++;

            // オートセーブ
            if (_enableAutosave && turnNumber % _autosaveInterval == 0)
            {
                SaveLoadManager.Instance?.SaveGame();
            }

            // 最大ターン数チェック
            if (_currentCampaign.CurrentTurn > _maxTurns)
            {
                CheckTimeoutCondition();
            }

            OnCampaignStateChanged?.Invoke(_currentCampaign);
        }

        private void OnTerritoryConquered(TerritoryConqueredEventArgs args)
        {
            if (!_isCampaignActive) return;

            if (args.NewOwnerId == _currentCampaign.PlayerFactionId)
            {
                _currentCampaign.Statistics.TerritoriesConquered++;
            }
            else if (args.PreviousOwnerId == _currentCampaign.PlayerFactionId)
            {
                _currentCampaign.Statistics.TerritoriesLost++;
            }

            // 勝利条件チェック
            CheckVictoryCondition();
            CheckDefeatCondition();
        }

        private void OnBattleEnded(BattleResultEventArgs args)
        {
            if (!_isCampaignActive) return;

            bool isPlayerVictory = args.VictorFactionId == _currentCampaign.PlayerFactionId;
            RecordBattle(isPlayerVictory, args.AttackerLosses, args.DefenderLosses);
        }

        /// <summary>
        /// 勝利条件をチェック
        /// </summary>
        private void CheckVictoryCondition()
        {
            var playerFaction = GameManager.Instance?.GetFaction(_currentCampaign.PlayerFactionId);
            if (playerFaction == null) return;

            bool victoryAchieved = false;
            int currentProgress = 0;

            switch (_currentCampaign.VictoryCondition)
            {
                case VictoryConditionType.Conquest:
                    int totalTerritories = GameManager.Instance.GameData.Territories.Count;
                    currentProgress = playerFaction.TerritoryIds.Count;
                    victoryAchieved = currentProgress >= totalTerritories;
                    break;

                case VictoryConditionType.TerritoryCount:
                    currentProgress = playerFaction.TerritoryIds.Count;
                    victoryAchieved = currentProgress >= _currentCampaign.VictoryTargetValue;
                    break;

                case VictoryConditionType.SurviveYears:
                    int yearsElapsed = GameManager.Instance.CurrentYear - _currentCampaign.StartYear;
                    currentProgress = yearsElapsed;
                    victoryAchieved = yearsElapsed >= _currentCampaign.VictoryTargetValue;
                    break;

                case VictoryConditionType.DefeatFaction:
                    // 特定勢力が滅亡したかチェック
                    int remainingEnemies = 0;
                    foreach (var faction in GameManager.Instance.GameData.Factions.Values)
                    {
                        if (faction.Id != _currentCampaign.PlayerFactionId && faction.TerritoryIds.Count > 0)
                        {
                            remainingEnemies++;
                        }
                    }
                    currentProgress = GameManager.Instance.GameData.Factions.Count - 1 - remainingEnemies;
                    victoryAchieved = remainingEnemies == 0;
                    break;
            }

            // メイン目標の進捗更新
            UpdateObjectiveProgress("main_victory", currentProgress);

            if (victoryAchieved)
            {
                EndCampaign(CampaignEndReason.Victory);
            }
        }

        /// <summary>
        /// 敗北条件をチェック
        /// </summary>
        private void CheckDefeatCondition()
        {
            var playerFaction = GameManager.Instance?.GetFaction(_currentCampaign.PlayerFactionId);
            if (playerFaction == null) return;

            // 領地が0になったら敗北
            if (playerFaction.TerritoryIds.Count == 0)
            {
                EndCampaign(CampaignEndReason.Defeat);
            }
        }

        /// <summary>
        /// タイムアウト条件をチェック
        /// </summary>
        private void CheckTimeoutCondition()
        {
            // ターン上限に達した場合の処理
            var mainObjective = _currentCampaign.Objectives.Find(o => o.ObjectiveId == "main_victory");
            if (mainObjective != null && mainObjective.Status != ObjectiveStatus.Completed)
            {
                EndCampaign(CampaignEndReason.Defeat);
            }
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// キャンペーン状態
    /// </summary>
    [Serializable]
    public class CampaignState
    {
        public string CampaignId;
        public string ScenarioId;
        public string ScenarioName;
        public string PlayerFactionId;
        public int StartYear;
        public int CurrentTurn;
        public int Difficulty;
        public VictoryConditionType VictoryCondition;
        public int VictoryTargetValue;
        public CampaignStatus Status;
        public CampaignEndReason EndReason;
        public List<CampaignObjective> Objectives;
        public CampaignStatistics Statistics;
        public DateTime StartTime;
        public DateTime EndTime;
    }

    /// <summary>
    /// キャンペーンステータス
    /// </summary>
    public enum CampaignStatus
    {
        InProgress,
        Victory,
        Defeat,
        Abandoned
    }

    /// <summary>
    /// キャンペーン終了理由
    /// </summary>
    public enum CampaignEndReason
    {
        Victory,
        Defeat,
        Abandoned,
        Timeout
    }

    /// <summary>
    /// キャンペーン目標
    /// </summary>
    [Serializable]
    public class CampaignObjective
    {
        public string ObjectiveId;
        public string Title;
        public string Description;
        public ObjectiveType Type;
        public int TargetValue;
        public int CurrentValue;
        public ObjectiveStatus Status;
        public int CompletedTurn;
    }

    /// <summary>
    /// 目標タイプ
    /// </summary>
    public enum ObjectiveType
    {
        Main,
        Side,
        Hidden
    }

    /// <summary>
    /// 目標ステータス
    /// </summary>
    public enum ObjectiveStatus
    {
        InProgress,
        Completed,
        Failed
    }

    /// <summary>
    /// キャンペーン統計
    /// </summary>
    [Serializable]
    public class CampaignStatistics
    {
        public int TurnsPlayed;
        public int TotalBattles;
        public int BattlesWon;
        public int BattlesLost;
        public int TotalCasualties;
        public int TotalEnemyCasualties;
        public int TerritoriesConquered;
        public int TerritoriesLost;
        public int StratagemsUsed;
        public int StratagemsSucceeded;
        public int CurrentWinStreak;
        public int MaxWinStreak;
        public List<string> UniqueStratagemsUsed = new List<string>();
    }

    #endregion
}
