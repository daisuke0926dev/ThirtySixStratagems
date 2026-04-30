using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// 最適化されたアセットローダー
    /// アセットのキャッシュ、非同期ロード、プリロードをサポート
    /// </summary>
    public class AssetLoader : MonoBehaviour
    {
        private static AssetLoader _instance;
        public static AssetLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AssetLoader");
                    _instance = go.AddComponent<AssetLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("キャッシュ設定")]
        [SerializeField] private int _maxCacheSize = 100;
        [SerializeField] private float _cacheExpirationTime = 300f;

        [Header("ロード設定")]
        [SerializeField] private int _maxConcurrentLoads = 3;
        [SerializeField] private float _loadTimeout = 30f;

        private readonly Dictionary<string, CachedAsset> _assetCache = new Dictionary<string, CachedAsset>();
        private readonly Queue<LoadRequest> _loadQueue = new Queue<LoadRequest>();
        private readonly List<Coroutine> _activeLoads = new List<Coroutine>();
        private int _currentConcurrentLoads = 0;

        /// <summary>
        /// キャッシュされたアセット数
        /// </summary>
        public int CachedAssetCount => _assetCache.Count;

        /// <summary>
        /// 待機中のロードリクエスト数
        /// </summary>
        public int PendingLoadCount => _loadQueue.Count;

        /// <summary>
        /// アセットロード完了イベント
        /// </summary>
        public event Action<string, UnityEngine.Object> OnAssetLoaded;

        /// <summary>
        /// アセットロード失敗イベント
        /// </summary>
        public event Action<string, string> OnAssetLoadFailed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            StartCoroutine(ProcessLoadQueueCoroutine());
            StartCoroutine(CleanupCacheCoroutine());
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearCache();
                _instance = null;
            }
        }

        /// <summary>
        /// アセットを同期ロード（キャッシュあり）
        /// </summary>
        /// <typeparam name="T">アセットの型</typeparam>
        /// <param name="path">Resourcesフォルダからの相対パス</param>
        /// <returns>ロードされたアセット</returns>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[AssetLoader] Path cannot be null or empty");
                return null;
            }

            string cacheKey = GetCacheKey<T>(path);

            // キャッシュチェック
            if (_assetCache.TryGetValue(cacheKey, out var cached))
            {
                cached.LastAccessTime = Time.realtimeSinceStartup;
                return cached.Asset as T;
            }

            // ロード
            var asset = Resources.Load<T>(path);
            if (asset != null)
            {
                CacheAsset(cacheKey, asset);
            }
            else
            {
                Debug.LogWarning($"[AssetLoader] Failed to load asset: {path}");
            }

            return asset;
        }

        /// <summary>
        /// アセットを非同期ロード
        /// </summary>
        /// <typeparam name="T">アセットの型</typeparam>
        /// <param name="path">Resourcesフォルダからの相対パス</param>
        /// <param name="callback">完了時コールバック</param>
        public void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[AssetLoader] Path cannot be null or empty");
                callback?.Invoke(null);
                return;
            }

            string cacheKey = GetCacheKey<T>(path);

            // キャッシュチェック
            if (_assetCache.TryGetValue(cacheKey, out var cached))
            {
                cached.LastAccessTime = Time.realtimeSinceStartup;
                callback?.Invoke(cached.Asset as T);
                return;
            }

            // リクエストをキューに追加
            _loadQueue.Enqueue(new LoadRequest
            {
                Path = path,
                CacheKey = cacheKey,
                AssetType = typeof(T),
                Callback = (asset) => callback?.Invoke(asset as T)
            });
        }

        /// <summary>
        /// 複数アセットを一括プリロード
        /// </summary>
        /// <param name="paths">ロードするパスのリスト</param>
        /// <param name="onProgress">進捗コールバック（0.0～1.0）</param>
        /// <param name="onComplete">完了コールバック</param>
        public void PreloadAssets<T>(IList<string> paths, Action<float> onProgress = null, Action onComplete = null) where T : UnityEngine.Object
        {
            StartCoroutine(PreloadAssetsCoroutine<T>(paths, onProgress, onComplete));
        }

        private IEnumerator PreloadAssetsCoroutine<T>(IList<string> paths, Action<float> onProgress, Action onComplete) where T : UnityEngine.Object
        {
            int total = paths.Count;
            int loaded = 0;

            foreach (var path in paths)
            {
                string cacheKey = GetCacheKey<T>(path);

                if (!_assetCache.ContainsKey(cacheKey))
                {
                    var request = Resources.LoadAsync<T>(path);
                    yield return request;

                    if (request.asset != null)
                    {
                        CacheAsset(cacheKey, request.asset);
                    }
                }

                loaded++;
                onProgress?.Invoke((float)loaded / total);
            }

            onComplete?.Invoke();
        }

        private IEnumerator ProcessLoadQueueCoroutine()
        {
            while (true)
            {
                while (_loadQueue.Count > 0 && _currentConcurrentLoads < _maxConcurrentLoads)
                {
                    var request = _loadQueue.Dequeue();
                    StartCoroutine(ProcessLoadRequestCoroutine(request));
                }

                yield return null;
            }
        }

        private IEnumerator ProcessLoadRequestCoroutine(LoadRequest request)
        {
            _currentConcurrentLoads++;

            var resourceRequest = Resources.LoadAsync(request.Path, request.AssetType);

            float startTime = Time.realtimeSinceStartup;

            while (!resourceRequest.isDone)
            {
                if (Time.realtimeSinceStartup - startTime > _loadTimeout)
                {
                    Debug.LogError($"[AssetLoader] Load timeout: {request.Path}");
                    OnAssetLoadFailed?.Invoke(request.Path, "Timeout");
                    request.Callback?.Invoke(null);
                    _currentConcurrentLoads--;
                    yield break;
                }

                yield return null;
            }

            if (resourceRequest.asset != null)
            {
                CacheAsset(request.CacheKey, resourceRequest.asset);
                OnAssetLoaded?.Invoke(request.Path, resourceRequest.asset);
                request.Callback?.Invoke(resourceRequest.asset);
            }
            else
            {
                Debug.LogWarning($"[AssetLoader] Failed to load: {request.Path}");
                OnAssetLoadFailed?.Invoke(request.Path, "Asset not found");
                request.Callback?.Invoke(null);
            }

            _currentConcurrentLoads--;
        }

        private void CacheAsset(string key, UnityEngine.Object asset)
        {
            // キャッシュサイズチェック
            if (_assetCache.Count >= _maxCacheSize)
            {
                EvictOldestCacheEntry();
            }

            _assetCache[key] = new CachedAsset
            {
                Asset = asset,
                LoadTime = Time.realtimeSinceStartup,
                LastAccessTime = Time.realtimeSinceStartup
            };
        }

        private void EvictOldestCacheEntry()
        {
            string oldestKey = null;
            float oldestTime = float.MaxValue;

            foreach (var kvp in _assetCache)
            {
                if (kvp.Value.LastAccessTime < oldestTime)
                {
                    oldestTime = kvp.Value.LastAccessTime;
                    oldestKey = kvp.Key;
                }
            }

            if (oldestKey != null)
            {
                _assetCache.Remove(oldestKey);
            }
        }

        private IEnumerator CleanupCacheCoroutine()
        {
            var wait = new WaitForSeconds(60f);

            while (true)
            {
                yield return wait;

                float currentTime = Time.realtimeSinceStartup;
                var keysToRemove = new List<string>();

                foreach (var kvp in _assetCache)
                {
                    if (currentTime - kvp.Value.LastAccessTime > _cacheExpirationTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _assetCache.Remove(key);
                }

                if (keysToRemove.Count > 0)
                {
                    Debug.Log($"[AssetLoader] Cleaned up {keysToRemove.Count} expired cache entries");
                }
            }
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            _assetCache.Clear();
            Debug.Log("[AssetLoader] Cache cleared");
        }

        /// <summary>
        /// 特定のアセットをキャッシュから削除
        /// </summary>
        public void RemoveFromCache<T>(string path) where T : UnityEngine.Object
        {
            string cacheKey = GetCacheKey<T>(path);
            _assetCache.Remove(cacheKey);
        }

        /// <summary>
        /// アセットがキャッシュされているか確認
        /// </summary>
        public bool IsCached<T>(string path) where T : UnityEngine.Object
        {
            string cacheKey = GetCacheKey<T>(path);
            return _assetCache.ContainsKey(cacheKey);
        }

        /// <summary>
        /// キャッシュ統計を取得
        /// </summary>
        public CacheStatistics GetCacheStatistics()
        {
            return new CacheStatistics
            {
                CachedCount = _assetCache.Count,
                MaxSize = _maxCacheSize,
                PendingLoads = _loadQueue.Count,
                ActiveLoads = _currentConcurrentLoads
            };
        }

        private string GetCacheKey<T>(string path)
        {
            return $"{typeof(T).Name}:{path}";
        }

        private class CachedAsset
        {
            public UnityEngine.Object Asset;
            public float LoadTime;
            public float LastAccessTime;
        }

        private class LoadRequest
        {
            public string Path;
            public string CacheKey;
            public Type AssetType;
            public Action<UnityEngine.Object> Callback;
        }
    }

    /// <summary>
    /// キャッシュ統計
    /// </summary>
    public class CacheStatistics
    {
        public int CachedCount;
        public int MaxSize;
        public int PendingLoads;
        public int ActiveLoads;

        public override string ToString()
        {
            return $"Cache: {CachedCount}/{MaxSize}, Pending: {PendingLoads}, Active: {ActiveLoads}";
        }
    }
}
