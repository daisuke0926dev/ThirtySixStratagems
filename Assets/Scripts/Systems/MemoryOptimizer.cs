using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// メモリ最適化マネージャー
    /// メモリ使用量の監視と最適化を行う
    /// </summary>
    public class MemoryOptimizer : MonoBehaviour
    {
        private static MemoryOptimizer _instance;
        public static MemoryOptimizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("MemoryOptimizer");
                    _instance = go.AddComponent<MemoryOptimizer>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("メモリ監視設定")]
        [SerializeField] private bool _enableMonitoring = true;
        [SerializeField] private float _monitoringInterval = 5f;
        [SerializeField] private long _warningThresholdMB = 512;
        [SerializeField] private long _criticalThresholdMB = 768;

        [Header("自動GC設定")]
        [SerializeField] private bool _enableAutoGC = true;
        [SerializeField] private float _autoGCInterval = 60f;
        [SerializeField] private long _autoGCThresholdMB = 256;

        [Header("アセットアンロード設定")]
        [SerializeField] private bool _enableAutoUnload = true;
        [SerializeField] private float _autoUnloadInterval = 120f;

        /// <summary>
        /// 現在のメモリ使用量（MB）
        /// </summary>
        public long CurrentMemoryMB => GC.GetTotalMemory(false) / (1024 * 1024);

        /// <summary>
        /// メモリ使用量警告イベント
        /// </summary>
        public event Action<MemoryWarningLevel, long> OnMemoryWarning;

        /// <summary>
        /// GC実行イベント
        /// </summary>
        public event Action<long, long> OnGCExecuted;

        private Coroutine _monitoringCoroutine;
        private Coroutine _autoGCCoroutine;
        private Coroutine _autoUnloadCoroutine;
        private long _lastGCMemory;
        private readonly List<WeakReference> _trackedObjects = new List<WeakReference>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            StartOptimization();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                StopOptimization();
                _instance = null;
            }
        }

        /// <summary>
        /// 最適化処理を開始
        /// </summary>
        public void StartOptimization()
        {
            StopOptimization();

            if (_enableMonitoring)
            {
                _monitoringCoroutine = StartCoroutine(MonitorMemoryCoroutine());
            }

            if (_enableAutoGC)
            {
                _autoGCCoroutine = StartCoroutine(AutoGCCoroutine());
            }

            if (_enableAutoUnload)
            {
                _autoUnloadCoroutine = StartCoroutine(AutoUnloadCoroutine());
            }
        }

        /// <summary>
        /// 最適化処理を停止
        /// </summary>
        public void StopOptimization()
        {
            if (_monitoringCoroutine != null)
            {
                StopCoroutine(_monitoringCoroutine);
                _monitoringCoroutine = null;
            }

            if (_autoGCCoroutine != null)
            {
                StopCoroutine(_autoGCCoroutine);
                _autoGCCoroutine = null;
            }

            if (_autoUnloadCoroutine != null)
            {
                StopCoroutine(_autoUnloadCoroutine);
                _autoUnloadCoroutine = null;
            }
        }

        private IEnumerator MonitorMemoryCoroutine()
        {
            var wait = new WaitForSeconds(_monitoringInterval);

            while (true)
            {
                yield return wait;

                long currentMB = CurrentMemoryMB;

                if (currentMB >= _criticalThresholdMB)
                {
                    OnMemoryWarning?.Invoke(MemoryWarningLevel.Critical, currentMB);
                    Debug.LogWarning($"[MemoryOptimizer] Critical memory usage: {currentMB} MB");

                    // 緊急GC実行
                    ForceGarbageCollection();
                }
                else if (currentMB >= _warningThresholdMB)
                {
                    OnMemoryWarning?.Invoke(MemoryWarningLevel.Warning, currentMB);
                    Debug.LogWarning($"[MemoryOptimizer] High memory usage: {currentMB} MB");
                }
            }
        }

        private IEnumerator AutoGCCoroutine()
        {
            var wait = new WaitForSeconds(_autoGCInterval);

            while (true)
            {
                yield return wait;

                long currentMB = CurrentMemoryMB;
                long deltaMB = currentMB - _lastGCMemory;

                if (deltaMB >= _autoGCThresholdMB)
                {
                    RequestGarbageCollection();
                }
            }
        }

        private IEnumerator AutoUnloadCoroutine()
        {
            var wait = new WaitForSeconds(_autoUnloadInterval);

            while (true)
            {
                yield return wait;

                UnloadUnusedAssets();
            }
        }

        /// <summary>
        /// ガベージコレクションを要求（低優先度）
        /// </summary>
        public void RequestGarbageCollection()
        {
            StartCoroutine(GarbageCollectionCoroutine(false));
        }

        /// <summary>
        /// ガベージコレクションを強制実行
        /// </summary>
        public void ForceGarbageCollection()
        {
            long beforeMB = CurrentMemoryMB;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long afterMB = CurrentMemoryMB;
            _lastGCMemory = afterMB;

            OnGCExecuted?.Invoke(beforeMB, afterMB);
            Debug.Log($"[MemoryOptimizer] GC executed: {beforeMB} MB -> {afterMB} MB (freed: {beforeMB - afterMB} MB)");
        }

        private IEnumerator GarbageCollectionCoroutine(bool force)
        {
            // フレームを分散させてGCを実行
            long beforeMB = CurrentMemoryMB;

            yield return null;

            if (force)
            {
                GC.Collect();
            }
            else
            {
                GC.Collect(0, GCCollectionMode.Optimized);
            }

            yield return null;

            GC.WaitForPendingFinalizers();

            yield return null;

            if (force)
            {
                GC.Collect();
            }
            else
            {
                GC.Collect(0, GCCollectionMode.Optimized);
            }

            long afterMB = CurrentMemoryMB;
            _lastGCMemory = afterMB;

            OnGCExecuted?.Invoke(beforeMB, afterMB);
        }

        /// <summary>
        /// 未使用アセットをアンロード
        /// </summary>
        public void UnloadUnusedAssets()
        {
            StartCoroutine(UnloadUnusedAssetsCoroutine());
        }

        private IEnumerator UnloadUnusedAssetsCoroutine()
        {
            var operation = Resources.UnloadUnusedAssets();
            yield return operation;

            RequestGarbageCollection();

            Debug.Log("[MemoryOptimizer] Unused assets unloaded");
        }

        /// <summary>
        /// メモリスナップショットを取得
        /// </summary>
        public MemorySnapshot GetSnapshot()
        {
            return new MemorySnapshot
            {
                Timestamp = DateTime.Now,
                TotalMemoryMB = CurrentMemoryMB,
                MonoHeapMB = Profiler.GetMonoHeapSizeLong() / (1024 * 1024),
                MonoUsedMB = Profiler.GetMonoUsedSizeLong() / (1024 * 1024),
                AllocatedMemoryMB = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024),
                ReservedMemoryMB = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024),
                UnusedReservedMB = Profiler.GetTotalUnusedReservedMemoryLong() / (1024 * 1024),
                GCCollectionCount0 = GC.CollectionCount(0),
                GCCollectionCount1 = GC.CollectionCount(1),
                GCCollectionCount2 = GC.CollectionCount(2)
            };
        }

        /// <summary>
        /// オブジェクトを追跡対象に追加
        /// </summary>
        public void TrackObject(object obj)
        {
            _trackedObjects.Add(new WeakReference(obj));
        }

        /// <summary>
        /// 追跡中のオブジェクトをクリーンアップ
        /// </summary>
        public int CleanupTrackedObjects()
        {
            int removed = _trackedObjects.RemoveAll(wr => !wr.IsAlive);
            return removed;
        }

        /// <summary>
        /// 追跡中のアクティブなオブジェクト数を取得
        /// </summary>
        public int GetActiveTrackedObjectCount()
        {
            int count = 0;
            foreach (var wr in _trackedObjects)
            {
                if (wr.IsAlive) count++;
            }
            return count;
        }

        /// <summary>
        /// シーン遷移時の最適化を実行
        /// </summary>
        public void OptimizeOnSceneChange()
        {
            StartCoroutine(SceneChangeOptimizationCoroutine());
        }

        private IEnumerator SceneChangeOptimizationCoroutine()
        {
            // プールをクリア
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ClearAllPools();
            }

            yield return null;

            // アセットアンロード
            var unloadOp = Resources.UnloadUnusedAssets();
            yield return unloadOp;

            yield return null;

            // GC実行
            ForceGarbageCollection();

            Debug.Log("[MemoryOptimizer] Scene change optimization completed");
        }

        /// <summary>
        /// 低メモリ状況での緊急最適化
        /// </summary>
        public void EmergencyOptimization()
        {
            Debug.LogWarning("[MemoryOptimizer] Emergency optimization started");

            // 全プールクリア
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ClearAllPools();
            }

            // 強制アセットアンロード
            Resources.UnloadUnusedAssets();

            // 強制GC
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            Debug.LogWarning($"[MemoryOptimizer] Emergency optimization completed. Memory: {CurrentMemoryMB} MB");
        }
    }

    /// <summary>
    /// メモリ警告レベル
    /// </summary>
    public enum MemoryWarningLevel
    {
        Normal,
        Warning,
        Critical
    }

    /// <summary>
    /// メモリスナップショット
    /// </summary>
    public class MemorySnapshot
    {
        public DateTime Timestamp;
        public long TotalMemoryMB;
        public long MonoHeapMB;
        public long MonoUsedMB;
        public long AllocatedMemoryMB;
        public long ReservedMemoryMB;
        public long UnusedReservedMB;
        public int GCCollectionCount0;
        public int GCCollectionCount1;
        public int GCCollectionCount2;

        public override string ToString()
        {
            return $"Memory Snapshot [{Timestamp:HH:mm:ss}]\n" +
                   $"  Total: {TotalMemoryMB} MB\n" +
                   $"  Mono Heap: {MonoHeapMB} MB, Used: {MonoUsedMB} MB\n" +
                   $"  Allocated: {AllocatedMemoryMB} MB, Reserved: {ReservedMemoryMB} MB\n" +
                   $"  GC Counts: Gen0={GCCollectionCount0}, Gen1={GCCollectionCount1}, Gen2={GCCollectionCount2}";
        }
    }

    /// <summary>
    /// Unity Profilerラッパー
    /// </summary>
    internal static class Profiler
    {
        public static long GetMonoHeapSizeLong()
        {
#if UNITY_2020_1_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
#else
            return GC.GetTotalMemory(false);
#endif
        }

        public static long GetMonoUsedSizeLong()
        {
#if UNITY_2020_1_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
#else
            return GC.GetTotalMemory(false);
#endif
        }

        public static long GetTotalAllocatedMemoryLong()
        {
#if UNITY_2020_1_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
#else
            return GC.GetTotalMemory(false);
#endif
        }

        public static long GetTotalReservedMemoryLong()
        {
#if UNITY_2020_1_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
#else
            return GC.GetTotalMemory(false);
#endif
        }

        public static long GetTotalUnusedReservedMemoryLong()
        {
#if UNITY_2020_1_OR_NEWER
            return UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong();
#else
            return 0;
#endif
        }
    }
}
