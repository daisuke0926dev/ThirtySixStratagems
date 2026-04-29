using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Campaign;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ゲーム初期化システム
    /// 起動時の初期化シーケンスを管理
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        public static GameInitializer Instance { get; private set; }

        [Header("初期化設定")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private float _minLoadingTime = 1f;
        [SerializeField] private string _mainMenuScene = "MainMenu";

        [Header("マネージャープレファブ")]
        [SerializeField] private GameObject _gameManagerPrefab;
        [SerializeField] private GameObject _audioManagerPrefab;
        [SerializeField] private GameObject _localizationManagerPrefab;
        [SerializeField] private GameObject _visualEffectsManagerPrefab;
        [SerializeField] private GameObject _uiTransitionManagerPrefab;
        [SerializeField] private GameObject _notificationSystemPrefab;

        [Header("プリロード")]
        [SerializeField] private bool _preloadAssets = true;
        [SerializeField] private List<UnityEngine.Object> _preloadedAssets = new List<UnityEngine.Object>();

        // 初期化状態
        private InitializationState _state = InitializationState.NotStarted;
        private float _initializationProgress = 0f;
        private string _currentStepName = "";
        private List<string> _initializationLog = new List<string>();

        // イベント
        public event Action<float, string> OnProgressUpdated;
        public event Action OnInitializationStarted;
        public event Action OnInitializationCompleted;
        public event Action<string> OnInitializationFailed;

        /// <summary>
        /// 初期化状態
        /// </summary>
        public InitializationState State => _state;

        /// <summary>
        /// 初期化進捗（0-1）
        /// </summary>
        public float Progress => _initializationProgress;

        /// <summary>
        /// 現在のステップ名
        /// </summary>
        public string CurrentStepName => _currentStepName;

        /// <summary>
        /// 初期化完了済みか
        /// </summary>
        public bool IsInitialized => _state == InitializationState.Completed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (_initializeOnAwake)
                {
                    StartCoroutine(InitializeGame());
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Initialization Sequence

        /// <summary>
        /// ゲームを初期化
        /// </summary>
        public IEnumerator InitializeGame()
        {
            if (_state == InitializationState.InProgress)
            {
                Debug.LogWarning("Initialization already in progress");
                yield break;
            }

            _state = InitializationState.InProgress;
            _initializationProgress = 0f;
            _initializationLog.Clear();

            float startTime = Time.realtimeSinceStartup;

            OnInitializationStarted?.Invoke();
            LogStep("Initialization started");

            // ステップ1: 基本システム初期化
            yield return InitializeCoreSystems();

            // ステップ2: マネージャー初期化
            yield return InitializeManagers();

            // ステップ3: アセットプリロード
            if (_preloadAssets)
            {
                yield return PreloadAssets();
            }

            // ステップ4: 設定読み込み
            yield return LoadSettings();

            // ステップ5: 最終チェック
            yield return FinalizeInitialization();

            // 最小ロード時間を保証
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadingTime)
            {
                yield return new WaitForSeconds(_minLoadingTime - elapsed);
            }

            _state = InitializationState.Completed;
            _initializationProgress = 1f;

            LogStep("Initialization completed");
            OnInitializationCompleted?.Invoke();

            Debug.Log($"Game initialization completed in {Time.realtimeSinceStartup - startTime:F2} seconds");
        }

        /// <summary>
        /// コアシステムを初期化
        /// </summary>
        private IEnumerator InitializeCoreSystems()
        {
            UpdateProgress(0.1f, "コアシステムを初期化中...");
            LogStep("Initializing core systems");

            // Application設定
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // ログ設定
            Debug.unityLogger.logEnabled = Debug.isDebugBuild;

            yield return null;
        }

        /// <summary>
        /// マネージャーを初期化
        /// </summary>
        private IEnumerator InitializeManagers()
        {
            UpdateProgress(0.2f, "マネージャーを初期化中...");
            LogStep("Initializing managers");

            // GameManager
            if (GameManager.Instance == null && _gameManagerPrefab != null)
            {
                InstantiateManager(_gameManagerPrefab, "GameManager");
            }
            yield return null;
            UpdateProgress(0.3f, "GameManager初期化完了");

            // AudioManager
            if (AudioManager.Instance == null && _audioManagerPrefab != null)
            {
                InstantiateManager(_audioManagerPrefab, "AudioManager");
            }
            yield return null;
            UpdateProgress(0.4f, "AudioManager初期化完了");

            // LocalizationSystem
            if (LocalizationSystem.Instance == null && _localizationManagerPrefab != null)
            {
                InstantiateManager(_localizationManagerPrefab, "LocalizationSystem");
            }
            yield return null;
            UpdateProgress(0.5f, "LocalizationSystem初期化完了");

            // VisualEffectsManager
            if (VisualEffectsManager.Instance == null && _visualEffectsManagerPrefab != null)
            {
                InstantiateManager(_visualEffectsManagerPrefab, "VisualEffectsManager");
            }
            yield return null;
            UpdateProgress(0.55f, "VisualEffectsManager初期化完了");

            // UITransitionManager
            if (UITransitionManager.Instance == null && _uiTransitionManagerPrefab != null)
            {
                InstantiateManager(_uiTransitionManagerPrefab, "UITransitionManager");
            }
            yield return null;
            UpdateProgress(0.6f, "UITransitionManager初期化完了");
        }

        /// <summary>
        /// マネージャーをインスタンス化
        /// </summary>
        private void InstantiateManager(GameObject prefab, string name)
        {
            try
            {
                var instance = Instantiate(prefab);
                instance.name = name;
                LogStep($"Instantiated {name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to instantiate {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// アセットをプリロード
        /// </summary>
        private IEnumerator PreloadAssets()
        {
            UpdateProgress(0.65f, "アセットをプリロード中...");
            LogStep("Preloading assets");

            int totalAssets = _preloadedAssets.Count;
            int loadedAssets = 0;

            foreach (var asset in _preloadedAssets)
            {
                if (asset != null)
                {
                    // アセットにアクセスしてメモリに読み込む
                    var temp = asset.name;
                    loadedAssets++;

                    float progress = 0.65f + (0.15f * loadedAssets / Mathf.Max(1, totalAssets));
                    UpdateProgress(progress, $"アセットを読み込み中... ({loadedAssets}/{totalAssets})");
                }
                yield return null;
            }

            UpdateProgress(0.8f, "アセットプリロード完了");
        }

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        private IEnumerator LoadSettings()
        {
            UpdateProgress(0.85f, "設定を読み込み中...");
            LogStep("Loading settings");

            // 音量設定はAudioManagerで自動読み込み
            // 言語設定はLocalizationSystemで自動読み込み

            // その他の設定
            LoadGraphicsSettings();
            LoadGameplaySettings();

            yield return null;
            UpdateProgress(0.9f, "設定読み込み完了");
        }

        /// <summary>
        /// グラフィック設定を読み込み
        /// </summary>
        private void LoadGraphicsSettings()
        {
            int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            QualitySettings.SetQualityLevel(qualityLevel);

            bool vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
            QualitySettings.vSyncCount = vsync ? 1 : 0;

            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
        /// ゲームプレイ設定を読み込み
        /// </summary>
        private void LoadGameplaySettings()
        {
            // ゲームプレイ関連の設定を読み込み
            // 例: オート保存、難易度など
        }

        /// <summary>
        /// 初期化を完了
        /// </summary>
        private IEnumerator FinalizeInitialization()
        {
            UpdateProgress(0.95f, "初期化を完了中...");
            LogStep("Finalizing initialization");

            // 初期化完了の検証
            bool isValid = ValidateInitialization();

            if (!isValid)
            {
                _state = InitializationState.Failed;
                OnInitializationFailed?.Invoke("Initialization validation failed");
                yield break;
            }

            yield return null;
        }

        /// <summary>
        /// 初期化を検証
        /// </summary>
        private bool ValidateInitialization()
        {
            List<string> errors = new List<string>();

            if (GameManager.Instance == null)
                errors.Add("GameManager not initialized");

            if (AudioManager.Instance == null)
                errors.Add("AudioManager not initialized");

            if (LocalizationSystem.Instance == null)
                errors.Add("LocalizationSystem not initialized");

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError($"Initialization error: {error}");
                }
                return false;
            }

            return true;
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// メインメニューへ遷移
        /// </summary>
        public void GoToMainMenu()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Game not initialized yet");
                return;
            }

            StartCoroutine(LoadSceneAsync(_mainMenuScene));
        }

        /// <summary>
        /// シーンを非同期で読み込み
        /// </summary>
        public IEnumerator LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            // フェードアウト
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.FadeOut(0.5f);
                yield return new WaitForSeconds(0.5f);
            }

            // シーン読み込み
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                yield return null;
            }

            operation.allowSceneActivation = true;
            yield return operation;

            // フェードイン
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.FadeIn(0.5f);
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// シーンを読み込みしトランジション
        /// </summary>
        public void LoadSceneWithTransition(string sceneName)
        {
            if (UITransitionManager.Instance != null)
            {
                UITransitionManager.Instance.DoScreenTransition(() =>
                {
                    SceneManager.LoadScene(sceneName);
                });
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// 進捗を更新
        /// </summary>
        private void UpdateProgress(float progress, string stepName)
        {
            _initializationProgress = progress;
            _currentStepName = stepName;
            OnProgressUpdated?.Invoke(progress, stepName);
        }

        /// <summary>
        /// ステップをログ
        /// </summary>
        private void LogStep(string message)
        {
            _initializationLog.Add($"[{Time.realtimeSinceStartup:F2}] {message}");
            Debug.Log($"[GameInitializer] {message}");
        }

        /// <summary>
        /// 初期化ログを取得
        /// </summary>
        public IReadOnlyList<string> GetInitializationLog()
        {
            return _initializationLog;
        }

        /// <summary>
        /// 強制的に再初期化
        /// </summary>
        public void ForceReinitialize()
        {
            _state = InitializationState.NotStarted;
            StartCoroutine(InitializeGame());
        }

        #endregion

        #region Settings Management

        /// <summary>
        /// グラフィック設定を保存
        /// </summary>
        public void SaveGraphicsSettings(int qualityLevel, bool vsync, bool fullscreen)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Screen.fullScreen = fullscreen;

            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            PlayerPrefs.SetInt("VSync", vsync ? 1 : 0);
            PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 全ての設定をリセット
        /// </summary>
        public void ResetAllSettings()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("All settings have been reset");
        }

        #endregion
    }

    /// <summary>
    /// 初期化状態
    /// </summary>
    public enum InitializationState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
}
