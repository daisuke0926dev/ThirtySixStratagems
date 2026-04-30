using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.UI
{
    /// <summary>
    /// 計略図鑑
    /// 三十六計の詳細情報と使用履歴を表示
    /// </summary>
    public class StratagemEncyclopedia : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private RectTransform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private RectTransform _detailPanel;

        [Header("詳細パネル")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _historyText;
        [SerializeField] private TextMeshProUGUI _usageText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private Image _unlockIcon;

        [Header("統計")]
        [SerializeField] private TextMeshProUGUI _totalUnlockedText;
        [SerializeField] private TextMeshProUGUI _totalUsedText;
        [SerializeField] private Slider _completionSlider;

        [Header("フィルター")]
        [SerializeField] private TMP_Dropdown _categoryFilter;
        [SerializeField] private Toggle _unlockedOnlyToggle;

        // データ
        private List<StratagemEncyclopediaEntry> _entries = new List<StratagemEncyclopediaEntry>();
        private StratagemEncyclopediaEntry _selectedEntry;
        private StratagemCategory? _currentFilter;

        // イベント
        public event Action<StratagemEncyclopediaEntry> OnEntrySelected;

        private void Start()
        {
            InitializeEntries();
            SetupFilters();
            RefreshList();
            UpdateStatistics();
        }

        /// <summary>
        /// エントリを初期化
        /// </summary>
        private void InitializeEntries()
        {
            _entries.Clear();

            // 三十六計の全データを取得
            var stratagems = StratagemEncyclopediaData.GetAllStratagems();
            foreach (var stratagem in stratagems)
            {
                var entry = new StratagemEncyclopediaEntry
                {
                    Data = stratagem,
                    IsUnlocked = LoadUnlockStatus(stratagem.Id),
                    UseCount = LoadUseCount(stratagem.Id),
                    SuccessCount = LoadSuccessCount(stratagem.Id)
                };
                _entries.Add(entry);
            }
        }

        /// <summary>
        /// フィルターをセットアップ
        /// </summary>
        private void SetupFilters()
        {
            if (_categoryFilter != null)
            {
                _categoryFilter.ClearOptions();
                _categoryFilter.AddOptions(new List<string>
                {
                    "すべて",
                    "勝戦計",
                    "敵戦計",
                    "攻戦計",
                    "混戦計",
                    "併戦計",
                    "敗戦計"
                });
                _categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
            }

            if (_unlockedOnlyToggle != null)
            {
                _unlockedOnlyToggle.onValueChanged.AddListener(OnUnlockedFilterChanged);
            }
        }

        /// <summary>
        /// リストを更新
        /// </summary>
        private void RefreshList()
        {
            // 既存のエントリをクリア
            foreach (Transform child in _listContainer)
            {
                Destroy(child.gameObject);
            }

            bool unlockedOnly = _unlockedOnlyToggle != null && _unlockedOnlyToggle.isOn;

            foreach (var entry in _entries)
            {
                // フィルター適用
                if (_currentFilter.HasValue && entry.Data.Category != _currentFilter.Value)
                    continue;

                if (unlockedOnly && !entry.IsUnlocked)
                    continue;

                CreateEntryUI(entry);
            }
        }

        /// <summary>
        /// エントリUIを作成
        /// </summary>
        private void CreateEntryUI(StratagemEncyclopediaEntry entry)
        {
            if (_entryPrefab == null || _listContainer == null) return;

            var entryObj = Instantiate(_entryPrefab, _listContainer);
            var button = entryObj.GetComponent<Button>();

            // テキスト設定
            var nameText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                if (entry.IsUnlocked)
                {
                    nameText.text = $"第{entry.Data.Number}計 {entry.Data.Name}";
                }
                else
                {
                    nameText.text = $"第{entry.Data.Number}計 ???";
                    nameText.color = new Color(0.5f, 0.5f, 0.5f);
                }
            }

            // ボタンイベント
            if (button != null)
            {
                button.onClick.AddListener(() => SelectEntry(entry));
            }
        }

        /// <summary>
        /// エントリを選択
        /// </summary>
        public void SelectEntry(StratagemEncyclopediaEntry entry)
        {
            _selectedEntry = entry;
            UpdateDetailPanel();
            OnEntrySelected?.Invoke(entry);
        }

        /// <summary>
        /// 詳細パネルを更新
        /// </summary>
        private void UpdateDetailPanel()
        {
            if (_selectedEntry == null) return;

            var data = _selectedEntry.Data;
            bool unlocked = _selectedEntry.IsUnlocked;

            if (_numberText != null)
                _numberText.text = $"第{data.Number}計";

            if (_nameText != null)
                _nameText.text = unlocked ? data.Name : "???";

            if (_categoryText != null)
                _categoryText.text = GetCategoryName(data.Category);

            if (_descriptionText != null)
                _descriptionText.text = unlocked ? data.Description : "この計略はまだ解放されていません。";

            if (_historyText != null)
                _historyText.text = unlocked ? data.HistoricalBackground : "";

            if (_usageText != null)
                _usageText.text = unlocked ? data.UsageExample : "";

            if (_effectText != null)
                _effectText.text = unlocked ? data.Effect : "";

            if (_unlockIcon != null)
                _unlockIcon.gameObject.SetActive(!unlocked);

            // 使用統計
            UpdateUsageStats();
        }

        /// <summary>
        /// 使用統計を更新
        /// </summary>
        private void UpdateUsageStats()
        {
            if (_selectedEntry == null) return;

            // 使用回数と成功率を表示（詳細パネル内に追加のTextがある場合）
        }

        /// <summary>
        /// 統計を更新
        /// </summary>
        private void UpdateStatistics()
        {
            int totalUnlocked = 0;
            int totalUsed = 0;

            foreach (var entry in _entries)
            {
                if (entry.IsUnlocked) totalUnlocked++;
                if (entry.UseCount > 0) totalUsed++;
            }

            if (_totalUnlockedText != null)
                _totalUnlockedText.text = $"解放済み: {totalUnlocked}/36";

            if (_totalUsedText != null)
                _totalUsedText.text = $"使用済み: {totalUsed}/36";

            if (_completionSlider != null)
                _completionSlider.value = totalUnlocked / 36f;
        }

        /// <summary>
        /// カテゴリ名を取得
        /// </summary>
        private string GetCategoryName(StratagemCategory category)
        {
            return category switch
            {
                StratagemCategory.Winning => "勝戦計（第一套）",
                StratagemCategory.Enemy => "敵戦計（第二套）",
                StratagemCategory.Attack => "攻戦計（第三套）",
                StratagemCategory.Chaos => "混戦計（第四套）",
                StratagemCategory.Merge => "併戦計（第五套）",
                StratagemCategory.Defeat => "敗戦計（第六套）",
                _ => "不明"
            };
        }

        #region フィルターイベント

        private void OnCategoryFilterChanged(int index)
        {
            _currentFilter = index == 0 ? null : (StratagemCategory?)(index - 1);
            RefreshList();
        }

        private void OnUnlockedFilterChanged(bool value)
        {
            RefreshList();
        }

        #endregion

        #region セーブ/ロード

        private bool LoadUnlockStatus(string stratagemId)
        {
            return PlayerPrefs.GetInt($"Stratagem_Unlocked_{stratagemId}", 0) == 1;
        }

        private int LoadUseCount(string stratagemId)
        {
            return PlayerPrefs.GetInt($"Stratagem_UseCount_{stratagemId}", 0);
        }

        private int LoadSuccessCount(string stratagemId)
        {
            return PlayerPrefs.GetInt($"Stratagem_SuccessCount_{stratagemId}", 0);
        }

        /// <summary>
        /// 計略を解放
        /// </summary>
        public void UnlockStratagem(string stratagemId)
        {
            PlayerPrefs.SetInt($"Stratagem_Unlocked_{stratagemId}", 1);
            PlayerPrefs.Save();

            var entry = _entries.Find(e => e.Data.Id == stratagemId);
            if (entry != null)
            {
                entry.IsUnlocked = true;
            }

            RefreshList();
            UpdateStatistics();
        }

        /// <summary>
        /// 使用を記録
        /// </summary>
        public void RecordUsage(string stratagemId, bool success)
        {
            int useCount = LoadUseCount(stratagemId) + 1;
            int successCount = LoadSuccessCount(stratagemId) + (success ? 1 : 0);

            PlayerPrefs.SetInt($"Stratagem_UseCount_{stratagemId}", useCount);
            PlayerPrefs.SetInt($"Stratagem_SuccessCount_{stratagemId}", successCount);
            PlayerPrefs.Save();

            var entry = _entries.Find(e => e.Data.Id == stratagemId);
            if (entry != null)
            {
                entry.UseCount = useCount;
                entry.SuccessCount = successCount;
            }

            // 初使用で解放
            if (useCount == 1)
            {
                UnlockStratagem(stratagemId);
            }
        }

        #endregion
    }

    /// <summary>
    /// 計略図鑑エントリ
    /// </summary>
    public class StratagemEncyclopediaEntry
    {
        public StratagemEncyclopediaData Data;
        public bool IsUnlocked;
        public int UseCount;
        public int SuccessCount;

        public float SuccessRate => UseCount > 0 ? (float)SuccessCount / UseCount : 0f;
    }

    /// <summary>
    /// 計略図鑑データ
    /// </summary>
    [Serializable]
    public class StratagemEncyclopediaData
    {
        public string Id;
        public int Number;
        public string Name;
        public StratagemCategory Category;
        public string Description;
        public string HistoricalBackground;
        public string UsageExample;
        public string Effect;

        /// <summary>
        /// 全計略データを取得
        /// </summary>
        public static List<StratagemEncyclopediaData> GetAllStratagems()
        {
            return new List<StratagemEncyclopediaData>
            {
                // 勝戦計（1-6）
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_01", Number = 1, Name = "瞞天過海",
                    Category = StratagemCategory.Winning,
                    Description = "天を欺いて海を渡る。相手の油断を誘い、大胆な行動を成功させる。",
                    HistoricalBackground = "唐の太宗が海を渡る際、船酔いを恐れた皇帝を欺いて船に乗せた故事に由来。",
                    UsageExample = "敵の監視が緩んだ隙に大規模な移動を行う。",
                    Effect = "軍の隠密移動が可能"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_02", Number = 2, Name = "囲魏救趙",
                    Category = StratagemCategory.Winning,
                    Description = "魏を囲んで趙を救う。敵の本拠を攻め、包囲軍を撤退させる。",
                    HistoricalBackground = "戦国時代、孫臏が魏軍に包囲された趙を救うため魏の首都を攻撃した。",
                    UsageExample = "敵の補給線や本拠地を脅かして包囲を解かせる。",
                    Effect = "敵軍を撤退させる"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_03", Number = 3, Name = "借刀殺人",
                    Category = StratagemCategory.Winning,
                    Description = "他人の刀を借りて人を殺す。第三者を利用して敵を倒す。",
                    HistoricalBackground = "三国志で曹操が劉備を使って呂布を討たせた例など。",
                    UsageExample = "同盟国を利用して共通の敵を攻撃させる。",
                    Effect = "他勢力を戦争に巻き込む"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_04", Number = 4, Name = "以逸待労",
                    Category = StratagemCategory.Winning,
                    Description = "逸を以って労を待つ。休養して疲弊した敵を迎え撃つ。",
                    HistoricalBackground = "孫子の兵法にある基本原則。",
                    UsageExample = "防御に徹して敵の攻撃を待ち受ける。",
                    Effect = "防御力上昇、敵の士気低下"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_05", Number = 5, Name = "趁火打劫",
                    Category = StratagemCategory.Winning,
                    Description = "火事場泥棒。敵の混乱に乗じて攻める。",
                    HistoricalBackground = "古来より混乱した敵を攻める基本戦術。",
                    UsageExample = "内乱や災害で弱体化した敵領を攻める。",
                    Effect = "混乱中の敵への攻撃力上昇"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_06", Number = 6, Name = "声東撃西",
                    Category = StratagemCategory.Winning,
                    Description = "東に声して西を撃つ。陽動作戦で敵の注意をそらす。",
                    HistoricalBackground = "古代中国の戦術書に見られる基本戦術。",
                    UsageExample = "別方向に攻撃を示唆して本命の攻撃を成功させる。",
                    Effect = "奇襲成功率上昇"
                },

                // 敵戦計（7-12）
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_07", Number = 7, Name = "無中生有",
                    Category = StratagemCategory.Enemy,
                    Description = "無から有を生む。偽情報で敵を惑わす。",
                    HistoricalBackground = "虚実を巧みに使い分ける兵法の基本。",
                    UsageExample = "存在しない軍勢をでっち上げて敵を警戒させる。",
                    Effect = "偽情報を流布"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_08", Number = 8, Name = "暗渡陳倉",
                    Category = StratagemCategory.Enemy,
                    Description = "密かに陳倉を渡る。正面を見せて別ルートから攻める。",
                    HistoricalBackground = "韓信が項羽を欺いて陳倉から進軍した故事。",
                    UsageExample = "正面で牽制しつつ別働隊で攻撃する。",
                    Effect = "迂回攻撃が可能"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_09", Number = 9, Name = "隔岸観火",
                    Category = StratagemCategory.Enemy,
                    Description = "岸を隔てて火を観る。敵同士を争わせて消耗させる。",
                    HistoricalBackground = "漁夫の利を得る戦略。",
                    UsageExample = "敵対する二勢力の争いを傍観して漁夫の利を得る。",
                    Effect = "敵勢力間の対立を誘発"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_10", Number = 10, Name = "笑裏蔵刀",
                    Category = StratagemCategory.Enemy,
                    Description = "笑いの中に刀を隠す。友好を装って攻撃の機会を窺う。",
                    HistoricalBackground = "外見と本心を使い分ける策略。",
                    UsageExample = "和平を装いながら攻撃準備を進める。",
                    Effect = "外交的偽装"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_11", Number = 11, Name = "李代桃僵",
                    Category = StratagemCategory.Enemy,
                    Description = "李が桃に代わって枯れる。犠牲を払って大局を守る。",
                    HistoricalBackground = "小を捨てて大を取る戦略。",
                    UsageExample = "重要でない領地を捨てて主力を守る。",
                    Effect = "戦略的撤退が可能"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_12", Number = 12, Name = "順手牽羊",
                    Category = StratagemCategory.Enemy,
                    Description = "手順に従って羊を牽く。機会を逃さず利益を得る。",
                    HistoricalBackground = "小さな機会も逃さない戦術。",
                    UsageExample = "敵の隙を突いて資源を奪う。",
                    Effect = "資源略奪"
                },

                // 以下、残りの計略も同様に定義...
                // 簡略化のため、代表的なものを記載

                new StratagemEncyclopediaData
                {
                    Id = "stratagem_31", Number = 31, Name = "美人計",
                    Category = StratagemCategory.Defeat,
                    Description = "美人を使って敵を惑わす。",
                    HistoricalBackground = "西施が呉王夫差を惑わした故事など。",
                    UsageExample = "敵将の判断力を鈍らせる。",
                    Effect = "敵将の能力低下"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_32", Number = 32, Name = "空城計",
                    Category = StratagemCategory.Defeat,
                    Description = "空の城で敵を欺く。弱さを見せて敵を警戒させる。",
                    HistoricalBackground = "諸葛亮が司馬懿を欺いた有名な計略。",
                    UsageExample = "少数で大軍を退ける。",
                    Effect = "敵軍を撤退させる"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_33", Number = 33, Name = "反間計",
                    Category = StratagemCategory.Defeat,
                    Description = "敵のスパイを利用して偽情報を流す。",
                    HistoricalBackground = "周瑜が曹操軍の間者を利用した例。",
                    UsageExample = "敵に偽情報を信じさせる。",
                    Effect = "敵に偽情報を流布"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_34", Number = 34, Name = "苦肉計",
                    Category = StratagemCategory.Defeat,
                    Description = "自らを傷つけて敵を欺く。",
                    HistoricalBackground = "黄蓋が曹操を欺くため周瑜に打たれた故事。",
                    UsageExample = "偽装投降で敵に接近する。",
                    Effect = "偽降伏が成功"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_35", Number = 35, Name = "連環計",
                    Category = StratagemCategory.Defeat,
                    Description = "複数の計略を連鎖させる。",
                    HistoricalBackground = "赤壁で龐統が曹操の船を連結させた計略。",
                    UsageExample = "複数の計略を組み合わせて大きな効果を得る。",
                    Effect = "計略の連続発動"
                },
                new StratagemEncyclopediaData
                {
                    Id = "stratagem_36", Number = 36, Name = "走為上",
                    Category = StratagemCategory.Defeat,
                    Description = "逃げるが勝ち。不利な状況では撤退が最善。",
                    HistoricalBackground = "三十六計逃げるに如かず。",
                    UsageExample = "圧倒的不利な状況で戦力を温存する。",
                    Effect = "安全な撤退が可能"
                }
            };
        }
    }
}
