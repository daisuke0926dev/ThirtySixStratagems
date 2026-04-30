using System.Collections.Generic;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data
{
    /// <summary>
    /// 武将定義
    /// 三国志の有名な武将データ
    /// </summary>
    public static class CharacterDefinitions
    {
        /// <summary>
        /// 全武将を取得
        /// </summary>
        public static List<CharacterData> GetAllCharacters()
        {
            var characters = new List<CharacterData>();

            // 魏の武将
            characters.AddRange(GetWeiCharacters());

            // 蜀の武将
            characters.AddRange(GetShuCharacters());

            // 呉の武将
            characters.AddRange(GetWuCharacters());

            // その他の武将
            characters.AddRange(GetOtherCharacters());

            return characters;
        }

        #region 魏

        private static List<CharacterData> GetWeiCharacters()
        {
            return new List<CharacterData>
            {
                // 曹操
                new CharacterData
                {
                    Id = "cao_cao",
                    Name = "曹操",
                    Courtesy = "孟徳",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 96, Warfare = 92, Intelligence = 95, Politics = 94, Charisma = 93 },
                    Description = "乱世の奸雄。詩人としても名高く、「天下に英雄あり」と自ら称した。",
                    BirthYear = 155,
                    Skills = new List<string> { "詩人", "奸雄", "覇王" }
                },
                // 曹丕
                new CharacterData
                {
                    Id = "cao_pi",
                    Name = "曹丕",
                    Courtesy = "子桓",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 78, Warfare = 72, Intelligence = 85, Politics = 88, Charisma = 75 },
                    Description = "曹操の後継者。魏を建国し初代皇帝となった。文学にも長けた。",
                    BirthYear = 187,
                    Skills = new List<string> { "文人", "皇帝" }
                },
                // 司馬懿
                new CharacterData
                {
                    Id = "sima_yi",
                    Name = "司馬懿",
                    Courtesy = "仲達",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 90, Warfare = 86, Intelligence = 98, Politics = 95, Charisma = 82 },
                    Description = "魏の軍師。諸葛亮と五丈原で対峙。後に晋王朝の礎を築く。",
                    BirthYear = 179,
                    Skills = new List<string> { "隠忍", "野心家", "策士" }
                },
                // 郭嘉
                new CharacterData
                {
                    Id = "guo_jia",
                    Name = "郭嘉",
                    Courtesy = "奉孝",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 65, Warfare = 68, Intelligence = 97, Politics = 85, Charisma = 78 },
                    Description = "曹操の軍師。「十勝十敗論」で官渡の勝利を予言した天才軍師。",
                    BirthYear = 170,
                    DeathYear = 207,
                    Skills = new List<string> { "天才軍師", "洞察" }
                },
                // 荀彧
                new CharacterData
                {
                    Id = "xun_yu",
                    Name = "荀彧",
                    Courtesy = "文若",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 70, Warfare = 55, Intelligence = 95, Politics = 98, Charisma = 88 },
                    Description = "曹操の参謀。内政と人事に優れ「王佐の才」と称された。",
                    BirthYear = 163,
                    DeathYear = 212,
                    Skills = new List<string> { "王佐", "内政家" }
                },
                // 張遼
                new CharacterData
                {
                    Id = "zhang_liao",
                    Name = "張遼",
                    Courtesy = "文遠",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 90, Warfare = 94, Intelligence = 72, Politics = 58, Charisma = 80 },
                    Description = "五子良将の筆頭。合肥の戦いで孫権軍を撃破した猛将。",
                    BirthYear = 169,
                    Skills = new List<string> { "猛将", "突撃" }
                },
                // 夏侯惇
                new CharacterData
                {
                    Id = "xiahou_dun",
                    Name = "夏侯惇",
                    Courtesy = "元譲",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 88, Warfare = 90, Intelligence = 62, Politics = 55, Charisma = 75 },
                    Description = "曹操の従兄弟。片目を失うも戦場に立ち続けた忠義の将。",
                    BirthYear = 157,
                    Skills = new List<string> { "忠義", "隻眼" }
                },
                // 許褚
                new CharacterData
                {
                    Id = "xu_chu",
                    Name = "許褚",
                    Courtesy = "仲康",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 68, Warfare = 96, Intelligence = 35, Politics = 25, Charisma = 55 },
                    Description = "曹操の護衛。「虎痴」と呼ばれた怪力の猛将。",
                    BirthYear = 170,
                    Skills = new List<string> { "護衛", "怪力" }
                }
            };
        }

        #endregion

        #region 蜀

        private static List<CharacterData> GetShuCharacters()
        {
            return new List<CharacterData>
            {
                // 劉備
                new CharacterData
                {
                    Id = "liu_bei",
                    Name = "劉備",
                    Courtesy = "玄徳",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 88, Warfare = 75, Intelligence = 76, Politics = 82, Charisma = 98 },
                    Description = "漢王室の末裔。仁徳をもって民を慈しみ、蜀漢を建国した。",
                    BirthYear = 161,
                    Skills = new List<string> { "仁君", "義兄弟", "人望" }
                },
                // 諸葛亮
                new CharacterData
                {
                    Id = "zhuge_liang",
                    Name = "諸葛亮",
                    Courtesy = "孔明",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 92, Warfare = 88, Intelligence = 100, Politics = 95, Charisma = 92 },
                    Description = "臥龍と呼ばれた天下の奇才。劉備に三顧の礼で迎えられた。",
                    BirthYear = 181,
                    Skills = new List<string> { "臥龍", "八陣図", "木牛流馬" }
                },
                // 関羽
                new CharacterData
                {
                    Id = "guan_yu",
                    Name = "関羽",
                    Courtesy = "雲長",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 92, Warfare = 97, Intelligence = 75, Politics = 62, Charisma = 93 },
                    Description = "劉備の義弟。武聖と称えられ、青龍偃月刀を振るう。",
                    BirthYear = 160,
                    Skills = new List<string> { "武聖", "義侠", "単騎駆" }
                },
                // 張飛
                new CharacterData
                {
                    Id = "zhang_fei",
                    Name = "張飛",
                    Courtesy = "翼徳",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 82, Warfare = 98, Intelligence = 42, Politics = 35, Charisma = 72 },
                    Description = "劉備の義弟。長坂橋で一喝し曹操軍を止めた猛将。",
                    BirthYear = 167,
                    Skills = new List<string> { "猛将", "一喝", "蛇矛" }
                },
                // 趙雲
                new CharacterData
                {
                    Id = "zhao_yun",
                    Name = "趙雲",
                    Courtesy = "子龍",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 88, Warfare = 96, Intelligence = 76, Politics = 65, Charisma = 85 },
                    Description = "白馬の騎士。長坂で阿斗を救い出した忠義の将。",
                    BirthYear = 168,
                    Skills = new List<string> { "白馬義従", "忠義", "一騎当千" }
                },
                // 馬超
                new CharacterData
                {
                    Id = "ma_chao",
                    Name = "馬超",
                    Courtesy = "孟起",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 85, Warfare = 95, Intelligence = 58, Politics = 48, Charisma = 78 },
                    Description = "西涼の錦馬超。曹操を追い詰めた勇猛な騎馬武者。",
                    BirthYear = 176,
                    Skills = new List<string> { "錦馬超", "騎兵", "西涼" }
                },
                // 黄忠
                new CharacterData
                {
                    Id = "huang_zhong",
                    Name = "黄忠",
                    Courtesy = "漢升",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 80, Warfare = 92, Intelligence = 62, Politics = 55, Charisma = 70 },
                    Description = "老いて益々盛んな老将。定軍山で夏侯淵を討った。",
                    BirthYear = 148,
                    Skills = new List<string> { "老当益壮", "弓術" }
                }
            };
        }

        #endregion

        #region 呉

        private static List<CharacterData> GetWuCharacters()
        {
            return new List<CharacterData>
            {
                // 孫堅
                new CharacterData
                {
                    Id = "sun_jian",
                    Name = "孫堅",
                    Courtesy = "文台",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 90, Warfare = 92, Intelligence = 72, Politics = 68, Charisma = 85 },
                    Description = "江東の虎。孫呉の礎を築いた勇将。",
                    BirthYear = 155,
                    DeathYear = 191,
                    Skills = new List<string> { "江東の虎", "猛将" }
                },
                // 孫策
                new CharacterData
                {
                    Id = "sun_ce",
                    Name = "孫策",
                    Courtesy = "伯符",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 92, Warfare = 94, Intelligence = 75, Politics = 72, Charisma = 90 },
                    Description = "小覇王。若くして江東を平定した英雄。",
                    BirthYear = 175,
                    DeathYear = 200,
                    Skills = new List<string> { "小覇王", "江東平定" }
                },
                // 孫権
                new CharacterData
                {
                    Id = "sun_quan",
                    Name = "孫権",
                    Courtesy = "仲謀",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 85, Warfare = 72, Intelligence = 88, Politics = 90, Charisma = 88 },
                    Description = "呉の皇帝。父兄の遺業を継ぎ呉を大国に育てた。",
                    BirthYear = 182,
                    Skills = new List<string> { "帝王", "人材登用" }
                },
                // 周瑜
                new CharacterData
                {
                    Id = "zhou_yu",
                    Name = "周瑜",
                    Courtesy = "公瑾",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 90, Warfare = 88, Intelligence = 96, Politics = 82, Charisma = 92 },
                    Description = "美周郎。赤壁で曹操軍を撃破した名将軍師。",
                    BirthYear = 175,
                    DeathYear = 210,
                    Skills = new List<string> { "美周郎", "火計", "音律" }
                },
                // 陸遜
                new CharacterData
                {
                    Id = "lu_xun",
                    Name = "陸遜",
                    Courtesy = "伯言",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 88, Warfare = 85, Intelligence = 95, Politics = 88, Charisma = 78 },
                    Description = "夷陵の戦いで劉備軍を撃破した呉の名将。",
                    BirthYear = 183,
                    Skills = new List<string> { "火計", "大都督" }
                },
                // 魯粛
                new CharacterData
                {
                    Id = "lu_su",
                    Name = "魯粛",
                    Courtesy = "子敬",
                    Type = CharacterType.Strategist,
                    Stats = new CharacterStats { Leadership = 72, Warfare = 58, Intelligence = 90, Politics = 92, Charisma = 85 },
                    Description = "孫劉同盟の立役者。外交に長けた名参謀。",
                    BirthYear = 172,
                    DeathYear = 217,
                    Skills = new List<string> { "外交家", "天下三分" }
                },
                // 甘寧
                new CharacterData
                {
                    Id = "gan_ning",
                    Name = "甘寧",
                    Courtesy = "興覇",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 78, Warfare = 94, Intelligence = 52, Politics = 42, Charisma = 68 },
                    Description = "百騎夜襲で曹操軍を撃破した勇将。元は海賊。",
                    BirthYear = 170,
                    Skills = new List<string> { "百騎夜襲", "海賊" }
                },
                // 黄蓋
                new CharacterData
                {
                    Id = "huang_gai",
                    Name = "黄蓋",
                    Courtesy = "公覆",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 80, Warfare = 85, Intelligence = 72, Politics = 58, Charisma = 70 },
                    Description = "赤壁の火計で偽降伏を演じた忠将。",
                    BirthYear = 150,
                    Skills = new List<string> { "苦肉計", "忠義" }
                }
            };
        }

        #endregion

        #region その他

        private static List<CharacterData> GetOtherCharacters()
        {
            return new List<CharacterData>
            {
                // 董卓
                new CharacterData
                {
                    Id = "dong_zhuo",
                    Name = "董卓",
                    Courtesy = "仲穎",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 82, Warfare = 88, Intelligence = 55, Politics = 48, Charisma = 35 },
                    Description = "暴虐の権臣。洛陽を支配し帝を傀儡とした。",
                    BirthYear = 139,
                    DeathYear = 192,
                    Skills = new List<string> { "暴虐", "恐怖" }
                },
                // 呂布
                new CharacterData
                {
                    Id = "lu_bu",
                    Name = "呂布",
                    Courtesy = "奉先",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 75, Warfare = 100, Intelligence = 32, Politics = 22, Charisma = 45 },
                    Description = "飛将軍。赤兎馬に跨り方天画戟を振るう最強の武将。",
                    BirthYear = 160,
                    DeathYear = 199,
                    Skills = new List<string> { "飛将", "赤兎馬", "無双" }
                },
                // 袁紹
                new CharacterData
                {
                    Id = "yuan_shao",
                    Name = "袁紹",
                    Courtesy = "本初",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 80, Warfare = 68, Intelligence = 62, Politics = 72, Charisma = 82 },
                    Description = "四世三公の名門。河北を制した大勢力の君主。",
                    BirthYear = 154,
                    DeathYear = 202,
                    Skills = new List<string> { "名門", "優柔不断" }
                },
                // 張角
                new CharacterData
                {
                    Id = "zhang_jiao",
                    Name = "張角",
                    Courtesy = "",
                    Type = CharacterType.Ruler,
                    Stats = new CharacterStats { Leadership = 88, Warfare = 55, Intelligence = 82, Politics = 78, Charisma = 95 },
                    Description = "太平道の教祖。黄巾の乱を起こした宗教指導者。",
                    BirthYear = 140,
                    DeathYear = 184,
                    Skills = new List<string> { "太平道", "呪術", "扇動" }
                },
                // 顔良
                new CharacterData
                {
                    Id = "yan_liang",
                    Name = "顔良",
                    Courtesy = "",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 78, Warfare = 92, Intelligence = 45, Politics = 35, Charisma = 55 },
                    Description = "袁紹配下の猛将。関羽に討たれた。",
                    BirthYear = 160,
                    DeathYear = 200,
                    Skills = new List<string> { "猛将" }
                },
                // 文醜
                new CharacterData
                {
                    Id = "wen_chou",
                    Name = "文醜",
                    Courtesy = "",
                    Type = CharacterType.General,
                    Stats = new CharacterStats { Leadership = 76, Warfare = 90, Intelligence = 42, Politics = 32, Charisma = 52 },
                    Description = "袁紹配下の猛将。顔良と並び称された。",
                    BirthYear = 162,
                    DeathYear = 200,
                    Skills = new List<string> { "猛将" }
                }
            };
        }

        #endregion
    }

    /// <summary>
    /// 武将データ
    /// </summary>
    [System.Serializable]
    public class CharacterData
    {
        public string Id;
        public string Name;
        public string Courtesy;
        public CharacterType Type;
        public CharacterStats Stats;
        public string Description;
        public int BirthYear;
        public int DeathYear;
        public List<string> Skills;
    }

    /// <summary>
    /// 武将ステータス
    /// </summary>
    [System.Serializable]
    public class CharacterStats
    {
        public int Leadership;   // 統率
        public int Warfare;      // 武力
        public int Intelligence; // 知力
        public int Politics;     // 政治
        public int Charisma;     // 魅力
    }
}
