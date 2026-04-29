using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Campaign;

namespace ThirtySixStratagems.UI.Campaign
{
    /// <summary>
    /// チュートリアルパネル
    /// チュートリアルの表示UIを管理
    /// </summary>
    public class TutorialPanel : MonoBehaviour
    {
        [Header("ダイアログ")]
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _characterImage;

        [Header("ボタン")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _hintButton;
        [SerializeField] private TextMeshProUGUI _nextButtonText;

        [Header("進捗")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _chapterText;

        [Header("ハイライト")]
        [SerializeField] private GameObject _highlightOverlay;
        [SerializeField] private RectTransform _highlightFrame;
        [SerializeField] private GameObject _arrowIndicator;

        [Header("ヒント")]
        [SerializeField] private GameObject _hintPanel;
        [SerializeField] private TextMeshProUGUI _hintText;

        [Header("設定")]
        [SerializeField] private float _typingSpeed = 0.03f;
        [SerializeField] private bool _useTypingEffect = true;

        // 状態
        private bool _isTyping = false;
        private string _fullText = "";
        private Coroutine _typingCoroutine;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            if (TutorialSystem.Instance != null)
            {
                TutorialSystem.Instance.OnStepStarted += OnStepStarted;
                TutorialSystem.Instance.OnStepCompleted += OnStepCompleted;
                TutorialSystem.Instance.OnChapterStarted += OnChapterStarted;
                TutorialSystem.Instance.OnTutorialCompleted += OnTutorialCompleted;
                TutorialSystem.Instance.OnHintRequested += OnHintRequested;
            }
        }

        private void OnDisable()
        {
            if (TutorialSystem.Instance != null)
            {
                TutorialSystem.Instance.OnStepStarted -= OnStepStarted;
                TutorialSystem.Instance.OnStepCompleted -= OnStepCompleted;
                TutorialSystem.Instance.OnChapterStarted -= OnChapterStarted;
                TutorialSystem.Instance.OnTutorialCompleted -= OnTutorialCompleted;
                TutorialSystem.Instance.OnHintRequested -= OnHintRequested;
            }
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNextClicked);

            if (_previousButton != null)
                _previousButton.onClick.AddListener(OnPreviousClicked);

            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipClicked);

            if (_hintButton != null)
                _hintButton.onClick.AddListener(OnHintClicked);
        }

        #endregion

        #region Display

        /// <summary>
        /// ステップを表示
        /// </summary>
        private void ShowStep(TutorialStep step)
        {
            if (step == null)
            {
                Hide();
                return;
            }

            // ダイアログパネル表示
            if (_dialogPanel != null)
                _dialogPanel.SetActive(true);

            // タイトル
            if (_titleText != null)
                _titleText.text = step.Title;

            // 説明文
            if (_descriptionText != null)
            {
                if (_useTypingEffect)
                {
                    StartTypingEffect(step.Description);
                }
                else
                {
                    _descriptionText.text = step.Description;
                }
            }

            // ボタン状態
            UpdateButtonStates(step);

            // ハイライト
            if (step.Type == TutorialStepType.Highlight || step.Type == TutorialStepType.Action)
            {
                ShowHighlight(step.HighlightTarget);
            }
            else
            {
                HideHighlight();
            }

            // ヒントボタン
            if (_hintButton != null)
                _hintButton.gameObject.SetActive(!string.IsNullOrEmpty(step.Hint));

            // 進捗更新
            UpdateProgress();
        }

        /// <summary>
        /// ボタン状態を更新
        /// </summary>
        private void UpdateButtonStates(TutorialStep step)
        {
            // 次へボタン
            if (_nextButton != null)
            {
                bool canProceed = step.Type != TutorialStepType.Action;
                _nextButton.interactable = canProceed;

                if (_nextButtonText != null)
                {
                    _nextButtonText.text = step.Type == TutorialStepType.Action ? "待機中..." : "次へ";
                }
            }

            // 前へボタン
            if (_previousButton != null)
            {
                _previousButton.interactable = true;
            }
        }

