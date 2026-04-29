using System;
using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.Campaign
{
    /// <summary>
    /// チュートリアルシステム
    /// ゲームプレイと三十六計の学習をガイド
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        public static TutorialSystem Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private bool _enableTutorial = true;
        [SerializeField] private float _stepDelay = 0.5f;

        [Header("チュートリアルデータ")]
        [SerializeField] private List<TutorialChapter> _chapters = new List<TutorialChapter>();

        // 状態
        private TutorialState _state;
        private int _currentChapterIndex = 0;
        private int _currentStepIndex = 0;
        private bool _isRunning = false;
        private bool _waitingForAction = false;

        // イベント
        public event Action<TutorialStep> OnStepStarted;
        public event Action<TutorialStep> OnStepCompleted;
        public event Action<TutorialChapter> OnChapterStarted;
        public event Action<TutorialChapter> OnChapterCompleted;
        public event Action OnTutorialCompleted;
        public event Action<string> OnHintRequested;

        /// <summary>
        /// チュートリアルが有効か
        /// </summary>
        public bool IsTutorialEnabled => _enableTutorial;

        /// <summary>
        /// チュートリアル実行中か
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 現在のステップ
        /// </summary>
        public TutorialStep CurrentStep
        {
            get
            {
                if (_currentChapterIndex >= _chapters.Count) return null;
                var chapter = _chapters[_currentChapterIndex];
                if (_currentStepIndex >= chapter.Steps.Count) return null;
                return chapter.Steps[_currentStepIndex];
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeDefaultChapters();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #region Initialization

        /// <summary>
        /// デフォルトチャプターを初期化
        /// </summary>
        private void InitializeDefaultChapters()
        {
            if (_chapters.Count > 0) return;

            // 第1章: 基本操作
            var chapter1 = new TutorialChapter
            {
                ChapterId = "basics",
                Title = "第一章: 基本操作",
                Description = "ゲームの基本操作を学びます",
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        StepId = "welcome",
                        Title = "ようこそ",
                        Description = "三十六計シミュレーションへようこそ。\nこのチュートリアルでは、ゲームの基本と計略の使い方を学びます。",
                        Type = TutorialStepType.Dialog
                    },
                    new TutorialStep
                    {
                        StepId = "map_overview",
                        Title = "マップ概要",
                        Description = "画面には領地マップが表示されています。\n自軍の領地は青、敵軍は赤で表示されます。",
                        Type = TutorialStepType.Highlight,
                        HighlightTarget = "MapPanel"
                    },
                    new TutorialStep
                    {
                        StepId = "select_territory",
                        Title = "領地を選択",
                        Description = "自軍の領地をクリックして選択してみましょう。",
                        Type = TutorialStepType.Action,
                        RequiredAction = "SelectTerritory"
                    },
                    new TutorialStep
                    {
                        StepId = "territory_info",
                        Title = "領地情報",
                        Description = "選択した領地の情報が表示されます。\n人口、経済力、防御力などを確認できます。",
                        Type = TutorialStepType.Highlight,
                        HighlightTarget = "TerritoryInfoPanel"
                    }
                }
            };
            _chapters.Add(chapter1);

            // 第2章: 計略
            var chapter2 = new TutorialChapter
            {
                ChapterId = "stratagems",
                Title = "第二章: 計略",
                Description = "三十六計の使い方を学びます",
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        StepId = "stratagem_intro",
                        Title = "計略とは",
                        Description = "計略は戦いを有利に進めるための策略です。\n三十六計には様々な計略があり、状況に応じて使い分けます。",
                        Type = TutorialStepType.Dialog
                    },
                    new TutorialStep
                    {
                        StepId = "open_stratagem_panel",
                        Title = "計略パネルを開く",
                        Description = "画面右の計略ボタンをクリックして、計略パネルを開きましょう。",
                        Type = TutorialStepType.Action,
                        RequiredAction = "OpenStratagemPanel",
                        HighlightTarget = "StratagemButton"
                    },
                    new TutorialStep
                    {
                        StepId = "stratagem_categories",
                        Title = "計略の分類",
                        Description = "計略は6つのカテゴリに分類されています。\n・勝戦計: 優勢時の計略\n・敵戦計: 敵を操る計略\n・攻戦計: 攻撃的な計略\n・混戦計: 混乱を利用する計略\n・並戦計: 同等時の計略\n・敗戦計: 劣勢時の計略",
                        Type = TutorialStepType.Dialog
                    },
                    new TutorialStep
                    {
                        StepId = "use_stratagem",
                        Title = "計略を使用",
                        Description = "計略を選択して、対象を指定し、実行してみましょう。",
                        Type = TutorialStepType.Action,
                        RequiredAction = "UseStratagem"
                    }
                }
            };
            _chapters.Add(chapter2);

            // 第3章: 戦闘
            var chapter3 = new TutorialChapter
            {
                ChapterId = "battle",
                Title = "第三章: 戦闘",
                Description = "戦闘システムを学びます",
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        StepId = "army_movement",
                        Title = "軍の移動",
                        Description = "軍を選択し、隣接する領地に移動できます。\n敵領地に移動すると戦闘が発生します。",
                        Type = TutorialStepType.Dialog
                    },
                    new TutorialStep
                    {
                        StepId = "battle_system",
                        Title = "戦闘システム",
                        Description = "戦闘はラウンド制で進行します。\n兵力、士気、指揮官の能力が勝敗を左右します。",
                        Type = TutorialStepType.Dialog
                    },
                    new TutorialStep
                    {
                        StepId = "battle_stratagem",
                        Title = "戦闘中の計略",
                        Description = "戦闘中にも計略を使用できます。\n「奇襲」「挟撃」などの戦術的計略が効果的です。",
                        Type = TutorialStepType.Dialog
                    }
                }
            };
            _chapters.Add(chapter3);
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnTerritorySelected += OnTerritorySelected;
            EventBus.OnStratagemExecuted += OnStratagemExecuted;
            EventBus.OnBattleStarted += OnBattleStarted;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnTerritorySelected -= OnTerritorySelected;
            EventBus.OnStratagemExecuted -= OnStratagemExecuted;
            EventBus.OnBattleStarted -= OnBattleStarted;
        }

        #endregion

        #region Tutorial Control

        /// <summary>
        /// チュートリアルを開始
        /// </summary>
        public void StartTutorial()
        {
            if (!_enableTutorial) return;

            _state = new TutorialState();
            _currentChapterIndex = 0;
            _currentStepIndex = 0;
            _isRunning = true;

            Debug.Log("Tutorial started");

            StartCurrentChapter();
        }

        /// <summary>
        /// チュートリアルをスキップ
        /// </summary>
        public void SkipTutorial()
        {
            _isRunning = false;
            _state.IsCompleted = true;
            _state.WasSkipped = true;

            Debug.Log("Tutorial skipped");
        }

        /// <summary>
        /// 次のステップへ
        /// </summary>
        public void NextStep()
        {
            if (!_isRunning) return;
            if (_waitingForAction) return;

            var currentStep = CurrentStep;
            if (currentStep != null)
            {
                OnStepCompleted?.Invoke(currentStep);
                _state.CompletedSteps.Add(currentStep.StepId);
            }

            _currentStepIndex++;

            var chapter = _chapters[_currentChapterIndex];
            if (_currentStepIndex >= chapter.Steps.Count)
            {
                CompleteCurrentChapter();
            }
            else
            {
                StartCurrentStep();
            }
        }

        /// <summary>
        /// 前のステップへ
        /// </summary>
        public void PreviousStep()
        {
            if (!_isRunning) return;
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                StartCurrentStep();
            }
        }

        /// <summary>
        /// 現在のチャプターを開始
        /// </summary>
        private void StartCurrentChapter()
        {
            if (_currentChapterIndex >= _chapters.Count)
            {
                CompleteTutorial();
                return;
            }

            var chapter = _chapters[_currentChapterIndex];
            _currentStepIndex = 0;

            Debug.Log($"Starting chapter: {chapter.Title}");
            OnChapterStarted?.Invoke(chapter);

            StartCurrentStep();
        }

        /// <summary>
        /// 現在のステップを開始
        /// </summary>
        private void StartCurrentStep()
        {
            var step = CurrentStep;
            if (step == null) return;

            Debug.Log($"Starting step: {step.Title}");

            _waitingForAction = step.Type == TutorialStepType.Action;

            OnStepStarted?.Invoke(step);
        }

        /// <summary>
        /// 現在のチャプターを完了
        /// </summary>
        private void CompleteCurrentChapter()
        {
            var chapter = _chapters[_currentChapterIndex];
            _state.CompletedChapters.Add(chapter.ChapterId);

            Debug.Log($"Chapter completed: {chapter.Title}");
            OnChapterCompleted?.Invoke(chapter);

            _currentChapterIndex++;
            StartCurrentChapter();
        }

        /// <summary>
        /// チュートリアルを完了
        /// </summary>
        private void CompleteTutorial()
        {
            _isRunning = false;
            _state.IsCompleted = true;

            Debug.Log("Tutorial completed!");
            OnTutorialCompleted?.Invoke();

            // 設定を保存
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }

        #endregion

        #region Action Validation

        /// <summary>
        /// アクションを検証
        /// </summary>
        public void ValidateAction(string actionId)
        {
            if (!_isRunning || !_waitingForAction) return;

            var step = CurrentStep;
            if (step != null && step.RequiredAction == actionId)
            {
                _waitingForAction = false;
                NextStep();
            }
        }

        #endregion

        #region Hints

        /// <summary>
        /// ヒントを要求
        /// </summary>
        public void RequestHint()
        {
            var step = CurrentStep;
            if (step != null && !string.IsNullOrEmpty(step.Hint))
            {
                OnHintRequested?.Invoke(step.Hint);
            }
        }

        #endregion

        #region Event Handlers

        private void OnTerritorySelected(string territoryId)
        {
            ValidateAction("SelectTerritory");
        }

        private void OnStratagemExecuted(StratagemEventArgs args)
        {
            ValidateAction("UseStratagem");
        }

        private void OnBattleStarted(BattleEventArgs args)
        {
            ValidateAction("StartBattle");
        }

        #endregion

        #region Progress

        /// <summary>
        /// チュートリアル完了済みか確認
        /// </summary>
        public bool IsTutorialCompleted()
        {
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        /// <summary>
        /// 進捗をリセット
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            _state = new TutorialState();
        }

        /// <summary>
        /// 進捗率を取得
        /// </summary>
        public float GetProgress()
        {
            if (_chapters.Count == 0) return 0f;

            int totalSteps = 0;
            int completedSteps = 0;

            for (int i = 0; i < _chapters.Count; i++)
            {
                totalSteps += _chapters[i].Steps.Count;

                if (i < _currentChapterIndex)
                {
                    completedSteps += _chapters[i].Steps.Count;
                }
                else if (i == _currentChapterIndex)
                {
                    completedSteps += _currentStepIndex;
                }
            }

            return totalSteps > 0 ? (float)completedSteps / totalSteps : 0f;
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// チュートリアル状態
    /// </summary>
    [Serializable]
    public class TutorialState
    {
        public bool IsCompleted;
        public bool WasSkipped;
        public List<string> CompletedChapters = new List<string>();
        public List<string> CompletedSteps = new List<string>();
    }

    /// <summary>
    /// チュートリアルチャプター
    /// </summary>
    [Serializable]
    public class TutorialChapter
    {
        public string ChapterId;
        public string Title;
        public string Description;
        public List<TutorialStep> Steps = new List<TutorialStep>();
    }

    /// <summary>
    /// チュートリアルステップ
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        public string StepId;
        public string Title;
        public string Description;
        public TutorialStepType Type;
        public string RequiredAction;
        public string HighlightTarget;
        public string Hint;
        public Vector2 ArrowPosition;
    }

    /// <summary>
    /// チュートリアルステップタイプ
    /// </summary>
    public enum TutorialStepType
    {
        Dialog,     // ダイアログ表示のみ
        Highlight,  // UI要素をハイライト
        Action,     // プレイヤーのアクションを待機
        Auto        // 自動進行
    }

    #endregion
}
