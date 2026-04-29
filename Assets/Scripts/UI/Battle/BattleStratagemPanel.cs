using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.ScriptableObjects;
using ThirtySixStratagems.Stratagem;

namespace ThirtySixStratagems.UI.Battle
{
    /// <summary>
    /// 戦闘中計略選択パネル
    /// 戦闘で使用可能な計略を表示・選択
    /// </summary>
    public class BattleStratagemPanel : MonoBehaviour
    {
        [Header("コンテンツ")]
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _stratagemItemPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("詳細表示")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _successRateText;

        [Header("ボタン")]
        [SerializeField] private Button _useButton;
        [SerializeField] private Button _cancelButton;

        [Header("データ")]
        [SerializeField] private StratagemDatabase _stratagemDatabase;

        // 状態
        private List<GameObject> _stratagemItems = new List<GameObject>();
        private string _selectedStratagemId;
        private string _currentFactionId;
        private string _targetId;

        // イベント
        public event Action<string> OnStratagemSelected;
        public event Action OnCancelled;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            ClearSelection();
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_useButton != null)
            {
                _useButton.onClick.AddListener(OnUseClicked);
                _useButton.interactable = false;
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// パネルを表示
        /// </summary>
        public void Show(string factionId, string targetId)
        {
            _currentFactionId = factionId;
            _targetId = targetId;

            gameObject.SetActive(true);

            PopulateStratagemList();
            ClearSelection();
        }

        /// <summary>
        /// パネルを非表示
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region List Management

        /// <summary>
        /// 計略リストを生成
        /// </summary>
        private void PopulateStratagemList()
        {
            ClearList();

            if (_stratagemDatabase == null)
            {
                _stratagemDatabase = GameManager.Instance?.StratagemDatabase;
            }

            if (_stratagemDatabase == null || _listContainer == null) return;

            var faction = GameManager.Instance?.GetFaction(_currentFactionId);
            int availableSP = faction?.StratagemPoints ?? 0;

            // 戦闘で使用可能な計略を取得
            var stratagems = GetBattleUsableStratagems();

            foreach (var stratagem in stratagems)
            {
                CreateStratagemItem(stratagem, availableSP);
            }
        }

        /// <summary>
        /// 戦闘で使用可能な計略を取得
        /// </summary>
        private List<StratagemData> GetBattleUsableStratagems()
        {
            var result = new List<StratagemData>();

            if (_stratagemDatabase == null) return result;

            foreach (var stratagem in _stratagemDatabase.AllStratagems)
            {
                // 戦闘カテゴリの計略のみ
                if (stratagem.Category == Data.Models.StratagemCategory.Military ||
                    stratagem.Category == Data.Models.StratagemCategory.Surprise)
                {
                    result.Add(stratagem);
                }
            }

            return result;
        }

        /// <summary>
        /// 計略アイテムを作成
        /// </summary>
        private void CreateStratagemItem(StratagemData stratagem, int availableSP)
        {
            if (_stratagemItemPrefab == null) return;

            var itemObj = Instantiate(_stratagemItemPrefab, _listContainer);
            _stratagemItems.Add(itemObj);

            // テキスト設定
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                texts[0].text = stratagem.StratagemName;
            }
            if (texts.Length > 1)
            {
                texts[1].text = $"消費SP: {stratagem.BaseCost}";
            }

            // ボタン設定
            var button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                string stratagemId = stratagem.StratagemId;
                bool canUse = availableSP >= stratagem.BaseCost;

                button.interactable = canUse;
                button.onClick.AddListener(() => SelectStratagem(stratagemId));

                // 使用不可の場合は色を変更
                if (!canUse)
                {
                    var colors = button.colors;
                    colors.normalColor = new Color(0.5f, 0.5f, 0.5f);
                    button.colors = colors;
                }
            }
        }

        /// <summary>
        /// リストをクリア
        /// </summary>
        private void ClearList()
        {
            foreach (var item in _stratagemItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _stratagemItems.Clear();
        }

        #endregion

        #region Selection

        /// <summary>
        /// 計略を選択
        /// </summary>
        private void SelectStratagem(string stratagemId)
        {
            _selectedStratagemId = stratagemId;

            var stratagem = _stratagemDatabase?.GetById(stratagemId);
            if (stratagem == null) return;

            // 詳細を表示
            if (_nameText != null)
            {
                _nameText.text = stratagem.StratagemName;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = stratagem.Description;
            }

            if (_effectText != null)
            {
                _effectText.text = GetEffectDescription(stratagem);
            }

            if (_costText != null)
            {
                _costText.text = $"消費計略ポイント: {stratagem.BaseCost}";
            }

            if (_successRateText != null)
            {
                int successRate = CalculateSuccessRate(stratagem);
                _successRateText.text = $"成功率: {successRate}%";
            }

            // 使用ボタンを有効化
            if (_useButton != null)
            {
                var faction = GameManager.Instance?.GetFaction(_currentFactionId);
                _useButton.interactable = (faction?.StratagemPoints ?? 0) >= stratagem.BaseCost;
            }
        }

        /// <summary>
        /// 選択をクリア
        /// </summary>
        private void ClearSelection()
        {
            _selectedStratagemId = null;

            if (_nameText != null) _nameText.text = "計略を選択してください";
            if (_descriptionText != null) _descriptionText.text = "";
            if (_effectText != null) _effectText.text = "";
            if (_costText != null) _costText.text = "";
            if (_successRateText != null) _successRateText.text = "";
            if (_useButton != null) _useButton.interactable = false;
        }

        /// <summary>
        /// 効果の説明を取得
        /// </summary>
        private string GetEffectDescription(StratagemData stratagem)
        {
            string effect = "";

            switch (stratagem.PrimaryEffect)
            {
                case Data.Models.StratagemEffectType.AttackBoost:
                    effect = $"攻撃力+{stratagem.EffectPower}%";
                    break;
                case Data.Models.StratagemEffectType.DefenseBoost:
                    effect = $"防御力+{stratagem.EffectPower}%";
                    break;
                case Data.Models.StratagemEffectType.MoraleDamage:
                    effect = $"敵士気-{stratagem.EffectPower}";
                    break;
                case Data.Models.StratagemEffectType.Ambush:
                    effect = "奇襲効果（初回攻撃+50%）";
                    break;
                case Data.Models.StratagemEffectType.Retreat:
                    effect = "強制撤退";
                    break;
                default:
                    effect = "特殊効果";
                    break;
            }

            return effect;
        }

        /// <summary>
        /// 成功率を計算
        /// </summary>
        private int CalculateSuccessRate(StratagemData stratagem)
        {
            int baseRate = stratagem.BaseSuccessRate;

            // 知力ボーナスを適用
            var faction = GameManager.Instance?.GetFaction(_currentFactionId);
            if (faction != null)
            {
                // 最高知力の武将を探す
                int maxInt = 0;
                foreach (var charId in faction.CharacterIds)
                {
                    var character = GameManager.Instance.GetCharacter(charId);
                    if (character != null && character.Intelligence > maxInt)
                    {
                        maxInt = character.Intelligence;
                    }
                }

                baseRate += maxInt / 5;
            }

            return Mathf.Clamp(baseRate, 5, 95);
        }

        #endregion

        #region Button Handlers

        private void OnUseClicked()
        {
            if (string.IsNullOrEmpty(_selectedStratagemId)) return;

            OnStratagemSelected?.Invoke(_selectedStratagemId);
        }

        private void OnCancelClicked()
        {
            OnCancelled?.Invoke();
            Hide();
        }

        #endregion
    }
}
