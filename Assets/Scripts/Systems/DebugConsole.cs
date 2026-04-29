using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ThirtySixStratagems.Core;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// デバッグコンソール
    /// 開発用のコマンドラインインターフェース
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private bool _enableInBuild = false;
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        [SerializeField] private int _maxLogLines = 100;
        [SerializeField] private int _maxHistorySize = 50;

        [Header("表示")]
        [SerializeField] private bool _showInEditor = true;
        [SerializeField] private float _consoleHeight = 300f;
        [SerializeField] private float _inputHeight = 30f;
        [SerializeField] private int _fontSize = 14;

        // 状態
        private bool _isVisible = false;
        private string _inputText = "";
        private Vector2 _scrollPosition;
        private List<LogEntry> _logEntries = new List<LogEntry>();
        private List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;

        // コマンド
        private Dictionary<string, DebugCommand> _commands = new Dictionary<string, DebugCommand>();

        // GUI スタイル
        private GUIStyle _consoleStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _logStyle;
        private bool _stylesInitialized = false;

        /// <summary>
        /// コンソールが表示中か
        /// </summary>
        public bool IsVisible => _isVisible;

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

        private void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                if (Debug.isDebugBuild || _enableInBuild || Application.isEditor)
                {
                    Toggle();
                }
            }

            if (_isVisible)
            {
                HandleInput();
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;
            if (!Debug.isDebugBuild && !_enableInBuild && !Application.isEditor) return;

            InitializeStyles();
            DrawConsole();
        }

        #region Initialization

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            RegisterBuiltInCommands();
        }

        /// <summary>
        /// 組み込みコマンドを登録
        /// </summary>
        private void RegisterBuiltInCommands()
        {
            // ヘルプ
            RegisterCommand("help", "コマンド一覧を表示", args =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== コマンド一覧 ===");
                foreach (var cmd in _commands.Values.OrderBy(c => c.Name))
                {
                    sb.AppendLine($"  {cmd.Name} - {cmd.Description}");
                }
                Log(sb.ToString());
            });

            // クリア
            RegisterCommand("clear", "ログをクリア", args =>
            {
                _logEntries.Clear();
            });

            // 終了
            RegisterCommand("quit", "ゲームを終了", args =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            // FPS表示
            RegisterCommand("fps", "FPS表示を切り替え", args =>
            {
                if (SettingsManager.Instance != null)
                {
                    var settings = SettingsManager.Instance.CurrentSettings;
                    settings.Graphics.ShowFPS = !settings.Graphics.ShowFPS;
                    Log($"FPS表示: {(settings.Graphics.ShowFPS ? "ON" : "OFF")}");
                }
            });

            // 時間スケール
            RegisterCommand("timescale", "時間スケールを設定 (例: timescale 2)", args =>
            {
                if (args.Length > 0 && float.TryParse(args[0], out float scale))
                {
                    Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
                    Log($"時間スケール: {Time.timeScale}");
                }
                else
                {
                    Log($"現在の時間スケール: {Time.timeScale}");
                }
            });

            // ゲーム状態
            RegisterCommand("gamestate", "ゲーム状態を表示", args =>
            {
                if (GameManager.Instance != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("=== ゲーム状態 ===");
                    sb.AppendLine($"  ターン: {GameManager.Instance.CurrentTurn}");
                    sb.AppendLine($"  年: {GameManager.Instance.CurrentYear}");
                    sb.AppendLine($"  シナリオ: {GameManager.Instance.CurrentScenarioId}");

                    if (GameManager.Instance.GameData != null)
                    {
                        sb.AppendLine($"  勢力数: {GameManager.Instance.GameData.Factions.Count}");
                        sb.AppendLine($"  領地数: {GameManager.Instance.GameData.Territories.Count}");
                        sb.AppendLine($"  武将数: {GameManager.Instance.GameData.Characters.Count}");
                    }

                    Log(sb.ToString());
                }
                else
                {
                    LogWarning("GameManager is not initialized");
                }
            });

            // 資源追加
            RegisterCommand("addgold", "金を追加 (例: addgold 1000)", args =>
            {
                if (args.Length > 0 && int.TryParse(args[0], out int amount))
                {
                    var playerFaction = GetPlayerFaction();
                    if (playerFaction != null)
                    {
                        playerFaction.Gold += amount;
                        Log($"金を{amount}追加しました (現在: {playerFaction.Gold})");
                    }
                }
                else
                {
                    LogWarning("使用法: addgold <金額>");
                }
            });

            RegisterCommand("addfood", "兵糧を追加 (例: addfood 1000)", args =>
            {
                if (args.Length > 0 && int.TryParse(args[0], out int amount))
                {
                    var playerFaction = GetPlayerFaction();
                    if (playerFaction != null)
                    {
                        playerFaction.Food += amount;
                        Log($"兵糧を{amount}追加しました (現在: {playerFaction.Food})");
                    }
                }
                else
                {
                    LogWarning("使用法: addfood <量>");
                }
            });

            RegisterCommand("addsp", "計略ポイントを追加 (例: addsp 10)", args =>
            {
                if (args.Length > 0 && int.TryParse(args[0], out int amount))
                {
                    var playerFaction = GetPlayerFaction();
                    if (playerFaction != null)
                    {
                        playerFaction.StratagemPoints += amount;
                        Log($"計略ポイントを{amount}追加しました (現在: {playerFaction.StratagemPoints})");
                    }
                }
                else
                {
                    LogWarning("使用法: addsp <ポイント>");
                }
            });

            // 勢力情報
            RegisterCommand("factions", "勢力一覧を表示", args =>
            {
                if (GameManager.Instance?.GameData == null)
                {
                    LogWarning("ゲームデータがありません");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("=== 勢力一覧 ===");
                foreach (var faction in GameManager.Instance.GameData.Factions.Values)
                {
                    string playerMark = faction.IsPlayer ? " [プレイヤー]" : "";
                    sb.AppendLine($"  {faction.Name}{playerMark}");
                    sb.AppendLine($"    金: {faction.Gold}, 兵糧: {faction.Food}, 計略P: {faction.StratagemPoints}");
                    sb.AppendLine($"    領地: {faction.TerritoryIds.Count}, 武将: {faction.CharacterIds.Count}");
                }
                Log(sb.ToString());
            });

            // 領地情報
            RegisterCommand("territories", "領地一覧を表示", args =>
            {
                if (GameManager.Instance?.GameData == null)
                {
                    LogWarning("ゲームデータがありません");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("=== 領地一覧 ===");
                foreach (var territory in GameManager.Instance.GameData.Territories.Values)
                {
                    string owner = "中立";
                    if (!string.IsNullOrEmpty(territory.OwnerId) &&
                        GameManager.Instance.GameData.Factions.TryGetValue(territory.OwnerId, out var faction))
                    {
                        owner = faction.Name;
                    }
                    sb.AppendLine($"  {territory.Name} ({owner})");
                    sb.AppendLine($"    人口: {territory.Population}, 経済: {territory.Economy}, 防御: {territory.Defense}");
                }
                Log(sb.ToString());
            });

            // セーブ
            RegisterCommand("save", "クイックセーブ", args =>
            {
                if (SaveLoadSystem.Instance != null)
                {
                    SaveLoadSystem.Instance.QuickSave();
                    Log("クイックセーブしました");
                }
            });

            // ロード
            RegisterCommand("load", "クイックロード", args =>
            {
                if (SaveLoadSystem.Instance != null)
                {
                    SaveLoadSystem.Instance.QuickLoad();
                    Log("クイックロードしました");
                }
            });

            // シーン読み込み
            RegisterCommand("scene", "シーンを読み込み (例: scene MainMenu)", args =>
            {
                if (args.Length > 0)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(args[0]);
                    Log($"シーン '{args[0]}' を読み込み中...");
                }
                else
                {
                    string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    Log($"現在のシーン: {currentScene}");
                }
            });

            // システム情報
            RegisterCommand("sysinfo", "システム情報を表示", args =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== システム情報 ===");
                sb.AppendLine($"  Unity: {Application.unityVersion}");
                sb.AppendLine($"  プラットフォーム: {Application.platform}");
                sb.AppendLine($"  OS: {SystemInfo.operatingSystem}");
                sb.AppendLine($"  CPU: {SystemInfo.processorType}");
                sb.AppendLine($"  メモリ: {SystemInfo.systemMemorySize} MB");
                sb.AppendLine($"  GPU: {SystemInfo.graphicsDeviceName}");
                sb.AppendLine($"  VRAM: {SystemInfo.graphicsMemorySize} MB");
                sb.AppendLine($"  解像度: {Screen.width}x{Screen.height}");
                sb.AppendLine($"  FPS: {1f / Time.deltaTime:F1}");
                Log(sb.ToString());
            });
        }

        /// <summary>
        /// プレイヤー勢力を取得
        /// </summary>
        private Faction GetPlayerFaction()
        {
            if (GameManager.Instance?.GameData == null) return null;

            foreach (var faction in GameManager.Instance.GameData.Factions.Values)
            {
                if (faction.IsPlayer) return faction;
            }
            return null;
        }

        #endregion

        #region Command System

        /// <summary>
        /// コマンドを登録
        /// </summary>
        public void RegisterCommand(string name, string description, Action<string[]> action)
        {
            name = name.ToLower();
            _commands[name] = new DebugCommand(name, description, action);
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            // ヒストリに追加
            _commandHistory.Add(input);
            if (_commandHistory.Count > _maxHistorySize)
            {
                _commandHistory.RemoveAt(0);
            }
            _historyIndex = _commandHistory.Count;

            // ログに表示
            Log($"> {input}", LogType.Log, Color.cyan);

            // パース
            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            // 実行
            if (_commands.TryGetValue(commandName, out var command))
            {
                try
                {
                    command.Action(args);
                }
                catch (Exception ex)
                {
                    LogError($"コマンドエラー: {ex.Message}");
                }
            }
            else
            {
                LogWarning($"不明なコマンド: {commandName}");
                Log("'help' でコマンド一覧を表示");
            }
        }

        #endregion

        #region Logging

        /// <summary>
        /// ログを追加
        /// </summary>
        public void Log(string message, LogType type = LogType.Log, Color? color = null)
        {
            var entry = new LogEntry
            {
                Message = message,
                Type = type,
                Color = color ?? GetLogColor(type),
                Time = DateTime.Now
            };

            _logEntries.Add(entry);

            while (_logEntries.Count > _maxLogLines)
            {
                _logEntries.RemoveAt(0);
            }

            // スクロールを最下部へ
            _scrollPosition.y = float.MaxValue;
        }

        /// <summary>
        /// 警告ログ
        /// </summary>
        public void LogWarning(string message)
        {
            Log(message, LogType.Warning);
        }

        /// <summary>
        /// エラーログ
        /// </summary>
        public void LogError(string message)
        {
            Log(message, LogType.Error);
        }

        /// <summary>
        /// Unityログのハンドラ
        /// </summary>
        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            Log(condition, type);
        }

        /// <summary>
        /// ログの色を取得
        /// </summary>
        private Color GetLogColor(LogType type)
        {
            return type switch
            {
                LogType.Error => Color.red,
                LogType.Exception => Color.red,
                LogType.Warning => Color.yellow,
                LogType.Assert => Color.magenta,
                _ => Color.white
            };
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            // 上下キーでヒストリ
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_historyIndex > 0)
                {
                    _historyIndex--;
                    _inputText = _commandHistory[_historyIndex];
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_historyIndex < _commandHistory.Count - 1)
                {
                    _historyIndex++;
                    _inputText = _commandHistory[_historyIndex];
                }
                else
                {
                    _historyIndex = _commandHistory.Count;
                    _inputText = "";
                }
            }
        }

        #endregion

        #region GUI

        /// <summary>
        /// スタイルを初期化
        /// </summary>
        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _consoleStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTexture(1, 1, new Color(0, 0, 0, 0.8f)) }
            };

            _inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = _fontSize,
                normal = { textColor = Color.white }
            };

            _logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                wordWrap = true,
                richText = true
            };

            _stylesInitialized = true;
        }

        /// <summary>
        /// テクスチャを作成
        /// </summary>
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            var texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// コンソールを描画
        /// </summary>
        private void DrawConsole()
        {
            float width = Screen.width;

            // コンソール背景
            GUI.Box(new Rect(0, 0, width, _consoleHeight + _inputHeight), "", _consoleStyle);

            // ログエリア
            GUILayout.BeginArea(new Rect(5, 5, width - 10, _consoleHeight - 10));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (var entry in _logEntries)
            {
                _logStyle.normal.textColor = entry.Color;
                GUILayout.Label(entry.Message, _logStyle);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // 入力エリア
            GUI.SetNextControlName("ConsoleInput");
            _inputText = GUI.TextField(new Rect(5, _consoleHeight, width - 10, _inputHeight - 5), _inputText, _inputStyle);

            // Enterで実行
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                ExecuteCommand(_inputText);
                _inputText = "";
                Event.current.Use();
            }

            // フォーカス
            GUI.FocusControl("ConsoleInput");
        }

        #endregion

        #region Toggle

        /// <summary>
        /// 表示を切り替え
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            _historyIndex = _commandHistory.Count;
        }

        /// <summary>
        /// 表示
        /// </summary>
        public void Show()
        {
            _isVisible = true;
        }

        /// <summary>
        /// 非表示
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// ログエントリ
    /// </summary>
    public class LogEntry
    {
        public string Message;
        public LogType Type;
        public Color Color;
        public DateTime Time;
    }

    /// <summary>
    /// デバッグコマンド
    /// </summary>
    public class DebugCommand
    {
        public string Name;
        public string Description;
        public Action<string[]> Action;

        public DebugCommand(string name, string description, Action<string[]> action)
        {
            Name = name;
            Description = description;
            Action = action;
        }
    }

    #endregion
}
