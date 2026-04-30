using System;
using System.Collections;
using UnityEngine;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// UIアニメーター
    /// UIパネルのアニメーションを管理
    /// </summary>
    public class UIAnimator : MonoBehaviour
    {
        [Header("アニメーション設定")]
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("アニメーションタイプ")]
        [SerializeField] private UIAnimationType _animationType = UIAnimationType.Scale;
        [SerializeField] private Vector2 _slideDirection = Vector2.up;

        // 内部状態
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _originalScale;
        private Vector2 _originalPosition;
        private Coroutine _currentAnimation;

        public event Action OnShowComplete;
        public event Action OnHideComplete;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
                _originalPosition = _rectTransform.anchoredPosition;
            }
        }

        /// <summary>
        /// 表示アニメーション
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(AnimateShow());
        }

        /// <summary>
        /// 非表示アニメーション
        /// </summary>
        public void Hide()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(AnimateHide());
        }

        /// <summary>
        /// 即座に表示
        /// </summary>
        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            SetAnimationState(1f);
        }

        /// <summary>
        /// 即座に非表示
        /// </summary>
        public void HideImmediate()
        {
            SetAnimationState(0f);
            gameObject.SetActive(false);
        }

        private IEnumerator AnimateShow()
        {
            float elapsed = 0f;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _showCurve.Evaluate(elapsed / _animationDuration);
                SetAnimationState(t);
                yield return null;
            }

            SetAnimationState(1f);
            OnShowComplete?.Invoke();
        }

        private IEnumerator AnimateHide()
        {
            float elapsed = 0f;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _hideCurve.Evaluate(elapsed / _animationDuration);
                SetAnimationState(t);
                yield return null;
            }

            SetAnimationState(0f);
            gameObject.SetActive(false);
            OnHideComplete?.Invoke();
        }

        private void SetAnimationState(float t)
        {
            if (_rectTransform == null) return;

            switch (_animationType)
            {
                case UIAnimationType.Scale:
                    _rectTransform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, t);
                    break;

                case UIAnimationType.Fade:
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = t;
                    }
                    break;

                case UIAnimationType.Slide:
                    Vector2 offset = _slideDirection * 500f * (1f - t);
                    _rectTransform.anchoredPosition = _originalPosition + offset;
                    break;

                case UIAnimationType.ScaleAndFade:
                    _rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.8f, _originalScale, t);
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = t;
                    }
                    break;

                case UIAnimationType.SlideAndFade:
                    Vector2 slideOffset = _slideDirection * 100f * (1f - t);
                    _rectTransform.anchoredPosition = _originalPosition + slideOffset;
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = t;
                    }
                    break;
            }
        }

        /// <summary>
        /// パルスアニメーション
        /// </summary>
        public void Pulse(float scale = 1.1f, float duration = 0.2f)
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(AnimatePulse(scale, duration));
        }

        private IEnumerator AnimatePulse(float scale, float duration)
        {
            float elapsed = 0f;
            float halfDuration = duration / 2f;

            // 拡大
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                _rectTransform.localScale = Vector3.Lerp(_originalScale, _originalScale * scale, t);
                yield return null;
            }

            elapsed = 0f;

            // 縮小
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / halfDuration;
                _rectTransform.localScale = Vector3.Lerp(_originalScale * scale, _originalScale, t);
                yield return null;
            }

            _rectTransform.localScale = _originalScale;
        }

        /// <summary>
        /// シェイクアニメーション
        /// </summary>
        public void Shake(float intensity = 10f, float duration = 0.3f)
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(AnimateShake(intensity, duration));
        }

        private IEnumerator AnimateShake(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float currentIntensity = intensity * (1f - elapsed / duration);

                Vector2 shake = new Vector2(
                    UnityEngine.Random.Range(-currentIntensity, currentIntensity),
                    UnityEngine.Random.Range(-currentIntensity, currentIntensity)
                );

                _rectTransform.anchoredPosition = _originalPosition + shake;
                yield return null;
            }

            _rectTransform.anchoredPosition = _originalPosition;
        }
    }

    /// <summary>
    /// UIアニメーションタイプ
    /// </summary>
    public enum UIAnimationType
    {
        Scale,          // スケール
        Fade,           // フェード
        Slide,          // スライド
        ScaleAndFade,   // スケール＋フェード
        SlideAndFade    // スライド＋フェード
    }
}
