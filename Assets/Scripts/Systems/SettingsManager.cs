using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// 設定管理システム
    /// ゲーム設定の一元管理
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        // 設定データ
        private GameSettings _currentSettings;
        private GameSettings _defaultSettings;

        // イベント
        public event Action OnSettingsLoaded;
        public event Action OnSettingsSaved;
        public event Action<string, object> OnSettingChanged;

        /// <summary>
        /// 現在の設定
        /// </summary>
        public GameSettings CurrentSettings => _currentSettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Initialization

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            _defaultSettings = CreateDefaultSettings();
            LoadSettings();
        }

        /// <summary>
        /// デフォルト設定を作成
        /// </summary>
        private GameSettings CreateDefaultSettings()
        {
            return new GameSettings
            {
                // グラフィック
                Graphics = new GraphicsSettings
                {
                    Resolution = new Resolution { width = 1920, height = 1080, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } },
                    Fullscreen = true,
                    VSync = true,
                    QualityLevel = 2,
                    TargetFrameRate = 60,
                    ShowFPS = false
                },

                // オーディオ
                Audio = new AudioSettings
                {
                    MasterVolume = 1f,
                    BGMVolume = 0.8f,
                    SEVolume = 1f,
                    VoiceVolume = 1f,
                    MuteOnFocusLost = true
                },

                // ゲームプレイ
                Gameplay = new GameplaySettings
                {
                    Difficulty = DifficultyLevel.Normal,
                    GameSpeed = 1f,
                    AutoSave = true,
                    AutoSaveInterval = 5,
                    ShowTutorial = true,
                    ConfirmActions = true,
                    ShowDamageNumbers = true,
                    CameraShake = true
                },

                // UI
                UI = new UISettings
                {
                    Language = SystemLanguage.Japanese,
                    UIScale = 1f,
                    ShowTooltips = true,
                    TooltipDelay = 0.5f,
                    NotificationDuration = 3f,
                    ShowMinimap = true
                },

                // 操作
                Controls = new ControlSettings
                {
                    MouseSensitivity = 1f,
                    InvertYAxis = false,
                    EdgeScrolling = true,
                    EdgeScrollSpeed = 10f,
                    DoubleClickTime = 0.3f
                }
            };
        }

        #endregion

        #region Load/Save

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public void LoadSettings()
        {
            _currentSettings = new GameSettings();

            // グラフィック設定
            _currentSettings.Graphics = new GraphicsSettings
            {
                Fullscreen = PlayerPrefs.GetInt("Graphics.Fullscreen", _defaultSettings.Graphics.Fullscreen ? 1 : 0) == 1,
                VSync = PlayerPrefs.GetInt("Graphics.VSync", _defaultSettings.Graphics.VSync ? 1 : 0) == 1,
                QualityLevel = PlayerPrefs.GetInt("Graphics.QualityLevel", _defaultSettings.Graphics.QualityLevel),
                TargetFrameRate = PlayerPrefs.GetInt("Graphics.TargetFrameRate", _defaultSettings.Graphics.TargetFrameRate),
                ShowFPS = PlayerPrefs.GetInt("Graphics.ShowFPS", _defaultSettings.Graphics.ShowFPS ? 1 : 0) == 1
            };

            // 解像度
            int resWidth = PlayerPrefs.GetInt("Graphics.ResolutionWidth", _defaultSettings.Graphics.Resolution.width);
            int resHeight = PlayerPrefs.GetInt("Graphics.ResolutionHeight", _defaultSettings.Graphics.Resolution.height);
            _currentSettings.Graphics.Resolution = new Resolution { width = resWidth, height = resHeight };

            // オーディオ設定
            _currentSettings.Audio = new AudioSettings
            {
                MasterVolume = PlayerPrefs.GetFloat("Audio.MasterVolume", _defaultSettings.Audio.MasterVolume),
                BGMVolume = PlayerPrefs.GetFloat("Audio.BGMVolume", _defaultSettings.Audio.BGMVolume),
                SEVolume = PlayerPrefs.GetFloat("Audio.SEVolume", _defaultSettings.Audio.SEVolume),
                VoiceVolume = PlayerPrefs.GetFloat("Audio.VoiceVolume", _defaultSettings.Audio.VoiceVolume),
                MuteOnFocusLost = PlayerPrefs.GetInt("Audio.MuteOnFocusLost", _defaultSettings.Audio.MuteOnFocusLost ? 1 : 0) == 1
            };

            // ゲームプレイ設定
            _currentSettings.Gameplay = new GameplaySettings
            {
                Difficulty = (DifficultyLevel)PlayerPrefs.GetInt("Gameplay.Difficulty", (int)_defaultSettings.Gameplay.Difficulty),
                GameSpeed = PlayerPrefs.GetFloat("Gameplay.GameSpeed", _defaultSettings.Gameplay.GameSpeed),
                AutoSave = PlayerPrefs.GetInt("Gameplay.AutoSave", _defaultSettings.Gameplay.AutoSave ? 1 : 0) == 1,
                AutoSaveInterval = PlayerPrefs.GetInt("Gameplay.AutoSaveInterval", _defaultSettings.Gameplay.AutoSaveInterval),
                ShowTutorial = PlayerPrefs.GetInt("Gameplay.ShowTutorial", _defaultSettings.Gameplay.ShowTutorial ? 1 : 0) == 1,
                ConfirmActions = PlayerPrefs.GetInt("Gameplay.ConfirmActions", _defaultSettings.Gameplay.ConfirmActions ? 1 : 0) == 1,
                ShowDamageNumbers = PlayerPrefs.GetInt("Gameplay.ShowDamageNumbers", _defaultSettings.Gameplay.ShowDamageNumbers ? 1 : 0) == 1,
                CameraShake = PlayerPrefs.GetInt("Gameplay.CameraShake", _defaultSettings.Gameplay.CameraShake ? 1 : 0) == 1
            };

            // UI設定
            _currentSettings.UI = new UISettings
            {
                Language = (SystemLanguage)PlayerPrefs.GetInt("UI.Language", (int)_defaultSettings.UI.Language),
                UIScale = PlayerPrefs.GetFloat("UI.UIScale", _defaultSettings.UI.UIScale),
                ShowTooltips = PlayerPrefs.GetInt("UI.ShowTooltips", _defaultSettings.UI.ShowTooltips ? 1 : 0) == 1,
                TooltipDelay = PlayerPrefs.GetFloat("UI.TooltipDelay", _defaultSettings.UI.TooltipDelay),
                NotificationDuration = PlayerPrefs.GetFloat("UI.NotificationDuration", _defaultSettings.UI.NotificationDuration),
                ShowMinimap = PlayerPrefs.GetInt("UI.ShowMinimap", _defaultSettings.UI.ShowMinimap ? 1 : 0) == 1
            };

            // 操作設定
            _currentSettings.Controls = new ControlSettings
            {
                MouseSensitivity = PlayerPrefs.GetFloat("Controls.MouseSensitivity", _defaultSettings.Controls.MouseSensitivity),
                InvertYAxis = PlayerPrefs.GetInt("Controls.InvertYAxis", _defaultSettings.Controls.InvertYAxis ? 1 : 0) == 1,
                EdgeScrolling = PlayerPrefs.GetInt("Controls.EdgeScrolling", _defaultSettings.Controls.EdgeScrolling ? 1 : 0) == 1,
                EdgeScrollSpeed = PlayerPrefs.GetFloat("Controls.EdgeScrollSpeed", _defaultSettings.Controls.EdgeScrollSpeed),
                DoubleClickTime = PlayerPrefs.GetFloat("Controls.DoubleClickTime", _defaultSettings.Controls.DoubleClickTime)
            };

            ApplySettings();
            OnSettingsLoaded?.Invoke();

            Debug.Log("Settings loaded");
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public void SaveSettings()
        {
            // グラフィック設定
            PlayerPrefs.SetInt("Graphics.Fullscreen", _currentSettings.Graphics.Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("Graphics.VSync", _currentSettings.Graphics.VSync ? 1 : 0);
            PlayerPrefs.SetInt("Graphics.QualityLevel", _currentSettings.Graphics.QualityLevel);
            PlayerPrefs.SetInt("Graphics.TargetFrameRate", _currentSettings.Graphics.TargetFrameRate);
            PlayerPrefs.SetInt("Graphics.ShowFPS", _currentSettings.Graphics.ShowFPS ? 1 : 0);
            PlayerPrefs.SetInt("Graphics.ResolutionWidth", _currentSettings.Graphics.Resolution.width);
            PlayerPrefs.SetInt("Graphics.ResolutionHeight", _currentSettings.Graphics.Resolution.height);

            // オーディオ設定
            PlayerPrefs.SetFloat("Audio.MasterVolume", _currentSettings.Audio.MasterVolume);
            PlayerPrefs.SetFloat("Audio.BGMVolume", _currentSettings.Audio.BGMVolume);
            PlayerPrefs.SetFloat("Audio.SEVolume", _currentSettings.Audio.SEVolume);
            PlayerPrefs.SetFloat("Audio.VoiceVolume", _currentSettings.Audio.VoiceVolume);
            PlayerPrefs.SetInt("Audio.MuteOnFocusLost", _currentSettings.Audio.MuteOnFocusLost ? 1 : 0);

            // ゲームプレイ設定
            PlayerPrefs.SetInt("Gameplay.Difficulty", (int)_currentSettings.Gameplay.Difficulty);
            PlayerPrefs.SetFloat("Gameplay.GameSpeed", _currentSettings.Gameplay.GameSpeed);
            PlayerPrefs.SetInt("Gameplay.AutoSave", _currentSettings.Gameplay.AutoSave ? 1 : 0);
            PlayerPrefs.SetInt("Gameplay.AutoSaveInterval", _currentSettings.Gameplay.AutoSaveInterval);
            PlayerPrefs.SetInt("Gameplay.ShowTutorial", _currentSettings.Gameplay.ShowTutorial ? 1 : 0);
            PlayerPrefs.SetInt("Gameplay.ConfirmActions", _currentSettings.Gameplay.ConfirmActions ? 1 : 0);
            PlayerPrefs.SetInt("Gameplay.ShowDamageNumbers", _currentSettings.Gameplay.ShowDamageNumbers ? 1 : 0);
            PlayerPrefs.SetInt("Gameplay.CameraShake", _currentSettings.Gameplay.CameraShake ? 1 : 0);

            // UI設定
            PlayerPrefs.SetInt("UI.Language", (int)_currentSettings.UI.Language);
            PlayerPrefs.SetFloat("UI.UIScale", _currentSettings.UI.UIScale);
            PlayerPrefs.SetInt("UI.ShowTooltips", _currentSettings.UI.ShowTooltips ? 1 : 0);
            PlayerPrefs.SetFloat("UI.TooltipDelay", _currentSettings.UI.TooltipDelay);
            PlayerPrefs.SetFloat("UI.NotificationDuration", _currentSettings.UI.NotificationDuration);
            PlayerPrefs.SetInt("UI.ShowMinimap", _currentSettings.UI.ShowMinimap ? 1 : 0);

            // 操作設定
            PlayerPrefs.SetFloat("Controls.MouseSensitivity", _currentSettings.Controls.MouseSensitivity);
            PlayerPrefs.SetInt("Controls.InvertYAxis", _currentSettings.Controls.InvertYAxis ? 1 : 0);
            PlayerPrefs.SetInt("Controls.EdgeScrolling", _currentSettings.Controls.EdgeScrolling ? 1 : 0);
            PlayerPrefs.SetFloat("Controls.EdgeScrollSpeed", _currentSettings.Controls.EdgeScrollSpeed);
            PlayerPrefs.SetFloat("Controls.DoubleClickTime", _currentSettings.Controls.DoubleClickTime);

            PlayerPrefs.Save();
            OnSettingsSaved?.Invoke();

            Debug.Log("Settings saved");
        }

        /// <summary>
        /// 設定をデフォルトにリセット
        /// </summary>
        public void ResetToDefaults()
        {
            _currentSettings = CreateDefaultSettings();
            ApplySettings();
            SaveSettings();

            Debug.Log("Settings reset to defaults");
        }

        #endregion

        #region Apply Settings

        /// <summary>
        /// 設定を適用
        /// </summary>
        public void ApplySettings()
        {
            ApplyGraphicsSettings();
            ApplyAudioSettings();
            ApplyUISettings();
        }

        /// <summary>
        /// グラフィック設定を適用
        /// </summary>
        private void ApplyGraphicsSettings()
        {
            var graphics = _currentSettings.Graphics;

            // 解像度とフルスクリーン
            Screen.SetResolution(graphics.Resolution.width, graphics.Resolution.height,
                graphics.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            // VSync
            QualitySettings.vSyncCount = graphics.VSync ? 1 : 0;

            // 品質レベル
            QualitySettings.SetQualityLevel(graphics.QualityLevel);

            // フレームレート
            Application.targetFrameRate = graphics.TargetFrameRate;
        }

        /// <summary>
        /// オーディオ設定を適用
        /// </summary>
        private void ApplyAudioSettings()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = _currentSettings.Audio.MasterVolume;
                AudioManager.Instance.BGMVolume = _currentSettings.Audio.BGMVolume;
                AudioManager.Instance.SEVolume = _currentSettings.Audio.SEVolume;
            }
        }

        /// <summary>
        /// UI設定を適用
        /// </summary>
        private void ApplyUISettings()
        {
            if (LocalizationSystem.Instance != null)
            {
                LocalizationSystem.Instance.SetLanguage(_currentSettings.UI.Language);
            }
        }

        #endregion

        #region Setting Getters/Setters

        /// <summary>
        /// マスター音量を設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _currentSettings.Audio.MasterVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            OnSettingChanged?.Invoke("Audio.MasterVolume", volume);
        }

        /// <summary>
        /// BGM音量を設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            _currentSettings.Audio.BGMVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            OnSettingChanged?.Invoke("Audio.BGMVolume", volume);
        }

        /// <summary>
        /// SE音量を設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            _currentSettings.Audio.SEVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            OnSettingChanged?.Invoke("Audio.SEVolume", volume);
        }

        /// <summary>
        /// 言語を設定
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            _currentSettings.UI.Language = language;
            ApplyUISettings();
            OnSettingChanged?.Invoke("UI.Language", language);
        }

        /// <summary>
        /// 難易度を設定
        /// </summary>
        public void SetDifficulty(DifficultyLevel difficulty)
        {
            _currentSettings.Gameplay.Difficulty = difficulty;
            OnSettingChanged?.Invoke("Gameplay.Difficulty", difficulty);
        }

        /// <summary>
        /// フルスクリーンを設定
        /// </summary>
        public void SetFullscreen(bool fullscreen)
        {
            _currentSettings.Graphics.Fullscreen = fullscreen;
            ApplyGraphicsSettings();
            OnSettingChanged?.Invoke("Graphics.Fullscreen", fullscreen);
        }

        /// <summary>
        /// 解像度を設定
        /// </summary>
        public void SetResolution(Resolution resolution)
        {
            _currentSettings.Graphics.Resolution = resolution;
            ApplyGraphicsSettings();
            OnSettingChanged?.Invoke("Graphics.Resolution", resolution);
        }

        /// <summary>
        /// 品質レベルを設定
        /// </summary>
        public void SetQualityLevel(int level)
        {
            _currentSettings.Graphics.QualityLevel = level;
            ApplyGraphicsSettings();
            OnSettingChanged?.Invoke("Graphics.QualityLevel", level);
        }

        #endregion

        #region Utility

        /// <summary>
        /// 利用可能な解像度を取得
        /// </summary>
        public Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions;
        }

        /// <summary>
        /// 品質レベル名を取得
        /// </summary>
        public string[] GetQualityLevelNames()
        {
            return QualitySettings.names;
        }

        /// <summary>
        /// 難易度名を取得
        /// </summary>
        public string GetDifficultyName(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => "初級",
                DifficultyLevel.Normal => "中級",
                DifficultyLevel.Hard => "上級",
                DifficultyLevel.Expert => "達人",
                _ => "不明"
            };
        }

        #endregion
    }

    #region Settings Data Classes

    /// <summary>
    /// ゲーム設定
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public GraphicsSettings Graphics;
        public AudioSettings Audio;
        public GameplaySettings Gameplay;
        public UISettings UI;
        public ControlSettings Controls;
    }

    /// <summary>
    /// グラフィック設定
    /// </summary>
    [Serializable]
    public class GraphicsSettings
    {
        public Resolution Resolution;
        public bool Fullscreen;
        public bool VSync;
        public int QualityLevel;
        public int TargetFrameRate;
        public bool ShowFPS;
    }

    /// <summary>
    /// オーディオ設定
    /// </summary>
    [Serializable]
    public class AudioSettings
    {
        public float MasterVolume;
        public float BGMVolume;
        public float SEVolume;
        public float VoiceVolume;
        public bool MuteOnFocusLost;
    }

    /// <summary>
    /// ゲームプレイ設定
    /// </summary>
    [Serializable]
    public class GameplaySettings
    {
        public DifficultyLevel Difficulty;
        public float GameSpeed;
        public bool AutoSave;
        public int AutoSaveInterval;
        public bool ShowTutorial;
        public bool ConfirmActions;
        public bool ShowDamageNumbers;
        public bool CameraShake;
    }

    /// <summary>
    /// UI設定
    /// </summary>
    [Serializable]
    public class UISettings
    {
        public SystemLanguage Language;
        public float UIScale;
        public bool ShowTooltips;
        public float TooltipDelay;
        public float NotificationDuration;
        public bool ShowMinimap;
    }

    /// <summary>
    /// 操作設定
    /// </summary>
    [Serializable]
    public class ControlSettings
    {
        public float MouseSensitivity;
        public bool InvertYAxis;
        public bool EdgeScrolling;
        public float EdgeScrollSpeed;
        public float DoubleClickTime;
    }

    /// <summary>
    /// 難易度レベル
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    #endregion
}
