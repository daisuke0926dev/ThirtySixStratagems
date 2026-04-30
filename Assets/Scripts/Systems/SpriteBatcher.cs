using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// スプライトバッチング最適化
    /// 同じマテリアルを使用するスプライトをグループ化してドローコールを削減
    /// </summary>
    public class SpriteBatcher : MonoBehaviour
    {
        private static SpriteBatcher _instance;
        public static SpriteBatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SpriteBatcher");
                    _instance = go.AddComponent<SpriteBatcher>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("バッチ設定")]
        [SerializeField] private bool _enableBatching = true;
        [SerializeField] private int _maxBatchSize = 1000;
        [SerializeField] private bool _dynamicBatching = true;

        [Header("ソート設定")]
        [SerializeField] private bool _enableSorting = true;
        [SerializeField] private SortingMode _sortingMode = SortingMode.ByMaterial;

        private readonly Dictionary<Material, List<SpriteRenderer>> _materialGroups = new Dictionary<Material, List<SpriteRenderer>>();
        private readonly List<SpriteRenderer> _registeredSprites = new List<SpriteRenderer>();

        /// <summary>
        /// 登録されているスプライト数
        /// </summary>
        public int RegisteredSpriteCount => _registeredSprites.Count;

        /// <summary>
        /// マテリアルグループ数
        /// </summary>
        public int MaterialGroupCount => _materialGroups.Count;

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

        private void LateUpdate()
        {
            if (_enableBatching && _enableSorting)
            {
                OptimizeSorting();
            }
        }

        /// <summary>
        /// スプライトを登録
        /// </summary>
        public void RegisterSprite(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null) return;

            if (!_registeredSprites.Contains(spriteRenderer))
            {
                _registeredSprites.Add(spriteRenderer);
                AddToMaterialGroup(spriteRenderer);
            }
        }

        /// <summary>
        /// スプライトの登録を解除
        /// </summary>
        public void UnregisterSprite(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null) return;

            _registeredSprites.Remove(spriteRenderer);
            RemoveFromMaterialGroup(spriteRenderer);
        }

        /// <summary>
        /// 全スプライトの登録を解除
        /// </summary>
        public void ClearAllSprites()
        {
            _registeredSprites.Clear();
            _materialGroups.Clear();
        }

        private void AddToMaterialGroup(SpriteRenderer spriteRenderer)
        {
            var material = spriteRenderer.sharedMaterial;
            if (material == null) return;

            if (!_materialGroups.TryGetValue(material, out var group))
            {
                group = new List<SpriteRenderer>();
                _materialGroups[material] = group;
            }

            group.Add(spriteRenderer);
        }

        private void RemoveFromMaterialGroup(SpriteRenderer spriteRenderer)
        {
            var material = spriteRenderer.sharedMaterial;
            if (material == null) return;

            if (_materialGroups.TryGetValue(material, out var group))
            {
                group.Remove(spriteRenderer);
                if (group.Count == 0)
                {
                    _materialGroups.Remove(material);
                }
            }
        }

        private void OptimizeSorting()
        {
            switch (_sortingMode)
            {
                case SortingMode.ByMaterial:
                    SortByMaterial();
                    break;
                case SortingMode.ByDepth:
                    SortByDepth();
                    break;
                case SortingMode.ByPosition:
                    SortByPosition();
                    break;
            }
        }

        private void SortByMaterial()
        {
            int sortingOrder = 0;

            foreach (var group in _materialGroups.Values)
            {
                foreach (var sprite in group)
                {
                    if (sprite != null && sprite.gameObject.activeInHierarchy)
                    {
                        sprite.sortingOrder = sortingOrder++;
                    }
                }
            }
        }

        private void SortByDepth()
        {
            // Z座標でソート
            _registeredSprites.Sort((a, b) =>
            {
                if (a == null || b == null) return 0;
                return b.transform.position.z.CompareTo(a.transform.position.z);
            });

            int sortingOrder = 0;
            foreach (var sprite in _registeredSprites)
            {
                if (sprite != null && sprite.gameObject.activeInHierarchy)
                {
                    sprite.sortingOrder = sortingOrder++;
                }
            }
        }

        private void SortByPosition()
        {
            // Y座標でソート（上から下へ）
            _registeredSprites.Sort((a, b) =>
            {
                if (a == null || b == null) return 0;
                return b.transform.position.y.CompareTo(a.transform.position.y);
            });

            int sortingOrder = 0;
            foreach (var sprite in _registeredSprites)
            {
                if (sprite != null && sprite.gameObject.activeInHierarchy)
                {
                    sprite.sortingOrder = sortingOrder++;
                }
            }
        }

        /// <summary>
        /// シーン内の全スプライトを自動登録
        /// </summary>
        public void AutoRegisterAllSprites()
        {
            ClearAllSprites();

            var allSprites = FindObjectsOfType<SpriteRenderer>();
            foreach (var sprite in allSprites)
            {
                RegisterSprite(sprite);
            }

            Debug.Log($"[SpriteBatcher] Auto-registered {allSprites.Length} sprites");
        }

        /// <summary>
        /// バッチング統計を取得
        /// </summary>
        public BatchingStatistics GetStatistics()
        {
            var stats = new BatchingStatistics
            {
                TotalSprites = _registeredSprites.Count,
                MaterialGroups = _materialGroups.Count,
                EstimatedDrawCalls = _materialGroups.Count
            };

            // 各マテリアルグループ内のスプライト数をカウント
            foreach (var group in _materialGroups.Values)
            {
                int activeCount = 0;
                foreach (var sprite in group)
                {
                    if (sprite != null && sprite.gameObject.activeInHierarchy)
                    {
                        activeCount++;
                    }
                }
                stats.ActiveSprites += activeCount;

                // バッチサイズを超える場合は追加のドローコールが必要
                if (activeCount > _maxBatchSize)
                {
                    stats.EstimatedDrawCalls += (activeCount / _maxBatchSize);
                }
            }

            return stats;
        }

        /// <summary>
        /// バッチングを有効化/無効化
        /// </summary>
        public void SetBatchingEnabled(bool enabled)
        {
            _enableBatching = enabled;
        }

        /// <summary>
        /// ソートモードを設定
        /// </summary>
        public void SetSortingMode(SortingMode mode)
        {
            _sortingMode = mode;
        }

        /// <summary>
        /// 死んでいるスプライト参照をクリーンアップ
        /// </summary>
        public int CleanupDeadReferences()
        {
            int removed = _registeredSprites.RemoveAll(s => s == null);

            foreach (var group in _materialGroups.Values)
            {
                group.RemoveAll(s => s == null);
            }

            // 空のグループを削除
            var emptyMaterials = new List<Material>();
            foreach (var kvp in _materialGroups)
            {
                if (kvp.Value.Count == 0)
                {
                    emptyMaterials.Add(kvp.Key);
                }
            }

            foreach (var material in emptyMaterials)
            {
                _materialGroups.Remove(material);
            }

            return removed;
        }
    }

    /// <summary>
    /// ソートモード
    /// </summary>
    public enum SortingMode
    {
        /// <summary>マテリアルでグループ化</summary>
        ByMaterial,
        /// <summary>Z深度でソート</summary>
        ByDepth,
        /// <summary>Y位置でソート</summary>
        ByPosition
    }

    /// <summary>
    /// バッチング統計
    /// </summary>
    public class BatchingStatistics
    {
        public int TotalSprites;
        public int ActiveSprites;
        public int MaterialGroups;
        public int EstimatedDrawCalls;

        public override string ToString()
        {
            return $"Sprites: {ActiveSprites}/{TotalSprites}, Groups: {MaterialGroups}, Draw Calls: ~{EstimatedDrawCalls}";
        }
    }

    /// <summary>
    /// スプライト自動登録コンポーネント
    /// SpriteRendererに追加すると自動的にSpriteBatcherに登録される
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BatchedSprite : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (SpriteBatcher.Instance != null)
            {
                SpriteBatcher.Instance.RegisterSprite(_spriteRenderer);
            }
        }

        private void OnDisable()
        {
            if (SpriteBatcher.Instance != null)
            {
                SpriteBatcher.Instance.UnregisterSprite(_spriteRenderer);
            }
        }
    }
}
