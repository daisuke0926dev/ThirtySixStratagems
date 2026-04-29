using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// UI遷移管理システム
    /// パネル間のトランジションとアニメーションを管理
    /// </summary>
    public class UITransitionManager : MonoBehaviour
    {
        public static UITransitionManager Instance { get; private set; }

        [Header("トランジション設定")]
        [SerializeField] private float _defaultTransitionDuration = 0.3f;
        [SerializeField] private TransitionType _defaultTransitionType = TransitionType.Fade;

        [Header("オーバーレイ")]
        [SerializeField] private CanvasGroup _transitionOverlay;
        [SerializeField] private Image _transitionImage;

        // アクティブなトランジション
        private Coroutine _activeTransition;
        private Stack<GameObject> _panelStack = new Stack<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            if (_transitionOverlay != null)
            {
                _transitionOverlay.alpha = 0;
                _transitionOverlay.gameObject.SetActive(false);
            }
        }

        #region Panel Management

        /// <summary>
        /// パネルを表示
        /// </summary>
        public void ShowPanel(GameObject panel, TransitionType transition = TransitionType.None, float duration = -1f, Action onComplete = null)
        {
            if (panel == null) return;
            if (duration < 0) duration = _defaultTransitionDuration;

            switch (transition)
            {
                case TransitionType.None:
                    panel.SetActive(true);
                    onComplete?.Invoke();
                    break;

                case TransitionType.Fade:
                    ShowWithFade(panel, duration, onComplete);
                    break;

                case TransitionType.Scale:
                    ShowWithScale(panel, duration, onComplete);
                    break;

                case TransitionType.SlideLeft:
                    ShowWithSlide(panel, SlideDirection.Left, duration, onComplete);
                    break;

                case TransitionType.SlideRight:
                    ShowWithSlide(panel, SlideDirection.Right, duration, onComplete);
                    break;

                case TransitionType.SlideUp:
                    ShowWithSlide(panel, SlideDirection.Up, duration, onComplete);
                    break;

                case TransitionType.SlideDown:
                    ShowWithSlide(panel, SlideDirection.Down, duration, onComplete);
                    break;
            }

            _panelStack.Push(panel);
        }

        /// <summary>
        /// パネルを非表示
        /// </summary>
        public void HidePanel(GameObject panel, TransitionType transition = TransitionType.None, float duration = -1f, Action onComplete = null)
        {
            if (panel == null) return;
            if (duration < 0) duration = _defaultTransitionDuration;

            switch (transition)
            {
                case TransitionType.None:
                    panel.SetActive(false);
                    onComplete?.Invoke();
                    break;

                case TransitionType.Fade:
                    HideWithFade(panel, duration, onComplete);
                    break;

                case TransitionType.Scale:
                    HideWithScale(panel, duration, onComplete);
                    break;

                case TransitionType.SlideLeft:
                    HideWithSlide(panel, SlideDirection.Left, duration, onComplete);
                    break;

                case TransitionType.SlideRight:
                    HideWithSlide(panel, SlideDirection.Right, duration, onComplete);
                    break;

                case TransitionType.SlideUp:
                    HideWithSlide(panel, SlideDirection.Up, duration, onComplete);
                    break;

                case TransitionType.SlideDown:
                    HideWithSlide(panel, SlideDirection.Down, duration, onComplete);
                    break;
            }
        }

        /// <summary>
        /// パネルを切り替え
        /// </summary>
        public void SwitchPanel(GameObject fromPanel, GameObject toPanel, TransitionType transition = TransitionType.Fade, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = _defaultTransitionDuration;

            HidePanel(fromPanel, transition, duration / 2f, () =>
            {
                ShowPanel(toPanel, transition, duration / 2f, onComplete);
            });
        }

        /// <summary>
        /// 前のパネルに戻る
        /// </summary>
        public void GoBack(TransitionType transition = TransitionType.None)
        {
            if (_panelStack.Count > 1)
            {
                var currentPanel = _panelStack.Pop();
                var previousPanel = _panelStack.Peek();

                HidePanel(currentPanel, transition);
                ShowPanel(previousPanel, transition);
            }
        }

        #endregion

        #region Fade Transitions

        /// <summary>
        /// フェードで表示
        /// </summary>
        private void ShowWithFade(GameObject panel, float duration, Action onComplete)
        {
            var canvasGroup = GetOrAddCanvasGroup(panel);
            canvasGroup.alpha = 0;
            panel.SetActive(true);

            StartCoroutine(FadeCoroutine(canvasGroup, 0, 1, duration, onComplete));
        }

        /// <summary>
        /// フェードで非表示
        /// </summary>
        private void HideWithFade(GameObject panel, float duration, Action onComplete)
        {
            var canvasGroup = GetOrAddCanvasGroup(panel);

            StartCoroutine(FadeCoroutine(canvasGroup, 1, 0, duration, () =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            }));
        }

        /// <summary>
        /// フェードコルーチン
        /// </summary>
        private IEnumerator FadeCoroutine(CanvasGroup target, float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                target.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
                yield return null;
            }

            target.alpha = toAlpha;
            onComplete?.Invoke();
        }

        #endregion

        #region Scale Transitions

        /// <summary>
        /// スケールで表示
        /// </summary>
        private void ShowWithScale(GameObject panel, float duration, Action onComplete)
        {
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);

            StartCoroutine(ScaleCoroutine(panel.transform, Vector3.zero, Vector3.one, duration, onComplete));
        }

        /// <summary>
        /// スケールで非表示
        /// </summary>
        private void HideWithScale(GameObject panel, float duration, Action onComplete)
        {
            StartCoroutine(ScaleCoroutine(panel.transform, Vector3.one, Vector3.zero, duration, () =>
            {
                panel.SetActive(false);
                panel.transform.localScale = Vector3.one;
                onComplete?.Invoke();
            }));
        }

        /// <summary>
        /// スケールコルーチン
        /// </summary>
        private IEnumerator ScaleCoroutine(Transform target, Vector3 fromScale, Vector3 toScale, float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutBack(elapsed / duration);
                target.localScale = Vector3.Lerp(fromScale, toScale, t);
                yield return null;
            }

            target.localScale = toScale;
            onComplete?.Invoke();
        }

        #endregion

        #region Slide Transitions

        /// <summary>
        /// スライドで表示
        /// </summary>
        private void ShowWithSlide(GameObject panel, SlideDirection direction, float duration, Action onComplete)
        {
            var rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                panel.SetActive(true);
                onComplete?.Invoke();
                return;
            }

            Vector2 targetPosition = rectTransform.anchoredPosition;
            Vector2 startPosition = GetOffscreenPosition(rectTransform, direction);

            rectTransform.anchoredPosition = startPosition;
            panel.SetActive(true);

            StartCoroutine(SlideCoroutine(rectTransform, startPosition, targetPosition, duration, onComplete));
        }

        /// <summary>
        /// スライドで非表示
        /// </summary>
        private void HideWithSlide(GameObject panel, SlideDirection direction, float duration, Action onComplete)
        {
            var rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                panel.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 targetPosition = GetOffscreenPosition(rectTransform, direction);

            StartCoroutine(SlideCoroutine(rectTransform, startPosition, targetPosition, duration, () =>
            {
                panel.SetActive(false);
                rectTransform.anchoredPosition = startPosition;
                onComplete?.Invoke();
            }));
        }

        /// <summary>
        /// スライドコルーチン
        /// </summary>
        private IEnumerator SlideCoroutine(RectTransform target, Vector2 fromPosition, Vector2 toPosition, float duration, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutQuad(elapsed / duration);
                target.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, t);
                yield return null;
            }

            target.anchoredPosition = toPosition;
            onComplete?.Invoke();
        }

        /// <summary>
        /// オフスクリーン位置を取得
        /// </summary>
        private Vector2 GetOffscreenPosition(RectTransform rectTransform, SlideDirection direction)
        {
            Vector2 position = rectTransform.anchoredPosition;
            Vector2 size = rectTransform.rect.size;

            return direction switch
            {
                SlideDirection.Left => position - new Vector2(size.x + 100, 0),
                SlideDirection.Right => position + new Vector2(size.x + 100, 0),
                SlideDirection.Up => position + new Vector2(0, size.y + 100),
                SlideDirection.Down => position - new Vector2(0, size.y + 100),
                _ => position
            };
        }

        #endregion

        #region Screen Transition

        /// <summary>
        /// シーン遷移トランジション
        /// </summary>
        public void DoScreenTransition(Action onMidTransition, float duration = -1f, Action onComplete = null)
        {
            if (duration < 0) duration = _defaultTransitionDuration;

            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
            }

            _activeTransition = StartCoroutine(ScreenTransitionCoroutine(onMidTransition, duration, onComplete));
        }

        /// <summary>
        /// シーン遷移コルーチン
        /// </summary>
        private IEnumerator ScreenTransitionCoroutine(Action onMidTransition, float duration, Action onComplete)
        {
            if (_transitionOverlay == null) yield break;

            _transitionOverlay.gameObject.SetActive(true);

            // フェードアウト
            float elapsed = 0f;
            float halfDuration = duration / 2f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _transitionOverlay.alpha = Mathf.Lerp(0, 1, elapsed / halfDuration);
                yield return null;
            }

            _transitionOverlay.alpha = 1;

            // コールバック実行
            onMidTransition?.Invoke();
            yield return null;

            // フェードイン
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _transitionOverlay.alpha = Mathf.Lerp(1, 0, elapsed / halfDuration);
                yield return null;
            }

            _transitionOverlay.alpha = 0;
            _transitionOverlay.gameObject.SetActive(false);

            onComplete?.Invoke();
        }

        #endregion

        #region Utility

        /// <summary>
        /// CanvasGroupを取得または追加
        /// </summary>
        private CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }

        /// <summary>
        /// EaseOutBack
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        /// <summary>
        /// EaseOutQuad
        /// </summary>
        private float EaseOutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        #endregion
    }

    /// <summary>
    /// トランジションタイプ
    /// </summary>
    public enum TransitionType
    {
        None,
        Fade,
        Scale,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }

    /// <summary>
    /// スライド方向
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
