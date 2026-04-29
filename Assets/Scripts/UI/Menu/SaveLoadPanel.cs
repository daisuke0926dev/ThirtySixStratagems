using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Systems;

namespace ThirtySixStratagems.UI.Menu
{
    /// <summary>
    /// セーブ/ロードパネル
    /// セーブスロットの表示と操作を管理
    /// </summary>
    public class SaveLoadPanel : MonoBehaviour
    {
        [Header("モード")]
        [SerializeField] private SaveLoadMode _mode = SaveLoadMode.Save;

        [Header("UI要素")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _quickSaveLoadButton;
        [SerializeField] private TextMeshProUGUI _quickSaveLoadButtonText;

        [Header("確認ダイアログ")]
        [SerializeField] private GameObject _confirmDialog;
        [SerializeField] private TextMeshProUGUI _confirmText;
        [SerializeField] private Button _confirmYesButton;
        [SerializeField] private Button _confirmNoButton;

        // スロットUI
        private List<SaveSlotUI> _slotUIs = new List<SaveSlotUI>();
        private int _selectedSlotIndex = -1;
        private bool _isConfirmingOverwrite = false;

        /// <summary>
        /// モード
        /// </summary>
        public SaveLoadMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                UpdateUI();
            }
        }

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            RefreshSlots();

