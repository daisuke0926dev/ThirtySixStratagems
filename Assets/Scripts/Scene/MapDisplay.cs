using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Core;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Scene
{
    /// <summary>
    /// マップ表示
    /// ゲームマップの描画と領地の視覚的表示を管理
    /// </summary>
    public class MapDisplay : MonoBehaviour
    {
        [Header("プレハブ")]
        [SerializeField] private GameObject _territoryPrefab;
        [SerializeField] private GameObject _connectionLinePrefab;

        [Header("マップ設定")]
        [SerializeField] private Transform _territoriesContainer;
        [SerializeField] private Transform _connectionsContainer;
        [SerializeField] private float _territorySpacing = 2f;

        [Header("色設定")]
        [SerializeField] private Color _playerColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _enemyColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _neutralColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color _selectedOutlineColor = Color.yellow;
        [SerializeField] private Color _adjacentHighlightColor = new Color(1f, 1f, 0.5f, 0.5f);

        // 内部状態
        private Dictionary<string, TerritoryVisual> _territoryVisuals = new Dictionary<string, TerritoryVisual>();
        private Dictionary<string, LineRenderer> _connectionLines = new Dictionary<string, LineRenderer>();
        private string _selectedTerritoryId;
        private GameData _gameData;

        /// <summary>
        /// マップを初期化
        /// </summary>
        public void Initialize(GameData gameData)
        {
            _gameData = gameData;

            if (gameData == null)
            {
                Debug.LogError("GameData is null. Cannot initialize map.");
                return;
            }

            ClearMap();
            CreateTerritories();
            CreateConnections();

            Debug.Log($"Map initialized with {_territoryVisuals.Count} territories");
        }

        /// <summary>
        /// マップをクリア
        /// </summary>
        private void ClearMap()
        {
            // 既存の領地をクリア
            foreach (var visual in _territoryVisuals.Values)
            {
                if (visual != null)
                {
                    Destroy(visual.gameObject);
                }
            }
            _territoryVisuals.Clear();

            // 既存の接続線をクリア
            foreach (var line in _connectionLines.Values)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            _connectionLines.Clear();
        }

        /// <summary>
        /// 領地を作成
        /// </summary>
        private void CreateTerritories()
        {
            if (_gameData?.Territories == null) return;

            foreach (var territory in _gameData.Territories.Values)
            {
                CreateTerritoryVisual(territory);
            }
        }

        /// <summary>
        /// 領地ビジュアルを作成
        /// </summary>
        private void CreateTerritoryVisual(Territory territory)
        {
            if (territory == null) return;

            Vector3 position = new Vector3(territory.MapPosition.x, territory.MapPosition.y, 0);

            // プレハブがない場合は基本的な形状を作成
            GameObject territoryObj;
            if (_territoryPrefab != null)
            {
                territoryObj = Instantiate(_territoryPrefab, position, Quaternion.identity, _territoriesContainer);
            }
            else
            {
                // デフォルトの六角形を作成
                territoryObj = CreateDefaultTerritoryObject(position);
            }

            territoryObj.name = $"Territory_{territory.Id}";

            // TerritoryVisualコンポーネントを追加/取得
            var visual = territoryObj.GetComponent<TerritoryVisual>();
            if (visual == null)
            {
                visual = territoryObj.AddComponent<TerritoryVisual>();
            }

            // 初期化
            visual.Initialize(territory.Id, territory.Name);
            visual.SetColor(GetFactionColor(territory.OwnerId));

            _territoryVisuals[territory.Id] = visual;
        }

        /// <summary>
        /// デフォルトの領地オブジェクトを作成
        /// </summary>
        private GameObject CreateDefaultTerritoryObject(Vector3 position)
        {
            GameObject obj = new GameObject();
            obj.transform.position = position;
            obj.transform.SetParent(_territoriesContainer);

            // スプライトレンダラーを追加
            var spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateCircleSprite();
            spriteRenderer.sortingOrder = 1;

            // コライダーを追加
            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            // ラベル用の子オブジェクト
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform);
            labelObj.transform.localPosition = new Vector3(0, -0.7f, 0);

            var textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.characterSize = 0.15f;
            textMesh.fontSize = 40;
            textMesh.color = Color.white;

            return obj;
        }

        /// <summary>
        /// 円形スプライトを作成
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            int size = 128;
            Texture2D texture = new Texture2D(size, size);

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < radius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else if (dist < radius + 2)
                    {
                        // 境界線
                        texture.SetPixel(x, y, new Color(0.8f, 0.8f, 0.8f, 1f));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>
        /// 隣接接続線を作成
        /// </summary>
        private void CreateConnections()
        {
            if (_gameData?.Territories == null) return;

            HashSet<string> createdConnections = new HashSet<string>();

            foreach (var territory in _gameData.Territories.Values)
            {
                foreach (var adjId in territory.AdjacentTerritoryIds)
                {
                    // 重複を避ける
                    string connectionKey = GetConnectionKey(territory.Id, adjId);
                    if (createdConnections.Contains(connectionKey)) continue;

                    CreateConnectionLine(territory.Id, adjId);
                    createdConnections.Add(connectionKey);
                }
            }
        }

        /// <summary>
        /// 接続線を作成
        /// </summary>
        private void CreateConnectionLine(string fromId, string toId)
        {
            if (!_territoryVisuals.ContainsKey(fromId) || !_territoryVisuals.ContainsKey(toId))
                return;

            var fromVisual = _territoryVisuals[fromId];
            var toVisual = _territoryVisuals[toId];

            GameObject lineObj;
            LineRenderer lineRenderer;

            if (_connectionLinePrefab != null)
            {
                lineObj = Instantiate(_connectionLinePrefab, _connectionsContainer);
                lineRenderer = lineObj.GetComponent<LineRenderer>();
            }
            else
            {
                lineObj = new GameObject($"Connection_{fromId}_{toId}");
                lineObj.transform.SetParent(_connectionsContainer);
                lineRenderer = lineObj.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                lineRenderer.endColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;
                lineRenderer.sortingOrder = 0;
            }

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, fromVisual.transform.position);
            lineRenderer.SetPosition(1, toVisual.transform.position);

            string key = GetConnectionKey(fromId, toId);
            _connectionLines[key] = lineRenderer;
        }

        /// <summary>
        /// 接続キーを取得
        /// </summary>
        private string GetConnectionKey(string id1, string id2)
        {
            return string.Compare(id1, id2) < 0 ? $"{id1}_{id2}" : $"{id2}_{id1}";
        }

        #region Selection

        /// <summary>
        /// 領地を選択
        /// </summary>
        public void SelectTerritory(string territoryId)
        {
            // 前の選択を解除
            if (!string.IsNullOrEmpty(_selectedTerritoryId) && _territoryVisuals.ContainsKey(_selectedTerritoryId))
            {
                _territoryVisuals[_selectedTerritoryId].SetSelected(false);
                ClearAdjacentHighlight();
            }

            _selectedTerritoryId = territoryId;

            // 新しい選択を表示
            if (_territoryVisuals.TryGetValue(territoryId, out var visual))
            {
                visual.SetSelected(true);
                HighlightAdjacentTerritories(territoryId);
            }
        }

        /// <summary>
        /// 選択を解除
        /// </summary>
        public void DeselectTerritory()
        {
            if (!string.IsNullOrEmpty(_selectedTerritoryId) && _territoryVisuals.ContainsKey(_selectedTerritoryId))
            {
                _territoryVisuals[_selectedTerritoryId].SetSelected(false);
            }
            _selectedTerritoryId = null;
            ClearAdjacentHighlight();
        }

        /// <summary>
        /// 隣接領地をハイライト
        /// </summary>
        private void HighlightAdjacentTerritories(string territoryId)
        {
            var territory = _gameData?.Territories.GetValueOrDefault(territoryId);
            if (territory == null) return;

            foreach (var adjId in territory.AdjacentTerritoryIds)
            {
                if (_territoryVisuals.TryGetValue(adjId, out var visual))
                {
                    visual.SetHighlighted(true);
                }
            }
        }

        /// <summary>
        /// 隣接ハイライトをクリア
        /// </summary>
        private void ClearAdjacentHighlight()
        {
            foreach (var visual in _territoryVisuals.Values)
            {
                visual.SetHighlighted(false);
            }
        }

        #endregion

        #region Update Display

        /// <summary>
        /// 領地の所有者を更新
        /// </summary>
        public void UpdateTerritoryOwner(string territoryId, string newOwnerId)
        {
            if (_territoryVisuals.TryGetValue(territoryId, out var visual))
            {
                visual.SetColor(GetFactionColor(newOwnerId));
            }
        }

        /// <summary>
        /// 全体を更新
        /// </summary>
        public void RefreshAll()
        {
            foreach (var kvp in _territoryVisuals)
            {
                var territory = _gameData?.Territories.GetValueOrDefault(kvp.Key);
                if (territory != null)
                {
                    kvp.Value.SetColor(GetFactionColor(territory.OwnerId));
                }
            }
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

            // 勢力固有の色がある場合はそれを使用
            if (faction.Color != default)
            {
                return faction.Color;
            }

            return _enemyColor;
        }

        #endregion

        #region Public Access

        /// <summary>
        /// 領地ビジュアルを取得
        /// </summary>
        public TerritoryVisual GetTerritoryVisual(string territoryId)
        {
            return _territoryVisuals.GetValueOrDefault(territoryId);
        }

        /// <summary>
        /// 全領地ビジュアル
        /// </summary>
        public IReadOnlyDictionary<string, TerritoryVisual> TerritoryVisuals => _territoryVisuals;

        #endregion
    }
}
