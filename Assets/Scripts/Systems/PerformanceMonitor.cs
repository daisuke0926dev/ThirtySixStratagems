using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// パフォーマンス監視システム
    /// FPS、メモリ、描画コールなどのメトリクスを監視
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        private static PerformanceMonitor _instance;
        public static PerformanceMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PerformanceMonitor");
                    _instance = go.AddComponent<PerformanceMonitor>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("監視設定")]
        [SerializeField] private bool _enableMonitoring = true;
        [SerializeField] private float _sampleInterval = 0.5f;
        [SerializeField] private int _sampleHistorySize = 120;

        [Header("表示設定")]
        [SerializeField] private bool _showOverlay = false;
        [SerializeField] private OverlayPosition _overlayPosition = OverlayPosition.TopLeft;
        [SerializeField] private Color _overlayBackgroundColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private Color _overlayTextColor = Color.white;

        [Header("警告しきい値")]
        [SerializeField] private float _lowFPSThreshold = 30f;
        [SerializeField] private float _criticalFPSThreshold = 15f;
        [SerializeField] private long _highMemoryThresholdMB = 512;

        /// <summary>
        /// 現在のFPS
        /// </summary>
        public float CurrentFPS { get; private set; }

        /// <summary>
        /// 平均FPS
        /// </summary>
        public float AverageFPS { get; private set; }

        /// <summary>
        /// 最小FPS
        /// </summary>
        public float MinFPS { get; private set; } = float.MaxValue;

        /// <summary>
        /// 最大FPS
        /// </summary>
        public float MaxFPS { get; private set; }

        /// <summary>
        /// 現在のフレーム時間（ミリ秒）
        /// </summary>
        public float FrameTimeMS { get; private set; }

        /// <summary>
        /// メモリ使用量（MB）
        /// </summary>
        public long MemoryUsageMB { get; private set; }

        /// <summary>
        /// パフォーマンス警告イベント
        /// </summary>
        public event Action<PerformanceWarning> OnPerformanceWarning;

        private readonly Queue<float> _fpsSamples = new Queue<float>();
        private readonly Queue<float> _frameTimeSamples = new Queue<float>();
        private float _fpsAccumulator;
        private int _fpsFrameCount;
        private float _lastSampleTime;
        private float _deltaTime;
        private GUIStyle _overlayStyle;
        private Rect _overlayRect;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            if (!_enableMonitoring) return;

            UpdateFPS();
            UpdateMetrics();
        }

        private void UpdateFPS()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;

            if (Time.realtimeSinceStartup - _lastSampleTime >= _sampleInterval)
            {
                CurrentFPS = _fpsFrameCount / _fpsAccumulator;
                FrameTimeMS = _deltaTime * 1000f;

                // サンプル履歴更新
                _fpsSamples.Enqueue(CurrentFPS);
                _frameTimeSamples.Enqueue(FrameTimeMS);

                while (_fpsSamples.Count > _sampleHistorySize)
                {
                    _fpsSamples.Dequeue();
                    _frameTimeSamples.Dequeue();
                }

                // 統計更新
                UpdateStatistics();

                // 警告チェック
                CheckPerformanceWarnings();

                // リセット
                _fpsAccumulator = 0;
                _fpsFrameCount = 0;
                _lastSampleTime = Time.realtimeSinceStartup;
            }
        }

        private void UpdateMetrics()
        {
            MemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024);
        }

        private void UpdateStatistics()
        {
            if (_fpsSamples.Count == 0) return;

            float sum = 0;
            float min = float.MaxValue;
            float max = 0;

            foreach (var fps in _fpsSamples)
            {
                sum += fps;
                if (fps < min) min = fps;
                if (fps > max) max = fps;
            }

            AverageFPS = sum / _fpsSamples.Count;
            MinFPS = min;
            MaxFPS = max;
        }

        private void CheckPerformanceWarnings()
        {
            if (CurrentFPS < _criticalFPSThreshold)
            {
                OnPerformanceWarning?.Invoke(new PerformanceWarning
                {
                    Type = WarningType.CriticalFPS,
                    Message = $"Critical FPS: {CurrentFPS:F1}",
                    Value = CurrentFPS
                });
            }
            else if (CurrentFPS < _lowFPSThreshold)
            {
                OnPerformanceWarning?.Invoke(new PerformanceWarning
                {
                    Type = WarningType.LowFPS,
                    Message = $"Low FPS: {CurrentFPS:F1}",
                    Value = CurrentFPS
                });
            }

            if (MemoryUsageMB > _highMemoryThresholdMB)
            {
                OnPerformanceWarning?.Invoke(new PerformanceWarning
                {
                    Type = WarningType.HighMemory,
                    Message = $"High memory: {MemoryUsageMB} MB",
                    Value = MemoryUsageMB
                });
            }
        }

        private void OnGUI()
        {
            if (!_showOverlay || !_enableMonitoring) return;

            InitializeOverlayStyle();

            float width = 200;
            float height = 100;
            float padding = 10;

            switch (_overlayPosition)
            {
                case OverlayPosition.TopLeft:
                    _overlayRect = new Rect(padding, padding, width, height);
                    break;
                case OverlayPosition.TopRight:
                    _overlayRect = new Rect(Screen.width - width - padding, padding, width, height);
                    break;
                case OverlayPosition.BottomLeft:
                    _overlayRect = new Rect(padding, Screen.height - height - padding, width, height);
                    break;
                case OverlayPosition.BottomRight:
                    _overlayRect = new Rect(Screen.width - width - padding, Screen.height - height - padding, width, height);
                    break;
            }

            // 背景
            GUI.color = _overlayBackgroundColor;
            GUI.Box(_overlayRect, GUIContent.none);
            GUI.color = Color.white;

            // テキスト
            GUILayout.BeginArea(_overlayRect);
            GUILayout.BeginVertical();

            _overlayStyle.normal.textColor = GetFPSColor(CurrentFPS);
            GUILayout.Label($"FPS: {CurrentFPS:F1} ({FrameTimeMS:F1}ms)", _overlayStyle);

            _overlayStyle.normal.textColor = _overlayTextColor;
            GUILayout.Label($"Avg: {AverageFPS:F1} Min: {MinFPS:F1} Max: {MaxFPS:F1}", _overlayStyle);

            _overlayStyle.normal.textColor = GetMemoryColor(MemoryUsageMB);
            GUILayout.Label($"Memory: {MemoryUsageMB} MB", _overlayStyle);

            if (PoolManager.Instance != null)
            {
                var stats = PoolManager.Instance.GetStatistics();
                _overlayStyle.normal.textColor = _overlayTextColor;
                GUILayout.Label($"Pools: {stats.TotalPools} ({stats.ActiveObjects}/{stats.TotalObjects})", _overlayStyle);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void InitializeOverlayStyle()
        {
            if (_overlayStyle == null)
            {
                _overlayStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(5, 5, 2, 2)
                };
            }
        }

        private Color GetFPSColor(float fps)
        {
            if (fps < _criticalFPSThreshold) return Color.red;
            if (fps < _lowFPSThreshold) return Color.yellow;
            return Color.green;
        }

        private Color GetMemoryColor(long memoryMB)
        {
            if (memoryMB > _highMemoryThresholdMB) return Color.red;
            if (memoryMB > _highMemoryThresholdMB * 0.75f) return Color.yellow;
            return _overlayTextColor;
        }

        /// <summary>
        /// オーバーレイ表示を切り替え
        /// </summary>
        public void ToggleOverlay()
        {
            _showOverlay = !_showOverlay;
        }

        /// <summary>
        /// オーバーレイ表示を設定
        /// </summary>
        public void SetOverlayVisible(bool visible)
        {
            _showOverlay = visible;
        }

        /// <summary>
        /// 監視を有効化/無効化
        /// </summary>
        public void SetMonitoringEnabled(bool enabled)
        {
            _enableMonitoring = enabled;
        }

        /// <summary>
        /// サンプル履歴をクリア
        /// </summary>
        public void ClearHistory()
        {
            _fpsSamples.Clear();
            _frameTimeSamples.Clear();
            MinFPS = float.MaxValue;
            MaxFPS = 0;
        }

        /// <summary>
        /// パフォーマンスレポートを取得
        /// </summary>
        public PerformanceReport GetReport()
        {
            var report = new PerformanceReport
            {
                Timestamp = DateTime.Now,
                CurrentFPS = CurrentFPS,
                AverageFPS = AverageFPS,
                MinFPS = MinFPS,
                MaxFPS = MaxFPS,
                FrameTimeMS = FrameTimeMS,
                MemoryUsageMB = MemoryUsageMB,
                SampleCount = _fpsSamples.Count
            };

            if (PoolManager.Instance != null)
            {
                report.PoolStatistics = PoolManager.Instance.GetStatistics();
            }

            if (AssetLoader.Instance != null)
            {
                report.CacheStatistics = AssetLoader.Instance.GetCacheStatistics();
            }

            return report;
        }

        /// <summary>
        /// FPSサンプル履歴を取得
        /// </summary>
        public float[] GetFPSHistory()
        {
            return _fpsSamples.ToArray();
        }

        /// <summary>
        /// フレーム時間サンプル履歴を取得
        /// </summary>
        public float[] GetFrameTimeHistory()
        {
            return _frameTimeSamples.ToArray();
        }
    }

    /// <summary>
    /// オーバーレイ位置
    /// </summary>
    public enum OverlayPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// 警告タイプ
    /// </summary>
    public enum WarningType
    {
        LowFPS,
        CriticalFPS,
        HighMemory,
        HighDrawCalls
    }

    /// <summary>
    /// パフォーマンス警告
    /// </summary>
    public class PerformanceWarning
    {
        public WarningType Type;
        public string Message;
        public float Value;
    }

    /// <summary>
    /// パフォーマンスレポート
    /// </summary>
    public class PerformanceReport
    {
        public DateTime Timestamp;
        public float CurrentFPS;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float FrameTimeMS;
        public long MemoryUsageMB;
        public int SampleCount;
        public PoolStatistics PoolStatistics;
        public CacheStatistics CacheStatistics;

        public override string ToString()
        {
            return $"Performance Report [{Timestamp:HH:mm:ss}]\n" +
                   $"  FPS: {CurrentFPS:F1} (Avg: {AverageFPS:F1}, Min: {MinFPS:F1}, Max: {MaxFPS:F1})\n" +
                   $"  Frame Time: {FrameTimeMS:F2}ms\n" +
                   $"  Memory: {MemoryUsageMB} MB\n" +
                   $"  Samples: {SampleCount}\n" +
                   (PoolStatistics != null ? $"  {PoolStatistics}\n" : "") +
                   (CacheStatistics != null ? $"  {CacheStatistics}" : "");
        }
    }
}
