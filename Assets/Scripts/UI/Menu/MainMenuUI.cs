using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.UI.Menu
{
    /// <summary>
    /// メインメニューUI
    /// ゲーム開始時のメインメニュー画面を管理
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("パネル")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _scenarioSelectPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _stratagemLibraryPanel;
        [SerializeField] private GameObject _creditsPanel;

        [Header("メインメニューボタン")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _stratagemLibraryButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _exitButton;

        [Header("タイトル")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private TextMeshProUGUI _versionText;

        [Header("BGM")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioClip _menuBGM;

        // イベント
        public event Action OnNewGameRequested;
        public event Action OnContinueRequested;
        public event Action OnStratagemLibraryRequested;

        private void Awake()
        {
            SetupButtons();
            SetupTexts();
        }

        private void Start()
        {
            ShowMainPanel();
            CheckContinueAvailable();
            PlayBGM();
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(OnNewGameClicked);

            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinueClicked);

            if (_stratagemLibraryButton != null)
                _stratagemLibraryButton.onClick.AddListener(OnStratagemLibraryClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_creditsButton != null)
                _creditsButton.onClick.AddListener(OnCreditsClicked);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);
        }

        /// <summary>
        /// テキストの設定
        /// </summary>
        private void SetupTexts()
        {
            if (_titleText != null)
                _titleText.text = "三十六計";

            if (_subtitleText != null)
                _subtitleText.text = "The Thirty-Six Stratagems";

            if (_versionText != null)
                _versionText.text = $"Version {Application.version}";
        }

        /// <summary>
        /// 続きから再開可能かチェック
        /// </summary>
        private void CheckContinueAvailable()
        {
            bool hasSaveData = SaveLoadManager.Instance?.HasSaveData() ?? false;

            if (_continueButton != null)
            {
                _continueButton.interactable = hasSaveData;
            }
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        private void PlayBGM()
        {
            if (_bgmSource != null && _menuBGM != null)
            {
                _bgmSource.clip = _menuBGM;
                _bgmSource.loop = true;
                _bgmSource.Play();
            }
        }

        #endregion

        #region Panel Management

        /// <summary>
        /// メインパネルを表示
        /// </summary>
        public void ShowMainPanel()
        {
            HideAllPanels();
            if (_mainPanel != null)
                _mainPanel.SetActive(true);
        }

        /// <summary>
        /// シナリオ選択パネルを表示
        /// </summary>
        public void ShowScenarioSelectPanel()
        {
            HideAllPanels();
            if (_scenarioSelectPanel != null)
                _scenarioSelectPanel.SetActive(true);
        }

        /// <summary>
        /// 設定パネルを表示
        /// </summary>
        public void ShowSettingsPanel()
        {
            HideAllPanels();
            if (_settingsPanel != null)
                _settingsPanel.SetActive(true);
        }

        /// <summary>
        /// 計略図鑑パネルを表示
        /// </summary>
        public void ShowStratagemLibraryPanel()
        {
            HideAllPanels();
            if (_stratagemLibraryPanel != null)
                _stratagemLibraryPanel.SetActive(true);
        }

        /// <summary>
        /// クレジットパネルを表示
        /// </summary>
        public void ShowCreditsPanel()
        {
            HideAllPanels();
            if (_creditsPanel != null)
                _creditsPanel.SetActive(true);
        }

        /// <summary>
        /// 全パネルを非表示
        /// </summary>
        private void HideAllPanels()
        {
            if (_mainPanel != null) _mainPanel.SetActive(false);
            if (_scenarioSelectPanel != null) _scenarioSelectPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_stratagemLibraryPanel != null) _stratagemLibraryPanel.SetActive(false);
            if (_creditsPanel != null) _creditsPanel.SetActive(false);
        }

        #endregion

        #region Button Handlers

        private void OnNewGameClicked()
        {
            ShowScenarioSelectPanel();
            OnNewGameRequested?.Invoke();
        }

        private void OnContinueClicked()
        {
            OnContinueRequested?.Invoke();
            LoadSavedGame();
        }

        private void OnStratagemLibraryClicked()
        {
            ShowStratagemLibraryPanel();
            OnStratagemLibraryRequested?.Invoke();
        }

        private void OnSettingsClicked()
        {
            ShowSettingsPanel();
        }

        private void OnCreditsClicked()
        {
            ShowCreditsPanel();
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Game Flow

        /// <summary>
        /// セーブデータを読み込み
        /// </summary>
        private void LoadSavedGame()
        {
            bool success = SaveLoadManager.Instance?.LoadGame() ?? false;

            if (success)
            {
                Debug.Log("Save data loaded successfully");
                StartGame();
            }
            else
            {
                Debug.LogWarning("Failed to load save data");
                ShowMainPanel();
            }
        }

        /// <summary>
        /// 新規ゲームを開始
        /// </summary>
        public void StartNewGame(string scenarioId)
        {
            Debug.Log($"Starting new game with scenario: {scenarioId}");
            GameManager.Instance?.StartNewGame(scenarioId);
            StartGame();
        }

        /// <summary>
        /// ゲーム開始（シーン遷移等）
        /// </summary>
        private void StartGame()
        {
            // BGM停止
            if (_bgmSource != null)
            {
                _bgmSource.Stop();
            }

            // メインメニューを非表示にしてゲームUIを表示
            gameObject.SetActive(false);

            // GameManagerにゲーム開始を通知
            EventBus.GameStarted();
        }

        #endregion
    }
}
