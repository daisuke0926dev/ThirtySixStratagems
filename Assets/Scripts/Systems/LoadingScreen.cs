using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ローディング画面
    /// ゲーム初期化中の表示を管理
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _tipText;

        [Header("アニメーション")]
        [SerializeField] private Image _loadingIcon;
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("ヒント")]
        [SerializeField] private float _tipChangeInterval = 3f;
        [SerializeField] private string[] _loadingTips = new string[]
        {
            "計略は状況に応じて使い分けましょう",
            "敵の弱点を見極めることが勝利への鍵です",
            "三十六計には6つのカテゴリがあります",
            "士気が低いと戦闘で不利になります",
            "複数の計略を組み合わせると効果的です"
        };

        // 状態
        private bool _isActive = false;
        private Coroutine _tipCoroutine;

        private void Awake()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (GameInitializer.Instance != null)
            {
                GameInitializer.Instance.OnInitializationStarted += OnInitializationStarted;
                GameInitializer.Instance.OnProgressUpdated += OnProgressUpdated;
                GameInitializer.Instance.OnInitializationCompleted += OnInitializationCompleted;
                GameInitializer.Instance.OnInitializationFailed += OnInitializationFailed;
            }
        }

        private void OnDisable()
        {
            if (GameInitializer.Instance != null)
            {
                GameInitializer.Instance.OnInitializationStarted -= OnInitializationStarted;
                GameInitializer.Instance.OnProgressUpdated -= OnProgressUpdated;
                GameInitializer.Instance.OnInitializationCompleted -= OnInitializationCompleted;
                GameInitializer.Instance.OnInitializationFailed -= OnInitializationFailed;
            }
        }

        private void Update()
        {
            if (_isActive && _loadingIcon != null)
            {
                _loadingIcon.transform.Rotate(0, 0, -_rotationSpeed * Time.deltaTime);
            }
        }

        #region Show/Hide

        /// <summary>
        /// ローディング画面を表示
        /// </summary>
        public void Show()
        {
            _isActive = true;

            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(true);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            if (_progressBar != null)
            {
                _progressBar.value = 0;
            }

            // ヒント表示を開始
            if (_tipCoroutine != null)
            {
                StopCoroutine(_tipCoroutine);
            }
            _tipCoroutine = StartCoroutine(CycleTips());

            UpdateTip();
        }

        /// <summary>
        /// ローディング画面を非表示
        /// </summary>
        public void Hide()
        {
            _isActive = false;

            if (_tipCoroutine != null)
            {
                StopCoroutine(_tipCoroutine);
                _tipCoroutine = null;
            }

            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (_canvasGroup != null)
            {
                float elapsed = 0f;
                float duration = 0.5f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    yield return null;
                }

                _canvasGroup.alpha = 0f;
            }

            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(false);
            }
        }

        #endregion

        #region Progress

        /// <summary>
        /// 進捗を更新
        /// </summary>
        public void UpdateProgress(float progress, string status)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }

            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }

        #endregion

        #region Tips

        /// <summary>
        /// ヒントをサイクル表示
        /// </summary>
        private IEnumerator CycleTips()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(_tipChangeInterval);
                UpdateTip();
            }
        }

        /// <summary>
        /// ヒントを更新
        /// </summary>
        private void UpdateTip()
        {
            if (_tipText != null && _loadingTips != null && _loadingTips.Length > 0)
            {
                int index = Random.Range(0, _loadingTips.Length);
                _tipText.text = _loadingTips[index];
            }
        }

        /// <summary>
        /// ヒントを追加
        /// </summary>
        public void AddTip(string tip)
        {
            var list = new System.Collections.Generic.List<string>(_loadingTips);
            list.Add(tip);
            _loadingTips = list.ToArray();
        }

        #endregion

        #region Event Handlers

        private void OnInitializationStarted()
        {
            Show();
        }

        private void OnProgressUpdated(float progress, string status)
        {
            UpdateProgress(progress, status);
        }

        private void OnInitializationCompleted()
        {
            UpdateProgress(1f, "初期化完了");
            Hide();
        }

        private void OnInitializationFailed(string error)
        {
            if (_statusText != null)
            {
                _statusText.text = $"エラー: {error}";
                _statusText.color = Color.red;
            }
        }

        #endregion
    }
}
