using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// スプラッシュスクリーン
    /// ゲーム起動時のロゴ表示とローディング
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [Header("ロゴ設定")]
        [SerializeField] private Image _logoImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _logoCanvasGroup;
        [SerializeField] private float _logoDisplayTime = 2f;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        [Header("追加ロゴ")]
        [SerializeField] private Image _secondaryLogoImage;
        [SerializeField] private CanvasGroup _secondaryLogoCanvasGroup;
        [SerializeField] private float _secondaryLogoDisplayTime = 1.5f;

        [Header("ローディング")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _loadingBar;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _loadingTipText;
        [SerializeField] private TextMeshProUGUI _versionText;

        [Header("バージョン情報")]
        [SerializeField] private bool _showVersion = true;

        [Header("ヒント")]
        [SerializeField] private string[] _loadingTips = new string[]
        {
            "ヒント: 計略は知力の高い武将に使わせると成功率が上がります",
            "ヒント: 地形を活かした戦いが勝利への近道です",
            "ヒント: 兵糧の管理を怠ると軍の士気が下がります",
            "ヒント: 敵の弱点を見極め、適切な計略を選びましょう",
            "ヒント: 同盟を結ぶことで戦力を補うことができます",
            "ヒント: 三十六計を制する者が天下を制す"
        };

        /// <summary>
        /// スプラッシュ完了イベント
        /// </summary>
        public event Action OnSplashComplete;

        /// <summary>
        /// ロード進捗イベント
        /// </summary>
        public event Action<float> OnLoadProgress;

        private bool _isSkippable = false;
        private bool _isSkipped = false;

        private void Start()
        {
            Initialize();
            StartCoroutine(SplashSequence());
        }

        private void Update()
        {
            // スキップ入力チェック
            if (_isSkippable && !_isSkipped)
            {
                if (Input.anyKeyDown || Input.touchCount > 0)
                {
                    _isSkipped = true;
                }
            }
        }

        private void Initialize()
        {
            // 初期状態を設定
            if (_logoCanvasGroup != null)
            {
                _logoCanvasGroup.alpha = 0;
            }

            if (_secondaryLogoCanvasGroup != null)
            {
                _secondaryLogoCanvasGroup.alpha = 0;
            }

            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }

            if (_loadingBar != null)
            {
                _loadingBar.value = 0;
            }

            // バージョン表示
            if (_versionText != null && _showVersion)
            {
                if (Systems.VersionManager.Instance != null)
                {
                    _versionText.text = $"Version {Systems.VersionManager.Instance.DisplayVersion}";
                }
                else
                {
                    _versionText.text = $"Version {Application.version}";
                }
            }
        }

        private IEnumerator SplashSequence()
        {
            // メインロゴ表示
            if (_logoCanvasGroup != null)
            {
                yield return StartCoroutine(ShowLogo(_logoCanvasGroup, _logoDisplayTime));
            }

            // セカンダリロゴ表示
            if (_secondaryLogoCanvasGroup != null && _secondaryLogoImage != null)
            {
                yield return StartCoroutine(ShowLogo(_secondaryLogoCanvasGroup, _secondaryLogoDisplayTime));
            }

            // ローディング表示
            yield return StartCoroutine(ShowLoading());

            // 完了
            OnSplashComplete?.Invoke();
        }

        private IEnumerator ShowLogo(CanvasGroup canvasGroup, float displayTime)
        {
            _isSkippable = true;

            // フェードイン
            float elapsed = 0;
            while (elapsed < _fadeInDuration && !_isSkipped)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / _fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1;

            // 表示時間
            elapsed = 0;
            while (elapsed < displayTime && !_isSkipped)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // フェードアウト
            elapsed = 0;
            while (elapsed < _fadeOutDuration && !_isSkipped)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / _fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0;

            _isSkipped = false;
            _isSkippable = false;
        }

        private IEnumerator ShowLoading()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }

            // ヒントを表示
            if (_loadingTipText != null && _loadingTips.Length > 0)
            {
                int tipIndex = UnityEngine.Random.Range(0, _loadingTips.Length);
                _loadingTipText.text = _loadingTips[tipIndex];
            }

            // ローディングシミュレーション
            float progress = 0;
            float targetProgress = 0;
            float loadingDuration = 2f;
            float elapsed = 0;

            while (progress < 1f)
            {
                elapsed += Time.deltaTime;
                targetProgress = Mathf.Min(1f, elapsed / loadingDuration);

                // スムーズな進行
                progress = Mathf.Lerp(progress, targetProgress, Time.deltaTime * 5f);

                if (_loadingBar != null)
                {
                    _loadingBar.value = progress;
                }

                if (_loadingText != null)
                {
                    _loadingText.text = $"Loading... {Mathf.FloorToInt(progress * 100)}%";
                }

                OnLoadProgress?.Invoke(progress);

                yield return null;
            }

            // 完了表示
            if (_loadingBar != null)
            {
                _loadingBar.value = 1f;
            }

            if (_loadingText != null)
            {
                _loadingText.text = "Loading... 100%";
            }

            yield return new WaitForSeconds(0.5f);

            // フェードアウト
            if (_loadingPanel != null)
            {
                var panelCanvasGroup = _loadingPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup != null)
                {
                    float fadeElapsed = 0;
                    while (fadeElapsed < _fadeOutDuration)
                    {
                        fadeElapsed += Time.deltaTime;
                        panelCanvasGroup.alpha = Mathf.Lerp(1, 0, fadeElapsed / _fadeOutDuration);
                        yield return null;
                    }
                }
            }
        }

        /// <summary>
        /// ローディング進捗を設定
        /// </summary>
        public void SetLoadingProgress(float progress, string message = null)
        {
            if (_loadingBar != null)
            {
                _loadingBar.value = progress;
            }

            if (_loadingText != null && !string.IsNullOrEmpty(message))
            {
                _loadingText.text = message;
            }
        }

        /// <summary>
        /// ヒントテキストを設定
        /// </summary>
        public void SetTipText(string tip)
        {
            if (_loadingTipText != null)
            {
                _loadingTipText.text = tip;
            }
        }

        /// <summary>
        /// ランダムなヒントを表示
        /// </summary>
        public void ShowRandomTip()
        {
            if (_loadingTipText != null && _loadingTips.Length > 0)
            {
                int index = UnityEngine.Random.Range(0, _loadingTips.Length);
                _loadingTipText.text = _loadingTips[index];
            }
        }

        /// <summary>
        /// スプラッシュをスキップ
        /// </summary>
        public void Skip()
        {
            if (_isSkippable)
            {
                _isSkipped = true;
            }
        }

        /// <summary>
        /// スキップ可能な状態を設定
        /// </summary>
        public void SetSkippable(bool skippable)
        {
            _isSkippable = skippable;
        }
    }
}
