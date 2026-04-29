using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Campaign;

namespace ThirtySixStratagems.UI.Campaign
{
    /// <summary>
    /// ゲームオーバーパネル
    /// 勝利・敗北時の結果画面
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        [Header("共通")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _resultTitleText;
        [SerializeField] private TextMeshProUGUI _resultDescriptionText;
        [SerializeField] private Image _resultIcon;

        [Header("統計")]
        [SerializeField] private TextMeshProUGUI _turnsPlayedText;
        [SerializeField] private TextMeshProUGUI _battlesWonText;
        [SerializeField] private TextMeshProUGUI _territoriesConqueredText;
        [SerializeField] private TextMeshProUGUI _stratagemsUsedText;
        [SerializeField] private TextMeshProUGUI _maxWinStreakText;
        [SerializeField] private TextMeshProUGUI _totalScoreText;

        [Header("目標")]
        [SerializeField] private Transform _objectiveListContent;
        [SerializeField] private GameObject _objectiveItemPrefab;

        [Header("ボタン")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("ビジュアル")]
        [SerializeField] private Sprite _victoryIcon;
        [SerializeField] private Sprite _defeatIcon;
        [SerializeField] private Color _victoryColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color _defeatColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("アニメーション")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _statsRevealDelay = 0.5f;

        // イベント
        public event Action OnContinueClicked;
        public event Action OnRetryClicked;
        public event Action OnMainMenuClicked;

        private void Awake()
        {
            SetupButtons();
            Hide();
        }

        private void OnEnable()
        {
            if (VictoryConditionSystem.Instance != null)
            {
                VictoryConditionSystem.Instance.OnVictoryAchieved += OnVictory;
                VictoryConditionSystem.Instance.OnDefeatOccurred += OnDefeat;
            }
        }

        private void OnDisable()
        {
            if (VictoryConditionSystem.Instance != null)
            {
                VictoryConditionSystem.Instance.OnVictoryAchieved -= OnVictory;
                VictoryConditionSystem.Instance.OnDefeatOccurred -= OnDefeat;
            }
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());

            if (_retryButton != null)
                _retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(() =>
                {
                    OnMainMenuClicked?.Invoke();
                    Hide();
                });
        }

        #endregion

        #region Show Methods

        /// <summary>
        /// 勝利画面を表示
        /// </summary>
        public void ShowVictory(VictoryInfo info, CampaignStatistics stats)
        {
            Show();

            if (_resultTitleText != null)
                _resultTitleText.text = "勝利";

            if (_resultDescriptionText != null)
                _resultDescriptionText.text = info.Description;

            if (_resultIcon != null)
            {
                _resultIcon.sprite = _victoryIcon;
                _resultIcon.color = _victoryColor;
            }

            if (_resultTitleText != null)
                _resultTitleText.color = _victoryColor;

            // 続けるボタンは勝利時のみ表示
            if (_continueButton != null)
                _continueButton.gameObject.SetActive(true);

            UpdateStatistics(stats);
            UpdateObjectives();

            // スコア計算
            int score = CalculateScore(stats, true);
            if (_totalScoreText != null)
                _totalScoreText.text = $"総合スコア: {score:N0}";

            PlayAnimation("Victory");
        }

        /// <summary>
        /// 敗北画面を表示
        /// </summary>
        public void ShowDefeat(DefeatInfo info, CampaignStatistics stats)
        {
            Show();

            if (_resultTitleText != null)
                _resultTitleText.text = "敗北";

            if (_resultDescriptionText != null)
                _resultDescriptionText.text = info.Description;

            if (_resultIcon != null)
            {
                _resultIcon.sprite = _defeatIcon;
                _resultIcon.color = _defeatColor;
            }

            if (_resultTitleText != null)
                _resultTitleText.color = _defeatColor;

            // 続けるボタンは敗北時は非表示
            if (_continueButton != null)
                _continueButton.gameObject.SetActive(false);

            UpdateStatistics(stats);
            UpdateObjectives();

            int score = CalculateScore(stats, false);
            if (_totalScoreText != null)
                _totalScoreText.text = $"総合スコア: {score:N0}";

            PlayAnimation("Defeat");
        }

