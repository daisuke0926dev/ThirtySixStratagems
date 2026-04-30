using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Systems;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// オーディオ設定パネル
    /// 音量スライダーとミュートトグルを提供
    /// </summary>
    public class AudioSettingsPanel : MonoBehaviour
    {
        [Header("マスター音量")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterVolumeLabel;
        [SerializeField] private Toggle _masterMuteToggle;

        [Header("BGM音量")]
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private TextMeshProUGUI _bgmVolumeLabel;
        [SerializeField] private Toggle _bgmMuteToggle;

        [Header("SE音量")]
        [SerializeField] private Slider _seVolumeSlider;
        [SerializeField] private TextMeshProUGUI _seVolumeLabel;
        [SerializeField] private Toggle _seMuteToggle;

        [Header("ボイス音量")]
        [SerializeField] private Slider _voiceVolumeSlider;
        [SerializeField] private TextMeshProUGUI _voiceVolumeLabel;
        [SerializeField] private Toggle _voiceMuteToggle;

        [Header("ボタン")]
        [SerializeField] private Button _testBGMButton;
        [SerializeField] private Button _testSEButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _closeButton;

        // 設定バックアップ（キャンセル用）
        private float _backupMasterVolume;
        private float _backupBGMVolume;
        private float _backupSEVolume;
        private float _backupVoiceVolume;

        private void OnEnable()
        {
            LoadCurrentSettings();
            BackupSettings();
            SetupListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        /// <summary>
        /// 現在の設定を読み込み
        /// </summary>
        private void LoadCurrentSettings()
        {
            if (AudioManager.Instance == null) return;

            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
                UpdateVolumeLabel(_masterVolumeLabel, AudioManager.Instance.MasterVolume);
            }

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.value = AudioManager.Instance.BGMVolume;
                UpdateVolumeLabel(_bgmVolumeLabel, AudioManager.Instance.BGMVolume);
            }

            if (_seVolumeSlider != null)
            {
                _seVolumeSlider.value = AudioManager.Instance.SEVolume;
                UpdateVolumeLabel(_seVolumeLabel, AudioManager.Instance.SEVolume);
            }
        }

        /// <summary>
        /// 設定をバックアップ
        /// </summary>
        private void BackupSettings()
        {
            if (AudioManager.Instance == null) return;

            _backupMasterVolume = AudioManager.Instance.MasterVolume;
            _backupBGMVolume = AudioManager.Instance.BGMVolume;
            _backupSEVolume = AudioManager.Instance.SEVolume;
        }

        /// <summary>
        /// リスナーをセットアップ
        /// </summary>
        private void SetupListeners()
        {
            // スライダー
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (_bgmVolumeSlider != null)
                _bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

            if (_seVolumeSlider != null)
                _seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);

            if (_voiceVolumeSlider != null)
                _voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);

            // ボタン
            if (_testBGMButton != null)
                _testBGMButton.onClick.AddListener(OnTestBGMClicked);

            if (_testSEButton != null)
                _testSEButton.onClick.AddListener(OnTestSEClicked);

            if (_resetButton != null)
                _resetButton.onClick.AddListener(OnResetClicked);

            if (_applyButton != null)
                _applyButton.onClick.AddListener(OnApplyClicked);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        /// <summary>
        /// リスナーを削除
        /// </summary>
        private void RemoveListeners()
        {
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

            if (_bgmVolumeSlider != null)
                _bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

            if (_seVolumeSlider != null)
                _seVolumeSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);

            if (_voiceVolumeSlider != null)
                _voiceVolumeSlider.onValueChanged.RemoveListener(OnVoiceVolumeChanged);

            if (_testBGMButton != null)
                _testBGMButton.onClick.RemoveListener(OnTestBGMClicked);

            if (_testSEButton != null)
                _testSEButton.onClick.RemoveListener(OnTestSEClicked);

            if (_resetButton != null)
                _resetButton.onClick.RemoveListener(OnResetClicked);

            if (_applyButton != null)
                _applyButton.onClick.RemoveListener(OnApplyClicked);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        #region Event Handlers

        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = value;
            }
            UpdateVolumeLabel(_masterVolumeLabel, value);
        }

        private void OnBGMVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.BGMVolume = value;
            }
            UpdateVolumeLabel(_bgmVolumeLabel, value);
        }

        private void OnSEVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SEVolume = value;
            }
            UpdateVolumeLabel(_seVolumeLabel, value);
        }

        private void OnVoiceVolumeChanged(float value)
        {
            UpdateVolumeLabel(_voiceVolumeLabel, value);
        }

        private void OnTestBGMClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(BGMType.Title, false);
            }
        }

        private void OnTestSEClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(SEType.ButtonClick);
            }
        }

        private void OnResetClicked()
        {
            // デフォルト値に戻す
            if (_masterVolumeSlider != null) _masterVolumeSlider.value = 1f;
            if (_bgmVolumeSlider != null) _bgmVolumeSlider.value = 0.8f;
            if (_seVolumeSlider != null) _seVolumeSlider.value = 1f;
            if (_voiceVolumeSlider != null) _voiceVolumeSlider.value = 1f;
        }

        private void OnApplyClicked()
        {
            // 設定を保存（AudioManagerが自動で保存）
            BackupSettings();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(SEType.Confirm);
            }
        }

        private void OnCloseClicked()
        {
            // 変更をキャンセルして元に戻す
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = _backupMasterVolume;
                AudioManager.Instance.BGMVolume = _backupBGMVolume;
                AudioManager.Instance.SEVolume = _backupSEVolume;
            }

            gameObject.SetActive(false);
        }

        #endregion

        /// <summary>
        /// 音量ラベルを更新
        /// </summary>
        private void UpdateVolumeLabel(TextMeshProUGUI label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }
    }
}
