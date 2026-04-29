using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ThirtySixStratagems.UI.Notification
{
    /// <summary>
    /// 通知アイテム
    /// 個別の通知表示を管理
    /// </summary>
    public class NotificationItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI要素")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _progressBar;

        [Header("アニメーション")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _slideInDuration = 0.3f;
        [SerializeField] private float _slideDistance = 100f;

        // 状態
        private NotificationData _data;
        private bool _isDismissing = false;
        private bool _isPaused = false;
        private float _remainingTime;
        private float _totalDuration;

        // イベント
        public event Action OnDismissed;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_closeButton != null)
                _closeButton.onClick.AddListener(() => Dismiss(0.2f));
        }

        private void Update()
        {
            // プログレスバー更新
            if (_progressBar != null && !_isPaused && !_isDismissing && _totalDuration > 0)
            {
                _remainingTime -= Time.deltaTime;
                _progressBar.fillAmount = Mathf.Clamp01(_remainingTime / _totalDuration);
            }
        }

        #region Setup

        /// <summary>
        /// 通知をセットアップ
        /// </summary>
        public void Setup(NotificationData data, Sprite icon, Color color)
        {
            _data = data;
            _totalDuration = data.Duration;
            _remainingTime = data.Duration;

            // テキスト
            if (_titleText != null)
            {
                if (!string.IsNullOrEmpty(data.Title))
                {
                    _titleText.text = data.Title;
                    _titleText.gameObject.SetActive(true);
                }
                else
                {
                    _titleText.gameObject.SetActive(false);
                }
            }

            if (_messageText != null)
                _messageText.text = data.Message;

            // アイコン
            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
                _iconImage.color = color;
            }

            // 背景色
            if (_backgroundImage != null)
            {
                Color bgColor = color;
                bgColor.a = 0.9f;
                _backgroundImage.color = bgColor;
            }

            // プログレスバー
            if (_progressBar != null)
            {
                _progressBar.fillAmount = 1f;
                _progressBar.color = Color.white;
            }

            // スライドインアニメーション
            StartCoroutine(SlideInAnimation());
        }

        /// <summary>
        /// スライドインアニメーション
        /// </summary>
        private IEnumerator SlideInAnimation()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) yield break;

            Vector2 startPos = rectTransform.anchoredPosition;
            startPos.x += _slideDistance;
            Vector2 endPos = rectTransform.anchoredPosition;

            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < _slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _slideInDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, smoothT);

                if (_canvasGroup != null)
                    _canvasGroup.alpha = smoothT;

                yield return null;
            }

            rectTransform.anchoredPosition = endPos;
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
        }

        #endregion

        #region Dismiss

        /// <summary>
        /// 通知を閉じる
        /// </summary>
        public void Dismiss(float fadeDuration = 0.5f)
        {
            if (_isDismissing) return;
            _isDismissing = true;

            StartCoroutine(DismissCoroutine(fadeDuration));
        }

        /// <summary>
        /// 閉じるコルーチン
        /// </summary>
        private IEnumerator DismissCoroutine(float duration)
        {
            if (_canvasGroup == null)
            {
                OnDismissed?.Invoke();
                yield break;
            }

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            OnDismissed?.Invoke();
        }

        #endregion

        #region Pointer Events

        public void OnPointerClick(PointerEventData eventData)
        {
            _data?.OnClick?.Invoke();
            Dismiss(0.2f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // ホバー時は自動閉じを一時停止
            _isPaused = true;

            // 視覚的フィードバック
            if (_backgroundImage != null)
            {
                Color c = _backgroundImage.color;
                c.a = 1f;
                _backgroundImage.color = c;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPaused = false;

            if (_backgroundImage != null)
            {
                Color c = _backgroundImage.color;
                c.a = 0.9f;
                _backgroundImage.color = c;
            }
        }

        #endregion
    }
}
