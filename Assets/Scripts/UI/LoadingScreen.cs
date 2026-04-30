using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// ローディング画面
    /// シーン読み込み時のローディング表示を管理
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        private static LoadingScreen _instance;
        public static LoadingScreen Instance => _instance;

        [Header("UI要素")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _progressBar;
        [SerializeField] private Image _progressBarFill;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _tipText;
        [SerializeField] private RectTransform _spinnerIcon;

        [Header("設定")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private float _minDisplayTime = 1f;
        [SerializeField] private float _spinnerSpeed = 180f;

        [Header("ヒント")]
        [SerializeField] private string[] _tips = new string[]
        {
            "「三十六計」は中国の兵法書で、36の計略が記されています。",
            "計略の成功率は知力と相手との相性に影響されます。",
            "同盟は強力ですが、裏切られる可能性もあります。",
            "兵力だけでなく、士気も戦闘に重要な要素です。",
            "地形によって防御ボーナスが変わります。",
            "将軍の能力は軍隊の戦闘力に大きく影響します。"
        };

        private float _currentProgress;
        private float _displayStartTime;
        private bool _isShowing;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            HideImmediate();
        }

        private void Update()
        {
            // スピナーアニメーション
            if (_isShowing && _spinnerIcon != null)
            {
                _spinnerIcon.Rotate(0, 0, -_spinnerSpeed * Time.unscaledDeltaTime);
            }
        }

        /// <summary>
        /// ローディング画面を表示
        /// </summary>
        public void Show(string message = null)
        {
            _isShowing = true;
            _displayStartTime = Time.unscaledTime;
            _currentProgress = 0f;

            if (_loadingText != null)
            {
                _loadingText.text = message ?? "読み込み中...";
            }

            if (_tipText != null && _tips.Length > 0)
            {
                _tipText.text = _tips[Random.Range(0, _tips.Length)];
            }

            UpdateProgressBar(0f);

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeIn());
        }

        /// <summary>
        /// ローディング画面を非表示
        /// </summary>
        public void Hide()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(HideWithDelay());
        }

        private IEnumerator HideWithDelay()
        {
            // 最小表示時間を待機
            float elapsed = Time.unscaledTime - _displayStartTime;
            if (elapsed < _minDisplayTime)
            {
                yield return new WaitForSecondsRealtime(_minDisplayTime - elapsed);
            }

            yield return StartCoroutine(FadeOut());

            _isShowing = false;
        }

        /// <summary>
        /// 即座に非表示
        /// </summary>
        public void HideImmediate()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
            _isShowing = false;
        }

        /// <summary>
        /// 進捗を更新
        /// </summary>
        public void SetProgress(float progress, string message = null)
        {
            _currentProgress = Mathf.Clamp01(progress);
            UpdateProgressBar(_currentProgress);

            if (message != null && _loadingText != null)
            {
                _loadingText.text = message;
            }
        }

        private void UpdateProgressBar(float progress)
        {
            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = progress;
            }
        }

        private IEnumerator FadeIn()
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;

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

        private IEnumerator FadeOut()
        {
            if (_canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < _fadeOutDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _canvasGroup.alpha = 1f - (elapsed / _fadeOutDuration);
                    yield return null;
                }

                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// メッセージを更新
        /// </summary>
        public void SetMessage(string message)
        {
            if (_loadingText != null)
            {
                _loadingText.text = message;
            }
        }
    }
}