        /// <summary>
        /// 進捗を更新
        /// </summary>
        private void UpdateProgress()
        {
            if (TutorialSystem.Instance == null) return;

            float progress = TutorialSystem.Instance.GetProgress();

            if (_progressSlider != null)
                _progressSlider.value = progress;

            if (_progressText != null)
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            if (_dialogPanel != null)
                _dialogPanel.SetActive(false);

            HideHighlight();
            HideHint();
        }

        #endregion

        #region Typing Effect

        /// <summary>
        /// タイピングエフェクトを開始
        /// </summary>
        private void StartTypingEffect(string text)
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            _fullText = text;
            _typingCoroutine = StartCoroutine(TypeText());
        }

        /// <summary>
        /// テキストをタイプ
        /// </summary>
        private System.Collections.IEnumerator TypeText()
        {
            _isTyping = true;
            _descriptionText.text = "";

            foreach (char c in _fullText)
            {
                _descriptionText.text += c;
                yield return new WaitForSeconds(_typingSpeed);
            }

            _isTyping = false;
        }

        /// <summary>
        /// タイピングをスキップ
        /// </summary>
        private void SkipTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            _descriptionText.text = _fullText;
            _isTyping = false;
        }

        #endregion

        #region Highlight

        /// <summary>
        /// ハイライトを表示
        /// </summary>
        private void ShowHighlight(string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                HideHighlight();
                return;
            }

            if (_highlightOverlay != null)
                _highlightOverlay.SetActive(true);

            // ターゲットを検索してハイライト
            var target = GameObject.Find(targetName);
            if (target != null && _highlightFrame != null)
            {
                var targetRect = target.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    // ハイライトフレームをターゲットに合わせる
                    _highlightFrame.position = targetRect.position;
                    _highlightFrame.sizeDelta = targetRect.sizeDelta + new Vector2(20, 20);
                }
            }

            // 矢印表示
            if (_arrowIndicator != null)
            {
                _arrowIndicator.SetActive(target != null);
            }
        }

        /// <summary>
        /// ハイライトを非表示
        /// </summary>
        private void HideHighlight()
        {
            if (_highlightOverlay != null)
                _highlightOverlay.SetActive(false);

            if (_arrowIndicator != null)
                _arrowIndicator.SetActive(false);
        }

        #endregion

        #region Hint

        /// <summary>
        /// ヒントを表示
        /// </summary>
        private void ShowHint(string hint)
        {
            if (_hintPanel != null)
                _hintPanel.SetActive(true);

            if (_hintText != null)
                _hintText.text = hint;
        }

        /// <summary>
        /// ヒントを非表示
        /// </summary>
        private void HideHint()
        {
            if (_hintPanel != null)
                _hintPanel.SetActive(false);
        }

        #endregion

        #region Button Handlers

        private void OnNextClicked()
        {
            if (_isTyping)
            {
                SkipTyping();
                return;
            }

            TutorialSystem.Instance?.NextStep();
        }

        private void OnPreviousClicked()
        {
            TutorialSystem.Instance?.PreviousStep();
        }

        private void OnSkipClicked()
        {
            TutorialSystem.Instance?.SkipTutorial();
            Hide();
        }

        private void OnHintClicked()
        {
            TutorialSystem.Instance?.RequestHint();
        }

        #endregion

        #region Event Handlers

        private void OnStepStarted(TutorialStep step)
        {
            ShowStep(step);
        }

        private void OnStepCompleted(TutorialStep step)
        {
            // ステップ完了時の処理
        }

        private void OnChapterStarted(TutorialChapter chapter)
        {
            if (_chapterText != null)
                _chapterText.text = chapter.Title;
        }

        private void OnTutorialCompleted()
        {
            Hide();
        }

        private void OnHintRequested(string hint)
        {
            ShowHint(hint);
        }

        #endregion
    }
}
