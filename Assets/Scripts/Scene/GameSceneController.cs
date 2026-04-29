using System;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;
using ThirtySixStratagems.UI.HUD;
using ThirtySixStratagems.UI.Territory;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// ゲームシーンコントローラー
    /// GameSceneの初期化と全体制御を担当
    /// </summary>
    public class GameSceneController : MonoBehaviour
    {
        public static GameSceneController Instance { get; private set; }

        [Header("UI参照")]
        [SerializeField] private GameHUD _gameHUD;
        [SerializeField] private TerritoryInfoPanel _territoryInfoPanel;
        [SerializeField] private GameObject _loadingPanel;

        [Header("マップ")]
        [SerializeField] private MapDisplay _mapDisplay;
        [SerializeField] private Camera _mainCamera;

        [Header("設定")]
        [SerializeField] private float _cameraMoveSpeed = 10f;
        [SerializeField] private float _cameraZoomSpeed = 5f;
        [SerializeField] private float _minZoom = 3f;
        [SerializeField] private float _maxZoom = 15f;

        // 状態
        private bool _isInitialized;
        private bool _isDragging;
        private Vector3 _lastMousePosition;
        private string _selectedTerritoryId;

        // イベント
        public event Action OnSceneInitialized;
        public event Action<string> OnTerritoryClicked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeScene();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (!_isInitialized) return;

            HandleCameraInput();
            HandleTerritorySelection();
        }

        #region Initialization

        /// <summary>
        /// シーンを初期化
        /// </summary>
        private void InitializeScene()
        {
            ShowLoading(true);

            try
            {
                // ゲームデータの確認
                if (GameManager.Instance?.GameData == null)
                {
                    Debug.LogError("GameData is null. Cannot initialize scene.");
                    return;
                }

                // マップを初期化
                InitializeMap();

                // UIを初期化
                InitializeUI();

                // カメラを初期化
                InitializeCamera();

                _isInitialized = true;
                Debug.Log("GameScene initialized successfully");

                OnSceneInitialized?.Invoke();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        /// <summary>
        /// マップを初期化
        /// </summary>
        private void InitializeMap()
        {
            if (_mapDisplay != null)
            {
                _mapDisplay.Initialize(GameManager.Instance.GameData);
            }
        }

        /// <summary>
        /// UIを初期化
        /// </summary>
        private void InitializeUI()
        {
            // HUDを更新
            if (_gameHUD != null)
            {
                _gameHUD.RefreshAll();
            }

            // 領地パネルを非表示
            if (_territoryInfoPanel != null)
            {
                _territoryInfoPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// カメラを初期化
        /// </summary>
        private void InitializeCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            // プレイヤー勢力の首都にカメラを移動
            var playerFaction = GetPlayerFaction();
            if (playerFaction != null && playerFaction.TerritoryIds.Count > 0)
            {
                var capital = GameManager.Instance.GetTerritory(playerFaction.TerritoryIds[0]);
                if (capital != null)
                {
                    CenterCameraOn(capital.MapPosition);
                }
            }
        }

        /// <summary>
        /// ローディング表示
        /// </summary>
        private void ShowLoading(bool show)
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetActive(show);
            }
        }

        #endregion

        #region Camera Control

        /// <summary>
        /// カメラ入力を処理
        /// </summary>
        private void HandleCameraInput()
        {
            if (_mainCamera == null) return;

            // ドラッグでカメラ移動
            HandleCameraDrag();

            // ズーム
            HandleCameraZoom();

            // キーボードでの移動
            HandleCameraKeyboard();
        }

        /// <summary>
        /// ドラッグでカメラ移動
        /// </summary>
        private void HandleCameraDrag()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * Time.deltaTime * _cameraMoveSpeed * 0.1f;
                _mainCamera.transform.Translate(move, Space.World);
                _lastMousePosition = Input.mousePosition;
            }
        }

        /// <summary>
        /// ズーム処理
        /// </summary>
        private void HandleCameraZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                if (_mainCamera.orthographic)
                {
                    _mainCamera.orthographicSize -= scroll * _cameraZoomSpeed;
                    _mainCamera.orthographicSize = Mathf.Clamp(_mainCamera.orthographicSize, _minZoom, _maxZoom);
                }
                else
                {
                    _mainCamera.transform.Translate(0, 0, scroll * _cameraZoomSpeed, Space.Self);
                }
            }
        }

        /// <summary>
        /// キーボードでカメラ移動
        /// </summary>
        private void HandleCameraKeyboard()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
            {
                Vector3 move = new Vector3(horizontal, vertical, 0) * Time.deltaTime * _cameraMoveSpeed;
                _mainCamera.transform.Translate(move, Space.World);
            }
        }

        /// <summary>
        /// カメラを指定位置に中心合わせ
        /// </summary>
        public void CenterCameraOn(Vector2 position)
        {
            if (_mainCamera != null)
            {
                _mainCamera.transform.position = new Vector3(position.x, position.y, _mainCamera.transform.position.z);
            }
        }

        /// <summary>
        /// 領地にカメラを移動
        /// </summary>
        public void FocusOnTerritory(string territoryId)
        {
            var territory = GameManager.Instance?.GetTerritory(territoryId);
            if (territory != null)
            {
                CenterCameraOn(territory.MapPosition);
            }
        }

        #endregion

        #region Territory Selection

        /// <summary>
        /// 領地選択を処理
        /// </summary>
        private void HandleTerritorySelection()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    var territoryVisual = hit.collider.GetComponent<TerritoryVisual>();
                    if (territoryVisual != null)
                    {
                        SelectTerritory(territoryVisual.TerritoryId);
                    }
                }
                else
                {
                    // 何もない場所をクリックしたら選択解除
                    DeselectTerritory();
                }
            }
        }

        /// <summary>
        /// 領地を選択
        /// </summary>
        public void SelectTerritory(string territoryId)
        {
            _selectedTerritoryId = territoryId;

            // マップ上の選択表示を更新
            if (_mapDisplay != null)
            {
                _mapDisplay.SelectTerritory(territoryId);
            }

            // 情報パネルを表示
            if (_territoryInfoPanel != null)
            {
                _territoryInfoPanel.ShowTerritory(territoryId);
            }

            // イベント発火
            EventBus.TerritorySelected(territoryId);
            OnTerritoryClicked?.Invoke(territoryId);

            Debug.Log($"Territory selected: {territoryId}");
        }

        /// <summary>
        /// 領地選択を解除
        /// </summary>
        public void DeselectTerritory()
        {
            _selectedTerritoryId = null;

            if (_mapDisplay != null)
            {
                _mapDisplay.DeselectTerritory();
            }

            if (_territoryInfoPanel != null)
            {
                _territoryInfoPanel.Close();
            }
        }

        /// <summary>
        /// 選択中の領地ID
        /// </summary>
        public string SelectedTerritoryId => _selectedTerritoryId;

        #endregion

        #region Event Handlers

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            EventBus.OnGameStateChanged += OnGameStateChanged;
            EventBus.OnTerritoryConquered += OnTerritoryConquered;
            EventBus.OnTurnStarted += OnTurnStarted;
            EventBus.OnFactionTurnStarted += OnFactionTurnStarted;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventBus.OnGameStateChanged -= OnGameStateChanged;
            EventBus.OnTerritoryConquered -= OnTerritoryConquered;
            EventBus.OnTurnStarted -= OnTurnStarted;
            EventBus.OnFactionTurnStarted -= OnFactionTurnStarted;
        }

        private void OnGameStateChanged(GameState state)
        {
            // 戦闘シーンに遷移する場合はこのシーンを一時停止
            if (state == GameState.Battle)
            {
                // Battle scene will be loaded additively
            }
        }

        private void OnTerritoryConquered(TerritoryConqueredEventArgs args)
        {
            // マップを更新
            if (_mapDisplay != null)
            {
                _mapDisplay.UpdateTerritoryOwner(args.TerritoryId, args.NewOwnerId);
            }
        }

        private void OnTurnStarted(int turn)
        {
            Debug.Log($"Turn {turn} started");
        }

        private void OnFactionTurnStarted(string factionId)
        {
            // プレイヤーのターンの場合、操作を有効化
            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction != null && faction.IsPlayer)
            {
                Debug.Log("Player turn started");
            }
        }

        #endregion

        #region Helper

        /// <summary>
        /// プレイヤー勢力を取得
        /// </summary>
        private Faction GetPlayerFaction()
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.IsPlayer)
                {
                    return faction;
                }
            }
            return null;
        }

        /// <summary>
        /// マップを更新
        /// </summary>
        public void RefreshMap()
        {
            if (_mapDisplay != null)
            {
                _mapDisplay.RefreshAll();
            }
        }

        #endregion
    }
}
