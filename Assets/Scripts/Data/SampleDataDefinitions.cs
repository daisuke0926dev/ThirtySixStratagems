using UnityEngine;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data
{
    /// <summary>
    /// サンプルデータ定義
    /// </summary>
    public static class SampleDataDefinitions
    {
        /// <summary>
        /// サンプル領地データ（8領地）
        /// </summary>
        public static readonly TerritoryDefinition[] SampleTerritories = new TerritoryDefinition[]
        {
            new TerritoryDefinition
            {
                Id = "territory_luoyang",
                Name = "洛陽",
                Population = 80000,
                Economy = 90,
                Defense = 85,
                Position = new Vector2(500, 400),
                Description = "中原の中心都市。古くから都として栄え、経済と文化の中心地。"
            },
            new TerritoryDefinition
            {
                Id = "territory_changan",
                Name = "長安",
                Population = 70000,
                Economy = 85,
                Defense = 80,
                Position = new Vector2(200, 350),
                Description = "西方の要衝。シルクロードの起点として繁栄。"
            },
            new TerritoryDefinition
            {
                Id = "territory_xuchang",
                Name = "許昌",
                Population = 50000,
                Economy = 70,
                Defense = 70,
                Position = new Vector2(550, 450),
                Description = "曹操が漢の献帝を迎えた地。政治の中心。"
            },
            new TerritoryDefinition
            {
                Id = "territory_jiangdong",
                Name = "江東",
                Population = 60000,
                Economy = 75,
                Defense = 65,
                Position = new Vector2(700, 500),
                Description = "長江下流の豊かな地域。水軍の拠点。"
            },
            new TerritoryDefinition
            {
                Id = "territory_jingzhou",
                Name = "荊州",
                Population = 55000,
                Economy = 70,
                Defense = 60,
                Position = new Vector2(500, 550),
                Description = "長江中流の要衝。南北を結ぶ交通の要所。"
            },
            new TerritoryDefinition
            {
                Id = "territory_yizhou",
                Name = "益州",
                Population = 45000,
                Economy = 65,
                Defense = 90,
                Position = new Vector2(250, 550),
                Description = "天府の国と呼ばれる豊かな盆地。天然の要害。"
            },
            new TerritoryDefinition
            {
                Id = "territory_hanzhong",
                Name = "漢中",
                Population = 30000,
                Economy = 50,
                Defense = 85,
                Position = new Vector2(300, 450),
                Description = "蜀への入り口。険しい山々に囲まれた要害。"
            },
            new TerritoryDefinition
            {
                Id = "territory_youzhou",
                Name = "幽州",
                Population = 40000,
                Economy = 55,
                Defense = 70,
                Position = new Vector2(600, 200),
                Description = "北方の辺境。騎馬民族との交流地。"
            }
        };

        /// <summary>
        /// サンプル武将データ（12人）
        /// </summary>
        public static readonly CharacterDefinition[] SampleCharacters = new CharacterDefinition[]
        {
            // 勢力A（魏）の武将
            new CharacterDefinition
            {
                Id = "char_caocao",
                Name = "曹操",
                Type = CharacterType.Ruler,
                Strength = 72,
                Intelligence = 91,
                Leadership = 96,
                Politics = 94,
                Charisma = 96,
                Specialty = StratagemCategory.Winning,
                Biography = "乱世の奸雄。詩人としても名高い。「治世の能臣、乱世の奸雄」と評される。"
            },
            new CharacterDefinition
            {
                Id = "char_simayi",
                Name = "司馬懿",
                Type = CharacterType.Strategist,
                Strength = 48,
                Intelligence = 98,
                Leadership = 86,
                Politics = 92,
                Charisma = 78,
                Specialty = StratagemCategory.Defeat,
                Biography = "魏の重臣。狼顧の相を持つと言われた。後に晋の基礎を築く。"
            },
            new CharacterDefinition
            {
                Id = "char_xiahouyuan",
                Name = "夏侯淵",
                Type = CharacterType.General,
                Strength = 91,
                Intelligence = 52,
                Leadership = 83,
                Politics = 40,
                Charisma = 68,
                Specialty = StratagemCategory.Attack,
                Biography = "曹操の従弟。電撃戦を得意とする猛将。「虎歩関右」と称される。"
            },
            new CharacterDefinition
            {
                Id = "char_xuhuang",
                Name = "徐晃",
                Type = CharacterType.General,
                Strength = 90,
                Intelligence = 68,
                Leadership = 88,
                Politics = 52,
                Charisma = 72,
                Specialty = StratagemCategory.Attack,
                Biography = "魏の五大将の一人。関羽を破った名将。周亜夫に匹敵すると曹操に評された。"
            },

            // 勢力B（蜀）の武将
            new CharacterDefinition
            {
                Id = "char_liubei",
                Name = "劉備",
                Type = CharacterType.Ruler,
                Strength = 71,
                Intelligence = 65,
                Leadership = 80,
                Politics = 78,
                Charisma = 99,
                Specialty = StratagemCategory.Merge,
                Biography = "蜀漢の初代皇帝。仁徳の人として知られ、民の心を掴む。"
            },
            new CharacterDefinition
            {
                Id = "char_zhugeliang",
                Name = "諸葛亮",
                Type = CharacterType.Strategist,
                Strength = 38,
                Intelligence = 100,
                Leadership = 92,
                Politics = 95,
                Charisma = 92,
                Specialty = StratagemCategory.Chaos,
                Biography = "臥龍と呼ばれた天才軍師。出師の表は名文として知られる。"
            },
            new CharacterDefinition
            {
                Id = "char_guanyu",
                Name = "関羽",
                Type = CharacterType.General,
                Strength = 97,
                Intelligence = 75,
                Leadership = 95,
                Politics = 62,
                Charisma = 93,
                Specialty = StratagemCategory.Winning,
                Biography = "劉備の義弟。武聖として後世に崇められる。義の人。"
            },
            new CharacterDefinition
            {
                Id = "char_zhangfei",
                Name = "張飛",
                Type = CharacterType.General,
                Strength = 98,
                Intelligence = 42,
                Leadership = 85,
                Politics = 28,
                Charisma = 72,
                Specialty = StratagemCategory.Attack,
                Biography = "劉備の義弟。万夫不当の猛将。長坂橋で曹操軍を退けた。"
            },

            // 勢力C（呉）の武将
            new CharacterDefinition
            {
                Id = "char_sunquan",
                Name = "孫権",
                Type = CharacterType.Ruler,
                Strength = 70,
                Intelligence = 80,
                Leadership = 84,
                Politics = 88,
                Charisma = 90,
                Specialty = StratagemCategory.Enemy,
                Biography = "呉の初代皇帝。若くして江東を治め、赤壁で曹操を破る。"
            },
            new CharacterDefinition
            {
                Id = "char_zhouyu",
                Name = "周瑜",
                Type = CharacterType.Strategist,
                Strength = 68,
                Intelligence = 96,
                Leadership = 95,
                Politics = 80,
                Charisma = 95,
                Specialty = StratagemCategory.Attack,
                Biography = "美周郎と呼ばれた名将。赤壁の戦いで曹操を大敗させた。"
            },
            new CharacterDefinition
            {
                Id = "char_lümeng",
                Name = "呂蒙",
                Type = CharacterType.General,
                Strength = 82,
                Intelligence = 85,
                Leadership = 88,
                Politics = 68,
                Charisma = 70,
                Specialty = StratagemCategory.Defeat,
                Biography = "呉の名将。白衣渡江で関羽を破る。士別三日当刮目相看の故事で知られる。"
            },
            new CharacterDefinition
            {
                Id = "char_luxun",
                Name = "陸遜",
                Type = CharacterType.General,
                Strength = 65,
                Intelligence = 95,
                Leadership = 92,
                Politics = 85,
                Charisma = 82,
                Specialty = StratagemCategory.Chaos,
                Biography = "呉の大都督。夷陵の戦いで劉備を大敗させた名将。"
            }
        };

        /// <summary>
        /// サンプル勢力データ（3勢力）
        /// </summary>
        public static readonly FactionDefinition[] SampleFactions = new FactionDefinition[]
        {
            new FactionDefinition
            {
                Id = "faction_wei",
                Name = "魏",
                Color = new Color(0.2f, 0.4f, 0.8f),
                InitialGold = 5000,
                InitialFood = 3000,
                RulerId = "char_caocao",
                CharacterIds = new[] { "char_caocao", "char_simayi", "char_xiahouyuan", "char_xuhuang" },
                TerritoryIds = new[] { "territory_luoyang", "territory_xuchang", "territory_youzhou" },
                Description = "曹操率いる中原の覇者。最大の勢力を誇る。"
            },
            new FactionDefinition
            {
                Id = "faction_shu",
                Name = "蜀",
                Color = new Color(0.2f, 0.7f, 0.3f),
                InitialGold = 3000,
                InitialFood = 2500,
                RulerId = "char_liubei",
                CharacterIds = new[] { "char_liubei", "char_zhugeliang", "char_guanyu", "char_zhangfei" },
                TerritoryIds = new[] { "territory_yizhou", "territory_hanzhong" },
                Description = "劉備率いる漢室復興を掲げる勢力。天然の要害を拠点とする。"
            },
            new FactionDefinition
            {
                Id = "faction_wu",
                Name = "呉",
                Color = new Color(0.8f, 0.2f, 0.2f),
                InitialGold = 4000,
                InitialFood = 3500,
                RulerId = "char_sunquan",
                CharacterIds = new[] { "char_sunquan", "char_zhouyu", "char_lümeng", "char_luxun" },
                TerritoryIds = new[] { "territory_jiangdong", "territory_jingzhou", "territory_changan" },
                Description = "孫権率いる江東の雄。水軍と豊かな経済力を持つ。"
            }
        };
    }

    public class TerritoryDefinition
    {
        public string Id;
        public string Name;
        public int Population;
        public int Economy;
        public int Defense;
        public Vector2 Position;
        public string Description;
    }

    public class CharacterDefinition
    {
        public string Id;
        public string Name;
        public CharacterType Type;
        public int Strength;
        public int Intelligence;
        public int Leadership;
        public int Politics;
        public int Charisma;
        public StratagemCategory Specialty;
        public string Biography;
    }

    public class FactionDefinition
    {
        public string Id;
        public string Name;
        public Color Color;
        public int InitialGold;
        public int InitialFood;
        public string RulerId;
        public string[] CharacterIds;
        public string[] TerritoryIds;
        public string Description;
    }
}
