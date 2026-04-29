using UnityEngine;
using TMPro;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// ローカライズテキストコンポーネント
    /// 言語変更時に自動的にテキストを更新
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Header("ローカライズ設定")]
        [SerializeField] private string _localizationKey;
        [SerializeField] private bool _updateOnStart = true;

        [Header("フォーマット")]
        [SerializeField] private bool _useFormat = false;
        [SerializeField] private string[] _formatArgs;

        private TextMeshProUGUI _textComponent;

        /// <summary>
        /// ローカライズキー
        /// </summary>
        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                UpdateText();
            }
        }

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (LocalizationSystem.Instance != null)
            {
                LocalizationSystem.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            if (_updateOnStart)
            {
                UpdateText();
            }
        }

        private void OnDisable()
        {
            if (LocalizationSystem.Instance != null)
            {
                LocalizationSystem.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        /// <summary>
        /// 言語変更時のハンドラ
        /// </summary>
        private void OnLanguageChanged(SystemLanguage language)
        {
            UpdateText();
        }

        /// <summary>
        /// テキストを更新
        /// </summary>
        public void UpdateText()
        {
            if (_textComponent == null || string.IsNullOrEmpty(_localizationKey)) return;
            if (LocalizationSystem.Instance == null) return;

            if (_useFormat && _formatArgs != null && _formatArgs.Length > 0)
            {
                _textComponent.text = LocalizationSystem.Instance.GetString(_localizationKey, _formatArgs);
            }
            else
            {
                _textComponent.text = LocalizationSystem.Instance.GetString(_localizationKey);
            }
        }

        /// <summary>
        /// フォーマット引数を設定してテキストを更新
        /// </summary>
        public void UpdateTextWithArgs(params object[] args)
        {
            if (_textComponent == null || string.IsNullOrEmpty(_localizationKey)) return;
            if (LocalizationSystem.Instance == null) return;

            _textComponent.text = LocalizationSystem.Instance.GetString(_localizationKey, args);
        }

        /// <summary>
        /// キーとフォーマット引数を設定してテキストを更新
        /// </summary>
        public void SetKeyAndUpdate(string key, params object[] args)
        {
            _localizationKey = key;
            if (args != null && args.Length > 0)
            {
                UpdateTextWithArgs(args);
            }
            else
            {
                UpdateText();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタでの検証
        /// </summary>
        private void OnValidate()
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }

            // エディタ上でプレビュー（キーを表示）
            if (_textComponent != null && !string.IsNullOrEmpty(_localizationKey))
            {
                if (!Application.isPlaying)
                {
                    _textComponent.text = $"[{_localizationKey}]";
                }
            }
        }
#endif
    }
}
