using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ThirtySixStratagems.Editor
{
    /// <summary>
    /// ビルド設定管理
    /// マルチプラットフォームビルドの設定と実行
    /// </summary>
    public class BuildConfiguration : EditorWindow
    {
        private const string BUILD_OUTPUT_PATH = "Builds";
        private const string PRODUCT_NAME = "三十六計 〜天下統一への道〜";
        private const string BUNDLE_IDENTIFIER = "com.thirtysixstratagems.game";

        private Vector2 _scrollPosition;
        private BuildTarget _selectedTarget = BuildTarget.StandaloneWindows64;
        private bool _developmentBuild = false;
        private bool _autoConnectProfiler = false;
        private bool _deepProfiling = false;
        private bool _scriptDebugging = false;
        private bool _compressWithLz4 = true;
        private string _customBuildPath = "";

        // ビルドするシーンのリスト
        private static readonly string[] _scenes = new string[]
        {
            "Assets/Scenes/TitleScene.unity",
            "Assets/Scenes/MainMenuScene.unity",
            "Assets/Scenes/GameScene.unity",
            "Assets/Scenes/BattleScene.unity"
        };

        [MenuItem("三十六計/Build Configuration", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildConfiguration>("Build Configuration");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawPlatformSelection();
            EditorGUILayout.Space(10);

            DrawBuildOptions();
            EditorGUILayout.Space(10);

            DrawOutputSettings();
            EditorGUILayout.Space(10);

            DrawSceneList();
            EditorGUILayout.Space(10);

            DrawBuildButtons();
            EditorGUILayout.Space(10);

            DrawQuickBuildButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("三十六計 〜天下統一への道〜", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Version: {Application.version}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Unity: {Application.unityVersion}", EditorStyles.miniLabel);
        }

        private void DrawPlatformSelection()
        {
            EditorGUILayout.LabelField("ターゲットプラットフォーム", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            _selectedTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _selectedTarget);

            // プラットフォーム固有の情報表示
            EditorGUILayout.HelpBox(GetPlatformInfo(_selectedTarget), MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildOptions()
        {
            EditorGUILayout.LabelField("ビルドオプション", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            _developmentBuild = EditorGUILayout.Toggle("Development Build", _developmentBuild);

            EditorGUI.BeginDisabledGroup(!_developmentBuild);
            _autoConnectProfiler = EditorGUILayout.Toggle("Auto Connect Profiler", _autoConnectProfiler);
            _deepProfiling = EditorGUILayout.Toggle("Deep Profiling", _deepProfiling);
            _scriptDebugging = EditorGUILayout.Toggle("Script Debugging", _scriptDebugging);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);
            _compressWithLz4 = EditorGUILayout.Toggle("Compress with LZ4", _compressWithLz4);

            EditorGUILayout.EndVertical();
        }

        private void DrawOutputSettings()
        {
            EditorGUILayout.LabelField("出力設定", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"デフォルト出力先: {BUILD_OUTPUT_PATH}/", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            _customBuildPath = EditorGUILayout.TextField("カスタムパス", _customBuildPath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                var path = EditorUtility.OpenFolderPanel("出力フォルダを選択", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _customBuildPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawSceneList()
        {
            EditorGUILayout.LabelField("ビルドに含まれるシーン", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            for (int i = 0; i < _scenes.Length; i++)
            {
                EditorGUILayout.LabelField($"{i}: {_scenes[i]}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildButtons()
        {
            EditorGUILayout.LabelField("ビルド実行", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("ビルド", GUILayout.Height(30)))
            {
                PerformBuild(_selectedTarget);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ビルド & 実行"))
            {
                PerformBuildAndRun(_selectedTarget);
            }
            if (GUILayout.Button("出力フォルダを開く"))
            {
                OpenBuildFolder();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickBuildButtons()
        {
            EditorGUILayout.LabelField("クイックビルド", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Windows x64"))
            {
                PerformBuild(BuildTarget.StandaloneWindows64);
            }
            if (GUILayout.Button("macOS"))
            {
                PerformBuild(BuildTarget.StandaloneOSX);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("WebGL"))
            {
                PerformBuild(BuildTarget.WebGL);
            }
            if (GUILayout.Button("Android"))
            {
                PerformBuild(BuildTarget.Android);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("全プラットフォームビルド", GUILayout.Height(25)))
            {
                PerformAllPlatformBuilds();
            }

            EditorGUILayout.EndVertical();
        }

        private string GetPlatformInfo(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    return "Windows 64-bit スタンドアロン実行ファイル (.exe)";
                case BuildTarget.StandaloneWindows:
                    return "Windows 32-bit スタンドアロン実行ファイル (.exe)";
                case BuildTarget.StandaloneOSX:
                    return "macOS アプリケーションバンドル (.app)";
                case BuildTarget.WebGL:
                    return "WebGL ビルド - ブラウザで実行可能";
                case BuildTarget.Android:
                    return "Android APK/AAB パッケージ";
                case BuildTarget.iOS:
                    return "iOS Xcode プロジェクト";
                default:
                    return "プラットフォーム情報なし";
            }
        }

        private void PerformBuild(BuildTarget target)
        {
            string outputPath = GetOutputPath(target);
            var options = GetBuildOptions();

            Debug.Log($"[BuildConfiguration] Starting build for {target}...");
            Debug.Log($"[BuildConfiguration] Output path: {outputPath}");

            var report = BuildPipeline.BuildPlayer(_scenes, outputPath, target, options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildConfiguration] Build succeeded! Time: {report.summary.totalTime}");
                EditorUtility.DisplayDialog("ビルド完了", $"ビルドが完了しました。\n出力先: {outputPath}", "OK");
            }
            else
            {
                Debug.LogError($"[BuildConfiguration] Build failed: {report.summary.result}");
                EditorUtility.DisplayDialog("ビルド失敗", $"ビルドに失敗しました。\nエラー: {report.summary.result}", "OK");
            }
        }

        private void PerformBuildAndRun(BuildTarget target)
        {
            string outputPath = GetOutputPath(target);
            var options = GetBuildOptions() | BuildOptions.AutoRunPlayer;

            var report = BuildPipeline.BuildPlayer(_scenes, outputPath, target, options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildConfiguration] Build and run failed: {report.summary.result}");
            }
        }

        private void PerformAllPlatformBuilds()
        {
            var targets = new BuildTarget[]
            {
                BuildTarget.StandaloneWindows64,
                BuildTarget.StandaloneOSX,
                BuildTarget.WebGL
            };

            int successCount = 0;
            var results = new List<string>();

            foreach (var target in targets)
            {
                try
                {
                    string outputPath = GetOutputPath(target);
                    var options = GetBuildOptions();

                    Debug.Log($"[BuildConfiguration] Building for {target}...");

                    var report = BuildPipeline.BuildPlayer(_scenes, outputPath, target, options);

                    if (report.summary.result == BuildResult.Succeeded)
                    {
                        successCount++;
                        results.Add($"✓ {target}: 成功");
                    }
                    else
                    {
                        results.Add($"✗ {target}: 失敗 ({report.summary.result})");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"✗ {target}: エラー ({ex.Message})");
                }
            }

            string resultMessage = $"ビルド結果: {successCount}/{targets.Length} 成功\n\n" +
                                   string.Join("\n", results);

            EditorUtility.DisplayDialog("全プラットフォームビルド完了", resultMessage, "OK");
        }

        private string GetOutputPath(BuildTarget target)
        {
            string basePath = string.IsNullOrEmpty(_customBuildPath)
                ? Path.Combine(Application.dataPath, "..", BUILD_OUTPUT_PATH)
                : _customBuildPath;

            string platformFolder = GetPlatformFolderName(target);
            string fileName = GetBuildFileName(target);

            return Path.Combine(basePath, platformFolder, fileName);
        }

        private string GetPlatformFolderName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "macOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                default:
                    return target.ToString();
            }
        }

        private string GetBuildFileName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    return "ThirtySixStratagems.exe";
                case BuildTarget.StandaloneOSX:
                    return "ThirtySixStratagems.app";
                case BuildTarget.WebGL:
                    return "";  // WebGLはフォルダ
                case BuildTarget.Android:
                    return "ThirtySixStratagems.apk";
                case BuildTarget.iOS:
                    return "";  // iOSはXcodeプロジェクト
                default:
                    return "ThirtySixStratagems";
            }
        }

        private BuildOptions GetBuildOptions()
        {
            var options = BuildOptions.None;

            if (_developmentBuild)
            {
                options |= BuildOptions.Development;

                if (_autoConnectProfiler)
                    options |= BuildOptions.ConnectWithProfiler;

                if (_deepProfiling)
                    options |= BuildOptions.EnableDeepProfilingSupport;

                if (_scriptDebugging)
                    options |= BuildOptions.AllowDebugging;
            }

            if (_compressWithLz4)
            {
                options |= BuildOptions.CompressWithLz4;
            }

            return options;
        }

        private void OpenBuildFolder()
        {
            string basePath = string.IsNullOrEmpty(_customBuildPath)
                ? Path.Combine(Application.dataPath, "..", BUILD_OUTPUT_PATH)
                : _customBuildPath;

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            EditorUtility.RevealInFinder(basePath);
        }

        /// <summary>
        /// コマンドラインからのビルド用メソッド
        /// </summary>
        public static void BuildFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var targetArg = Array.Find(args, a => a.StartsWith("-buildTarget="));

            BuildTarget target = BuildTarget.StandaloneWindows64;
            if (!string.IsNullOrEmpty(targetArg))
            {
                var targetName = targetArg.Replace("-buildTarget=", "");
                target = (BuildTarget)Enum.Parse(typeof(BuildTarget), targetName);
            }

            var outputPath = GetCommandLineBuildPath(target);
            var options = BuildOptions.None;

            var devBuild = Array.Exists(args, a => a == "-development");
            if (devBuild)
            {
                options |= BuildOptions.Development;
            }

            var report = BuildPipeline.BuildPlayer(_scenes, outputPath, target, options);

            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static string GetCommandLineBuildPath(BuildTarget target)
        {
            var args = Environment.GetCommandLineArgs();
            var outputArg = Array.Find(args, a => a.StartsWith("-output="));

            if (!string.IsNullOrEmpty(outputArg))
            {
                return outputArg.Replace("-output=", "");
            }

            return Path.Combine(BUILD_OUTPUT_PATH, target.ToString(), "ThirtySixStratagems");
        }
    }
}
