using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ゲームブートストラップ
    /// ゲーム起動時の全システム初期化を統括
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("起動設定")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private string _startScene = "MainMenu";
        [SerializeField] private float _splashDuration = 2f;

        [Header("システムプレファブ")]
        [SerializeField] private GameObject _gameManagerPrefab;
        [SerializeField] private GameObject _audioManagerPrefab;
        [SerializeField] private GameObject _localizationSystemPrefab;
        [SerializeField] private GameObject _visualEffectsManagerPrefab;
        [SerializeField] private GameObject _uiTransitionManagerPrefab;
        [SerializeField] private GameObject _settingsManagerPrefab;
        [SerializeField] private GameObject _saveLoadSystemPrefab;
        [SerializeField] private GameObject _debugConsolePrefab;

        [Header("スプラッシュ")]
        [SerializeField] private GameObject _splashScreen;
        [SerializeField] private CanvasGroup _splashCanvasGroup;

        // 状態
        private bool _isInitialized = false;
        private BootstrapState _state = BootstrapState.NotStarted;

        // イベント
        public event Action OnBootstrapStarted;
        public event Action<float, string> OnBootstrapProgress;
        public event Action OnBootstrapCompleted;
        public event Action<string> OnBootstrapFailed;

        /// <summary>
        /// 初期化済みか
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 現在の状態
        /// </summary>
        public BootstrapState State => _state;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (_autoInitialize)
                {
                    StartCoroutine(Bootstrap());
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Bootstrap Sequence

        /// <summary>
        /// ブートストラップを開始
        /// </summary>
        public IEnumerator Bootstrap()
        {
            if (_state == BootstrapState.InProgress)
            {
                Debug.LogWarning("Bootstrap already in progress");
                yield break;
            }

            _state = BootstrapState.InProgress;
            OnBootstrapStarted?.Invoke();

            Debug.Log("=== GameBootstrap: Starting ===");

            // スプラッシュ表示
            ShowSplash();
            float startTime = Time.realtimeSinceStartup;

            // Step 1: コアシステム
            yield return InitializeCoreSystemsStep();

            // Step 2: オーディオ・ビジュアル
            yield return InitializeAudioVisualStep();

            // Step 3: データシステム
            yield return InitializeDataSystemsStep();

            // Step 4: UIシステム
            yield return InitializeUISystemsStep();

            // Step 5: デバッグシステム
            yield return InitializeDebugSystemsStep();

            // Step 6: 最終検証
            yield return FinalValidationStep();

            // スプラッシュ時間を確保
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _splashDuration)
            {
                yield return new WaitForSeconds(_splashDuration - elapsed);
            }

            // スプラッシュを隠す
            yield return HideSplash();

            _isInitialized = true;
            _state = BootstrapState.Completed;

            Debug.Log($"=== GameBootstrap: Completed ({Time.realtimeSinceStartup - startTime:F2}s) ===");
            OnBootstrapCompleted?.Invoke();

            // 開始シーンへ遷移
            if (!string.IsNullOrEmpty(_startScene) && SceneManager.GetActiveScene().name != _startScene)
            {
                SceneManager.LoadScene(_startScene);
            }
        }

        /// <summary>
        /// コアシステム初期化
        /// </summary>
        private IEnumerator InitializeCoreSystemsStep()
        {
            OnBootstrapProgress?.Invoke(0.1f, "コアシステムを初期化中...");
            Debug.Log("Initializing core systems...");

            // GameManager
            if (GameManager.Instance == null)
            {
                InstantiateSystem(_gameManagerPrefab, "GameManager");
            }
            yield return null;

            // SettingsManager
            if (SettingsManager.Instance == null)
            {
                InstantiateSystem(_settingsManagerPrefab, "SettingsManager");
            }
            yield return null;

            OnBootstrapProgress?.Invoke(0.2f, "コアシステム初期化完了");
        }

        /// <summary>
        /// オーディオ・ビジュアル初期化
        /// </summary>
        private IEnumerator InitializeAudioVisualStep()
        {
            OnBootstrapProgress?.Invoke(0.3f, "オーディオシステムを初期化中...");
            Debug.Log("Initializing audio/visual systems...");

            // AudioManager
            if (AudioManager.Instance == null)
            {
                InstantiateSystem(_audioManagerPrefab, "AudioManager");
            }
            yield return null;

            // VisualEffectsManager
            if (VisualEffectsManager.Instance == null)
            {
                InstantiateSystem(_visualEffectsManagerPrefab, "VisualEffectsManager");
            }
            yield return null;

            OnBootstrapProgress?.Invoke(0.4f, "オーディオシステム初期化完了");
        }

        /// <summary>
        /// データシステム初期化
        /// </summary>
        private IEnumerator InitializeDataSystemsStep()
        {
            OnBootstrapProgress?.Invoke(0.5f, "データシステムを初期化中...");
            Debug.Log("Initializing data systems...");

            // LocalizationSystem
            if (LocalizationSystem.Instance == null)
            {
                InstantiateSystem(_localizationSystemPrefab, "LocalizationSystem");
            }
            yield return null;

            // SaveLoadSystem
            if (SaveLoadSystem.Instance == null)
            {
                InstantiateSystem(_saveLoadSystemPrefab, "SaveLoadSystem");
            }
            yield return null;

            OnBootstrapProgress?.Invoke(0.6f, "データシステム初期化完了");
        }

        /// <summary>
        /// UIシステム初期化
        /// </summary>
        private IEnumerator InitializeUISystemsStep()
        {
            OnBootstrapProgress?.Invoke(0.7f, "UIシステムを初期化中...");
            Debug.Log("Initializing UI systems...");

            // UITransitionManager
            if (UITransitionManager.Instance == null)
            {
                InstantiateSystem(_uiTransitionManagerPrefab, "UITransitionManager");
            }
            yield return null;

            OnBootstrapProgress?.Invoke(0.8f, "UIシステム初期化完了");
        }

        /// <summary>
        /// デバッグシステム初期化
        /// </summary>
        private IEnumerator InitializeDebugSystemsStep()
        {
            OnBootstrapProgress?.Invoke(0.85f, "デバッグシステムを初期化中...");

            // デバッグビルドのみ
            if (Debug.isDebugBuild || Application.isEditor)
            {
                Debug.Log("Initializing debug systems...");

                if (DebugConsole.Instance == null)
                {
                    InstantiateSystem(_debugConsolePrefab, "DebugConsole");
                }
            }
            yield return null;

            OnBootstrapProgress?.Invoke(0.9f, "デバッグシステム初期化完了");
        }

        /// <summary>
        /// 最終検証
        /// </summary>
        private IEnumerator FinalValidationStep()
        {
            OnBootstrapProgress?.Invoke(0.95f, "検証中...");
            Debug.Log("Validating initialization...");

            var errors = new System.Collections.Generic.List<string>();

            // 必須システムチェック
            if (GameManager.Instance == null)
                errors.Add("GameManager");
            if (SettingsManager.Instance == null)
                errors.Add("SettingsManager");
            if (LocalizationSystem.Instance == null)
                errors.Add("LocalizationSystem");

            if (errors.Count > 0)
            {
                string errorMsg = $"初期化に失敗: {string.Join(", ", errors)}";
                Debug.LogError(errorMsg);
                _state = BootstrapState.Failed;
                OnBootstrapFailed?.Invoke(errorMsg);
                yield break;
            }

            yield return null;
            OnBootstrapProgress?.Invoke(1f, "初期化完了");
        }

        #endregion

        #region System Instantiation

        /// <summary>
        /// システムをインスタンス化
        /// </summary>
        private void InstantiateSystem(GameObject prefab, string systemName)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab for {systemName} is not assigned, creating empty GameObject");
                CreateEmptySystem(systemName);
                return;
            }

            try
            {
                var instance = Instantiate(prefab);
                instance.name = systemName;
                Debug.Log($"Instantiated: {systemName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to instantiate {systemName}: {ex.Message}");
            }
        }

        /// <summary>
        /// 空のシステムを作成
        /// </summary>
        private void CreateEmptySystem(string systemName)
        {
            var go = new GameObject(systemName);

            switch (systemName)
            {
                case "GameManager":
                    go.AddComponent<GameManager>();
                    break;
                case "AudioManager":
                    go.AddComponent<AudioManager>();
                    break;
                case "LocalizationSystem":
                    go.AddComponent<LocalizationSystem>();
                    break;
                case "VisualEffectsManager":
                    go.AddComponent<VisualEffectsManager>();
                    break;
                case "UITransitionManager":
                    go.AddComponent<UITransitionManager>();
                    break;
                case "SettingsManager":
                    go.AddComponent<SettingsManager>();
                    break;
                case "SaveLoadSystem":
                    go.AddComponent<SaveLoadSystem>();
                    break;
                case "DebugConsole":
                    go.AddComponent<DebugConsole>();
                    break;
            }
        }

        #endregion

        #region Splash Screen

        /// <summary>
        /// スプラッシュを表示
        /// </summary>
        private void ShowSplash()
        {
            if (_splashScreen != null)
            {
                _splashScreen.SetActive(true);
            }

            if (_splashCanvasGroup != null)
            {
                _splashCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// スプラッシュを隠す
        /// </summary>
        private IEnumerator HideSplash()
        {
            if (_splashCanvasGroup != null)
            {
                float elapsed = 0f;
                float duration = 0.5f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _splashCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    yield return null;
                }

                _splashCanvasGroup.alpha = 0f;
            }

            if (_splashScreen != null)
            {
                _splashScreen.SetActive(false);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// 再起動
        /// </summary>
        public void Restart()
        {
            _isInitialized = false;
            _state = BootstrapState.NotStarted;
            StartCoroutine(Bootstrap());
        }

        /// <summary>
        /// シーンを読み込み
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Game not initialized yet");
                return;
            }

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

        /// <summary>
        /// ゲームを終了
        /// </summary>
        public void QuitGame()
        {
            // 設定を保存
            SettingsManager.Instance?.SaveSettings();

            // オートセーブ
            if (SettingsManager.Instance?.CurrentSettings?.Gameplay?.AutoSave == true)
            {
                SaveLoadSystem.Instance?.AutoSave();
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }

    /// <summary>
    /// ブートストラップ状態
    /// </summary>
    public enum BootstrapState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
}
