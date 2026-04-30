using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// ツールチップマネージャー
    /// ホバー時のツールチップ表示を管理
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        private static TooltipManager _instance;
        public static TooltipManager Instance => _instance;

        [Header("ツールチップ設定")]
        [SerializeField] private RectTransform _tooltipPanel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _backgroundImage;

        [Header("表示設定")]
        [SerializeField] private float _showDelay = 0.5f;
        [SerializeField] private float _fadeInDuration = 0.15f;
        [SerializeField] private Vector2 _offset = new Vector2(10, -10);
        [SerializeField] private float _padding = 20f;

        [Header("スタイル")]
        [SerializeField] private Color _backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        [SerializeField] private Color _titleColor = new Color(1f, 0.9f, 0.7f);
        [SerializeField] private Color _descriptionColor = new Color(0.9f, 0.9f, 0.9f);

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Coroutine _showCoroutine;
        private Coroutine _fadeCoroutine;
        private bool _isShowing;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _canvas = GetComponentInParent<Canvas>();

            if (_tooltipPanel != null)
            {
                _canvasGroup = _tooltipPanel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _tooltipPanel.gameObject.AddComponent<CanvasGroup>();
                }

                _tooltipPanel.gameObject.SetActive(false);
            }

            ApplyStyle();
        }

        private void ApplyStyle()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _backgroundColor;
            }

            if (_titleText != null)
            {
                _titleText.color = _titleColor;
            }

            if (_descriptionText != null)
            {
                _descriptionText.color = _descriptionColor;
            }
        }

        /// <summary>
        /// ツールチップを表示
        /// </summary>
        public void Show(string title, string description = null)
        {
            if (_tooltipPanel == null) return;

            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }

            _showCoroutine = StartCoroutine(ShowWithDelay(title, description));
        }

        /// <summary>
        /// ツールチップを即座に表示
        /// </summary>
        public void ShowImmediate(string title, string description = null)
        {
            if (_tooltipPanel == null) return;

            SetContent(title, description);
            UpdatePosition();
            _tooltipPanel.gameObject.SetActive(true);

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeIn());
            _isShowing = true;
        }

        private IEnumerator ShowWithDelay(string title, string description)
        {
            yield return new WaitForSecondsRealtime(_showDelay);

            ShowImmediate(title, description);
        }

        /// <summary>
        /// ツールチップを非表示
        /// </summary>
        public void Hide()
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (_tooltipPanel != null)
            {
                _tooltipPanel.gameObject.SetActive(false);
            }

            _isShowing = false;
        }

        private void SetContent(string title, string description)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
                _titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = description ?? "";
                _descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }

            // コンテンツに合わせてサイズを調整
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipPanel);
        }

        private void UpdatePosition()
        {
            if (_tooltipPanel == null || _canvas == null) return;

            Vector2 mousePos = Input.mousePosition;
            Vector2 tooltipSize = _tooltipPanel.sizeDelta;

            // キャンバス座標に変換
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                mousePos,
                _canvas.worldCamera,
                out Vector2 localPoint
            );

            // オフセット適用
            localPoint += _offset;

            // 画面外にはみ出さないように調整
            RectTransform canvasRect = _canvas.transform as RectTransform;
            Vector2 canvasSize = canvasRect.sizeDelta;

            // 右端チェック
            if (localPoint.x + tooltipSize.x / 2 > canvasSize.x / 2 - _padding)
            {
                localPoint.x = canvasSize.x / 2 - tooltipSize.x / 2 - _padding;
            }

            // 左端チェック
            if (localPoint.x - tooltipSize.x / 2 < -canvasSize.x / 2 + _padding)
            {
                localPoint.x = -canvasSize.x / 2 + tooltipSize.x / 2 + _padding;
            }

            // 上端チェック
            if (localPoint.y + tooltipSize.y / 2 > canvasSize.y / 2 - _padding)
            {
                localPoint.y = canvasSize.y / 2 - tooltipSize.y / 2 - _padding;
            }

            // 下端チェック
            if (localPoint.y - tooltipSize.y / 2 < -canvasSize.y / 2 + _padding)
            {
                localPoint.y = -canvasSize.y / 2 + tooltipSize.y / 2 + _padding;
            }

            _tooltipPanel.anchoredPosition = localPoint;
        }

        private void Update()
        {
            if (_isShowing)
            {
                UpdatePosition();
            }
        }

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;

            _canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = elapsed / _fadeInDuration;
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }
    }
}
