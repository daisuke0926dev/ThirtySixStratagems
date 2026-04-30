using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// プール管理マネージャー
    /// 複数のオブジェクトプールを一元管理
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PoolManager");
                    _instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("プール設定")]
        [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();

        [Header("デフォルト設定")]
        [SerializeField] private int _defaultCapacity = 10;
        [SerializeField] private int _defaultMaxSize = 100;

        private readonly Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();
        private Transform _poolRoot;

        /// <summary>
        /// 登録されているプールのキー一覧
        /// </summary>
        public IEnumerable<string> PoolKeys => _pools.Keys;

        /// <summary>
        /// プール統計情報
        /// </summary>
        public PoolStatistics GetStatistics()
        {
            var stats = new PoolStatistics();
            foreach (var kvp in _pools)
            {
                stats.TotalPools++;
                stats.TotalObjects += kvp.Value.CountAll;
                stats.ActiveObjects += kvp.Value.CountActive;
                stats.InactiveObjects += kvp.Value.CountInactive;
            }
            return stats;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _poolRoot = new GameObject("PoolRoot").transform;
            _poolRoot.SetParent(transform);

            InitializePools();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearAllPools();
                _instance = null;
            }
        }

        private void InitializePools()
        {
            foreach (var config in _poolConfigs)
            {
                if (config.Prefab != null && !string.IsNullOrEmpty(config.Key))
                {
                    CreatePool(config.Key, config.Prefab, config.InitialCapacity, config.MaxSize, config.Prewarm);
                }
            }
        }

        /// <summary>
        /// 新しいプールを作成
        /// </summary>
        /// <param name="key">プールの識別キー</param>
        /// <param name="prefab">プールするプレハブ</param>
        /// <param name="initialCapacity">初期容量</param>
        /// <param name="maxSize">最大サイズ</param>
        /// <param name="prewarm">事前生成するオブジェクト数</param>
        public void CreatePool(string key, GameObject prefab, int initialCapacity = -1, int maxSize = -1, int prewarm = 0)
        {
            if (_pools.ContainsKey(key))
            {
                Debug.LogWarning($"Pool with key '{key}' already exists.");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError($"Cannot create pool '{key}' with null prefab.");
                return;
            }

            var poolParent = new GameObject($"Pool_{key}").transform;
            poolParent.SetParent(_poolRoot);

            var pool = new GameObjectPool(
                prefab,
                poolParent,
                initialCapacity > 0 ? initialCapacity : _defaultCapacity,
                maxSize > 0 ? maxSize : _defaultMaxSize
            );

            if (prewarm > 0)
            {
                pool.Prewarm(prewarm);
            }

            _pools[key] = pool;
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        /// <param name="key">プールの識別キー</param>
        /// <returns>プールされたオブジェクト（プールが存在しない場合はnull）</returns>
        public GameObject Get(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                return pool.Get();
            }

            Debug.LogWarning($"Pool with key '{key}' does not exist.");
            return null;
        }

        /// <summary>
        /// プールからオブジェクトを取得し、位置と回転を設定
        /// </summary>
        public GameObject Get(string key, Vector3 position, Quaternion rotation)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                return pool.Get(position, rotation);
            }

            Debug.LogWarning($"Pool with key '{key}' does not exist.");
            return null;
        }

        /// <summary>
        /// プールからオブジェクトを取得し、親を設定
        /// </summary>
        public GameObject Get(string key, Transform parent)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                return pool.Get(parent);
            }

            Debug.LogWarning($"Pool with key '{key}' does not exist.");
            return null;
        }

        /// <summary>
        /// オブジェクトをプールに返却
        /// </summary>
        /// <param name="key">プールの識別キー</param>
        /// <param name="obj">返却するオブジェクト</param>
        public void Release(string key, GameObject obj)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Release(obj);
            }
            else
            {
                Debug.LogWarning($"Pool with key '{key}' does not exist. Destroying object.");
                Destroy(obj);
            }
        }

        /// <summary>
        /// 指定したプールをクリア
        /// </summary>
        public void ClearPool(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// 全てのプールをクリア
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// プールが存在するか確認
        /// </summary>
        public bool HasPool(string key)
        {
            return _pools.ContainsKey(key);
        }

        /// <summary>
        /// プールの情報を取得
        /// </summary>
        public PoolInfo GetPoolInfo(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                return new PoolInfo
                {
                    Key = key,
                    CountAll = pool.CountAll,
                    CountActive = pool.CountActive,
                    CountInactive = pool.CountInactive
                };
            }
            return null;
        }

        /// <summary>
        /// 全プールの情報を取得
        /// </summary>
        public List<PoolInfo> GetAllPoolInfo()
        {
            var infos = new List<PoolInfo>();
            foreach (var kvp in _pools)
            {
                infos.Add(new PoolInfo
                {
                    Key = kvp.Key,
                    CountAll = kvp.Value.CountAll,
                    CountActive = kvp.Value.CountActive,
                    CountInactive = kvp.Value.CountInactive
                });
            }
            return infos;
        }
    }

    /// <summary>
    /// プール設定
    /// </summary>
    [Serializable]
    public class PoolConfig
    {
        public string Key;
        public GameObject Prefab;
        public int InitialCapacity = 10;
        public int MaxSize = 100;
        public int Prewarm = 0;
    }

    /// <summary>
    /// プール情報
    /// </summary>
    public class PoolInfo
    {
        public string Key;
        public int CountAll;
        public int CountActive;
        public int CountInactive;
    }

    /// <summary>
    /// プール統計情報
    /// </summary>
    public class PoolStatistics
    {
        public int TotalPools;
        public int TotalObjects;
        public int ActiveObjects;
        public int InactiveObjects;

        public override string ToString()
        {
            return $"Pools: {TotalPools}, Total: {TotalObjects}, Active: {ActiveObjects}, Inactive: {InactiveObjects}";
        }
    }
}
