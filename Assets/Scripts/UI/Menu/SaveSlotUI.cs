using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Systems;

namespace ThirtySixStratagems.UI.Menu
{
    /// <summary>
    /// セーブスロットUI
    /// 個別のセーブスロットの表示と操作
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private Button _slotButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private TextMeshProUGUI _slotNumberText;
        [SerializeField] private TextMeshProUGUI _saveNameText;
        [SerializeField] private TextMeshProUGUI _saveTimeText;
        [SerializeField] private TextMeshProUGUI _turnInfoText;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _filledState;

        [Header("ビジュアル")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = new Color(0.8f, 0.9f, 1f);

        // データ
        private SaveSlotInfo _slotInfo;
        private SaveLoadMode _mode;

        // イベント
        public event Action<int> OnSlotClicked;
        public event Action<int> OnDeleteClicked;

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(SaveSlotInfo info, SaveLoadMode mode)
        {
            _slotInfo = info;
            _mode = mode;

            UpdateDisplay();
            SetupButtons();
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        private void UpdateDisplay()
        {
            // スロット番号
            if (_slotNumberText != null)
            {
                _slotNumberText.text = $"スロット {_slotInfo.SlotIndex + 1}";
            }

            if (_slotInfo.IsEmpty)
            {
                // 空のスロット
                if (_emptyState != null)
                    _emptyState.SetActive(true);
                if (_filledState != null)
                    _filledState.SetActive(false);
                if (_deleteButton != null)
                    _deleteButton.gameObject.SetActive(false);

                // ロードモードでは空スロットを無効化
                if (_slotButton != null)
                    _slotButton.interactable = _mode == SaveLoadMode.Save;
            }
            else
            {
                // データありスロット
                if (_emptyState != null)
                    _emptyState.SetActive(false);
                if (_filledState != null)
                    _filledState.SetActive(true);
                if (_deleteButton != null)
                    _deleteButton.gameObject.SetActive(true);

                // セーブ名
                if (_saveNameText != null)
                {
                    _saveNameText.text = _slotInfo.SaveName;
                }

                // セーブ日時
                if (_saveTimeText != null)
                {
                    _saveTimeText.text = _slotInfo.SaveTime;
                }

                // ターン情報
                if (_turnInfoText != null)
                {
                    _turnInfoText.text = $"ターン {_slotInfo.CurrentTurn}";
                }

                if (_slotButton != null)
                    _slotButton.interactable = true;
            }
        }

        /// <summary>
        /// ボタンを設定
        /// </summary>
        private void SetupButtons()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveAllListeners();
                _slotButton.onClick.AddListener(OnSlotButtonClicked);
            }

            if (_deleteButton != null)
            {
                _deleteButton.onClick.RemoveAllListeners();
                _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            }
        }

        /// <summary>
        /// スロットボタンがクリックされた
        /// </summary>
        private void OnSlotButtonClicked()
        {
            OnSlotClicked?.Invoke(_slotInfo.SlotIndex);
        }

        /// <summary>
        /// 削除ボタンがクリックされた
        /// </summary>
        private void OnDeleteButtonClicked()
        {
            OnDeleteClicked?.Invoke(_slotInfo.SlotIndex);
        }

        /// <summary>
        /// 選択状態を設定
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = selected ? _selectedColor : _normalColor;
            }
        }
    }
}
