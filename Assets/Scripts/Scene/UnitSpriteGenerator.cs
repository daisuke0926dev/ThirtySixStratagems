using UnityEngine;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// ユニットスプライト生成
    /// 戦闘ユニットのプロシージャルスプライトを生成
    /// </summary>
    public static class UnitSpriteGenerator
    {
        private const int UNIT_SIZE = 64;

        /// <summary>
        /// 兵士ユニットスプライトを生成
        /// </summary>
        public static Sprite CreateSoldierSprite(Color factionColor, UnitType unitType)
        {
            var texture = new Texture2D(UNIT_SIZE, UNIT_SIZE, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            // 背景をクリア
            ClearTexture(texture, Color.clear);

            // ユニットタイプに応じて描画
            switch (unitType)
            {
                case UnitType.Infantry:
                    DrawInfantry(texture, factionColor);
                    break;
                case UnitType.Cavalry:
                    DrawCavalry(texture, factionColor);
                    break;
                case UnitType.Archer:
                    DrawArcher(texture, factionColor);
                    break;
                case UnitType.General:
                    DrawGeneral(texture, factionColor);
                    break;
                default:
                    DrawInfantry(texture, factionColor);
                    break;
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, UNIT_SIZE, UNIT_SIZE),
                new Vector2(0.5f, 0.5f),
                64f
            );
        }

        /// <summary>
        /// 歩兵を描画
        /// </summary>
        private static void DrawInfantry(Texture2D texture, Color color)
        {
            int cx = UNIT_SIZE / 2;
            int cy = UNIT_SIZE / 2;

            // 頭（円）
            DrawCircle(texture, cx, cy + 12, 8, color);

            // 体（四角形）
            DrawRect(texture, cx - 8, cy - 8, 16, 16, color);

            // 足
            DrawRect(texture, cx - 7, cy - 16, 5, 8, color * 0.8f);
            DrawRect(texture, cx + 2, cy - 16, 5, 8, color * 0.8f);

            // 盾（左手）
            DrawRect(texture, cx - 14, cy - 6, 6, 12, Color.Lerp(color, Color.gray, 0.5f));

            // 槍（右手）
            DrawRect(texture, cx + 10, cy - 4, 3, 24, new Color(0.5f, 0.3f, 0.1f)); // 木の色
            DrawTriangle(texture, cx + 11, cy + 20, 4, Color.gray); // 穂先
        }

        /// <summary>
        /// 騎兵を描画
        /// </summary>
        private static void DrawCavalry(Texture2D texture, Color color)
        {
            int cx = UNIT_SIZE / 2;
            int cy = UNIT_SIZE / 2;

            // 馬の体
            Color horseColor = new Color(0.4f, 0.25f, 0.15f);
            DrawEllipse(texture, cx, cy - 8, 16, 10, horseColor);

            // 馬の頭
            DrawCircle(texture, cx + 14, cy - 4, 6, horseColor);

            // 馬の足
            DrawRect(texture, cx - 10, cy - 20, 3, 12, horseColor * 0.8f);
            DrawRect(texture, cx - 4, cy - 20, 3, 12, horseColor * 0.8f);
            DrawRect(texture, cx + 4, cy - 20, 3, 12, horseColor * 0.8f);
            DrawRect(texture, cx + 10, cy - 20, 3, 12, horseColor * 0.8f);

            // 騎手の体
            DrawRect(texture, cx - 6, cy + 2, 12, 12, color);

            // 騎手の頭
            DrawCircle(texture, cx, cy + 16, 6, color);

            // 槍
            DrawRect(texture, cx + 8, cy + 4, 2, 20, new Color(0.5f, 0.3f, 0.1f));
            DrawTriangle(texture, cx + 9, cy + 24, 3, Color.gray);
        }

        /// <summary>
        /// 弓兵を描画
        /// </summary>
        private static void DrawArcher(Texture2D texture, Color color)
        {
            int cx = UNIT_SIZE / 2;
            int cy = UNIT_SIZE / 2;

            // 頭
            DrawCircle(texture, cx, cy + 12, 7, color);

            // 体
            DrawRect(texture, cx - 6, cy - 6, 12, 14, color);

            // 足
            DrawRect(texture, cx - 5, cy - 14, 4, 8, color * 0.8f);
            DrawRect(texture, cx + 1, cy - 14, 4, 8, color * 0.8f);

            // 弓
            Color bowColor = new Color(0.5f, 0.3f, 0.1f);
            DrawArc(texture, cx - 12, cy, 14, bowColor);

            // 矢
            DrawRect(texture, cx - 2, cy + 2, 20, 2, bowColor);
            DrawTriangle(texture, cx + 18, cy + 3, 4, Color.gray);
        }

        /// <summary>
        /// 将軍を描画
        /// </summary>
        private static void DrawGeneral(Texture2D texture, Color color)
        {
            int cx = UNIT_SIZE / 2;
            int cy = UNIT_SIZE / 2;

            // 頭
            DrawCircle(texture, cx, cy + 14, 9, color);

            // 兜（冠）
            Color goldColor = new Color(1f, 0.85f, 0.2f);
            DrawRect(texture, cx - 10, cy + 22, 20, 4, goldColor);
            DrawTriangle(texture, cx, cy + 28, 6, goldColor);

            // 鎧（体）
            DrawRect(texture, cx - 10, cy - 8, 20, 18, Color.Lerp(color, Color.gray, 0.3f));

            // 肩当て
            DrawRect(texture, cx - 14, cy + 4, 6, 8, color);
            DrawRect(texture, cx + 8, cy + 4, 6, 8, color);

            // 足
            DrawRect(texture, cx - 6, cy - 16, 5, 8, color * 0.8f);
            DrawRect(texture, cx + 1, cy - 16, 5, 8, color * 0.8f);

            // マント
            DrawTriangle(texture, cx, cy - 10, 16, Color.Lerp(color, Color.red, 0.5f));

            // 剣
            DrawRect(texture, cx + 12, cy - 4, 3, 20, Color.gray);
            DrawRect(texture, cx + 10, cy + 14, 7, 3, goldColor); // 鍔
        }

        /// <summary>
        /// エフェクトスプライトを生成
        /// </summary>
        public static Sprite CreateEffectSprite(EffectType effectType, Color color)
        {
            int size = 48;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            ClearTexture(texture, Color.clear);

            switch (effectType)
            {
                case EffectType.Slash:
                    DrawSlashEffect(texture, color);
                    break;
                case EffectType.Impact:
                    DrawImpactEffect(texture, color);
                    break;
                case EffectType.Arrow:
                    DrawArrowEffect(texture, color);
                    break;
                case EffectType.Magic:
                    DrawMagicEffect(texture, color);
                    break;
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                48f
            );
        }

        /// <summary>
        /// 斬撃エフェクト
        /// </summary>
        private static void DrawSlashEffect(Texture2D texture, Color color)
        {
            int cx = texture.width / 2;
            int cy = texture.height / 2;

            // 斜めの線を複数描画
            for (int i = -2; i <= 2; i++)
            {
                DrawLine(texture, cx - 20 + i, cy - 20, cx + 20 + i, cy + 20, color);
            }

            // 周囲に光のパーティクル
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                int px = cx + (int)(Mathf.Cos(angle) * 16);
                int py = cy + (int)(Mathf.Sin(angle) * 16);
                DrawCircle(texture, px, py, 2, color * 0.7f);
            }
        }

        /// <summary>
        /// 衝撃エフェクト
        /// </summary>
        private static void DrawImpactEffect(Texture2D texture, Color color)
        {
            int cx = texture.width / 2;
            int cy = texture.height / 2;

            // 同心円を描画
            for (int r = 5; r <= 20; r += 5)
            {
                float alpha = 1f - (r / 25f);
                DrawCircleOutline(texture, cx, cy, r, new Color(color.r, color.g, color.b, alpha));
            }

            // 中心に輝き
            DrawCircle(texture, cx, cy, 4, Color.white);
        }

        /// <summary>
        /// 矢エフェクト
        /// </summary>
        private static void DrawArrowEffect(Texture2D texture, Color color)
        {
            int cx = texture.width / 2;
            int cy = texture.height / 2;

            // 矢の軌跡
            Color trailColor = new Color(color.r, color.g, color.b, 0.5f);
            for (int i = 0; i < 20; i++)
            {
                float alpha = 1f - (i / 20f);
                DrawRect(texture, cx - 20 + i, cy - 1, 2, 2, new Color(color.r, color.g, color.b, alpha * 0.5f));
            }

            // 矢本体
            DrawTriangle(texture, cx + 8, cy, 6, color);
        }

        /// <summary>
        /// 魔法エフェクト（計略用）
        /// </summary>
        private static void DrawMagicEffect(Texture2D texture, Color color)
        {
            int cx = texture.width / 2;
            int cy = texture.height / 2;

            // 星型パターン
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2f / 5f - Mathf.PI / 2f;
                float nextAngle = ((i + 2) % 5) * Mathf.PI * 2f / 5f - Mathf.PI / 2f;

                int x1 = cx + (int)(Mathf.Cos(angle) * 18);
                int y1 = cy + (int)(Mathf.Sin(angle) * 18);
                int x2 = cx + (int)(Mathf.Cos(nextAngle) * 18);
                int y2 = cy + (int)(Mathf.Sin(nextAngle) * 18);

                DrawLine(texture, x1, y1, x2, y2, color);
            }

            // 外周の円
            DrawCircleOutline(texture, cx, cy, 20, color * 0.7f);

            // 中心の輝き
            DrawCircle(texture, cx, cy, 5, Color.white);
        }

        #region Drawing Helpers

        private static void ClearTexture(Texture2D texture, Color color)
        {
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
        }

        private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        SetPixelSafe(texture, cx + x, cy + y, color);
                    }
                }
            }
        }

        private static void DrawCircleOutline(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int distSq = x * x + y * y;
                    int innerSq = (radius - 1) * (radius - 1);
                    int outerSq = radius * radius;

                    if (distSq >= innerSq && distSq <= outerSq)
                    {
                        SetPixelSafe(texture, cx + x, cy + y, color);
                    }
                }
            }
        }

        private static void DrawEllipse(Texture2D texture, int cx, int cy, int rx, int ry, Color color)
        {
            for (int y = -ry; y <= ry; y++)
            {
                for (int x = -rx; x <= rx; x++)
                {
                    float check = (x * x) / (float)(rx * rx) + (y * y) / (float)(ry * ry);
                    if (check <= 1f)
                    {
                        SetPixelSafe(texture, cx + x, cy + y, color);
                    }
                }
            }
        }

        private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    SetPixelSafe(texture, x + dx, y + dy, color);
                }
            }
        }

        private static void DrawTriangle(Texture2D texture, int cx, int cy, int size, Color color)
        {
            for (int y = 0; y < size; y++)
            {
                int width = (size - y);
                for (int x = -width / 2; x <= width / 2; x++)
                {
                    SetPixelSafe(texture, cx + x, cy + y - size / 2, color);
                }
            }
        }

        private static void DrawArc(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = 0; x <= radius; x++)
                {
                    int distSq = x * x + y * y;
                    int innerSq = (radius - 2) * (radius - 2);
                    int outerSq = radius * radius;

                    if (distSq >= innerSq && distSq <= outerSq && x >= 0)
                    {
                        SetPixelSafe(texture, cx + x, cy + y, color);
                    }
                }
            }
        }

        private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixelSafe(texture, x0, y0, color);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private static void SetPixelSafe(Texture2D texture, int x, int y, Color color)
        {
            if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
            {
                // アルファブレンディング
                Color existing = texture.GetPixel(x, y);
                Color blended = Color.Lerp(existing, color, color.a);
                blended.a = Mathf.Max(existing.a, color.a);
                texture.SetPixel(x, y, blended);
            }
        }

        #endregion
    }

    /// <summary>
    /// ユニットタイプ
    /// </summary>
    public enum UnitType
    {
        Infantry,   // 歩兵
        Cavalry,    // 騎兵
        Archer,     // 弓兵
        General     // 将軍
    }

    /// <summary>
    /// エフェクトタイプ
    /// </summary>
    public enum EffectType
    {
        Slash,      // 斬撃
        Impact,     // 衝撃
        Arrow,      // 矢
        Magic       // 魔法
    }
}
