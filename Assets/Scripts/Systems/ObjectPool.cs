using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ジェネリックオブジェクトプール
    /// GameObjectやコンポーネントの再利用によりGC負荷を軽減
    /// </summary>
    /// <typeparam name="T">プールするオブジェクトの型</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;
        private int _countAll;

        /// <summary>
        /// プール内のアクティブでないオブジェクト数
        /// </summary>
        public int CountInactive => _pool.Count;

        /// <summary>
        /// 作成された全オブジェクト数
        /// </summary>
        public int CountAll => _countAll;

        /// <summary>
        /// 使用中のオブジェクト数
        /// </summary>
        public int CountActive => _countAll - _pool.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="createFunc">新しいオブジェクトを作成する関数</param>
        /// <param name="onGet">オブジェクト取得時のコールバック</param>
        /// <param name="onRelease">オブジェクト返却時のコールバック</param>
        /// <param name="onDestroy">オブジェクト破棄時のコールバック</param>
        /// <param name="defaultCapacity">初期容量</param>
        /// <param name="maxSize">最大サイズ（0で無制限）</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 1000)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;
            _pool = new Stack<T>(defaultCapacity);
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        public T Get()
        {
            T obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Pop();
            }
            else
            {
                obj = _createFunc();
                _countAll++;
            }

            _onGet?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// オブジェクトをプールに返却
        /// </summary>
        public void Release(T obj)
        {
            if (obj == null) return;

            _onRelease?.Invoke(obj);

            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                _onDestroy?.Invoke(obj);
                _countAll--;
            }
            else
            {
                _pool.Push(obj);
            }
        }

        /// <summary>
        /// プールをクリア
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                _onDestroy?.Invoke(obj);
                _countAll--;
            }
        }

        /// <summary>
        /// プールを事前に指定数まで拡張
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count && (_maxSize == 0 || _countAll < _maxSize); i++)
            {
                var obj = _createFunc();
                _countAll++;
                _pool.Push(obj);
            }
        }
    }

    /// <summary>
    /// GameObjectプール
    /// MonoBehaviourコンポーネントのプーリングに特化
    /// </summary>
    public class GameObjectPool
    {
        private readonly ObjectPool<GameObject> _pool;
        private readonly Transform _parent;
        private readonly GameObject _prefab;

        /// <summary>
        /// プール内のアクティブでないオブジェクト数
        /// </summary>
        public int CountInactive => _pool.CountInactive;

        /// <summary>
        /// 作成された全オブジェクト数
        /// </summary>
        public int CountAll => _pool.CountAll;

        /// <summary>
        /// 使用中のオブジェクト数
        /// </summary>
        public int CountActive => _pool.CountActive;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="prefab">プールするプレハブ</param>
        /// <param name="parent">プールオブジェクトの親Transform</param>
        /// <param name="defaultCapacity">初期容量</param>
        /// <param name="maxSize">最大サイズ</param>
        public GameObjectPool(
            GameObject prefab,
            Transform parent = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;

            _pool = new ObjectPool<GameObject>(
                createFunc: CreateObject,
                onGet: OnGetObject,
                onRelease: OnReleaseObject,
                onDestroy: OnDestroyObject,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private GameObject CreateObject()
        {
            var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            obj.name = $"{_prefab.name}_Pooled";
            obj.SetActive(false);
            return obj;
        }

        private void OnGetObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
            if (_parent != null)
            {
                obj.transform.SetParent(_parent);
            }
        }

        private void OnDestroyObject(GameObject obj)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        public GameObject Get()
        {
            return _pool.Get();
        }

        /// <summary>
        /// プールからオブジェクトを取得し、位置と回転を設定
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var obj = _pool.Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        /// <summary>
        /// プールからオブジェクトを取得し、親を設定
        /// </summary>
        public GameObject Get(Transform parent)
        {
            var obj = _pool.Get();
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        /// <summary>
        /// オブジェクトをプールに返却
        /// </summary>
        public void Release(GameObject obj)
        {
            _pool.Release(obj);
        }

        /// <summary>
        /// プールをクリア
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// プールを事前に指定数まで拡張
        /// </summary>
        public void Prewarm(int count)
        {
            _pool.Prewarm(count);
        }
    }

    /// <summary>
    /// コンポーネントプール
    /// 特定のコンポーネントを持つGameObjectのプーリング
    /// </summary>
    /// <typeparam name="T">プールするコンポーネントの型</typeparam>
    public class ComponentPool<T> where T : Component
    {
        private readonly GameObjectPool _gameObjectPool;

        /// <summary>
        /// プール内のアクティブでないオブジェクト数
        /// </summary>
        public int CountInactive => _gameObjectPool.CountInactive;

        /// <summary>
        /// 作成された全オブジェクト数
        /// </summary>
        public int CountAll => _gameObjectPool.CountAll;

        /// <summary>
        /// 使用中のオブジェクト数
        /// </summary>
        public int CountActive => _gameObjectPool.CountActive;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="prefab">プールするプレハブ</param>
        /// <param name="parent">プールオブジェクトの親Transform</param>
        /// <param name="defaultCapacity">初期容量</param>
        /// <param name="maxSize">最大サイズ</param>
        public ComponentPool(
            T prefab,
            Transform parent = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            _gameObjectPool = new GameObjectPool(
                prefab.gameObject,
                parent,
                defaultCapacity,
                maxSize
            );
        }

        /// <summary>
        /// プールからコンポーネントを取得
        /// </summary>
        public T Get()
        {
            return _gameObjectPool.Get().GetComponent<T>();
        }

        /// <summary>
        /// プールからコンポーネントを取得し、位置と回転を設定
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            return _gameObjectPool.Get(position, rotation).GetComponent<T>();
        }

        /// <summary>
        /// プールからコンポーネントを取得し、親を設定
        /// </summary>
        public T Get(Transform parent)
        {
            return _gameObjectPool.Get(parent).GetComponent<T>();
        }

        /// <summary>
        /// コンポーネントをプールに返却
        /// </summary>
        public void Release(T component)
        {
            if (component != null)
            {
                _gameObjectPool.Release(component.gameObject);
            }
        }

        /// <summary>
        /// プールをクリア
        /// </summary>
        public void Clear()
        {
            _gameObjectPool.Clear();
        }

        /// <summary>
        /// プールを事前に指定数まで拡張
        /// </summary>
        public void Prewarm(int count)
        {
            _gameObjectPool.Prewarm(count);
        }
    }
}
