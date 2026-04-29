using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// FPSカウンター
    /// フレームレートの表示と計測
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [Header("表示設定")]
        [SerializeField] private bool _showOnStart = false;
        [SerializeField] private TextAnchor _position = TextAnchor.UpperLeft;
        [SerializeField] private int _fontSize = 20;
        [SerializeField] private Color _goodColor = Color.green;
        [SerializeField] private Color _okColor = Color.yellow;
        [SerializeField] private Color _badColor = Color.red;

        [Header("閾値")]
        [SerializeField] private float _goodFPS = 55f;
        [SerializeField] private float _okFPS = 30f;

        [Header("更新設定")]
        [SerializeField] private float _updateInterval = 0.5f;

        // 状態
        private bool _isVisible;
        private float _fps;
        private float _deltaTime;
        private float _lastUpdate;
        private int _frameCount;

        // GUI
        private GUIStyle _style;
        private Rect _rect;

        private void Awake()
        {
            _isVisible = _showOnStart;
        }

        private void OnEnable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingChanged += OnSettingChanged;
                _isVisible = SettingsManager.Instance.CurrentSettings?.Graphics?.ShowFPS ?? _showOnStart;
            }
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingChanged -= OnSettingChanged;
            }
        }

        private void Update()
        {
            if (!_isVisible) return;

            _frameCount++;
            _deltaTime += Time.unscaledDeltaTime;

            if (Time.realtimeSinceStartup - _lastUpdate > _updateInterval)
            {
                _fps = _frameCount / _deltaTime;
                _frameCount = 0;
                _deltaTime = 0f;
                _lastUpdate = Time.realtimeSinceStartup;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            if (_style == null)
            {
                InitializeStyle();
            }

            // 色を決定
            Color color;
            if (_fps >= _goodFPS)
                color = _goodColor;
            else if (_fps >= _okFPS)
                color = _okColor;
            else
                color = _badColor;

            _style.normal.textColor = color;

            // 位置を計算
            UpdateRect();

            // 描画
            string text = $"FPS: {_fps:F1}";
            GUI.Label(_rect, text, _style);
        }

        /// <summary>
        /// スタイルを初期化
        /// </summary>
        private void InitializeStyle()
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                fontStyle = FontStyle.Bold
            };
        }

        /// <summary>
        /// 矩形を更新
        /// </summary>
        private void UpdateRect()
        {
            float width = 150;
            float height = 30;
            float margin = 10;

            float x = margin;
            float y = margin;

            switch (_position)
            {
                case TextAnchor.UpperLeft:
                    x = margin;
                    y = margin;
                    _style.alignment = TextAnchor.UpperLeft;
                    break;
                case TextAnchor.UpperRight:
                    x = Screen.width - width - margin;
                    y = margin;
                    _style.alignment = TextAnchor.UpperRight;
                    break;
                case TextAnchor.LowerLeft:
                    x = margin;
                    y = Screen.height - height - margin;
                    _style.alignment = TextAnchor.LowerLeft;
                    break;
                case TextAnchor.LowerRight:
                    x = Screen.width - width - margin;
                    y = Screen.height - height - margin;
                    _style.alignment = TextAnchor.LowerRight;
                    break;
                case TextAnchor.UpperCenter:
                    x = (Screen.width - width) / 2;
                    y = margin;
                    _style.alignment = TextAnchor.UpperCenter;
                    break;
                case TextAnchor.LowerCenter:
                    x = (Screen.width - width) / 2;
                    y = Screen.height - height - margin;
                    _style.alignment = TextAnchor.LowerCenter;
                    break;
            }

            _rect = new Rect(x, y, width, height);
        }

        /// <summary>
        /// 設定変更時のハンドラ
        /// </summary>
        private void OnSettingChanged(string key, object value)
        {
            if (key == "Graphics.ShowFPS" && value is bool showFps)
            {
                _isVisible = showFps;
            }
        }

        /// <summary>
        /// 表示を切り替え
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
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

        /// <summary>
        /// 現在のFPSを取得
        /// </summary>
        public float GetFPS()
        {
            return _fps;
        }
    }
}
