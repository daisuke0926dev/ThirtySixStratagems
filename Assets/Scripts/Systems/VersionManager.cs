using System;
using UnityEngine;

namespace ThirtySixStratagems.Systems
{
    /// <summary>
    /// バージョン管理システム
    /// ゲームのバージョン情報と更新チェックを管理
    /// </summary>
    public class VersionManager : MonoBehaviour
    {
        private static VersionManager _instance;
        public static VersionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("VersionManager");
                    _instance = go.AddComponent<VersionManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("バージョン情報")]
        [SerializeField] private string _majorVersion = "1";
        [SerializeField] private string _minorVersion = "0";
        [SerializeField] private string _patchVersion = "0";
        [SerializeField] private string _buildNumber = "1";

        [Header("リリース情報")]
        [SerializeField] private ReleaseType _releaseType = ReleaseType.Release;
        [SerializeField] private string _buildDate = "";
        [SerializeField] private string _commitHash = "";

        /// <summary>
        /// フルバージョン文字列
        /// </summary>
        public string FullVersion => $"{_majorVersion}.{_minorVersion}.{_patchVersion}";

        /// <summary>
        /// ビルド番号を含むバージョン文字列
        /// </summary>
        public string FullVersionWithBuild => $"{FullVersion}.{_buildNumber}";

        /// <summary>
        /// 表示用バージョン文字列
        /// </summary>
        public string DisplayVersion
        {
            get
            {
                string version = FullVersion;
                if (_releaseType != ReleaseType.Release)
                {
                    version += $"-{GetReleaseTypeSuffix()}";
                }
                return version;
            }
        }

        /// <summary>
        /// メジャーバージョン
        /// </summary>
        public int Major => int.TryParse(_majorVersion, out int v) ? v : 0;

        /// <summary>
        /// マイナーバージョン
        /// </summary>
        public int Minor => int.TryParse(_minorVersion, out int v) ? v : 0;

        /// <summary>
        /// パッチバージョン
        /// </summary>
        public int Patch => int.TryParse(_patchVersion, out int v) ? v : 0;

        /// <summary>
        /// ビルド番号
        /// </summary>
        public int Build => int.TryParse(_buildNumber, out int v) ? v : 0;

        /// <summary>
        /// リリースタイプ
        /// </summary>
        public ReleaseType CurrentReleaseType => _releaseType;

        /// <summary>
        /// ビルド日時
        /// </summary>
        public string BuildDate => string.IsNullOrEmpty(_buildDate) ? DateTime.Now.ToString("yyyy-MM-dd") : _buildDate;

