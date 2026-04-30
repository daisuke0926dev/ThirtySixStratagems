using System;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// プラットフォーム管理
    /// プラットフォーム固有の設定と最適化を管理
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        private static PlatformManager _instance;
        public static PlatformManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PlatformManager");
                    _instance = go.AddComponent<PlatformManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("品質設定")]
        [SerializeField] private QualityPreset _mobileQuality = QualityPreset.Medium;
        [SerializeField] private QualityPreset _webGLQuality = QualityPreset.Medium;
        [SerializeField] private QualityPreset _desktopQuality = QualityPreset.High;

        [Header("解像度設定")]
        [SerializeField] private bool _autoResolution = true;
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private int _mobileTargetFrameRate = 30;

        [Header("プラットフォーム固有")]
        [SerializeField] private bool _enableMobileOptimizations = true;
        [SerializeField] private bool _enableWebGLOptimizations = true;

        /// <summary>
        /// 現在のプラットフォーム
        /// </summary>
        public GamePlatform CurrentPlatform { get; private set; }

        /// <summary>
        /// モバイルプラットフォームかどうか
        /// </summary>
        public bool IsMobile => CurrentPlatform == GamePlatform.Android || CurrentPlatform == GamePlatform.iOS;

        /// <summary>
        /// WebGLかどうか
        /// </summary>
        public bool IsWebGL => CurrentPlatform == GamePlatform.WebGL;

        /// <summary>
        /// デスクトップかどうか
        /// </summary>
        public bool IsDesktop => CurrentPlatform == GamePlatform.Windows ||
                                  CurrentPlatform == GamePlatform.MacOS ||
                                  CurrentPlatform == GamePlatform.Linux;

        /// <summary>
        /// プラットフォーム初期化完了イベント
        /// </summary>
        public event Action<GamePlatform> OnPlatformInitialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            DetectPlatform();
            ApplyPlatformSettings();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void DetectPlatform()
        {
#if UNITY_ANDROID
            CurrentPlatform = GamePlatform.Android;
#elif UNITY_IOS
            CurrentPlatform = GamePlatform.iOS;
#elif UNITY_WEBGL
            CurrentPlatform = GamePlatform.WebGL;
#elif UNITY_STANDALONE_WIN
            CurrentPlatform = GamePlatform.Windows;
#elif UNITY_STANDALONE_OSX
            CurrentPlatform = GamePlatform.MacOS;
#elif UNITY_STANDALONE_LINUX
            CurrentPlatform = GamePlatform.Linux;
#else
            CurrentPlatform = GamePlatform.Unknown;
#endif

            Debug.Log($"[PlatformManager] Detected platform: {CurrentPlatform}");
        }

        private void ApplyPlatformSettings()
        {
            ApplyQualitySettings();
            ApplyFrameRateSettings();
            ApplyPlatformOptimizations();

            OnPlatformInitialized?.Invoke(CurrentPlatform);
        }

        private void ApplyQualitySettings()
        {
            QualityPreset preset;

            if (IsMobile)
            {
                preset = _mobileQuality;
            }
            else if (IsWebGL)
            {
                preset = _webGLQuality;
            }
            else
            {
                preset = _desktopQuality;
            }

            ApplyQualityPreset(preset);
        }

        private void ApplyQualityPreset(QualityPreset preset)
        {
            int qualityLevel;

            switch (preset)
            {
                case QualityPreset.Low:
                    qualityLevel = 0;
                    break;
                case QualityPreset.Medium:
                    qualityLevel = 2;
                    break;
                case QualityPreset.High:
                    qualityLevel = 4;
                    break;
                case QualityPreset.Ultra:
                    qualityLevel = 5;
                    break;
                default:
                    qualityLevel = 2;
                    break;
            }

            // Unityの品質レベルの範囲をチェック
            if (qualityLevel >= QualitySettings.names.Length)
            {
                qualityLevel = QualitySettings.names.Length - 1;
            }

            QualitySettings.SetQualityLevel(qualityLevel, true);
            Debug.Log($"[PlatformManager] Quality level set to: {QualitySettings.names[qualityLevel]}");
        }

        private void ApplyFrameRateSettings()
        {
            int targetFPS = IsMobile ? _mobileTargetFrameRate : _targetFrameRate;
            Application.targetFrameRate = targetFPS;

            // VSyncの設定
            if (IsMobile || IsWebGL)
            {
                QualitySettings.vSyncCount = 0;
            }
            else
            {
                QualitySettings.vSyncCount = 1;
            }

            Debug.Log($"[PlatformManager] Target frame rate: {targetFPS}, VSync: {QualitySettings.vSyncCount}");
        }

        private void ApplyPlatformOptimizations()
        {
            if (IsMobile && _enableMobileOptimizations)
            {
                ApplyMobileOptimizations();
            }

            if (IsWebGL && _enableWebGLOptimizations)
            {
                ApplyWebGLOptimizations();
            }

            if (IsDesktop)
            {
                ApplyDesktopOptimizations();
            }
        }

        private void ApplyMobileOptimizations()
        {
            // モバイル向け最適化
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 解像度の最適化
            if (_autoResolution)
            {
                float scale = 1.0f;
                int screenWidth = Screen.width;

                if (screenWidth > 1920)
                {
                    scale = 1920f / screenWidth;
                }

                if (scale < 1.0f)
                {
                    Screen.SetResolution(
                        (int)(Screen.width * scale),
                        (int)(Screen.height * scale),
                        true
                    );
                }
            }

            // シャドウを軽量化
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;

            Debug.Log("[PlatformManager] Mobile optimizations applied");
        }

        private void ApplyWebGLOptimizations()
        {
            // WebGL向け最適化
            Application.runInBackground = false;

            // メモリ使用量の制限
            // WebGLではGCを頻繁に行わない方がよい

            Debug.Log("[PlatformManager] WebGL optimizations applied");
        }

        private void ApplyDesktopOptimizations()
        {
            // デスクトップ向け最適化
            Application.runInBackground = true;

            // フルスクリーンモードの設定
            if (_autoResolution)
            {
                var resolutions = Screen.resolutions;
                if (resolutions.Length > 0)
                {
                    var bestResolution = resolutions[resolutions.Length - 1];
                    // 現在のフルスクリーンモードを維持
                }
            }

            Debug.Log("[PlatformManager] Desktop optimizations applied");
        }

        /// <summary>
        /// 品質プリセットを変更
        /// </summary>
        public void SetQualityPreset(QualityPreset preset)
        {
            ApplyQualityPreset(preset);
        }

        /// <summary>
        /// ターゲットフレームレートを設定
        /// </summary>
        public void SetTargetFrameRate(int fps)
        {
            Application.targetFrameRate = fps;
        }

        /// <summary>
        /// フルスクリーンモードを切り替え
        /// </summary>
        public void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        /// <summary>
        /// 解像度を設定
        /// </summary>
        public void SetResolution(int width, int height, bool fullscreen)
        {
            Screen.SetResolution(width, height, fullscreen);
        }

        /// <summary>
        /// 利用可能な解像度を取得
        /// </summary>
        public Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions;
        }

        /// <summary>
        /// プラットフォーム情報を取得
        /// </summary>
        public PlatformInfo GetPlatformInfo()
        {
            return new PlatformInfo
            {
                Platform = CurrentPlatform,
                DeviceModel = SystemInfo.deviceModel,
                DeviceName = SystemInfo.deviceName,
                OperatingSystem = SystemInfo.operatingSystem,
                ProcessorType = SystemInfo.processorType,
                ProcessorCount = SystemInfo.processorCount,
                SystemMemoryMB = SystemInfo.systemMemorySize,
                GraphicsDeviceName = SystemInfo.graphicsDeviceName,
                GraphicsMemoryMB = SystemInfo.graphicsMemorySize,
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                ScreenDPI = Screen.dpi,
                CurrentQualityLevel = QualitySettings.names[QualitySettings.GetQualityLevel()],
                TargetFrameRate = Application.targetFrameRate
            };
        }
    }

    /// <summary>
    /// ゲームプラットフォーム
    /// </summary>
    public enum GamePlatform
    {
        Unknown,
        Windows,
        MacOS,
        Linux,
        WebGL,
        Android,
        iOS
    }

    /// <summary>
    /// 品質プリセット
    /// </summary>
    public enum QualityPreset
    {
        Low,
        Medium,
        High,
        Ultra
    }

    /// <summary>
    /// プラットフォーム情報
    /// </summary>
    public class PlatformInfo
    {
        public GamePlatform Platform;
        public string DeviceModel;
        public string DeviceName;
        public string OperatingSystem;
        public string ProcessorType;
        public int ProcessorCount;
        public int SystemMemoryMB;
        public string GraphicsDeviceName;
        public int GraphicsMemoryMB;
        public int ScreenWidth;
        public int ScreenHeight;
        public float ScreenDPI;
        public string CurrentQualityLevel;
        public int TargetFrameRate;

        public override string ToString()
        {
            return $"Platform Info:\n" +
                   $"  Platform: {Platform}\n" +
                   $"  Device: {DeviceModel} ({DeviceName})\n" +
                   $"  OS: {OperatingSystem}\n" +
                   $"  CPU: {ProcessorType} ({ProcessorCount} cores)\n" +
                   $"  RAM: {SystemMemoryMB} MB\n" +
                   $"  GPU: {GraphicsDeviceName} ({GraphicsMemoryMB} MB)\n" +
                   $"  Screen: {ScreenWidth}x{ScreenHeight} @ {ScreenDPI} DPI\n" +
                   $"  Quality: {CurrentQualityLevel}\n" +
                   $"  Target FPS: {TargetFrameRate}";
        }
    }
}
