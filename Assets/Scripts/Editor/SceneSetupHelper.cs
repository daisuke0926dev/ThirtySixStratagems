using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace ThirtySixStratagems.Editor
{
    /// <summary>
    /// シーンセットアップヘルパー
    /// 各シーンの基本構成を自動生成
    /// </summary>
    public class SceneSetupHelper : EditorWindow
    {
        private enum SceneType
        {
            Title,
            MainMenu,
            Game,
            Battle
        }

        private SceneType _selectedSceneType = SceneType.Title;
        private bool _createCanvas = true;
        private bool _createEventSystem = true;
        private bool _createCamera = true;

        [MenuItem("ThirtySixStratagems/Scene Setup Helper")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSetupHelper>("Scene Setup Helper");
            window.minSize = new Vector2(350, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("シーンセットアップヘルパー", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "現在のシーンに必要なオブジェクトを自動生成します。\n" +
                "既存のオブジェクトがある場合はスキップします。",
                MessageType.Info);

            EditorGUILayout.Space(10);

            _selectedSceneType = (SceneType)EditorGUILayout.EnumPopup("シーンタイプ", _selectedSceneType);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("生成オプション", EditorStyles.boldLabel);
            _createCanvas = EditorGUILayout.Toggle("UI Canvas", _createCanvas);
            _createEventSystem = EditorGUILayout.Toggle("Event System", _createEventSystem);
            _createCamera = EditorGUILayout.Toggle("Main Camera", _createCamera);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("シーンをセットアップ", GUILayout.Height(40)))
            {
                SetupScene();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("個別生成", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Managers"))
            {
                CreateManagersObject();
            }
            if (GUILayout.Button("UI Canvas"))
            {
                CreateUICanvas();
            }
            if (GUILayout.Button("EventSystem"))
            {
                CreateEventSystem();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SetupScene()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Scene Setup");

            // 基本オブジェクト
            if (_createCamera && !FindObjectOfType<Camera>())
            {
                CreateMainCamera();
            }

            if (_createEventSystem && !FindObjectOfType<EventSystem>())
            {
                CreateEventSystem();
            }

            if (_createCanvas && !GameObject.Find("Canvas"))
            {
                CreateUICanvas();
            }

            // シーン固有のセットアップ
            switch (_selectedSceneType)
            {
                case SceneType.Title:
                    SetupTitleScene();
                    break;
                case SceneType.MainMenu:
                    SetupMainMenuScene();
                    break;
                case SceneType.Game:
                    SetupGameScene();
                    break;
                case SceneType.Battle:
                    SetupBattleScene();
                    break;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"{_selectedSceneType}シーンのセットアップが完了しました");
        }

        #region Basic Objects

        private void CreateMainCamera()
        {
            var cameraObj = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(cameraObj, "Create Main Camera");

            var camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5;

            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";

            Debug.Log("Main Camera created");
        }

        private void CreateEventSystem()
        {
            if (FindObjectOfType<EventSystem>())
            {
                Debug.Log("EventSystem already exists");
                return;
            }

            var eventSystemObj = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");

            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            Debug.Log("EventSystem created");
        }

        private void CreateUICanvas()
        {
            var canvasObj = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("UI Canvas created");

            return;
        }

        private void CreateManagersObject()
        {
            if (GameObject.Find("Managers"))
            {
                Debug.Log("Managers object already exists");
                return;
            }

            var managersObj = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(managersObj, "Create Managers");

            Debug.Log("Managers object created - add manager scripts in Inspector");
        }

        #endregion

        #region Scene-Specific Setup

        private void SetupTitleScene()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            // タイトルパネル
            if (!canvas.transform.Find("TitlePanel"))
            {
                var titlePanel = CreateUIPanel(canvas, "TitlePanel");

                // タイトルテキスト
                var titleText = CreateUIText(titlePanel, "TitleText", "三十六計", 72);
                var titleRect = titleText.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0.7f);
                titleRect.anchorMax = new Vector2(0.5f, 0.7f);
                titleRect.anchoredPosition = Vector2.zero;

                // サブタイトル
                var subText = CreateUIText(titlePanel, "SubtitleText", "戦略シミュレーション", 36);
                var subRect = subText.GetComponent<RectTransform>();
                subRect.anchorMin = new Vector2(0.5f, 0.55f);
                subRect.anchorMax = new Vector2(0.5f, 0.55f);
                subRect.anchoredPosition = Vector2.zero;

                // スタートボタン
                var startButton = CreateUIButton(titlePanel, "StartButton", "ゲームを始める");
                var buttonRect = startButton.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
                buttonRect.anchoredPosition = Vector2.zero;
                buttonRect.sizeDelta = new Vector2(300, 60);
            }

            Debug.Log("Title scene UI created");
        }

        private void SetupMainMenuScene()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            // メインメニューパネル
            if (!canvas.transform.Find("MainMenuPanel"))
            {
                var menuPanel = CreateUIPanel(canvas, "MainMenuPanel");

                string[] buttonNames = { "NewGameButton", "ContinueButton", "StratagemLibraryButton", "SettingsButton", "ExitButton" };
                string[] buttonLabels = { "新規ゲーム", "コンティニュー", "計略図鑑", "設定", "終了" };

                for (int i = 0; i < buttonNames.Length; i++)
                {
                    var button = CreateUIButton(menuPanel, buttonNames[i], buttonLabels[i]);
                    var rect = button.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.7f - i * 0.12f);
                    rect.anchorMax = new Vector2(0.5f, 0.7f - i * 0.12f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(300, 50);
                }
            }

            Debug.Log("Main menu scene UI created");
        }

        private void SetupGameScene()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            // HUDパネル
            if (!canvas.transform.Find("HUDPanel"))
            {
                var hudPanel = CreateUIPanel(canvas, "HUDPanel");

                // 上部情報バー
                var topBar = CreateUIPanel(hudPanel, "TopBar");
                var topRect = topBar.GetComponent<RectTransform>();
                topRect.anchorMin = new Vector2(0, 1);
                topRect.anchorMax = new Vector2(1, 1);
                topRect.pivot = new Vector2(0.5f, 1);
                topRect.sizeDelta = new Vector2(0, 80);
                topRect.anchoredPosition = Vector2.zero;

                // リソース表示
                var resourceText = CreateUIText(topBar, "ResourceText", "金: 1000  兵糧: 500  計略P: 10", 24);
                var resRect = resourceText.GetComponent<RectTransform>();
                resRect.anchorMin = new Vector2(0, 0.5f);
                resRect.anchorMax = new Vector2(0, 0.5f);
                resRect.anchoredPosition = new Vector2(200, 0);

                // ターン表示
                var turnText = CreateUIText(topBar, "TurnText", "第1ターン", 24);
                var turnRect = turnText.GetComponent<RectTransform>();
                turnRect.anchorMin = new Vector2(1, 0.5f);
                turnRect.anchorMax = new Vector2(1, 0.5f);
                turnRect.anchoredPosition = new Vector2(-100, 0);

                // 下部コマンドバー
                var bottomBar = CreateUIPanel(hudPanel, "BottomBar");
                var bottomRect = bottomBar.GetComponent<RectTransform>();
                bottomRect.anchorMin = new Vector2(0, 0);
                bottomRect.anchorMax = new Vector2(1, 0);
                bottomRect.pivot = new Vector2(0.5f, 0);
                bottomRect.sizeDelta = new Vector2(0, 100);
                bottomRect.anchoredPosition = Vector2.zero;

                // ターン終了ボタン
                var endTurnButton = CreateUIButton(bottomBar, "EndTurnButton", "ターン終了");
                var endRect = endTurnButton.GetComponent<RectTransform>();
                endRect.anchorMin = new Vector2(1, 0.5f);
                endRect.anchorMax = new Vector2(1, 0.5f);
                endRect.anchoredPosition = new Vector2(-100, 0);
                endRect.sizeDelta = new Vector2(150, 50);
            }

            // マップコンテナ
            if (!GameObject.Find("MapContainer"))
            {
                var mapContainer = new GameObject("MapContainer");
                Undo.RegisterCreatedObjectUndo(mapContainer, "Create MapContainer");
            }

            Debug.Log("Game scene UI created");
        }

        private void SetupBattleScene()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            // 戦闘UIパネル
            if (!canvas.transform.Find("BattlePanel"))
            {
                var battlePanel = CreateUIPanel(canvas, "BattlePanel");

                // 攻撃側表示
                var attackerPanel = CreateUIPanel(battlePanel, "AttackerPanel");
                var attackerRect = attackerPanel.GetComponent<RectTransform>();
                attackerRect.anchorMin = new Vector2(0, 0.3f);
                attackerRect.anchorMax = new Vector2(0.4f, 0.9f);
                attackerRect.offsetMin = new Vector2(20, 0);
                attackerRect.offsetMax = new Vector2(0, -20);

                var attackerLabel = CreateUIText(attackerPanel, "AttackerLabel", "攻撃側", 32);

                // 防御側表示
                var defenderPanel = CreateUIPanel(battlePanel, "DefenderPanel");
                var defenderRect = defenderPanel.GetComponent<RectTransform>();
                defenderRect.anchorMin = new Vector2(0.6f, 0.3f);
                defenderRect.anchorMax = new Vector2(1f, 0.9f);
                defenderRect.offsetMin = new Vector2(0, 0);
                defenderRect.offsetMax = new Vector2(-20, -20);

                var defenderLabel = CreateUIText(defenderPanel, "DefenderLabel", "防御側", 32);

                // 戦闘アクションボタン
                var actionPanel = CreateUIPanel(battlePanel, "ActionPanel");
                var actionRect = actionPanel.GetComponent<RectTransform>();
                actionRect.anchorMin = new Vector2(0, 0);
                actionRect.anchorMax = new Vector2(1, 0.25f);
                actionRect.offsetMin = new Vector2(20, 20);
                actionRect.offsetMax = new Vector2(-20, 0);

                var attackButton = CreateUIButton(actionPanel, "AttackButton", "攻撃");
                var atkRect = attackButton.GetComponent<RectTransform>();
                atkRect.anchorMin = new Vector2(0.2f, 0.5f);
                atkRect.anchorMax = new Vector2(0.2f, 0.5f);
                atkRect.sizeDelta = new Vector2(150, 50);

                var stratagemButton = CreateUIButton(actionPanel, "StratagemButton", "計略");
                var strRect = stratagemButton.GetComponent<RectTransform>();
                strRect.anchorMin = new Vector2(0.5f, 0.5f);
                strRect.anchorMax = new Vector2(0.5f, 0.5f);
                strRect.sizeDelta = new Vector2(150, 50);

                var retreatButton = CreateUIButton(actionPanel, "RetreatButton", "撤退");
                var retRect = retreatButton.GetComponent<RectTransform>();
                retRect.anchorMin = new Vector2(0.8f, 0.5f);
                retRect.anchorMax = new Vector2(0.8f, 0.5f);
                retRect.sizeDelta = new Vector2(150, 50);
            }

            Debug.Log("Battle scene UI created");
        }

        #endregion

        #region UI Helpers

        private Canvas FindCanvas()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found. Please create Canvas first.");
            }
            return canvas;
        }

        private GameObject CreateUIPanel(Component parent, string name)
        {
            var panelObj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(panelObj, $"Create {name}");
            panelObj.transform.SetParent(parent.transform, false);

            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = panelObj.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            return panelObj;
        }

        private GameObject CreateUIText(Component parent, string name, string text, int fontSize)
        {
            var textObj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(textObj, $"Create {name}");
            textObj.transform.SetParent(parent.transform, false);

            var rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 100);

            // Try to use TextMeshPro if available
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            return textObj;
        }

        private GameObject CreateUIButton(Component parent, string name, string label)
        {
            var buttonObj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(buttonObj, $"Create {name}");
            buttonObj.transform.SetParent(parent.transform, false);

            var rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);

            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.3f, 0.5f, 1f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // Button colors
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.3f, 0.5f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.6f, 1f);
            colors.pressedColor = new Color(0.15f, 0.25f, 0.4f, 1f);
            button.colors = colors;

            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(buttonObj.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmpText = labelObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = label;
            tmpText.fontSize = 24;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            return buttonObj;
        }

        #endregion
    }
}
