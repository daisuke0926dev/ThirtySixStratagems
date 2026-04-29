using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.UI.Character
{
    /// <summary>
    /// 武将情報パネル
    /// 武将の詳細情報を表示
    /// </summary>
    public class CharacterInfoPanel : MonoBehaviour
    {
        public static CharacterInfoPanel Instance { get; private set; }

        [Header("基本情報")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _factionText;
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private Image _portraitImage;

        [Header("能力値")]
        [SerializeField] private TextMeshProUGUI _strengthText;
        [SerializeField] private TextMeshProUGUI _intelligenceText;
        [SerializeField] private TextMeshProUGUI _leadershipText;
        [SerializeField] private TextMeshProUGUI _politicsText;
        [SerializeField] private TextMeshProUGUI _charmText;
        [SerializeField] private Slider _strengthSlider;
        [SerializeField] private Slider _intelligenceSlider;
        [SerializeField] private Slider _leadershipSlider;
        [SerializeField] private Slider _politicsSlider;
        [SerializeField] private Slider _charmSlider;

        [Header("ステータス")]
        [SerializeField] private TextMeshProUGUI _loyaltyText;
        [SerializeField] private Slider _loyaltySlider;
        [SerializeField] private TextMeshProUGUI _ageText;
        [SerializeField] private TextMeshProUGUI _experienceText;

        [Header("所属")]
        [SerializeField] private TextMeshProUGUI _locationText;
        [SerializeField] private TextMeshProUGUI _commandingText;

        [Header("スキル")]
        [SerializeField] private Transform _skillListContent;
        [SerializeField] private GameObject _skillItemPrefab;

        [Header("アクションボタン")]
        [SerializeField] private Button _appointButton;
        [SerializeField] private Button _assignButton;
        [SerializeField] private Button _rewardButton;
        [SerializeField] private Button _dismissButton;

        [Header("パネル")]
        [SerializeField] private Button _closeButton;

        // 状態
        private string _selectedCharacterId;

        // イベント
        public event Action<string> OnCharacterSelected;
        public event Action OnAppointRequested;
        public event Action OnAssignRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            SetupButtons();
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            if (_appointButton != null)
                _appointButton.onClick.AddListener(OnAppointClicked);

            if (_assignButton != null)
                _assignButton.onClick.AddListener(OnAssignClicked);

            if (_rewardButton != null)
                _rewardButton.onClick.AddListener(OnRewardClicked);

            if (_dismissButton != null)
                _dismissButton.onClick.AddListener(OnDismissClicked);
        }

        #endregion

        #region Display

        /// <summary>
        /// 武将情報を表示
        /// </summary>
        public void ShowCharacter(string characterId)
        {
            _selectedCharacterId = characterId;
            gameObject.SetActive(true);

            var character = GameManager.Instance?.GetCharacter(characterId);
            if (character == null)
            {
                Close();
                return;
            }

            UpdateBasicInfo(character);
            UpdateStats(character);
            UpdateStatus(character);
            UpdateAffiliation(character);
            UpdateActionButtons(character);

            OnCharacterSelected?.Invoke(characterId);
        }

        /// <summary>
        /// 基本情報を更新
        /// </summary>
        private void UpdateBasicInfo(Data.Models.Character character)
        {
            if (_nameText != null)
                _nameText.text = character.Name;

            var faction = GameManager.Instance?.GetFaction(character.FactionId);
            if (_factionText != null)
                _factionText.text = faction?.Name ?? "無所属";

            if (_roleText != null)
                _roleText.text = GetRoleText(character);

            // TODO: ポートレート画像
        }

        /// <summary>
        /// 役職テキストを取得
        /// </summary>
        private string GetRoleText(Data.Models.Character character)
        {
            var faction = GameManager.Instance?.GetFaction(character.FactionId);

            if (faction != null && faction.LeaderId == character.Id)
                return "君主";

            // TODO: その他の役職判定
            return "武将";
        }

        /// <summary>
        /// 能力値を更新
        /// </summary>
        private void UpdateStats(Data.Models.Character character)
        {
            // 武力
            if (_strengthText != null)
                _strengthText.text = character.Strength.ToString();
            if (_strengthSlider != null)
            {
                _strengthSlider.maxValue = 100;
                _strengthSlider.value = character.Strength;
            }

            // 知力
            if (_intelligenceText != null)
                _intelligenceText.text = character.Intelligence.ToString();
            if (_intelligenceSlider != null)
            {
                _intelligenceSlider.maxValue = 100;
                _intelligenceSlider.value = character.Intelligence;
            }

            // 統率
            if (_leadershipText != null)
                _leadershipText.text = character.Leadership.ToString();
            if (_leadershipSlider != null)
            {
                _leadershipSlider.maxValue = 100;
                _leadershipSlider.value = character.Leadership;
            }

            // 政治
            if (_politicsText != null)
                _politicsText.text = character.Politics.ToString();
            if (_politicsSlider != null)
            {
                _politicsSlider.maxValue = 100;
                _politicsSlider.value = character.Politics;
            }

            // 魅力
            if (_charmText != null)
                _charmText.text = character.Charm.ToString();
            if (_charmSlider != null)
            {
                _charmSlider.maxValue = 100;
                _charmSlider.value = character.Charm;
            }
        }

        /// <summary>
        /// ステータスを更新
        /// </summary>
        private void UpdateStatus(Data.Models.Character character)
        {
            // 忠誠度
            if (_loyaltyText != null)
                _loyaltyText.text = character.Loyalty.ToString();
            if (_loyaltySlider != null)
            {
                _loyaltySlider.maxValue = 100;
                _loyaltySlider.value = character.Loyalty;

                // 忠誠度に応じた色
                var fill = _loyaltySlider.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    if (character.Loyalty >= 80)
                        fill.color = Color.green;
                    else if (character.Loyalty >= 50)
                        fill.color = Color.yellow;
                    else
                        fill.color = Color.red;
                }
            }

            // 年齢
            int currentYear = GameManager.Instance?.CurrentYear ?? 0;
            int age = currentYear - character.BirthYear;
            if (_ageText != null)
                _ageText.text = $"{age}歳";

            // 経験
            if (_experienceText != null)
                _experienceText.text = $"EXP: {character.Experience}";
        }

        /// <summary>
        /// 所属情報を更新
        /// </summary>
        private void UpdateAffiliation(Data.Models.Character character)
        {
            // 所在地
            var territory = GameManager.Instance?.GetTerritory(character.LocationId);
            if (_locationText != null)
                _locationText.text = territory?.Name ?? "不明";

            // 指揮中の軍
            if (_commandingText != null)
            {
                var commandingArmy = FindCommandingArmy(character.Id);
                _commandingText.text = commandingArmy != null
                    ? $"{commandingArmy.Name} ({commandingArmy.SoldierCount}兵)"
                    : "なし";
            }
        }

        /// <summary>
        /// 指揮中の軍を探す
        /// </summary>
        private Army FindCommandingArmy(string characterId)
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var army in GameManager.Instance.GameData.Armies.Values)
            {
                if (army.CommanderId == characterId)
                    return army;
            }
            return null;
        }

        /// <summary>
        /// アクションボタンを更新
        /// </summary>
        private void UpdateActionButtons(Data.Models.Character character)
        {
            var playerFaction = GetPlayerFaction();
            bool isOwn = playerFaction != null && character.FactionId == playerFaction.Id;
            bool isLeader = playerFaction != null && playerFaction.LeaderId == character.Id;

            if (_appointButton != null)
                _appointButton.interactable = isOwn;

            if (_assignButton != null)
                _assignButton.interactable = isOwn;

            if (_rewardButton != null)
                _rewardButton.interactable = isOwn && !isLeader;

            if (_dismissButton != null)
                _dismissButton.interactable = isOwn && !isLeader;
        }

        #endregion

        #region Button Handlers

        private void OnAppointClicked()
        {
            OnAppointRequested?.Invoke();
            Debug.Log($"Appoint character: {_selectedCharacterId}");
        }

        private void OnAssignClicked()
        {
            OnAssignRequested?.Invoke();
            Debug.Log($"Assign character: {_selectedCharacterId}");
        }

        private void OnRewardClicked()
        {
            var character = GameManager.Instance?.GetCharacter(_selectedCharacterId);
            if (character == null) return;

            var playerFaction = GetPlayerFaction();
            if (playerFaction == null || playerFaction.Gold < 100) return;

            // 褒賞を与える
            ResourceManager.Instance?.SpendGold(playerFaction.Id, 100);
            character.Loyalty = Mathf.Min(100, character.Loyalty + 5);

            ShowCharacter(_selectedCharacterId);
        }

        private void OnDismissClicked()
        {
            Debug.Log($"Dismiss character: {_selectedCharacterId}");
            // 確認ダイアログを表示後、解雇処理
        }

        #endregion

        #region Helper

        /// <summary>
        /// 閉じる
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// プレイヤー勢力を取得
        /// </summary>
        private Faction GetPlayerFaction()
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.IsPlayer)
                    return faction;
            }
            return null;
        }

        /// <summary>
        /// 選択中の武将ID
        /// </summary>
        public string SelectedCharacterId => _selectedCharacterId;

        #endregion
    }
}
