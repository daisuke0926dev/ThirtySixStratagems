using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// 確認ダイアログ
    /// 重要なアクションの確認を求めるダイアログ
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        private static ConfirmDialog _instance;
        public static ConfirmDialog Instance => _instance;

        [Header("UI要素")]
        [SerializeField] private GameObject _dialogPanel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        [Header("スタイル")]
        [SerializeField] private Color _confirmNormalColor = new Color(0.2f, 0.6f, 0.3f, 1f);
        [SerializeField] private Color _confirmDangerColor = new Color(0.8f, 0.2f, 0.2f, 1f);

        private UIAnimator _animator;
        private Action _onConfirm;
        private Action _onCancel;

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

            if (_dialogPanel != null)
            {
                _animator = _dialogPanel.GetComponent<UIAnimator>();
                _dialogPanel.SetActive(false);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        public void Show(
            string title,
            string message,
            Action onConfirm,
            Action onCancel = null,
            string confirmText = "確認",
            string cancelText = "キャンセル",
            bool isDanger = false)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (_titleText != null)
            {
                _titleText.text = title;
            }

            if (_messageText != null)
            {
                _messageText.text = message;
            }

            if (_confirmButtonText != null)
            {
                _confirmButtonText.text = confirmText;
            }

            if (_cancelButtonText != null)
            {
                _cancelButtonText.text = cancelText;
            }

            // 危険なアクションの場合はボタンの色を変更
            if (_confirmButton != null)
            {
                var image = _confirmButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = isDanger ? _confirmDangerColor : _confirmNormalColor;
                }
            }

            if (_dialogPanel != null)
            {
                if (_animator != null)
                {
                    _dialogPanel.SetActive(true);
                    _animator.Show();
                }
                else
                {
                    _dialogPanel.SetActive(true);
                }
            }
        }

        /// <summary>
        /// シンプルな確認ダイアログを表示
        /// </summary>
        public void ShowSimple(string message, Action onConfirm)
        {
            Show("確認", message, onConfirm);
        }

        /// <summary>
        /// 危険なアクションの確認ダイアログを表示
        /// </summary>
        public void ShowDanger(string title, string message, Action onConfirm, string confirmText = "実行")
        {
            Show(title, message, onConfirm, null, confirmText, "キャンセル", true);
        }

        /// <summary>
        /// ダイアログを閉じる
        /// </summary>
        public void Hide()
        {
            if (_dialogPanel != null)
            {
                if (_animator != null)
                {
                    _animator.Hide();
                }
                else
                {
                    _dialogPanel.SetActive(false);
                }
            }

            _onConfirm = null;
            _onCancel = null;
        }

        private void OnConfirmClicked()
        {
            var callback = _onConfirm;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            var callback = _onCancel;
            Hide();
            callback?.Invoke();
        }

        private void OnDestroy()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
            }
        }
    }
}
