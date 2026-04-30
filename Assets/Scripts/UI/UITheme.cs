using UnityEngine;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// UIテーマ
    /// UI全体の統一されたスタイルを定義
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "ThirtySixStratagems/UI/Theme")]
    public class UITheme : ScriptableObject
    {
        private static UITheme _current;
        public static UITheme Current
        {
            get
            {
                if (_current == null)
                {
                    _current = CreateDefaultTheme();
                }
                return _current;
            }
            set => _current = value;
        }

        [Header("プライマリカラー")]
        public Color PrimaryColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        public Color PrimaryDarkColor = new Color(0.15f, 0.3f, 0.6f, 1f);
        public Color PrimaryLightColor = new Color(0.3f, 0.5f, 0.9f, 1f);

        [Header("セカンダリカラー")]
        public Color SecondaryColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        public Color SecondaryDarkColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        public Color SecondaryLightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("機能カラー")]
        public Color SuccessColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        public Color WarningColor = new Color(0.9f, 0.7f, 0.2f, 1f);
        public Color DangerColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        public Color InfoColor = new Color(0.2f, 0.6f, 0.9f, 1f);

        [Header("背景カラー")]
        public Color BackgroundColor = new Color(0.1f, 0.12f, 0.15f, 1f);
        public Color PanelColor = new Color(0.15f, 0.17f, 0.2f, 0.95f);
        public Color CardColor = new Color(0.2f, 0.22f, 0.25f, 0.9f);

        [Header("テキストカラー")]
        public Color TextPrimaryColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        public Color TextSecondaryColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        public Color TextMutedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color TextAccentColor = new Color(1f, 0.9f, 0.7f, 1f);

        [Header("ボーダー")]
        public Color BorderColor = new Color(0.3f, 0.35f, 0.4f, 1f);
        public Color BorderLightColor = new Color(0.4f, 0.45f, 0.5f, 1f);

        [Header("勢力カラー")]
        public Color PlayerColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color EnemyColor = new Color(1f, 0.3f, 0.3f, 1f);
        public Color NeutralColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        public Color AllyColor = new Color(0.3f, 0.8f, 0.4f, 1f);

        [Header("ゲームステータス")]
        public Color HealthHighColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        public Color HealthMediumColor = new Color(0.9f, 0.7f, 0.2f, 1f);
        public Color HealthLowColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        public Color MoraleHighColor = new Color(0.3f, 0.7f, 0.9f, 1f);
        public Color MoraleLowColor = new Color(0.5f, 0.3f, 0.3f, 1f);

        [Header("フォント設定")]
        public float FontSizeSmall = 12f;
        public float FontSizeNormal = 14f;
        public float FontSizeLarge = 18f;
        public float FontSizeTitle = 24f;
        public float FontSizeHeader = 32f;

        [Header("スペーシング")]
        public float SpacingSmall = 4f;
        public float SpacingNormal = 8f;
        public float SpacingLarge = 16f;
        public float SpacingXLarge = 24f;

        [Header("ボーダー半径")]
        public float BorderRadiusSmall = 4f;
        public float BorderRadiusNormal = 8f;
        public float BorderRadiusLarge = 12f;

        [Header("アニメーション")]
        public float AnimationFast = 0.15f;
        public float AnimationNormal = 0.3f;
        public float AnimationSlow = 0.5f;

        /// <summary>
        /// デフォルトテーマを作成
        /// </summary>
        public static UITheme CreateDefaultTheme()
        {
            return CreateInstance<UITheme>();
        }

        /// <summary>
        /// 健康状態に応じた色を取得
        /// </summary>
        public Color GetHealthColor(float ratio)
        {
            if (ratio > 0.6f)
            {
                return HealthHighColor;
            }
            else if (ratio > 0.3f)
            {
                return HealthMediumColor;
            }
            else
            {
                return HealthLowColor;
            }
        }

        /// <summary>
        /// 士気に応じた色を取得
        /// </summary>
        public Color GetMoraleColor(float ratio)
        {
            return Color.Lerp(MoraleLowColor, MoraleHighColor, ratio);
        }
    }
}
