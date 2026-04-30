using UnityEngine;
using TMPro;
using ThirtySixStratagems.Data.Models;

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
        [SerializeField] private SpriteRenderer _terrainBgSprite;
        [SerializeField] private TextMeshPro _nameLabel;
        [SerializeField] private TextMesh _nameLabelLegacy;
        [SerializeField] private TextMesh _armyCountLabel;

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
        private TerrainType _terrainType;
        private bool _isSelected;
        private bool _isHighlighted;
        private Color _baseColor = Color.white;
        private Color _terrainColor = Color.white;
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

        /// <summary>
        /// 地形タイプ
        /// </summary>
        public TerrainType TerrainType => _terrainType;

        private void Awake()
        {
            _originalScale = transform.localScale;

            // コンポーネントを自動取得
            if (_mainSprite == null)
            {
                _mainSprite = GetComponent<SpriteRenderer>();
            }

            // 地形背景スプライトを取得
            if (_terrainBgSprite == null)
            {
                var bgObj = transform.Find("TerrainBg");
                if (bgObj != null)
                {
                    _terrainBgSprite = bgObj.GetComponent<SpriteRenderer>();
                }
            }

            // 兵力表示ラベルを取得
            if (_armyCountLabel == null)
            {
                var countObj = transform.Find("ArmyCount");
                if (countObj != null)
                {
                    _armyCountLabel = countObj.GetComponent<TextMesh>();
                }
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
            Initialize(territoryId, territoryName, TerrainType.Plain);
        }

        /// <summary>
        /// 初期化（地形タイプ付き）
        /// </summary>
        public void Initialize(string territoryId, string territoryName, TerrainType terrainType)
        {
            _territoryId = territoryId;
            _territoryName = territoryName;
            _terrainType = terrainType;

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
        /// 地形色を設定
        /// </summary>
        public void SetTerrainColor(Color color)
        {
            _terrainColor = color;

            if (_terrainBgSprite != null)
            {
                _terrainBgSprite.color = color * 0.6f;
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
            if (_armyCountLabel == null) return;

            if (soldierCount > 0)
            {
                _armyCountLabel.gameObject.SetActive(true);
                _armyCountLabel.text = FormatSoldierCount(soldierCount);
            }
            else
            {
                _armyCountLabel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 兵力を表示用にフォーマット
        /// </summary>
        private string FormatSoldierCount(int count)
        {
            if (count >= 10000)
            {
                return $"{count / 10000f:F1}万";
            }
            else if (count >= 1000)
            {
                return $"{count / 1000f:F1}千";
            }
            return count.ToString();
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
