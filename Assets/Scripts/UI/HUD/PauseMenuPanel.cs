using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.UI.HUD
{
    /// <summary>
    /// ポーズメニューパネル
    /// ゲーム中のポーズメニューを管理
    /// </summary>
    public class PauseMenuPanel : MonoBehaviour
    {
        [Header("ボタン")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _exitButton;

        [Header("パネル")]
        [SerializeField] private GameObject _confirmPanel;
        [SerializeField] private TextMeshProUGUI _confirmText;
        [SerializeField] private Button _confirmYesButton;
        [SerializeField] private Button _confirmNoButton;

        [Header("設定パネル")]
        [SerializeField] private GameObject _settingsPanel;

        [Header("セーブ/ロードパネル")]
        [SerializeField] private GameObject _saveLoadPanel;
        [SerializeField] private bool _isSaveMode;

        // イベント
        public event Action OnResumeClicked;
        public event Action OnMainMenuRequested;

        private Action _pendingConfirmAction;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            // ポーズ時にタイムスケールを0に
            Time.timeScale = 0f;

            // 確認パネルを非表示
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);
        }

        private void OnDisable()
        {
            // タイムスケールを戻す
            Time.timeScale = 1f;
        }

        #region Setup

        /// <summary>
        /// ボタンの設定
        /// </summary>
        private void SetupButtons()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeButtonClicked);

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSaveButtonClicked);

            if (_loadButton != null)
                _loadButton.onClick.AddListener(OnLoadButtonClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitButtonClicked);

            if (_confirmYesButton != null)
                _confirmYesButton.onClick.AddListener(OnConfirmYes);

            if (_confirmNoButton != null)
                _confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 再開ボタン
        /// </summary>
        private void OnResumeButtonClicked()
        {
            OnResumeClicked?.Invoke();
            Close();
        }

        /// <summary>
        /// セーブボタン
        /// </summary>
        private void OnSaveButtonClicked()
        {
            SaveGame();
        }

        /// <summary>
        /// ロードボタン
        /// </summary>
        private void OnLoadButtonClicked()
        {
            ShowConfirm("ロードすると現在の進行状況が失われます。\nよろしいですか？", () =>
            {
                LoadGame();
            });
        }

        /// <summary>
        /// 設定ボタン
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(true);
        }

        /// <summary>
        /// メインメニューボタン
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            ShowConfirm("メインメニューに戻ります。\nセーブしていない進行状況は失われます。\nよろしいですか？", () =>
            {
                ReturnToMainMenu();
            });
        }

        /// <summary>
        /// 終了ボタン
        /// </summary>
        private void OnExitButtonClicked()
        {
            ShowConfirm("ゲームを終了します。\nセーブしていない進行状況は失われます。\nよろしいですか？", () =>
            {
                ExitGame();
            });
        }

        #endregion

        #region Confirm Dialog

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        private void ShowConfirm(string message, Action onConfirm)
        {
            _pendingConfirmAction = onConfirm;

            if (_confirmPanel != null)
                _confirmPanel.SetActive(true);

            if (_confirmText != null)
                _confirmText.text = message;
        }

        /// <summary>
        /// 確認：はい
        /// </summary>
        private void OnConfirmYes()
        {
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);

            _pendingConfirmAction?.Invoke();
            _pendingConfirmAction = null;
        }

        /// <summary>
        /// 確認：いいえ
        /// </summary>
        private void OnConfirmNo()
        {
            if (_confirmPanel != null)
                _confirmPanel.SetActive(false);

            _pendingConfirmAction = null;
        }

        #endregion

        #region Actions

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        private void SaveGame()
        {
            bool success = SaveLoadManager.Instance?.SaveGame() ?? false;

            if (success)
            {
                Debug.Log("Game saved successfully");
                ShowTemporaryMessage("セーブしました");
            }
            else
            {
                Debug.LogWarning("Failed to save game");
                ShowTemporaryMessage("セーブに失敗しました");
            }
        }

        /// <summary>
        /// ゲームをロード
        /// </summary>
        private void LoadGame()
        {
            bool success = SaveLoadManager.Instance?.LoadGame() ?? false;

            if (success)
            {
                Debug.Log("Game loaded successfully");
                Close();
            }
            else
            {
                Debug.LogWarning("Failed to load game");
                ShowTemporaryMessage("ロードに失敗しました");
            }
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            OnMainMenuRequested?.Invoke();

            // シーン遷移（将来の実装用）
            // SceneManager.LoadScene("MainMenu");

            Close();
        }

        /// <summary>
        /// ゲームを終了
        /// </summary>
        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 一時メッセージを表示
        /// </summary>
        private void ShowTemporaryMessage(string message)
        {
            // TODO: トースト通知の実装
            Debug.Log(message);
        }

        /// <summary>
        /// パネルを閉じる
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}
