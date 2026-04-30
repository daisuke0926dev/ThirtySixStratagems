using System.Collections.Generic;
using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data
{
    /// <summary>
    /// シナリオ定義
    /// 三国志の有名な戦いをシナリオとして定義
    /// </summary>
    public static class ScenarioDefinitions
    {
        /// <summary>
        /// 全シナリオを取得
        /// </summary>
        public static List<ScenarioData> GetAllScenarios()
        {
            return new List<ScenarioData>
            {
                CreateYellowTurbanScenario(),
                CreateAntiDongZhuoScenario(),
                CreateGuanduScenario(),
                CreateChiBiScenario(),
                CreateThreeKingdomsScenario()
            };
        }

        /// <summary>
        /// 黄巾の乱シナリオ (184年)
        /// </summary>
        public static ScenarioData CreateYellowTurbanScenario()
        {
            return new ScenarioData
            {
                Id = "scenario_yellow_turban",
                Name = "黄巾の乱",
                Description = "太平道の教祖・張角が「蒼天已死、黄天當立」を掲げ反乱を起こした。各地の豪傑たちが漢王朝を守るため立ち上がる。",
                Year = 184,
                Difficulty = 1,
                IsUnlocked = true,
                Factions = new List<FactionSetup>
                {
                    new FactionSetup
                    {
                        FactionId = "han_dynasty",
                        Name = "漢王朝",
                        IsPlayer = true,
                        Color = new Color(0.8f, 0.2f, 0.2f),
                        StartingGold = 50000,
                        StartingFood = 30000,
                        TerritoryIds = new List<string> { "luoyang", "changan", "xuchang" },
                        CharacterIds = new List<string> { "he_jin", "huangfu_song", "lu_zhi" }
                    },
                    new FactionSetup
                    {
                        FactionId = "yellow_turban",
                        Name = "黄巾賊",
                        IsPlayer = false,
                        Color = new Color(0.9f, 0.8f, 0.2f),
                        StartingGold = 30000,
                        StartingFood = 20000,
                        TerritoryIds = new List<string> { "julu", "yingchuan", "nanyang" },
                        CharacterIds = new List<string> { "zhang_jiao", "zhang_bao", "zhang_liang" },
                        AIPersonality = AIPersonality.Aggressive
                    },
                    new FactionSetup
                    {
                        FactionId = "liu_bei_early",
                        Name = "劉備義勇軍",
                        IsPlayer = false,
                        Color = new Color(0.2f, 0.6f, 0.2f),
                        StartingGold = 5000,
                        StartingFood = 3000,
                        TerritoryIds = new List<string> { "zhuo_county" },
                        CharacterIds = new List<string> { "liu_bei", "guan_yu", "zhang_fei" },
                        AIPersonality = AIPersonality.Balanced
                    }
                },
                VictoryConditions = new List<string>
                {
                    "黄巾賊を壊滅させる",
                    "張角を捕縛または討伐する"
                },
                DefeatConditions = new List<string>
                {
                    "洛陽を失う",
                    "プレイヤー勢力が滅亡"
                }
            };
        }

        /// <summary>
        /// 反董卓連合シナリオ (190年)
        /// </summary>
        public static ScenarioData CreateAntiDongZhuoScenario()
        {
            return new ScenarioData
            {
                Id = "scenario_anti_dong_zhuo",
                Name = "反董卓連合",
                Description = "暴虐の限りを尽くす董卓に対し、袁紹を盟主とする諸侯連合軍が結成された。虎牢関の戦いが始まる。",
                Year = 190,
                Difficulty = 2,
                IsUnlocked = true,
                Factions = new List<FactionSetup>
                {
                    new FactionSetup
                    {
                        FactionId = "coalition",
                        Name = "反董卓連合",
                        IsPlayer = true,
                        Color = new Color(0.2f, 0.5f, 0.8f),
                        StartingGold = 80000,
                        StartingFood = 50000,
                        TerritoryIds = new List<string> { "luoyang_coalition", "suanzao", "henei" },
                        CharacterIds = new List<string> { "yuan_shao", "cao_cao", "sun_jian", "liu_bei", "guan_yu", "zhang_fei" }
                    },
                    new FactionSetup
                    {
                        FactionId = "dong_zhuo",
                        Name = "董卓軍",
                        IsPlayer = false,
                        Color = new Color(0.5f, 0.1f, 0.1f),
                        StartingGold = 100000,
                        StartingFood = 60000,
                        TerritoryIds = new List<string> { "luoyang", "hulao", "changan" },
                        CharacterIds = new List<string> { "dong_zhuo", "lu_bu", "li_jue", "guo_si", "hua_xiong" },
                        AIPersonality = AIPersonality.Aggressive
                    }
                },
                VictoryConditions = new List<string>
                {
                    "董卓を討伐する",
                    "洛陽を奪還する"
                },
                DefeatConditions = new List<string>
                {
                    "連合軍が壊滅",
                    "袁紹が討たれる"
                }
            };
        }

        /// <summary>
        /// 官渡の戦いシナリオ (200年)
        /// </summary>
        public static ScenarioData CreateGuanduScenario()
        {
            return new ScenarioData
            {
                Id = "scenario_guandu",
                Name = "官渡の戦い",
                Description = "河北の覇者・袁紹と中原の曹操が激突する天下分け目の大戦。兵力で劣る曹操は計略を駆使して戦う。",
                Year = 200,
                Difficulty = 3,
                IsUnlocked = false,
                UnlockCondition = "黄巾の乱をクリア",
                Factions = new List<FactionSetup>
                {
                    new FactionSetup
                    {
                        FactionId = "cao_cao",
                        Name = "曹操軍",
                        IsPlayer = true,
                        Color = new Color(0.2f, 0.4f, 0.8f),
                        StartingGold = 30000,
                        StartingFood = 15000,
                        TerritoryIds = new List<string> { "xuchang", "guandu", "yanzhou" },
                        CharacterIds = new List<string> { "cao_cao", "guo_jia", "xun_yu", "cao_ren", "xiahou_dun", "xu_chu" }
                    },
                    new FactionSetup
                    {
                        FactionId = "yuan_shao",
                        Name = "袁紹軍",
                        IsPlayer = false,
                        Color = new Color(0.6f, 0.3f, 0.6f),
                        StartingGold = 100000,
                        StartingFood = 80000,
                        TerritoryIds = new List<string> { "yecheng", "jizhou", "wuchao" },
                        CharacterIds = new List<string> { "yuan_shao", "yan_liang", "wen_chou", "zhang_he", "gao_lan" },
                        AIPersonality = AIPersonality.Aggressive
                    }
                },
                VictoryConditions = new List<string>
                {
                    "袁紹軍を撃破",
                    "烏巣の兵糧を焼き討ち"
                },
                DefeatConditions = new List<string>
                {
                    "許昌を失う",
                    "曹操が討たれる"
                }
            };
        }

        /// <summary>
        /// 赤壁の戦いシナリオ (208年)
        /// </summary>
        public static ScenarioData CreateChiBiScenario()
        {
            return new ScenarioData
            {
                Id = "scenario_chibi",
                Name = "赤壁の戦い",
                Description = "天下統一を目指す曹操の大軍が南下。孫権と劉備は同盟を結び、赤壁で曹操軍を迎え撃つ。",
                Year = 208,
                Difficulty = 4,
                IsUnlocked = false,
                UnlockCondition = "官渡の戦いをクリア",
                Factions = new List<FactionSetup>
                {
                    new FactionSetup
                    {
                        FactionId = "sun_liu_alliance",
                        Name = "孫劉連合",
                        IsPlayer = true,
                        Color = new Color(0.8f, 0.4f, 0.2f),
                        StartingGold = 40000,
                        StartingFood = 25000,
                        TerritoryIds = new List<string> { "chibi", "jiangling", "xiakou" },
                        CharacterIds = new List<string> { "zhou_yu", "zhuge_liang", "sun_quan", "liu_bei", "lu_su", "huang_gai" }
                    },
                    new FactionSetup
                    {
                        FactionId = "cao_cao_chibi",
                        Name = "曹操軍",
                        IsPlayer = false,
                        Color = new Color(0.2f, 0.4f, 0.8f),
                        StartingGold = 150000,
                        StartingFood = 100000,
                        TerritoryIds = new List<string> { "jingzhou", "xiangyang", "wulin" },
                        CharacterIds = new List<string> { "cao_cao", "cao_ren", "cai_mao", "zhang_yun" },
                        AIPersonality = AIPersonality.Aggressive
                    }
                },
                VictoryConditions = new List<string>
                {
                    "曹操軍を撃退",
                    "連環計と火計を成功させる"
                },
                DefeatConditions = new List<string>
                {
                    "赤壁を失う",
                    "周瑜または諸葛亮が討たれる"
                }
            };
        }

        /// <summary>
        /// 三国鼎立シナリオ (220年)
        /// </summary>
        public static ScenarioData CreateThreeKingdomsScenario()
        {
            return new ScenarioData
            {
                Id = "scenario_three_kingdoms",
                Name = "三国鼎立",
                Description = "曹操の死後、曹丕が魏を建国。劉備は蜀を、孫権は呉を建て、三国時代が幕を開ける。天下統一を目指せ。",
                Year = 220,
                Difficulty = 5,
                IsUnlocked = false,
                UnlockCondition = "赤壁の戦いをクリア",
                Factions = new List<FactionSetup>
                {
                    new FactionSetup
                    {
                        FactionId = "wei",
                        Name = "魏",
                        IsPlayer = true,
                        Color = new Color(0.2f, 0.4f, 0.8f),
                        StartingGold = 100000,
                        StartingFood = 80000,
                        TerritoryIds = new List<string> { "luoyang", "xuchang", "yecheng", "changan" },
                        CharacterIds = new List<string> { "cao_pi", "sima_yi", "cao_zhen", "zhang_liao", "xu_huang" }
                    },
                    new FactionSetup
                    {
                        FactionId = "shu",
                        Name = "蜀",
                        IsPlayer = false,
                        Color = new Color(0.2f, 0.7f, 0.3f),
                        StartingGold = 50000,
                        StartingFood = 40000,
                        TerritoryIds = new List<string> { "chengdu", "hanzhong", "jiameng" },
                        CharacterIds = new List<string> { "liu_bei", "zhuge_liang", "guan_yu", "zhang_fei", "zhao_yun" },
                        AIPersonality = AIPersonality.Strategic
                    },
                    new FactionSetup
                    {
                        FactionId = "wu",
                        Name = "呉",
                        IsPlayer = false,
                        Color = new Color(0.8f, 0.4f, 0.2f),
                        StartingGold = 70000,
                        StartingFood = 50000,
                        TerritoryIds = new List<string> { "jianye", "wuchang", "changsha" },
                        CharacterIds = new List<string> { "sun_quan", "lu_xun", "zhou_tai", "gan_ning" },
                        AIPersonality = AIPersonality.Defensive
                    }
                },
                VictoryConditions = new List<string>
                {
                    "全領土を制圧して天下統一"
                },
                DefeatConditions = new List<string>
                {
                    "プレイヤー勢力が滅亡"
                }
            };
        }
    }

    /// <summary>
    /// シナリオデータ
    /// </summary>
    [System.Serializable]
    public class ScenarioData
    {
        public string Id;
        public string Name;
        public string Description;
        public int Year;
        public int Difficulty;
        public bool IsUnlocked;
        public string UnlockCondition;
        public List<FactionSetup> Factions;
        public List<string> VictoryConditions;
        public List<string> DefeatConditions;
    }

    /// <summary>
    /// 勢力設定
    /// </summary>
    [System.Serializable]
    public class FactionSetup
    {
        public string FactionId;
        public string Name;
        public bool IsPlayer;
        public Color Color;
        public int StartingGold;
        public int StartingFood;
        public List<string> TerritoryIds;
        public List<string> CharacterIds;
        public AIPersonality AIPersonality;
    }
}
