using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// トースト通知マネージャー
    /// 画面上のポップアップ通知を管理
    /// </summary>
    public class ToastManager : MonoBehaviour
    {
        private static ToastManager _instance;
        public static ToastManager Instance => _instance;

        [Header("設定")]
        [SerializeField] private RectTransform _toastContainer;
        [SerializeField] private GameObject _toastPrefab;
        [SerializeField] private int _maxToasts = 5;
        [SerializeField] private float _defaultDuration = 3f;
        [SerializeField] private float _animationDuration = 0.3f;

        [Header("スタイル")]
        [SerializeField] private Color _infoColor = new Color(0.2f, 0.5f, 0.8f, 0.9f);
        [SerializeField] private Color _successColor = new Color(0.2f, 0.7f, 0.3f, 0.9f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.7f, 0.2f, 0.9f);
        [SerializeField] private Color _errorColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);

        private Queue<ToastData> _pendingToasts = new Queue<ToastData>();
        private List<GameObject> _activeToasts = new List<GameObject>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 情報トーストを表示
        /// </summary>
        public void ShowInfo(string message, float duration = -1)
        {
            Show(message, ToastType.Info, duration);
        }

        /// <summary>
        /// 成功トーストを表示
        /// </summary>
        public void ShowSuccess(string message, float duration = -1)
        {
            Show(message, ToastType.Success, duration);
        }

        /// <summary>
        /// 警告トーストを表示
        /// </summary>
        public void ShowWarning(string message, float duration = -1)
        {
            Show(message, ToastType.Warning, duration);
        }

        /// <summary>
        /// エラートーストを表示
        /// </summary>
        public void ShowError(string message, float duration = -1)
        {
            Show(message, ToastType.Error, duration);
        }

        /// <summary>
        /// トーストを表示
        /// </summary>
        public void Show(string message, ToastType type = ToastType.Info, float duration = -1)
        {
            if (duration < 0)
            {
                duration = _defaultDuration;
            }

            var toastData = new ToastData
            {
                Message = message,
                Type = type,
                Duration = duration
            };

            if (_activeToasts.Count >= _maxToasts)
            {
                _pendingToasts.Enqueue(toastData);
            }
            else
            {
                CreateToast(toastData);
            }
        }

        private void CreateToast(ToastData data)
        {
            if (_toastPrefab == null || _toastContainer == null)
            {
                Debug.LogWarning("Toast prefab or container not set");
                return;
            }

            var toastObj = Instantiate(_toastPrefab, _toastContainer);
            _activeToasts.Add(toastObj);

            // テキストを設定
            var textComponent = toastObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = data.Message;
            }

            // 背景色を設定
            var image = toastObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetColorForType(data.Type);
            }

            // アニメーション開始
            StartCoroutine(AnimateToast(toastObj, data.Duration));
        }

        private IEnumerator AnimateToast(GameObject toastObj, float duration)
        {
            var rectTransform = toastObj.GetComponent<RectTransform>();
            var canvasGroup = toastObj.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = toastObj.AddComponent<CanvasGroup>();
            }

            // 初期状態
            canvasGroup.alpha = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 targetPos = startPos + new Vector2(0, 20);

            // フェードイン
            float elapsed = 0f;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _animationDuration;
                canvasGroup.alpha = t;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos - new Vector2(0, 20), startPos, t);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = startPos;

            // 表示時間待機
            yield return new WaitForSecondsRealtime(duration);

            // フェードアウト
            elapsed = 0f;
            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _animationDuration;
                canvasGroup.alpha = 1f - t;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            // トーストを削除
            _activeToasts.Remove(toastObj);
            Destroy(toastObj);

            // 待機中のトーストがあれば表示
            if (_pendingToasts.Count > 0)
            {
                CreateToast(_pendingToasts.Dequeue());
            }
        }

        private Color GetColorForType(ToastType type)
        {
            switch (type)
            {
                case ToastType.Info:
                    return _infoColor;
                case ToastType.Success:
                    return _successColor;
                case ToastType.Warning:
                    return _warningColor;
                case ToastType.Error:
                    return _errorColor;
                default:
                    return _infoColor;
            }
        }

        /// <summary>
        /// すべてのトーストをクリア
        /// </summary>
        public void ClearAll()
        {
            foreach (var toast in _activeToasts)
            {
                if (toast != null)
                {
                    Destroy(toast);
                }
            }

            _activeToasts.Clear();
            _pendingToasts.Clear();
        }

        private struct ToastData
        {
            public string Message;
            public ToastType Type;
            public float Duration;
        }
    }

    /// <summary>
    /// トーストタイプ
    /// </summary>
    public enum ToastType
    {
        Info,       // 情報
        Success,    // 成功
        Warning,    // 警告
        Error       // エラー
    }
}
