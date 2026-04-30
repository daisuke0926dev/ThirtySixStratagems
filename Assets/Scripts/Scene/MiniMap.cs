using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// ミニマップ
    /// マップ全体の俯瞰表示とクイックナビゲーション
    /// </summary>
    public class MiniMap : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private RectTransform _miniMapRect;
        [SerializeField] private RawImage _miniMapImage;
        [SerializeField] private RectTransform _viewportIndicator;
        [SerializeField] private float _mapPadding = 0.1f;

        [Header("色設定")]
        [SerializeField] private Color _playerColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _enemyColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _neutralColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _backgroundColor = new Color(0.1f, 0.15f, 0.1f, 0.8f);
        [SerializeField] private Color _connectionColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("サイズ設定")]
        [SerializeField] private int _textureWidth = 200;
        [SerializeField] private int _textureHeight = 150;
        [SerializeField] private int _territoryRadius = 6;

        // 内部状態
        private Texture2D _miniMapTexture;
        private GameData _gameData;
        private Camera _mainCamera;
        private Vector2 _mapMin;
        private Vector2 _mapMax;
        private bool _isInitialized;

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(GameData gameData, Camera mainCamera)
        {
            _gameData = gameData;
            _mainCamera = mainCamera;

            if (gameData == null || gameData.Territories == null)
            {
                Debug.LogWarning("MiniMap: GameData is null");
                return;
            }

            CalculateMapBounds();
            CreateMiniMapTexture();
            _isInitialized = true;

            RefreshMiniMap();
        }

        private void Update()
        {
            if (_isInitialized && _viewportIndicator != null && _mainCamera != null)
            {
                UpdateViewportIndicator();
            }
        }

        /// <summary>
        /// マップの境界を計算
        /// </summary>
        private void CalculateMapBounds()
        {
            if (_gameData?.Territories == null || _gameData.Territories.Count == 0)
            {
                _mapMin = Vector2.zero;
                _mapMax = Vector2.one * 10f;
                return;
            }

            _mapMin = new Vector2(float.MaxValue, float.MaxValue);
            _mapMax = new Vector2(float.MinValue, float.MinValue);

            foreach (var territory in _gameData.Territories.Values)
            {
                _mapMin.x = Mathf.Min(_mapMin.x, territory.MapPosition.x);
                _mapMin.y = Mathf.Min(_mapMin.y, territory.MapPosition.y);
                _mapMax.x = Mathf.Max(_mapMax.x, territory.MapPosition.x);
                _mapMax.y = Mathf.Max(_mapMax.y, territory.MapPosition.y);
            }

            // パディングを追加
            Vector2 size = _mapMax - _mapMin;
            Vector2 padding = size * _mapPadding;
            _mapMin -= padding;
            _mapMax += padding;
        }

        /// <summary>
        /// ミニマップテクスチャを作成
        /// </summary>
        private void CreateMiniMapTexture()
        {
            _miniMapTexture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGBA32, false);
            _miniMapTexture.filterMode = FilterMode.Point;

            if (_miniMapImage != null)
            {
                _miniMapImage.texture = _miniMapTexture;
            }
        }

        /// <summary>
        /// ミニマップを更新
        /// </summary>
        public void RefreshMiniMap()
        {
            if (!_isInitialized || _miniMapTexture == null) return;

            // 背景をクリア
            ClearTexture(_backgroundColor);

            // 接続線を描画
            DrawConnections();

            // 領地を描画
            DrawTerritories();

            _miniMapTexture.Apply();
        }

        /// <summary>
        /// テクスチャをクリア
        /// </summary>
        private void ClearTexture(Color color)
        {
            Color[] pixels = new Color[_textureWidth * _textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            _miniMapTexture.SetPixels(pixels);
        }

        /// <summary>
        /// 接続線を描画
        /// </summary>
        private void DrawConnections()
        {
            if (_gameData?.Territories == null) return;

            HashSet<string> drawnConnections = new HashSet<string>();

            foreach (var territory in _gameData.Territories.Values)
            {
                Vector2Int fromPos = WorldToMiniMapPosition(territory.MapPosition);

                foreach (var adjId in territory.AdjacentTerritoryIds)
                {
                    string connectionKey = GetConnectionKey(territory.Id, adjId);
                    if (drawnConnections.Contains(connectionKey)) continue;

                    var adjTerritory = _gameData.Territories.GetValueOrDefault(adjId);
                    if (adjTerritory == null) continue;

                    Vector2Int toPos = WorldToMiniMapPosition(adjTerritory.MapPosition);
                    DrawLine(fromPos, toPos, _connectionColor);

                    drawnConnections.Add(connectionKey);
                }
            }
        }

        /// <summary>
        /// 領地を描画
        /// </summary>
        private void DrawTerritories()
        {
            if (_gameData?.Territories == null) return;

            foreach (var territory in _gameData.Territories.Values)
            {
                Vector2Int pos = WorldToMiniMapPosition(territory.MapPosition);
                Color color = GetFactionColor(territory.OwnerId);
                DrawCircle(pos, _territoryRadius, color);
            }
        }

        /// <summary>
        /// 円を描画
        /// </summary>
        private void DrawCircle(Vector2Int center, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int px = center.x + x;
                        int py = center.y + y;

                        if (px >= 0 && px < _textureWidth && py >= 0 && py < _textureHeight)
                        {
                            // 外周は暗くする
                            float dist = Mathf.Sqrt(x * x + y * y);
                            if (dist > radius - 1)
                            {
                                _miniMapTexture.SetPixel(px, py, color * 0.6f);
                            }
                            else
                            {
                                _miniMapTexture.SetPixel(px, py, color);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 線を描画
        /// </summary>
        private void DrawLine(Vector2Int from, Vector2Int to, Color color)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            int sx = from.x < to.x ? 1 : -1;
            int sy = from.y < to.y ? 1 : -1;
            int err = dx - dy;

            int x = from.x;
            int y = from.y;

            while (true)
            {
                if (x >= 0 && x < _textureWidth && y >= 0 && y < _textureHeight)
                {
                    _miniMapTexture.SetPixel(x, y, color);
                }

                if (x == to.x && y == to.y) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }

        /// <summary>
        /// ワールド座標をミニマップ座標に変換
        /// </summary>
        private Vector2Int WorldToMiniMapPosition(Vector2 worldPos)
        {
            float normalizedX = (worldPos.x - _mapMin.x) / (_mapMax.x - _mapMin.x);
            float normalizedY = (worldPos.y - _mapMin.y) / (_mapMax.y - _mapMin.y);

            int x = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (_textureWidth - 1)), 0, _textureWidth - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(normalizedY * (_textureHeight - 1)), 0, _textureHeight - 1);

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// ビューポートインジケーターを更新
        /// </summary>
        private void UpdateViewportIndicator()
        {
            if (_viewportIndicator == null || _mainCamera == null || _miniMapRect == null) return;

            // カメラの表示範囲を計算
            float cameraHeight = _mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * _mainCamera.aspect;
            Vector3 cameraPos = _mainCamera.transform.position;

            // ミニマップ上の位置とサイズを計算
            float normalizedX = (cameraPos.x - _mapMin.x) / (_mapMax.x - _mapMin.x);
            float normalizedY = (cameraPos.y - _mapMin.y) / (_mapMax.y - _mapMin.y);

            float normalizedWidth = cameraWidth / (_mapMax.x - _mapMin.x);
            float normalizedHeight = cameraHeight / (_mapMax.y - _mapMin.y);

            // RectTransformに適用
            Vector2 miniMapSize = _miniMapRect.sizeDelta;
            _viewportIndicator.anchoredPosition = new Vector2(
                (normalizedX - 0.5f) * miniMapSize.x,
                (normalizedY - 0.5f) * miniMapSize.y
            );
            _viewportIndicator.sizeDelta = new Vector2(
                normalizedWidth * miniMapSize.x,
                normalizedHeight * miniMapSize.y
            );
        }

        /// <summary>
        /// 勢力の色を取得
        /// </summary>
        private Color GetFactionColor(string factionId)
        {
            if (string.IsNullOrEmpty(factionId))
            {
                return _neutralColor;
            }

            var faction = GameManager.Instance?.GetFaction(factionId);
            if (faction == null)
            {
                return _neutralColor;
            }

            if (faction.IsPlayer)
            {
                return _playerColor;
            }

            if (faction.Color != default)
            {
                return faction.Color;
            }

            return _enemyColor;
        }

        /// <summary>
        /// 接続キーを取得
        /// </summary>
        private string GetConnectionKey(string id1, string id2)
        {
            return string.Compare(id1, id2) < 0 ? $"{id1}_{id2}" : $"{id2}_{id1}";
        }

        /// <summary>
        /// ミニマップクリック時の処理
        /// </summary>
        public void OnMiniMapClicked(Vector2 localPosition)
        {
            if (!_isInitialized || _mainCamera == null || _miniMapRect == null) return;

            // ミニマップ上の正規化座標を計算
            Vector2 miniMapSize = _miniMapRect.sizeDelta;
            float normalizedX = (localPosition.x / miniMapSize.x) + 0.5f;
            float normalizedY = (localPosition.y / miniMapSize.y) + 0.5f;

            // ワールド座標に変換
            float worldX = _mapMin.x + normalizedX * (_mapMax.x - _mapMin.x);
            float worldY = _mapMin.y + normalizedY * (_mapMax.y - _mapMin.y);

            // カメラを移動
            Vector3 newPos = _mainCamera.transform.position;
            newPos.x = worldX;
            newPos.y = worldY;
            _mainCamera.transform.position = newPos;
        }

        private void OnDestroy()
        {
            if (_miniMapTexture != null)
            {
                Destroy(_miniMapTexture);
            }
        }
    }
}
