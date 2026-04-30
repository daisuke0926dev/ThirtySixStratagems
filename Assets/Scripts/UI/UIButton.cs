using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// 拡張UIボタン
    /// ホバーエフェクトとアニメーションを追加
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("ボタン設定")]
        [SerializeField] private bool _interactable = true;
        [SerializeField] private ButtonStyle _style = ButtonStyle.Primary;

        [Header("カラー設定")]
        [SerializeField] private Color _normalColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private Color _hoverColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        [SerializeField] private Color _pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("アニメーション")]
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressedScale = 0.95f;
        [SerializeField] private float _animationDuration = 0.1f;

        [Header("イベント")]
        public event Action OnClick;

        private Image _image;
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Coroutine _scaleCoroutine;
        private bool _isHovered;
        private bool _isPressed;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                UpdateVisual();
            }
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;

            ApplyStylePreset();
            UpdateVisual();
        }

        private void ApplyStylePreset()
        {
            switch (_style)
            {
                case ButtonStyle.Primary:
                    _normalColor = new Color(0.2f, 0.4f, 0.8f, 1f);
                    _hoverColor = new Color(0.3f, 0.5f, 0.9f, 1f);
                    _pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
                    break;

                case ButtonStyle.Secondary:
                    _normalColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                    _hoverColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                    _pressedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                    break;

                case ButtonStyle.Success:
                    _normalColor = new Color(0.2f, 0.6f, 0.3f, 1f);
                    _hoverColor = new Color(0.3f, 0.7f, 0.4f, 1f);
                    _pressedColor = new Color(0.15f, 0.5f, 0.25f, 1f);
                    break;

                case ButtonStyle.Danger:
                    _normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                    _hoverColor = new Color(0.9f, 0.3f, 0.3f, 1f);
                    _pressedColor = new Color(0.6f, 0.15f, 0.15f, 1f);
                    break;

                case ButtonStyle.Warning:
                    _normalColor = new Color(0.9f, 0.7f, 0.2f, 1f);
                    _hoverColor = new Color(1f, 0.8f, 0.3f, 1f);
                    _pressedColor = new Color(0.7f, 0.55f, 0.15f, 1f);
                    break;

                case ButtonStyle.Ghost:
                    _normalColor = new Color(1f, 1f, 1f, 0f);
                    _hoverColor = new Color(1f, 1f, 1f, 0.1f);
                    _pressedColor = new Color(1f, 1f, 1f, 0.2f);
                    break;
            }
        }

        private void UpdateVisual()
        {
            if (_image == null) return;

            if (!_interactable)
            {
                _image.color = _disabledColor;
                return;
            }

            if (_isPressed)
            {
                _image.color = _pressedColor;
            }
            else if (_isHovered)
            {
                _image.color = _hoverColor;
            }
            else
            {
                _image.color = _normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable) return;

            _isHovered = true;
            UpdateVisual();
            AnimateScale(_hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
            UpdateVisual();
            AnimateScale(1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable) return;

            _isPressed = true;
            UpdateVisual();
            AnimateScale(_pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactable) return;

            bool wasPressed = _isPressed;
            _isPressed = false;
            UpdateVisual();

            if (_isHovered)
            {
                AnimateScale(_hoverScale);

                if (wasPressed)
                {
                    OnClick?.Invoke();
                }
            }
            else
            {
                AnimateScale(1f);
            }
        }

        private void AnimateScale(float targetScale)
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }

            _scaleCoroutine = StartCoroutine(AnimateScaleCoroutine(targetScale));
        }

        private IEnumerator AnimateScaleCoroutine(float targetScale)
        {
            Vector3 startScale = _rectTransform.localScale;
            Vector3 endScale = _originalScale * targetScale;
            float elapsed = 0f;

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _animationDuration;
                _rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            _rectTransform.localScale = endScale;
        }

        /// <summary>
        /// ボタンスタイルを設定
        /// </summary>
        public void SetStyle(ButtonStyle style)
        {
            _style = style;
            ApplyStylePreset();
            UpdateVisual();
        }
    }

    /// <summary>
    /// ボタンスタイル
    /// </summary>
    public enum ButtonStyle
    {
        Primary,    // プライマリ（青）
        Secondary,  // セカンダリ（グレー）
        Success,    // 成功（緑）
        Danger,     // 危険（赤）
        Warning,    // 警告（黄）
        Ghost       // ゴースト（透明）
    }
}