        /// <summary>
        /// 表示
        /// </summary>
        private void Show()
        {
            if (_panel != null)
                _panel.SetActive(true);

            // ゲーム一時停止
            Time.timeScale = 0f;
        }

        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
        }

        #endregion

        #region Update Display

        /// <summary>
        /// 統計を更新
        /// </summary>
        private void UpdateStatistics(CampaignStatistics stats)
        {
            if (_turnsPlayedText != null)
                _turnsPlayedText.text = stats.TurnsPlayed.ToString();

            if (_battlesWonText != null)
                _battlesWonText.text = $"{stats.BattlesWon} / {stats.TotalBattles}";

            if (_territoriesConqueredText != null)
                _territoriesConqueredText.text = stats.TerritoriesConquered.ToString();

            if (_stratagemsUsedText != null)
                _stratagemsUsedText.text = $"{stats.StratagemsSucceeded} / {stats.StratagemsUsed}";

            if (_maxWinStreakText != null)
                _maxWinStreakText.text = stats.MaxWinStreak.ToString();
        }

        /// <summary>
        /// 目標を更新
        /// </summary>
        private void UpdateObjectives()
        {
            if (_objectiveListContent == null || _objectiveItemPrefab == null) return;

            // 既存のアイテムをクリア
            foreach (Transform child in _objectiveListContent)
            {
                Destroy(child.gameObject);
            }

            var campaign = CampaignManager.Instance?.CurrentCampaign;
            if (campaign == null) return;

            foreach (var objective in campaign.Objectives)
            {
                var item = Instantiate(_objectiveItemPrefab, _objectiveListContent);
                SetupObjectiveItem(item, objective);
            }
        }

        /// <summary>
        /// 目標アイテムをセットアップ
        /// </summary>
        private void SetupObjectiveItem(GameObject item, CampaignObjective objective)
        {
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length > 0)
                texts[0].text = objective.Title;

            if (texts.Length > 1)
            {
                string statusText = objective.Status switch
                {
                    ObjectiveStatus.Completed => "達成",
                    ObjectiveStatus.Failed => "失敗",
                    _ => $"{objective.CurrentValue}/{objective.TargetValue}"
                };
                texts[1].text = statusText;
            }

            // 色分け
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                image.color = objective.Status switch
                {
                    ObjectiveStatus.Completed => new Color(0.3f, 0.8f, 0.3f, 0.3f),
                    ObjectiveStatus.Failed => new Color(0.8f, 0.3f, 0.3f, 0.3f),
                    _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
                };
            }
        }

        #endregion

        #region Score Calculation

        /// <summary>
        /// スコアを計算
        /// </summary>
        private int CalculateScore(CampaignStatistics stats, bool isVictory)
        {
            int score = 0;

            // 基本点
            score += isVictory ? 10000 : 0;

            // 戦闘勝利ボーナス
            score += stats.BattlesWon * 100;

            // 領地制覇ボーナス
            score += stats.TerritoriesConquered * 500;

            // 計略成功ボーナス
            score += stats.StratagemsSucceeded * 200;

            // 連勝ボーナス
            score += stats.MaxWinStreak * 300;

            // 効率ボーナス（少ないターン数でクリア）
            if (isVictory && stats.TurnsPlayed > 0)
            {
                int efficiencyBonus = Mathf.Max(0, 5000 - (stats.TurnsPlayed * 50));
                score += efficiencyBonus;
            }

            // ユニーク計略ボーナス
            score += stats.UniqueStratagemsUsed.Count * 150;

            return score;
        }

        #endregion

        #region Animation

        /// <summary>
        /// アニメーションを再生
        /// </summary>
        private void PlayAnimation(string animationName)
        {
            if (_animator != null)
            {
                _animator.Play(animationName);
            }
        }

        #endregion

        #region Event Handlers

        private void OnVictory(VictoryInfo info)
        {
            var stats = CampaignManager.Instance?.CurrentCampaign?.Statistics;
            if (stats != null)
            {
                ShowVictory(info, stats);
            }
        }

        private void OnDefeat(DefeatInfo info)
        {
            var stats = CampaignManager.Instance?.CurrentCampaign?.Statistics;
            if (stats != null)
            {
                ShowDefeat(info, stats);
            }
        }

        #endregion
    }
}
