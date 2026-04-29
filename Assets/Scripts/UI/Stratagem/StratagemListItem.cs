using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.Data.ScriptableObjects;

namespace ThirtySixStratagems.UI.Stratagem
{
    /// <summary>
    /// 計略リストアイテム
    /// 計略一覧の各項目を表示
    /// </summary>
    public class StratagemListItem : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _categoryIcon;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _readingText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Image _background;

        [Header("カテゴリ色")]
        [SerializeField] private Color _winningColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _enemyColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _attackColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color _chaosColor = new Color(0.6f, 0.4f, 0.8f);
        [SerializeField] private Color _mergeColor = new Color(0.2f, 0.8f, 0.4f);
        [SerializeField] private Color _defeatColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("状態色")]
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _unavailableColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        private StratagemData _stratagem;
        private bool _isAvailable;

        // イベント
        public event Action<StratagemData> OnItemClicked;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }
        }

        /// <summary>
        /// アイテムをセットアップ
        /// </summary>
        public void Setup(StratagemData stratagem, bool isAvailable)
        {
            _stratagem = stratagem;
            _isAvailable = isAvailable;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_stratagem == null) return;

            // 番号
            if (_numberText != null)
            {
                _numberText.text = $"第{_stratagem.Number}計";
            }

            // 名前
            if (_nameText != null)
            {
                _nameText.text = _stratagem.NameJP;
            }

            // 読み
            if (_readingText != null)
            {
                _readingText.text = _stratagem.Reading;
            }

            // コスト
            if (_costText != null)
            {
                string cost = $"SP:{_stratagem.CostSP}";
                if (_stratagem.CostGold > 0)
                {
                    cost += $" 金:{_stratagem.CostGold}";
                }
                _costText.text = cost;
            }

            // アイコン
            if (_icon != null && _stratagem.Icon != null)
            {
                _icon.sprite = _stratagem.Icon;
                _icon.gameObject.SetActive(true);
            }
            else if (_icon != null)
            {
                _icon.gameObject.SetActive(false);
            }

            // カテゴリアイコンの色
            if (_categoryIcon != null)
            {
                _categoryIcon.color = GetCategoryColor(_stratagem.Category);
            }

            // 利用可能状態
            UpdateAvailabilityDisplay();
        }

        private void UpdateAvailabilityDisplay()
        {
            if (_background != null)
            {
                _background.color = _isAvailable ? _availableColor : _unavailableColor;
            }

            if (_button != null)
            {
                _button.interactable = _isAvailable;
            }

            // テキストの透明度
            float alpha = _isAvailable ? 1f : 0.5f;

            if (_nameText != null)
            {
                var color = _nameText.color;
                color.a = alpha;
                _nameText.color = color;
            }

            if (_readingText != null)
            {
                var color = _readingText.color;
                color.a = alpha;
                _readingText.color = color;
            }
        }

        private Color GetCategoryColor(StratagemCategory category)
        {
            switch (category)
            {
                case StratagemCategory.Winning:
                    return _winningColor;
                case StratagemCategory.Enemy:
                    return _enemyColor;
                case StratagemCategory.Attack:
                    return _attackColor;
                case StratagemCategory.Chaos:
                    return _chaosColor;
                case StratagemCategory.Merge:
                    return _mergeColor;
                case StratagemCategory.Defeat:
                    return _defeatColor;
                default:
                    return Color.white;
            }
        }

        private void OnClick()
        {
            OnItemClicked?.Invoke(_stratagem);
        }

        /// <summary>
        /// 計略データを取得
        /// </summary>
        public StratagemData GetStratagem()
        {
            return _stratagem;
        }

        /// <summary>
        /// 使用可能かどうか
        /// </summary>
        public bool IsAvailable()
        {
            return _isAvailable;
        }
    }
}
