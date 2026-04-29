using System;
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
    /// 計略詳細パネル
    /// 選択された計略の詳細情報を表示し、実行ボタンを提供
    /// </summary>
    public class StratagemDetailPanel : MonoBehaviour
    {
        [Header("基本情報")]
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _readingText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private Image _icon;

        [Header("説明")]
        [SerializeField] private TextMeshProUGUI _originalText;
        [SerializeField] private TextMeshProUGUI _translationText;
        [SerializeField] private TextMeshProUGUI _historicalText;
        [SerializeField] private TextMeshProUGUI _effectText;

        [Header("コストと成功率")]
        [SerializeField] private TextMeshProUGUI _costSPText;
        [SerializeField] private TextMeshProUGUI _costGoldText;
        [SerializeField] private TextMeshProUGUI _successRateText;
        [SerializeField] private Slider _successRateSlider;

        [Header("対象選択")]
        [SerializeField] private GameObject _targetSelectionArea;
        [SerializeField] private TextMeshProUGUI _targetTypeText;
        [SerializeField] private TMP_Dropdown _targetDropdown;

        [Header("ボタン")]
        [SerializeField] private Button _executeButton;
        [SerializeField] private TextMeshProUGUI _executeButtonText;
        [SerializeField] private Button _cancelButton;

        [Header("条件表示")]
        [SerializeField] private GameObject _conditionArea;
        [SerializeField] private TextMeshProUGUI _conditionText;

        // 現在の状態
        private StratagemData _currentStratagem;
        private string _currentFactionId;
        private string _currentCharacterId;
        private string _selectedTargetId;

        // イベント
        public event Action<StratagemData, string> OnExecuteRequested;
        public event Action OnCancelRequested;

        private void Awake()
        {
            if (_executeButton != null)
            {
                _executeButton.onClick.AddListener(OnExecuteClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (_targetDropdown != null)
            {
                _targetDropdown.onValueChanged.AddListener(OnTargetSelected);
            }
        }

        #region Public Methods

        /// <summary>
        /// 詳細パネルを表示
        /// </summary>
        public void Show(StratagemData stratagem, string factionId, string characterId)
        {
            _currentStratagem = stratagem;
            _currentFactionId = factionId;
            _currentCharacterId = characterId;
            _selectedTargetId = null;

            gameObject.SetActive(true);
            UpdateDisplay();
            UpdateTargetSelection();
            UpdateConditions();
            UpdateSuccessRate();
        }

        /// <summary>
        /// 詳細パネルを非表示
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _currentStratagem = null;
        }

        #endregion

        #region Display Update

        private void UpdateDisplay()
        {
            if (_currentStratagem == null) return;

            // 番号
            if (_numberText != null)
            {
                _numberText.text = _currentStratagem.GetFullName();
            }

            // 名前
            if (_nameText != null)
            {
                _nameText.text = _currentStratagem.NameJP;
            }

            // 読み
            if (_readingText != null)
            {
                _readingText.text = _currentStratagem.Reading;
            }

            // カテゴリ
            if (_categoryText != null)
            {
                _categoryText.text = $"{_currentStratagem.GetCategoryName()} / 第{_currentStratagem.GetCategoryNumber()}套";
            }

            // アイコン
            if (_icon != null)
            {
                if (_currentStratagem.Icon != null)
                {
                    _icon.sprite = _currentStratagem.Icon;
                    _icon.gameObject.SetActive(true);
                }
                else
                {
                    _icon.gameObject.SetActive(false);
                }
            }

            // 原典
            if (_originalText != null)
            {
                _originalText.text = _currentStratagem.OriginalText;
            }

            // 現代語訳
            if (_translationText != null)
            {
                _translationText.text = _currentStratagem.ModernTranslation;
            }

            // 歴史的例
            if (_historicalText != null)
            {
                _historicalText.text = _currentStratagem.HistoricalExample;
            }

            // ゲーム効果
            if (_effectText != null)
            {
                _effectText.text = _currentStratagem.GameEffectDescription;
            }

            // コスト
            if (_costSPText != null)
            {
                _costSPText.text = $"計略ポイント: {_currentStratagem.CostSP}";
            }

            if (_costGoldText != null)
            {
                if (_currentStratagem.CostGold > 0)
                {
                    _costGoldText.text = $"金: {_currentStratagem.CostGold}";
                    _costGoldText.gameObject.SetActive(true);
                }
                else
                {
                    _costGoldText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateTargetSelection()
        {
            if (_currentStratagem == null) return;

            bool needsTarget = _currentStratagem.TargetType != StratagemTarget.Self;

            if (_targetSelectionArea != null)
            {
                _targetSelectionArea.SetActive(needsTarget);
            }

            if (!needsTarget)
            {
                _selectedTargetId = _currentFactionId; // 自分自身
                return;
            }

            // 対象タイプの説明
            if (_targetTypeText != null)
            {
                _targetTypeText.text = GetTargetTypeDescription(_currentStratagem.TargetType);
            }

            // 対象ドロップダウンを更新
            if (_targetDropdown != null)
            {
                PopulateTargetDropdown();
            }
        }

        private void PopulateTargetDropdown()
        {
            if (_targetDropdown == null || _currentStratagem == null) return;

            _targetDropdown.ClearOptions();

            var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            var targetIds = new System.Collections.Generic.List<string>();

            options.Add(new TMP_Dropdown.OptionData("-- 対象を選択 --"));
            targetIds.Add(null);

            switch (_currentStratagem.TargetType)
            {
                case StratagemTarget.EnemyFaction:
                    PopulateFactionTargets(options, targetIds);
                    break;

                case StratagemTarget.EnemyArmy:
                    PopulateArmyTargets(options, targetIds);
                    break;

                case StratagemTarget.EnemyCharacter:
                    PopulateCharacterTargets(options, targetIds);
                    break;

                case StratagemTarget.EnemyTerritory:
                    PopulateTerritoryTargets(options, targetIds);
                    break;

                case StratagemTarget.Any:
                    PopulateFactionTargets(options, targetIds);
                    break;
            }

            _targetDropdown.AddOptions(options);
            _targetDropdown.SetValueWithoutNotify(0);
            _selectedTargetId = null;
        }

        private void PopulateFactionTargets(
            System.Collections.Generic.List<TMP_Dropdown.OptionData> options,
            System.Collections.Generic.List<string> targetIds)
        {
            if (GameManager.Instance?.GameData == null) return;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.Id != _currentFactionId && faction.TerritoryIds.Count > 0)
                {
                    options.Add(new TMP_Dropdown.OptionData(faction.Name));
                    targetIds.Add(faction.Id);
                }
            }
        }

        private void PopulateArmyTargets(
            System.Collections.Generic.List<TMP_Dropdown.OptionData> options,
            System.Collections.Generic.List<string> targetIds)
        {
            if (GameManager.Instance?.GameData == null) return;

            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.FactionId != _currentFactionId && army.SoldierCount > 0)
                {
                    var faction = GameManager.Instance.GetFaction(army.FactionId);
                    string factionName = faction?.Name ?? "不明";
                    options.Add(new TMP_Dropdown.OptionData($"{army.Name} ({factionName})"));
                    targetIds.Add(army.Id);
                }
            }
        }

        private void PopulateCharacterTargets(
            System.Collections.Generic.List<TMP_Dropdown.OptionData> options,
            System.Collections.Generic.List<string> targetIds)
        {
            if (GameManager.Instance?.GameData == null) return;

            foreach (var character in GameManager.Instance.GameData.Characters.Values)
            {
                if (character.FactionId != _currentFactionId)
                {
                    var faction = GameManager.Instance.GetFaction(character.FactionId);
                    string factionName = faction?.Name ?? "不明";
                    options.Add(new TMP_Dropdown.OptionData($"{character.Name} ({factionName})"));
                    targetIds.Add(character.Id);
                }
            }
        }

        private void PopulateTerritoryTargets(
            System.Collections.Generic.List<TMP_Dropdown.OptionData> options,
            System.Collections.Generic.List<string> targetIds)
        {
            if (GameManager.Instance?.GameData == null) return;

            foreach (var territory in GameManager.Instance.GameData.Territories.Values)
            {
                if (territory.OwnerId != _currentFactionId)
                {
                    var owner = GameManager.Instance.GetFaction(territory.OwnerId);
                    string ownerName = owner?.Name ?? "中立";
                    options.Add(new TMP_Dropdown.OptionData($"{territory.Name} ({ownerName})"));
                    targetIds.Add(territory.Id);
                }
            }
        }

        private void UpdateConditions()
        {
            if (_currentStratagem == null || StratagemConditionChecker.Instance == null) return;

            var result = StratagemConditionChecker.Instance.CheckStratagem(
                _currentStratagem, _currentFactionId, _currentCharacterId, _selectedTargetId);

            if (_conditionArea != null)
            {
                _conditionArea.SetActive(!result.CanUse);
            }

            if (_conditionText != null)
            {
                _conditionText.text = result.GetFailureReasons() ?? "";
            }

            // 実行ボタンの状態
            bool canExecute = result.CanUse && !string.IsNullOrEmpty(_selectedTargetId);
            if (_currentStratagem.TargetType == StratagemTarget.Self)
            {
                canExecute = result.CanUse;
            }

            if (_executeButton != null)
            {
                _executeButton.interactable = canExecute;
            }
        }

        private void UpdateSuccessRate()
        {
            if (_currentStratagem == null || StratagemManager.Instance == null) return;

            var character = GameManager.Instance?.GetCharacter(_currentCharacterId);
            int successRate = StratagemManager.Instance.CalculateSuccessRate(
                _currentStratagem, character, _selectedTargetId);

            if (_successRateText != null)
            {
                _successRateText.text = $"成功率: {successRate}%";
            }

            if (_successRateSlider != null)
            {
                _successRateSlider.value = successRate / 100f;
            }
        }

        private string GetTargetTypeDescription(StratagemTarget targetType)
        {
            switch (targetType)
            {
                case StratagemTarget.Self:
                    return "対象: 自勢力";
                case StratagemTarget.EnemyFaction:
                    return "対象: 敵勢力を選択";
                case StratagemTarget.EnemyArmy:
                    return "対象: 敵軍を選択";
                case StratagemTarget.EnemyCharacter:
                    return "対象: 敵武将を選択";
                case StratagemTarget.EnemyTerritory:
                    return "対象: 敵領地を選択";
                case StratagemTarget.Any:
                    return "対象: 任意の勢力を選択";
                default:
                    return "対象を選択";
            }
        }

        #endregion

        #region Event Handlers

        private void OnTargetSelected(int index)
        {
            if (index <= 0)
            {
                _selectedTargetId = null;
            }
            else
            {
                // ドロップダウンから選択されたIDを取得
                // 注：実際の実装では、PopulateTargetDropdownで保存したtargetIdsリストを参照
                _selectedTargetId = GetTargetIdFromDropdown(index);
            }

            UpdateConditions();
            UpdateSuccessRate();
        }

        private string GetTargetIdFromDropdown(int index)
        {
            // 簡易実装：ドロップダウンのテキストからIDを復元
            // 実際の実装では、リストを保持して参照する
            if (_targetDropdown == null || index < 0 || index >= _targetDropdown.options.Count)
                return null;

            var text = _targetDropdown.options[index].text;
            // TODO: 実際のIDマッピングを実装
            return text;
        }

        private void OnExecuteClicked()
        {
            if (_currentStratagem == null) return;

            string targetId = _selectedTargetId;
            if (_currentStratagem.TargetType == StratagemTarget.Self)
            {
                targetId = _currentFactionId;
            }

            OnExecuteRequested?.Invoke(_currentStratagem, targetId);
        }

        private void OnCancelClicked()
        {
            Hide();
            OnCancelRequested?.Invoke();
        }

        #endregion
    }
}