            if (SaveLoadSystem.Instance != null)
            {
                SaveLoadSystem.Instance.OnSaveCompleted += OnSaveCompleted;
                SaveLoadSystem.Instance.OnLoadCompleted += OnLoadCompleted;
                SaveLoadSystem.Instance.OnSaveSlotsUpdated += OnSaveSlotsUpdated;
            }
        }

        private void OnDisable()
        {
            if (SaveLoadSystem.Instance != null)
            {
                SaveLoadSystem.Instance.OnSaveCompleted -= OnSaveCompleted;
                SaveLoadSystem.Instance.OnLoadCompleted -= OnLoadCompleted;
                SaveLoadSystem.Instance.OnSaveSlotsUpdated -= OnSaveSlotsUpdated;
            }
        }

        #region Setup

        /// <summary>
        /// ボタンを設定
        /// </summary>
        private void SetupButtons()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            if (_quickSaveLoadButton != null)
                _quickSaveLoadButton.onClick.AddListener(OnQuickSaveLoadClicked);

            if (_confirmYesButton != null)
                _confirmYesButton.onClick.AddListener(OnConfirmYes);

            if (_confirmNoButton != null)
                _confirmNoButton.onClick.AddListener(OnConfirmNo);

            if (_confirmDialog != null)
                _confirmDialog.SetActive(false);
        }

        /// <summary>
        /// UIを更新
        /// </summary>
        private void UpdateUI()
        {
            if (_titleText != null)
            {
                _titleText.text = _mode == SaveLoadMode.Save ? "セーブ" : "ロード";
            }

            if (_quickSaveLoadButtonText != null)
            {
                _quickSaveLoadButtonText.text = _mode == SaveLoadMode.Save ? "クイックセーブ" : "クイックロード";
            }

            // クイックロードボタンの有効/無効
            if (_quickSaveLoadButton != null && _mode == SaveLoadMode.Load)
            {
                _quickSaveLoadButton.interactable = SaveLoadSystem.Instance?.HasQuickSave() ?? false;
            }
        }

        #endregion

        #region Slot Management

        /// <summary>
        /// スロットを更新
        /// </summary>
        public void RefreshSlots()
        {
            if (SaveLoadSystem.Instance == null) return;

            var slots = SaveLoadSystem.Instance.GetAllSaveSlots();

            // 既存のスロットUIを削除
            foreach (var slotUI in _slotUIs)
            {
                if (slotUI != null)
                    Destroy(slotUI.gameObject);
            }
            _slotUIs.Clear();

            // 新しいスロットUIを作成
            foreach (var slot in slots)
            {
                CreateSlotUI(slot);
            }

            UpdateUI();
        }

        /// <summary>
        /// スロットUIを作成
        /// </summary>
        private void CreateSlotUI(SaveSlotInfo slotInfo)
        {
            if (_slotPrefab == null || _slotContainer == null) return;

            var slotObj = Instantiate(_slotPrefab, _slotContainer);
            var slotUI = slotObj.GetComponent<SaveSlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(slotInfo, _mode);
                slotUI.OnSlotClicked += OnSlotClicked;
                slotUI.OnDeleteClicked += OnDeleteClicked;
                _slotUIs.Add(slotUI);
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// スロットがクリックされた
        /// </summary>
        private void OnSlotClicked(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;

            if (_mode == SaveLoadMode.Save)
            {
                var slotInfo = SaveLoadSystem.Instance?.GetSaveSlotInfo(slotIndex);

                if (slotInfo != null && !slotInfo.IsEmpty)
                {
                    // 上書き確認
                    ShowConfirmDialog($"スロット {slotIndex + 1} を上書きしますか？");
                    _isConfirmingOverwrite = true;
                }
                else
                {
                    // 直接保存
                    SaveToSlot(slotIndex);
                }
            }
            else
            {
                // ロード
                LoadFromSlot(slotIndex);
            }
        }

        /// <summary>
        /// 削除がクリックされた
        /// </summary>
        private void OnDeleteClicked(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;
            ShowConfirmDialog($"スロット {slotIndex + 1} を削除しますか？");
            _isConfirmingOverwrite = false;
        }

        /// <summary>
        /// クイックセーブ/ロードがクリックされた
        /// </summary>
        private void OnQuickSaveLoadClicked()
        {
            if (_mode == SaveLoadMode.Save)
            {
                SaveLoadSystem.Instance?.QuickSave();
            }
            else
            {
                SaveLoadSystem.Instance?.QuickLoad();
            }
        }

        /// <summary>
        /// スロットに保存
        /// </summary>
        private void SaveToSlot(int slotIndex)
        {
            SaveLoadSystem.Instance?.SaveToSlot(slotIndex);
        }

        /// <summary>
        /// スロットから読み込み
        /// </summary>
        private void LoadFromSlot(int slotIndex)
        {
            SaveLoadSystem.Instance?.LoadFromSlot(slotIndex);
        }

        /// <summary>
        /// スロットを削除
        /// </summary>
        private void DeleteSlot(int slotIndex)
        {
            SaveLoadSystem.Instance?.DeleteSaveSlot(slotIndex);
        }

        #endregion

        #region Confirm Dialog

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        private void ShowConfirmDialog(string message)
        {
            if (_confirmDialog != null)
            {
                _confirmDialog.SetActive(true);

                if (_confirmText != null)
                    _confirmText.text = message;
            }
        }

        /// <summary>
        /// 確認ダイアログを非表示
        /// </summary>
        private void HideConfirmDialog()
        {
            if (_confirmDialog != null)
                _confirmDialog.SetActive(false);
        }

        /// <summary>
        /// 確認「はい」
        /// </summary>
        private void OnConfirmYes()
        {
            HideConfirmDialog();

            if (_isConfirmingOverwrite)
            {
                SaveToSlot(_selectedSlotIndex);
            }
            else
            {
                DeleteSlot(_selectedSlotIndex);
            }
        }

        /// <summary>
        /// 確認「いいえ」
        /// </summary>
        private void OnConfirmNo()
        {
            HideConfirmDialog();
        }

        #endregion

        #region Event Handlers

        private void OnSaveCompleted(string saveName, bool success)
        {
            if (success)
            {
                RefreshSlots();
            }
        }

        private void OnLoadCompleted(string fileName, bool success)
        {
            if (success)
            {
                Close();
            }
        }

        private void OnSaveSlotsUpdated(List<SaveSlotInfo> slots)
        {
            RefreshSlots();
        }

        #endregion

        #region Panel Control

        /// <summary>
        /// セーブモードで表示
        /// </summary>
        public void ShowSaveMode()
        {
            _mode = SaveLoadMode.Save;
            gameObject.SetActive(true);
            RefreshSlots();
        }

        /// <summary>
        /// ロードモードで表示
        /// </summary>
        public void ShowLoadMode()
        {
            _mode = SaveLoadMode.Load;
            gameObject.SetActive(true);
            RefreshSlots();
        }

        /// <summary>
        /// 閉じる
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }

    /// <summary>
    /// セーブ/ロードモード
    /// </summary>
    public enum SaveLoadMode
    {
        Save,
        Load
    }
}