        /// <summary>
        /// コミットハッシュ
        /// </summary>
        public string CommitHash => _commitHash;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // ビルド日が未設定の場合は現在日時を設定
            if (string.IsNullOrEmpty(_buildDate))
            {
                _buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            Debug.Log($"[VersionManager] Game Version: {DisplayVersion} (Build {_buildNumber})");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private string GetReleaseTypeSuffix()
        {
            switch (_releaseType)
            {
                case ReleaseType.Alpha:
                    return "alpha";
                case ReleaseType.Beta:
                    return "beta";
                case ReleaseType.ReleaseCandidate:
                    return "rc";
                case ReleaseType.Development:
                    return "dev";
                default:
                    return "";
            }
        }

        /// <summary>
        /// バージョン情報を取得
        /// </summary>
        public VersionInfo GetVersionInfo()
        {
            return new VersionInfo
            {
                Major = Major,
                Minor = Minor,
                Patch = Patch,
                Build = Build,
                ReleaseType = _releaseType,
                FullVersion = FullVersion,
                DisplayVersion = DisplayVersion,
                BuildDate = BuildDate,
                CommitHash = CommitHash,
                UnityVersion = Application.unityVersion,
                Platform = Application.platform.ToString()
            };
        }

        /// <summary>
        /// バージョンを比較
        /// </summary>
        /// <param name="otherVersion">比較対象のバージョン文字列 (例: "1.2.3")</param>
        /// <returns>-1: 古い, 0: 同じ, 1: 新しい</returns>
        public int CompareVersion(string otherVersion)
        {
            var parts = otherVersion.Split('.');
            if (parts.Length < 3) return 0;

            if (!int.TryParse(parts[0], out int otherMajor)) return 0;
            if (!int.TryParse(parts[1], out int otherMinor)) return 0;
            if (!int.TryParse(parts[2], out int otherPatch)) return 0;

            // メジャーバージョン比較
            if (Major > otherMajor) return 1;
            if (Major < otherMajor) return -1;

            // マイナーバージョン比較
            if (Minor > otherMinor) return 1;
            if (Minor < otherMinor) return -1;

            // パッチバージョン比較
            if (Patch > otherPatch) return 1;
            if (Patch < otherPatch) return -1;

            return 0;
        }

        /// <summary>
        /// 指定バージョン以上かどうかを確認
        /// </summary>
        public bool IsVersionAtLeast(int major, int minor = 0, int patch = 0)
        {
            if (Major > major) return true;
            if (Major < major) return false;

            if (Minor > minor) return true;
            if (Minor < minor) return false;

            return Patch >= patch;
        }

        /// <summary>
        /// 開発ビルドかどうか
        /// </summary>
        public bool IsDevelopmentBuild()
        {
            return _releaseType == ReleaseType.Development || Debug.isDebugBuild;
        }

        /// <summary>
        /// プレリリースかどうか
        /// </summary>
        public bool IsPreRelease()
        {
            return _releaseType != ReleaseType.Release;
        }

        /// <summary>
        /// クレジット・著作権情報を取得
        /// </summary>
        public string GetCopyrightInfo()
        {
            int currentYear = DateTime.Now.Year;
            return $"© {currentYear} ThirtySixStratagems. All rights reserved.";
        }

        /// <summary>
        /// 詳細なバージョン情報文字列を取得
        /// </summary>
        public string GetDetailedVersionString()
        {
            string info = $"三十六計 〜天下統一への道〜\n";
            info += $"Version: {DisplayVersion}\n";
            info += $"Build: {_buildNumber}\n";
            info += $"Build Date: {BuildDate}\n";

            if (!string.IsNullOrEmpty(_commitHash))
            {
                info += $"Commit: {_commitHash.Substring(0, Math.Min(8, _commitHash.Length))}\n";
            }

            info += $"Unity: {Application.unityVersion}\n";
            info += $"Platform: {Application.platform}\n";
            info += $"\n{GetCopyrightInfo()}";

            return info;
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用: ビルド番号をインクリメント
        /// </summary>
        [ContextMenu("Increment Build Number")]
        private void IncrementBuildNumber()
        {
            if (int.TryParse(_buildNumber, out int current))
            {
                _buildNumber = (current + 1).ToString();
                _buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Debug.Log($"Build number incremented to {_buildNumber}");
            }
        }

        /// <summary>
        /// エディタ用: パッチバージョンをインクリメント
        /// </summary>
        [ContextMenu("Increment Patch Version")]
        private void IncrementPatchVersion()
        {
            if (int.TryParse(_patchVersion, out int current))
            {
                _patchVersion = (current + 1).ToString();
                _buildNumber = "1";
                _buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Debug.Log($"Patch version incremented to {_patchVersion}");
            }
        }

        /// <summary>
        /// エディタ用: マイナーバージョンをインクリメント
        /// </summary>
        [ContextMenu("Increment Minor Version")]
        private void IncrementMinorVersion()
        {
            if (int.TryParse(_minorVersion, out int current))
            {
                _minorVersion = (current + 1).ToString();
                _patchVersion = "0";
                _buildNumber = "1";
                _buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Debug.Log($"Minor version incremented to {_minorVersion}");
            }
        }
#endif
    }

    /// <summary>
    /// リリースタイプ
    /// </summary>
    public enum ReleaseType
    {
        Development,
        Alpha,
        Beta,
        ReleaseCandidate,
        Release
    }

    /// <summary>
    /// バージョン情報
    /// </summary>
    public class VersionInfo
    {
        public int Major;
        public int Minor;
        public int Patch;
        public int Build;
        public ReleaseType ReleaseType;
        public string FullVersion;
        public string DisplayVersion;
        public string BuildDate;
        public string CommitHash;
        public string UnityVersion;
        public string Platform;

        public override string ToString()
        {
            return $"Version {DisplayVersion} (Build {Build}) - {Platform}";
        }
    }
}
