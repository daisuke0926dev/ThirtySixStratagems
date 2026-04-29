using System;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Battle;
using ThirtySixStratagems.UI.Battle;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// 戦闘シーンコントローラー
    /// BattleSceneの初期化と全体制御を担当
    /// </summary>
    public class BattleSceneController : MonoBehaviour
    {
        public static BattleSceneController Instance { get; private set; }

        [Header("UI参照")]
        [SerializeField] private BattlePanel _battlePanel;
        [SerializeField] private BattleStratagemPanel _stratagemPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("表示")]
        [SerializeField] private BattleDisplay _battleDisplay;
        [SerializeField] private Camera _battleCamera;

        [Header("設定")]
        [SerializeField] private float _roundAnimationDuration = 1f;
        [SerializeField] private bool _autoStartBattle = true;

        // 状態
        private bool _isInitialized;
        private bool _isAnimating;

        // イベント
        public event Action OnBattleSceneReady;
        public event Action OnBattleSceneExit;

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
        }

        private void Start()
        {
            InitializeScene();
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
        /// シーンを初期化
        /// </summary>
        private void InitializeScene()
        {
            ShowLoading(true);

            try
            {
                // BattleManagerの確認
                if (BattleManager.Instance == null)
                {
                    Debug.LogError("BattleManager not found.");
                    return;
                }

                // UIを初期化
                InitializeUI();

                // 戦闘表示を初期化
                InitializeBattleDisplay();

                _isInitialized = true;
                Debug.Log("BattleScene initialized successfully");

                OnBattleSceneReady?.Invoke();

                // 自動開始
                if (_autoStartBattle && BattleManager.Instance.CurrentBattle != null)
                {
                    StartBattleDisplay(BattleManager.Instance.CurrentBattle);
                }
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// UIを初期化
        /// </summary>
        private void InitializeUI()
        {
            // 計略パネルを非表示
            if (_stratagemPanel != null)
            {
                _stratagemPanel.gameObject.SetActive(false);
                _stratagemPanel.OnStratagemSelected += OnStratagemSelected;
                _stratagemPanel.OnCancelled += OnStratagemCancelled;
            }

            // 戦闘パネルのイベント購読
            if (_battlePanel != null)
            {
                _battlePanel.OnStratagemRequested += ShowStratagemPanel;
                _battlePanel.OnResultClosed += OnBattleResultClosed;
            }
        }

        /// <summary>
        /// 戦闘表示を初期化
        /// </summary>
        private void InitializeBattleDisplay()
        {
            if (_battleDisplay != null && BattleManager.Instance.CurrentBattle != null)
            {
                _battleDisplay.Initialize(BattleManager.Instance.CurrentBattle);
            }
        }

        /// <summary>
        /// ローディング表示
        /// </summary>
        private void ShowLoading(bool show)
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(show);
            }
        }

        #endregion

        #region Battle Display

        /// <summary>
        /// 戦闘表示を開始
        /// </summary>
        public void StartBattleDisplay(BattleState battle)
        {
            if (battle == null) return;

            // 戦闘表示を更新
            if (_battleDisplay != null)
            {
                _battleDisplay.Initialize(battle);
            }

            // 戦闘パネルを表示
            if (_battlePanel != null)
            {
                _battlePanel.Show();
            }
        }

        /// <summary>
        /// ラウンドアニメーションを再生
        /// </summary>
        public void PlayRoundAnimation(BattleRoundResult result)
        {
            if (_isAnimating) return;

            StartCoroutine(RoundAnimationCoroutine(result));
        }

        /// <summary>
        /// ラウンドアニメーションコルーチン
        /// </summary>
        private System.Collections.IEnumerator RoundAnimationCoroutine(BattleRoundResult result)
        {
            _isAnimating = true;

            if (_battleDisplay != null)
            {
                // 戦闘アニメーション
                yield return _battleDisplay.PlayCombatAnimation(result);
            }

            yield return new WaitForSeconds(_roundAnimationDuration * 0.5f);

            _isAnimating = false;
        }

        #endregion

        #region Stratagem

        /// <summary>
        /// 計略パネルを表示
        /// </summary>
        private void ShowStratagemPanel()
        {
            if (_stratagemPanel == null) return;

            var battle = BattleManager.Instance?.CurrentBattle;
            if (battle == null) return;

            _stratagemPanel.Show(battle.Attacker.FactionId, battle.Defender.ArmyId);
        }

        /// <summary>
        /// 計略が選択された
        /// </summary>
        private void OnStratagemSelected(string stratagemId)
        {
            if (_stratagemPanel != null)
            {
                _stratagemPanel.gameObject.SetActive(false);
            }

            var battle = BattleManager.Instance?.CurrentBattle;
            if (battle == null) return;

            // 計略を実行
            bool success = BattleManager.Instance.UseStratagemInBattle(
                stratagemId,
                battle.Defender.ArmyId ?? battle.TerritoryId);

            if (success)
            {
                Debug.Log($"Stratagem {stratagemId} used in battle");

                // 表示を更新
                if (_battleDisplay != null)
                {
                    _battleDisplay.UpdateDisplay(battle);
                }
            }
        }

        /// <summary>
        /// 計略選択がキャンセルされた
        /// </summary>
        private void OnStratagemCancelled()
        {
            if (_stratagemPanel != null)
            {
                _stratagemPanel.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleRoundCompleted += OnBattleRoundCompleted;
                BattleManager.Instance.OnBattleEnded += OnBattleEnded;
            }

            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleRoundCompleted -= OnBattleRoundCompleted;
                BattleManager.Instance.OnBattleEnded -= OnBattleEnded;
            }

            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnBattleRoundCompleted(BattleRoundResult result)
        {
            PlayRoundAnimation(result);

            // 戦闘表示を更新
            if (_battleDisplay != null && BattleManager.Instance?.CurrentBattle != null)
            {
                _battleDisplay.UpdateDisplay(BattleManager.Instance.CurrentBattle);
            }
        }

        private void OnBattleEnded(BattleResult result)
        {
            // 結果アニメーション
            if (_battleDisplay != null)
            {
                _battleDisplay.ShowResult(result);
            }
        }

        private void OnBattleResultClosed()
        {
            ExitBattleScene();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.Battle)
            {
                // 戦闘状態でなくなったらシーンを終了
                ExitBattleScene();
            }
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// 戦闘シーンを終了
        /// </summary>
        public void ExitBattleScene()
        {
            OnBattleSceneExit?.Invoke();

            // GameSceneに戻る
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// 戦闘を強制終了（デバッグ用）
        /// </summary>
        public void ForceEndBattle()
        {
            if (BattleManager.Instance?.IsBattleInProgress == true)
            {
                BattleManager.Instance.EndBattle();
            }
        }

        #endregion
    }
}
