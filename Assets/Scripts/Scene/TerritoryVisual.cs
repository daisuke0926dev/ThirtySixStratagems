using UnityEngine;
using TMPro;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// 領地ビジュアル
    /// マップ上の各領地の視覚的表示を制御
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TerritoryVisual : MonoBehaviour
    {
        [Header("コンポーネント")]
        [SerializeField] private SpriteRenderer _mainSprite;
        [SerializeField] private SpriteRenderer _outlineSprite;
        [SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private TextMeshPro _nameLabel;
        [SerializeField] private TextMesh _nameLabelLegacy;

        [Header("色設定")]
        [SerializeField] private Color _selectedOutlineColor = Color.yellow;
        [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.5f, 0.3f);
        [SerializeField] private float _highlightScale = 1.2f;

        [Header("アニメーション")]
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseAmount = 0.1f;

        // 状態
        private string _territoryId;
        private string _territoryName;
        private bool _isSelected;
        private bool _isHighlighted;
        private Color _baseColor = Color.white;
        private Vector3 _originalScale;

        /// <summary>
        /// 領地ID
        /// </summary>
        public string TerritoryId => _territoryId;

        /// <summary>
        /// 領地名
        /// </summary>
        public string TerritoryName => _territoryName;

        /// <summary>
        /// 選択中か
        /// </summary>
        public bool IsSelected => _isSelected;

        private void Awake()
        {
            _originalScale = transform.localScale;

            // コンポーネントを自動取得
            if (_mainSprite == null)
            {
                _mainSprite = GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (_isSelected)
            {
                // 選択中はパルスアニメーション
                float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
                transform.localScale = _originalScale * pulse;
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(string territoryId, string territoryName)
        {
            _territoryId = territoryId;
            _territoryName = territoryName;

            SetName(territoryName);
        }

        /// <summary>
        /// 名前を設定
        /// </summary>
        public void SetName(string name)
        {
            _territoryName = name;

            if (_nameLabel != null)
            {
                _nameLabel.text = name;
            }

            if (_nameLabelLegacy != null)
            {
                _nameLabelLegacy.text = name;
            }
        }

        /// <summary>
        /// 色を設定
        /// </summary>
        public void SetColor(Color color)
        {
            _baseColor = color;

            if (_mainSprite != null)
            {
                _mainSprite.color = color;
            }
        }

        /// <summary>
        /// 選択状態を設定
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (_outlineSprite != null)
            {
                _outlineSprite.enabled = selected;
                _outlineSprite.color = _selectedOutlineColor;
            }

            if (!selected)
            {
                transform.localScale = _originalScale;
            }
        }

        /// <summary>
        /// ハイライト状態を設定
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            _isHighlighted = highlighted;

            if (_highlightSprite != null)
            {
                _highlightSprite.enabled = highlighted;
                _highlightSprite.color = _highlightColor;
            }
            else if (_mainSprite != null)
            {
                // ハイライトスプライトがない場合はメインスプライトの色を変更
                if (highlighted)
                {
                    _mainSprite.color = Color.Lerp(_baseColor, _highlightColor, 0.3f);
                }
                else
                {
                    _mainSprite.color = _baseColor;
                }
            }
        }

        /// <summary>
        /// ホバー状態を設定
        /// </summary>
        public void SetHovered(bool hovered)
        {
            if (!_isSelected)
            {
                float scale = hovered ? 1.1f : 1f;
                transform.localScale = _originalScale * scale;
            }
        }

        /// <summary>
        /// アイコンを設定
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            // 将来の拡張用
        }

        /// <summary>
        /// 兵力表示を更新
        /// </summary>
        public void UpdateArmyDisplay(int soldierCount)
        {
            // 将来の拡張用：領地上に兵力を表示
        }

        private void OnMouseEnter()
        {
            SetHovered(true);
        }

        private void OnMouseExit()
        {
            SetHovered(false);
        }

        private void OnMouseDown()
        {
            // GameSceneControllerがハンドリングするのでここでは何もしない
        }
    }
}
