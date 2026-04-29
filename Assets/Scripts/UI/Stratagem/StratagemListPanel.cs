using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;
using ThirtySixStratagems.Stratagem;

namespace ThirtySixStratagems.UI.Stratagem
{
    /// <summary>
    /// 計略一覧パネル
    /// 使用可能な計略を表示し、選択できるUI
    /// </summary>
    public class StratagemListPanel : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _stratagemItemPrefab;
        [SerializeField] private StratagemDetailPanel _detailPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Dropdown _categoryFilter;

        [Header("カテゴリタブ")]
        [SerializeField] private Button[] _categoryTabs;

        [Header("状態表示")]
        [SerializeField] private TextMeshProUGUI _spText;
        [SerializeField] private TextMeshProUGUI _goldText;

        // 現在の表示状態
        private string _currentFactionId;
        private string _currentCharacterId;
        private StratagemCategory? _selectedCategory = null;
        private List<StratagemListItem> _items = new List<StratagemListItem>();

        // イベント
        public event Action<StratagemData> OnStratagemSelected;
        public event Action OnPanelClosed;

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            SetupCategoryTabs();
            SetupCategoryDropdown();
        }

        private void OnEnable()
        {
            EventBus.OnResourceChanged += OnResourceChanged;
        }

        private void OnDisable()
        {
            EventBus.OnResourceChanged -= OnResourceChanged;
        }

        #region Public Methods

        /// <summary>
        /// パネルを開く
        /// </summary>
        public void Open(string factionId, string characterId)
        {
            _currentFactionId = factionId;
            _currentCharacterId = characterId;

            gameObject.SetActive(true);
            UpdateResourceDisplay();
            RefreshList();

            // 詳細パネルをリセット
            if (_detailPanel != null)
            {
                _detailPanel.Hide();
            }
        }

        /// <summary>
        /// パネルを閉じる
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            OnPanelClosed?.Invoke();
        }

        /// <summary>
        /// リストを更新
        /// </summary>
        public void RefreshList()
        {
            ClearList();

            if (StratagemConditionChecker.Instance == null) return;

            var availabilities = StratagemConditionChecker.Instance.GetAvailableStratagems(
                _currentFactionId, _currentCharacterId);

            foreach (var availability in availabilities)
            {
                // カテゴリフィルター
                if (_selectedCategory.HasValue && availability.StratagemData.Category != _selectedCategory.Value)
                {
                    continue;
                }

                CreateStratagemItem(availability);
            }
        }

        /// <summary>
        /// カテゴリでフィルター
        /// </summary>
        public void FilterByCategory(StratagemCategory? category)
        {
            _selectedCategory = category;
            RefreshList();

            // タブのハイライト更新
            UpdateCategoryTabHighlight();
        }

        #endregion

        #region UI Setup

        private void SetupCategoryTabs()
        {
            if (_categoryTabs == null || _categoryTabs.Length == 0) return;

            // 全カテゴリタブ
            if (_categoryTabs.Length > 0 && _categoryTabs[0] != null)
            {
                _categoryTabs[0].onClick.AddListener(() => FilterByCategory(null));
            }

            // 各カテゴリタブ
            for (int i = 1; i < _categoryTabs.Length && i <= 6; i++)
            {
                if (_categoryTabs[i] != null)
                {
                    var category = (StratagemCategory)(i - 1);
                    _categoryTabs[i].onClick.AddListener(() => FilterByCategory(category));
                }
            }
        }

        private void SetupCategoryDropdown()
        {
            if (_categoryFilter == null) return;

            _categoryFilter.ClearOptions();
            var options = new List<string>
            {
                "全て",
                "勝戦計",
                "敵戦計",
                "攻戦計",
                "混戦計",
                "併戦計",
                "敗戦計"
            };
            _categoryFilter.AddOptions(options);

            _categoryFilter.onValueChanged.AddListener(OnCategoryDropdownChanged);
        }

        private void OnCategoryDropdownChanged(int index)
        {
            if (index == 0)
            {
                FilterByCategory(null);
            }
            else
            {
                FilterByCategory((StratagemCategory)(index - 1));
            }
        }

        private void UpdateCategoryTabHighlight()
        {
            if (_categoryTabs == null) return;

            for (int i = 0; i < _categoryTabs.Length; i++)
            {
                if (_categoryTabs[i] == null) continue;

                bool isSelected = false;
                if (i == 0)
                {
                    isSelected = !_selectedCategory.HasValue;
                }
                else if (i <= 6)
                {
                    isSelected = _selectedCategory.HasValue &&
                                 _selectedCategory.Value == (StratagemCategory)(i - 1);
                }

                // タブの見た目を更新（Selectableの場合）
                var colors = _categoryTabs[i].colors;
                colors.normalColor = isSelected ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                _categoryTabs[i].colors = colors;
            }
        }

        #endregion

        #region List Management

        private void ClearList()
        {
            foreach (var item in _items)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _items.Clear();
        }

        private void CreateStratagemItem(StratagemAvailability availability)
        {
            if (_stratagemItemPrefab == null || _listContainer == null) return;

            var itemObj = Instantiate(_stratagemItemPrefab, _listContainer);
            var item = itemObj.GetComponent<StratagemListItem>();

            if (item != null)
            {
                item.Setup(availability.StratagemData, availability.IsAvailable);
                item.OnItemClicked += OnStratagemItemClicked;
                _items.Add(item);
            }
        }

        private void OnStratagemItemClicked(StratagemData stratagem)
        {
            // 詳細パネルを表示
            if (_detailPanel != null)
            {
                _detailPanel.Show(stratagem, _currentFactionId, _currentCharacterId);
            }

            OnStratagemSelected?.Invoke(stratagem);
        }

        #endregion

        #region Resource Display

        private void UpdateResourceDisplay()
        {
            var faction = GameManager.Instance?.GetFaction(_currentFactionId);
            if (faction == null) return;

            if (_spText != null)
            {
                _spText.text = $"計略: {faction.StratagemPoints}/{Constants.Balance.DefaultMaxStratagemPoints}";
            }

            if (_goldText != null)
            {
                _goldText.text = $"金: {faction.Gold}";
            }
        }

        private void OnResourceChanged(ResourceEventArgs args)
        {
            if (args.FactionId == _currentFactionId)
            {
                UpdateResourceDisplay();
                RefreshList();
            }
        }

        #endregion
    }
}
