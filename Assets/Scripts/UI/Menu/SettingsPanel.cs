using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThirtySixStratagems.UI.Menu
{
    /// <summary>
    /// 設定パネル
    /// ゲーム設定の変更を管理
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private MainMenuUI _mainMenu;

        [Header("オーディオ設定")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _seVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterVolumeText;
        [SerializeField] private TextMeshProUGUI _bgmVolumeText;
        [SerializeField] private TextMeshProUGUI _seVolumeText;

        [Header("画面設定")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Dropdown _qualityDropdown;

        [Header("ゲーム設定")]
        [SerializeField] private Slider _gameSpeedSlider;
        [SerializeField] private TextMeshProUGUI _gameSpeedText;
        [SerializeField] private Toggle _autosaveToggle;
        [SerializeField] private Toggle _showTutorialToggle;
        [SerializeField] private TMP_Dropdown _difficultyDropdown;

        [Header("言語設定")]
        [SerializeField] private TMP_Dropdown _languageDropdown;

        [Header("ボタン")]
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _backButton;

        // 設定値
        private GameSettings _currentSettings;
        private GameSettings _pendingSettings;

        private void Awake()
        {
            SetupButtons();
            SetupSliders();
            SetupDropdowns();
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
            ApplySettingsToUI();
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_applyButton != null)
                _applyButton.onClick.AddListener(OnApplyClicked);

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        /// <summary>
        /// スライダーの設定
        /// </summary>
        private void SetupSliders()
        {
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.minValue = 0f;
                _masterVolumeSlider.maxValue = 1f;
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.minValue = 0f;
                _bgmVolumeSlider.maxValue = 1f;
                _bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (_seVolumeSlider != null)
            {
                _seVolumeSlider.minValue = 0f;
                _seVolumeSlider.maxValue = 1f;
                _seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);
            }

            if (_gameSpeedSlider != null)
            {
                _gameSpeedSlider.minValue = 0.5f;
                _gameSpeedSlider.maxValue = 2f;
                _gameSpeedSlider.onValueChanged.AddListener(OnGameSpeedChanged);
            }
        }

        /// <summary>
        /// ドロップダウンの設定
        /// </summary>
        private void SetupDropdowns()
        {
            // 解像度ドロップダウン
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.ClearOptions();
                var resolutions = new System.Collections.Generic.List<string>
                {
                    "1280 x 720",
                    "1920 x 1080",
                    "2560 x 1440",
                    "3840 x 2160"
                };
                _resolutionDropdown.AddOptions(resolutions);
            }

            // 品質ドロップダウン
            if (_qualityDropdown != null)
            {
                _qualityDropdown.ClearOptions();
                var qualities = new System.Collections.Generic.List<string>
                {
                    "低",
                    "中",
                    "高",
                    "最高"
                };
                _qualityDropdown.AddOptions(qualities);
            }

            // 難易度ドロップダウン
            if (_difficultyDropdown != null)
            {
                _difficultyDropdown.ClearOptions();
                var difficulties = new System.Collections.Generic.List<string>
                {
                    "易しい",
                    "普通",
                    "難しい",
                    "非常に難しい"
                };
                _difficultyDropdown.AddOptions(difficulties);
            }

            // 言語ドロップダウン
            if (_languageDropdown != null)
            {
                _languageDropdown.ClearOptions();
                var languages = new System.Collections.Generic.List<string>
                {
                    "日本語",
                    "English",
                    "中文"
                };
                _languageDropdown.AddOptions(languages);
            }
        }

        #endregion

        #region Settings Management

        /// <summary>
        /// 現在の設定を読み込み
        /// </summary>
        private void LoadCurrentSettings()
        {
            _currentSettings = GameSettings.Load();
            _pendingSettings = _currentSettings.Clone();
        }

        /// <summary>
        /// 設定をUIに適用
        /// </summary>
        private void ApplySettingsToUI()
        {
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.value = _pendingSettings.MasterVolume;

            if (_bgmVolumeSlider != null)
                _bgmVolumeSlider.value = _pendingSettings.BGMVolume;

            if (_seVolumeSlider != null)
                _seVolumeSlider.value = _pendingSettings.SEVolume;

            if (_gameSpeedSlider != null)
                _gameSpeedSlider.value = _pendingSettings.GameSpeed;

            if (_fullscreenToggle != null)
                _fullscreenToggle.isOn = _pendingSettings.Fullscreen;

            if (_autosaveToggle != null)
                _autosaveToggle.isOn = _pendingSettings.Autosave;

            if (_showTutorialToggle != null)
                _showTutorialToggle.isOn = _pendingSettings.ShowTutorial;

            if (_resolutionDropdown != null)
                _resolutionDropdown.value = _pendingSettings.ResolutionIndex;

            if (_qualityDropdown != null)
                _qualityDropdown.value = _pendingSettings.QualityIndex;

            if (_difficultyDropdown != null)
                _difficultyDropdown.value = _pendingSettings.DifficultyIndex;

            if (_languageDropdown != null)
                _languageDropdown.value = _pendingSettings.LanguageIndex;

            UpdateVolumeTexts();
            UpdateGameSpeedText();
        }

        /// <summary>
        /// UIから設定を取得
        /// </summary>
        private void GetSettingsFromUI()
        {
            if (_masterVolumeSlider != null)
                _pendingSettings.MasterVolume = _masterVolumeSlider.value;

            if (_bgmVolumeSlider != null)
                _pendingSettings.BGMVolume = _bgmVolumeSlider.value;

            if (_seVolumeSlider != null)
                _pendingSettings.SEVolume = _seVolumeSlider.value;

            if (_gameSpeedSlider != null)
                _pendingSettings.GameSpeed = _gameSpeedSlider.value;

            if (_fullscreenToggle != null)
                _pendingSettings.Fullscreen = _fullscreenToggle.isOn;

            if (_autosaveToggle != null)
                _pendingSettings.Autosave = _autosaveToggle.isOn;

            if (_showTutorialToggle != null)
                _pendingSettings.ShowTutorial = _showTutorialToggle.isOn;

            if (_resolutionDropdown != null)
                _pendingSettings.ResolutionIndex = _resolutionDropdown.value;

            if (_qualityDropdown != null)
                _pendingSettings.QualityIndex = _qualityDropdown.value;

            if (_difficultyDropdown != null)
                _pendingSettings.DifficultyIndex = _difficultyDropdown.value;

            if (_languageDropdown != null)
                _pendingSettings.LanguageIndex = _languageDropdown.value;
        }

        #endregion

        #region Slider Handlers

        private void OnMasterVolumeChanged(float value)
        {
            _pendingSettings.MasterVolume = value;
            UpdateVolumeTexts();
            AudioListener.volume = value;
        }

        private void OnBGMVolumeChanged(float value)
        {
            _pendingSettings.BGMVolume = value;
            UpdateVolumeTexts();
        }

        private void OnSEVolumeChanged(float value)
        {
            _pendingSettings.SEVolume = value;
            UpdateVolumeTexts();
        }

        private void OnGameSpeedChanged(float value)
        {
            _pendingSettings.GameSpeed = value;
            UpdateGameSpeedText();
        }

        private void UpdateVolumeTexts()
        {
            if (_masterVolumeText != null)
                _masterVolumeText.text = $"{Mathf.RoundToInt(_pendingSettings.MasterVolume * 100)}%";

            if (_bgmVolumeText != null)
                _bgmVolumeText.text = $"{Mathf.RoundToInt(_pendingSettings.BGMVolume * 100)}%";

            if (_seVolumeText != null)
                _seVolumeText.text = $"{Mathf.RoundToInt(_pendingSettings.SEVolume * 100)}%";
        }

        private void UpdateGameSpeedText()
        {
            if (_gameSpeedText != null)
                _gameSpeedText.text = $"x{_pendingSettings.GameSpeed:F1}";
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 適用ボタンクリック
        /// </summary>
        private void OnApplyClicked()
        {
            GetSettingsFromUI();
            _currentSettings = _pendingSettings.Clone();
            _currentSettings.Save();
            ApplySettings(_currentSettings);

            Debug.Log("Settings applied and saved");
        }

        /// <summary>
        /// リセットボタンクリック
        /// </summary>
        private void OnResetClicked()
        {
            _pendingSettings = GameSettings.CreateDefault();
            ApplySettingsToUI();
        }

        /// <summary>
        /// 戻るボタンクリック
        /// </summary>
        private void OnBackClicked()
        {
            _mainMenu?.ShowMainPanel();
        }

        /// <summary>
        /// 設定を実際に適用
        /// </summary>
        private void ApplySettings(GameSettings settings)
        {
            // 音量
            AudioListener.volume = settings.MasterVolume;

            // 解像度
            var resolutions = new (int, int)[]
            {
                (1280, 720),
                (1920, 1080),
                (2560, 1440),
                (3840, 2160)
            };

            if (settings.ResolutionIndex >= 0 && settings.ResolutionIndex < resolutions.Length)
            {
                var res = resolutions[settings.ResolutionIndex];
                Screen.SetResolution(res.Item1, res.Item2, settings.Fullscreen);
            }

            // 品質
            QualitySettings.SetQualityLevel(settings.QualityIndex);
        }

        #endregion
    }

    /// <summary>
    /// ゲーム設定
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        // オーディオ
        public float MasterVolume = 1f;
        public float BGMVolume = 0.8f;
        public float SEVolume = 1f;

        // 画面
        public int ResolutionIndex = 1;
        public bool Fullscreen = true;
        public int QualityIndex = 2;

        // ゲーム
        public float GameSpeed = 1f;
        public bool Autosave = true;
        public bool ShowTutorial = true;
        public int DifficultyIndex = 1;

        // 言語
        public int LanguageIndex = 0;

        private const string SaveKey = "GameSettings";

        /// <summary>
        /// デフォルト設定を作成
        /// </summary>
        public static GameSettings CreateDefault()
        {
            return new GameSettings();
        }

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public static GameSettings Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                return JsonUtility.FromJson<GameSettings>(json) ?? CreateDefault();
            }
            return CreateDefault();
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// クローンを作成
        /// </summary>
        public GameSettings Clone()
        {
            return new GameSettings
            {
                MasterVolume = this.MasterVolume,
                BGMVolume = this.BGMVolume,
                SEVolume = this.SEVolume,
                ResolutionIndex = this.ResolutionIndex,
                Fullscreen = this.Fullscreen,
                QualityIndex = this.QualityIndex,
                GameSpeed = this.GameSpeed,
                Autosave = this.Autosave,
                ShowTutorial = this.ShowTutorial,
                DifficultyIndex = this.DifficultyIndex,
                LanguageIndex = this.LanguageIndex
            };
        }
    }
}
