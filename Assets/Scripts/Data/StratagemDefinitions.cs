using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Data
{
    /// <summary>
    /// 三十六計の定義データ
    /// エディター拡張でScriptableObject生成時に使用
    /// </summary>
    public static class StratagemDefinitions
    {
        /// <summary>
        /// 三十六計の全定義
        /// </summary>
        public static readonly StratagemDefinition[] AllStratagems = new StratagemDefinition[]
        {
            // ========== 第一套：勝戦計（優勢時に使う計略）==========
            new StratagemDefinition
            {
                Number = 1,
                NameJP = "瞞天過海",
                Reading = "まんてんかかい",
                Category = StratagemCategory.Winning,
                OriginalText = "備周則意怠、常見則不疑。陰在陽之内、不在陽之対。太陽、太陰。",
                ModernTranslation = "準備が万全だと油断し、見慣れたものは疑わない。陰は陽の中にあり、陽と対立するものではない。",
                HistoricalExample = "唐の太宗が海を渡る際、船を偽装して気づかぬうちに海を越えた故事。",
                Effect = StratagemEffectType.StealthMovement,
                EffectValue = 100,
                CostSP = 2,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 2,
                NameJP = "囲魏救趙",
                Reading = "いぎきゅうちょう",
                Category = StratagemCategory.Winning,
                OriginalText = "共敵不如分敵、敵陽不如敵陰。",
                ModernTranslation = "敵を集めて戦うより分散させよ。正面から戦うより背後を突け。",
                HistoricalExample = "斉の孫臏が趙を救うため、直接援軍を送らず魏の首都を攻めた故事。",
                Effect = StratagemEffectType.ForceRetreat,
                EffectValue = 80,
                CostSP = 3,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 3,
                NameJP = "借刀殺人",
                Reading = "しゃくとうさつじん",
                Category = StratagemCategory.Winning,
                OriginalText = "敵已明、友未定、引友殺敵、不自出力。",
                ModernTranslation = "敵が明らかで味方が定まらぬとき、味方を使って敵を倒し、自らは力を出さない。",
                HistoricalExample = "曹操が呂布を滅ぼす際、劉備の力を借りた故事。",
                Effect = StratagemEffectType.FactionConflict,
                EffectValue = 50,
                CostSP = 4,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 4,
                NameJP = "以逸待労",
                Reading = "いいつたいろう",
                Category = StratagemCategory.Winning,
                OriginalText = "困敵之勢、不以戦。損剛益柔。",
                ModernTranslation = "敵の勢いを困らせるのに戦わずして行う。剛を損じて柔を益す。",
                HistoricalExample = "諸葛亮が司馬懿と対峙し、持久戦で敵を疲弊させた故事。",
                Effect = StratagemEffectType.DefenseBoost,
                EffectValue = 30,
                CostSP = 2,
                Duration = 3
            },
            new StratagemDefinition
            {
                Number = 5,
                NameJP = "趁火打劫",
                Reading = "ちんかだごう",
                Category = StratagemCategory.Winning,
                OriginalText = "敵之害大、就勢取利。剛決柔也。",
                ModernTranslation = "敵の災いが大きいとき、勢いに乗じて利を取る。",
                HistoricalExample = "呉が荊州の内乱に乗じて関羽を討った故事。",
                Effect = StratagemEffectType.AttackBoost,
                EffectValue = 50,
                CostSP = 2,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 6,
                NameJP = "声東撃西",
                Reading = "せいとうげきせい",
                Category = StratagemCategory.Winning,
                OriginalText = "敵志乱萃、不虞。坤下兌上之象、利西南、不利東北。",
                ModernTranslation = "敵の意志を乱し、虚を突く。",
                HistoricalExample = "韓信が趙を攻める際、陽動作戦で敵を欺いた故事。",
                Effect = StratagemEffectType.Ambush,
                EffectValue = 40,
                CostSP = 3,
                Duration = 1
            },

            // ========== 第二套：敵戦計（拮抗時に使う計略）==========
            new StratagemDefinition
            {
                Number = 7,
                NameJP = "無中生有",
                Reading = "むちゅうしょうう",
                Category = StratagemCategory.Enemy,
                OriginalText = "誑也、非誑也、実其所誑也。",
                ModernTranslation = "欺くのは欺くためではなく、欺いたことを真実にするためである。",
                HistoricalExample = "張儀が楚の懐王を欺き、秦との同盟を破棄させた故事。",
                Effect = StratagemEffectType.Disinformation,
                EffectValue = 60,
                CostSP = 3,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 8,
                NameJP = "暗渡陳倉",
                Reading = "あんとちんそう",
                Category = StratagemCategory.Enemy,
                OriginalText = "示之以動、利其静而有主。益動而巽。",
                ModernTranslation = "動きを見せて敵を引きつけ、その隙に別の道から攻める。",
                HistoricalExample = "劉邦が桟道を修復すると見せかけ、陳倉から関中に入った故事。",
                Effect = StratagemEffectType.Ambush,
                EffectValue = 50,
                CostSP = 4,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 9,
                NameJP = "隔岸観火",
                Reading = "かくがんかんか",
                Category = StratagemCategory.Enemy,
                OriginalText = "陽乖序乱、陰以待逆。暴戻恣睢、其勢自斃。",
                ModernTranslation = "敵の内部が乱れるのを待ち、自滅を見守る。",
                HistoricalExample = "曹操が袁紹と袁術の争いを傍観し、両者の弱体化を待った故事。",
                Effect = StratagemEffectType.FactionConflict,
                EffectValue = 40,
                CostSP = 2,
                Duration = 3
            },
            new StratagemDefinition
            {
                Number = 10,
                NameJP = "笑裏蔵刀",
                Reading = "しょうりぞうとう",
                Category = StratagemCategory.Enemy,
                OriginalText = "信而安之、陰以図之。備而後動、勿使有変。",
                ModernTranslation = "信頼させて安心させ、密かに計画を立てる。準備してから動き、変化を生じさせない。",
                HistoricalExample = "荊軻が秦王を刺そうとした際、友好を装った故事。",
                Effect = StratagemEffectType.Diplomacy,
                EffectValue = 30,
                CostSP = 3,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 11,
                NameJP = "李代桃僵",
                Reading = "りだいとうきょう",
                Category = StratagemCategory.Enemy,
                OriginalText = "勢必有損、損陰以益陽。",
                ModernTranslation = "勢いには必ず損失がある。小を犠牲にして大を守る。",
                HistoricalExample = "趙の藺相如が和氏の璧を守るため、自らの命を賭した故事。",
                Effect = StratagemEffectType.DefenseBoost,
                EffectValue = 50,
                CostSP = 2,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 12,
                NameJP = "順手牽羊",
                Reading = "じゅんしゅけんよう",
                Category = StratagemCategory.Enemy,
                OriginalText = "微隙在所必乗、微利在所必得。少陰、少陽。",
                ModernTranslation = "わずかな隙も必ず乗じ、わずかな利も必ず得る。",
                HistoricalExample = "戦争中に敵の物資を奪う一般的な戦術。",
                Effect = StratagemEffectType.ResourcePlunder,
                EffectValue = 30,
                CostSP = 1,
                Duration = 1
            },

            // ========== 第三套：攻戦計（攻勢時に使う計略）==========
            new StratagemDefinition
            {
                Number = 13,
                NameJP = "打草驚蛇",
                Reading = "だそうきょうだ",
                Category = StratagemCategory.Attack,
                OriginalText = "疑以叩実、察而後動。復者、陰之媒也。",
                ModernTranslation = "疑わしきは確かめ、察してから動く。繰り返しは陰謀の兆しである。",
                HistoricalExample = "偵察行動で敵の動きを探る一般的な戦術。",
                Effect = StratagemEffectType.Reconnaissance,
                EffectValue = 100,
                CostSP = 1,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 14,
                NameJP = "借屍還魂",
                Reading = "しゃくしかんこん",
                Category = StratagemCategory.Attack,
                OriginalText = "有用者、不可借。不能用者、求借。借不能用者而用之。",
                ModernTranslation = "使えるものは借りられない。使えないものを借りて使う。",
                HistoricalExample = "劉備が荊州を借りて蜀を建国した故事。",
                Effect = StratagemEffectType.TerritoryControl,
                EffectValue = 60,
                CostSP = 5,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 15,
                NameJP = "調虎離山",
                Reading = "ちょうこりざん",
                Category = StratagemCategory.Attack,
                OriginalText = "待天以困之、用人以誘之。往蹇来返。",
                ModernTranslation = "天の時を待って困らせ、人を使って誘い出す。",
                HistoricalExample = "孫堅が劉表を誘い出して戦った故事。",
                Effect = StratagemEffectType.ForceRetreat,
                EffectValue = 60,
                CostSP = 3,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 16,
                NameJP = "欲擒姑縦",
                Reading = "よくきんこしょう",
                Category = StratagemCategory.Attack,
                OriginalText = "逼則反兵、走則減勢。緊随勿迫、累其気力、消其闘志。",
                ModernTranslation = "追い詰めれば反撃され、逃げれば勢いが減る。緊迫せずに追い、気力と闘志を消耗させる。",
                HistoricalExample = "諸葛亮が孟獲を七度捕らえて七度放した故事。",
                Effect = StratagemEffectType.LoyaltyReduce,
                EffectValue = 30,
                CostSP = 4,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 17,
                NameJP = "拋磚引玉",
                Reading = "ほうせんいんぎょく",
                Category = StratagemCategory.Attack,
                OriginalText = "類以誘之、撃蒙也。",
                ModernTranslation = "似たもので誘い、無知を撃つ。",
                HistoricalExample = "小さな利益で敵を誘い、大きな罠にかける戦術。",
                Effect = StratagemEffectType.Ambush,
                EffectValue = 40,
                CostSP = 2,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 18,
                NameJP = "擒賊擒王",
                Reading = "きんぞくきんおう",
                Category = StratagemCategory.Attack,
                OriginalText = "摧其堅、奪其魁、以解其体。龍戦于野、其道窮也。",
                ModernTranslation = "堅いところを摧き、首魁を奪い、全体を瓦解させる。",
                HistoricalExample = "敵の大将を直接狙い、一気に勝負を決める戦術。",
                Effect = StratagemEffectType.CharacterCapture,
                EffectValue = 50,
                CostSP = 5,
                Duration = 1
            },

            // ========== 第四套：混戦計（混乱時に使う計略）==========
            new StratagemDefinition
            {
                Number = 19,
                NameJP = "釜底抽薪",
                Reading = "ふていちゅうしん",
                Category = StratagemCategory.Chaos,
                OriginalText = "不敵其力、而消其勢。兌下乾上之象。",
                ModernTranslation = "力に対抗せず、その勢いを消す。",
                HistoricalExample = "敵の補給線を断ち、戦力を弱体化させる戦術。",
                Effect = StratagemEffectType.SupplyDisrupt,
                EffectValue = 80,
                CostSP = 4,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 20,
                NameJP = "混水摸魚",
                Reading = "こんすいぼぎょ",
                Category = StratagemCategory.Chaos,
                OriginalText = "乗其陰乱、利其弱而無主。随、以嚮晦入宴息。",
                ModernTranslation = "混乱に乗じて、弱くて主のないものから利を得る。",
                HistoricalExample = "戦場の混乱に乗じて目的を達成する戦術。",
                Effect = StratagemEffectType.ResourcePlunder,
                EffectValue = 50,
                CostSP = 2,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 21,
                NameJP = "金蝉脱殻",
                Reading = "きんせんだっかく",
                Category = StratagemCategory.Chaos,
                OriginalText = "存其形、完其勢。友不疑、敵不動。巽而止、蛊。",
                ModernTranslation = "形を残し、勢いを保つ。味方は疑わず、敵は動かない。",
                HistoricalExample = "諸葛亮が死後も陣形を保ち、司馬懿を欺いた故事。",
                Effect = StratagemEffectType.Escape,
                EffectValue = 100,
                CostSP = 3,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 22,
                NameJP = "関門捉賊",
                Reading = "かんもんそくぞく",
                Category = StratagemCategory.Chaos,
                OriginalText = "小敵困之。剥、不利有攸往。",
                ModernTranslation = "小さな敵は追い詰める。",
                HistoricalExample = "退路を断ち、敵を殲滅する戦術。",
                Effect = StratagemEffectType.AttackBoost,
                EffectValue = 40,
                CostSP = 3,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 23,
                NameJP = "遠交近攻",
                Reading = "えんこうきんこう",
                Category = StratagemCategory.Chaos,
                OriginalText = "形禁勢格、利従近取、害以遠隔。上火下沢。",
                ModernTranslation = "形勢が制約されるとき、近くから利を取り、遠くを害とする。",
                HistoricalExample = "秦の范雎が提唱した外交戦略。",
                Effect = StratagemEffectType.Diplomacy,
                EffectValue = 50,
                CostSP = 3,
                Duration = 3
            },
            new StratagemDefinition
            {
                Number = 24,
                NameJP = "仮道伐虢",
                Reading = "かどうばっかく",
                Category = StratagemCategory.Chaos,
                OriginalText = "両大之間、敵脅以従、我仮以勢。困、有言不信。",
                ModernTranslation = "二大国の間で、敵は脅して従わせ、我は勢いを借りる。",
                HistoricalExample = "晋が虞を通って虢を滅ぼし、帰路に虞も滅ぼした故事。",
                Effect = StratagemEffectType.TerritoryControl,
                EffectValue = 70,
                CostSP = 5,
                Duration = 1
            },

            // ========== 第五套：併戦計（同盟時に使う計略）==========
            new StratagemDefinition
            {
                Number = 25,
                NameJP = "偷梁換柱",
                Reading = "とうりょうかんちゅう",
                Category = StratagemCategory.Merge,
                OriginalText = "頻更其陣、抽其勁旅、待其自敗、而後乗之。曳其輪也。",
                ModernTranslation = "陣をしばしば変え、精鋭を抜き、自滅を待って乗じる。",
                HistoricalExample = "敵の主力を別の弱い部隊と入れ替える戦術。",
                Effect = StratagemEffectType.InternalStrife,
                EffectValue = 40,
                CostSP = 4,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 26,
                NameJP = "指桑罵槐",
                Reading = "しそうばかい",
                Category = StratagemCategory.Merge,
                OriginalText = "大凌小者、警以誘之。剛中而応、行険而順。",
                ModernTranslation = "大が小を凌ぐとき、警告して誘導する。",
                HistoricalExample = "間接的に警告を与え、行動を促す戦術。",
                Effect = StratagemEffectType.Diplomacy,
                EffectValue = 30,
                CostSP = 2,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 27,
                NameJP = "仮痴不癲",
                Reading = "かちふてん",
                Category = StratagemCategory.Merge,
                OriginalText = "寧偽作不知不為、不偽作假知妄為。静不露機、雲雷屯也。",
                ModernTranslation = "知らないふりをして動かない方が、知ったかぶりで動くより良い。",
                HistoricalExample = "司馬懿が病気を装い、曹爽を油断させた故事。",
                Effect = StratagemEffectType.DefenseBoost,
                EffectValue = 20,
                CostSP = 2,
                Duration = 3
            },
            new StratagemDefinition
            {
                Number = 28,
                NameJP = "上屋抽梯",
                Reading = "じょうおくちゅうてい",
                Category = StratagemCategory.Merge,
                OriginalText = "仮之以便、唆之使前、断其援応、陥之死地。遇毒、位不当也。",
                ModernTranslation = "便宜を与えて誘い込み、援助を断って死地に陥れる。",
                HistoricalExample = "敵を誘い込み、退路を断つ戦術。",
                Effect = StratagemEffectType.Ambush,
                EffectValue = 60,
                CostSP = 4,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 29,
                NameJP = "樹上開花",
                Reading = "じゅじょうかいか",
                Category = StratagemCategory.Merge,
                OriginalText = "借局布勢、力小勢大。鴻漸于陸、其羽可用為儀也。",
                ModernTranslation = "局面を借りて勢いを作り、力は小さくても勢いを大きく見せる。",
                HistoricalExample = "虚勢を張り、兵力を多く見せる戦術。",
                Effect = StratagemEffectType.DefenseBoost,
                EffectValue = 40,
                CostSP = 2,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 30,
                NameJP = "反客為主",
                Reading = "はんかくいしゅ",
                Category = StratagemCategory.Merge,
                OriginalText = "乗隙插足、扼其主機、漸之進也。",
                ModernTranslation = "隙に乗じて足を入れ、主導権を握る。",
                HistoricalExample = "同盟で主導権を奪い取る戦術。",
                Effect = StratagemEffectType.Diplomacy,
                EffectValue = 60,
                CostSP = 4,
                Duration = 3
            },

            // ========== 第六套：敗戦計（劣勢時に使う計略）==========
            new StratagemDefinition
            {
                Number = 31,
                NameJP = "美人計",
                Reading = "びじんけい",
                Category = StratagemCategory.Defeat,
                OriginalText = "兵強者、攻其将。将智者、伐其情。将弱兵頽、其勢自萎。",
                ModernTranslation = "兵が強ければ将を攻め、将が賢ければ情を伐つ。将が弱く兵が衰えれば、勢いは自然と萎む。",
                HistoricalExample = "王允が貂蝉を使い、董卓と呂布を離間させた故事。",
                Effect = StratagemEffectType.LoyaltyReduce,
                EffectValue = 50,
                CostSP = 4,
                Duration = 3
            },
            new StratagemDefinition
            {
                Number = 32,
                NameJP = "空城計",
                Reading = "くうじょうけい",
                Category = StratagemCategory.Defeat,
                OriginalText = "虚者虚之、疑中生疑。剛柔之際、奇而復奇。",
                ModernTranslation = "虚なるものを虚として、疑いの中に疑いを生じさせる。",
                HistoricalExample = "諸葛亮が司馬懿の前で城門を開き、琴を弾いた故事。",
                Effect = StratagemEffectType.DefenseBoost,
                EffectValue = 100,
                CostSP = 3,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 33,
                NameJP = "反間計",
                Reading = "はんかんけい",
                Category = StratagemCategory.Defeat,
                OriginalText = "疑中之疑。比之自内、不自失也。",
                ModernTranslation = "疑いの中の疑い。内から比べて、自ら失わない。",
                HistoricalExample = "周瑜が蔣幹を利用し、曹操に蔡瑁・張允を処刑させた故事。",
                Effect = StratagemEffectType.InternalStrife,
                EffectValue = 70,
                CostSP = 4,
                Duration = 2
            },
            new StratagemDefinition
            {
                Number = 34,
                NameJP = "苦肉計",
                Reading = "くにくけい",
                Category = StratagemCategory.Defeat,
                OriginalText = "人不自害、受害必真。仮真真仮、間以得行。",
                ModernTranslation = "人は自らを害さない。害を受ければ必ず真実。偽りを真とし、真を偽りとして、間を行う。",
                HistoricalExample = "黄蓋が周瑜に打たれ、偽って曹操に降った故事。",
                Effect = StratagemEffectType.Diplomacy,
                EffectValue = 80,
                CostSP = 5,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 35,
                NameJP = "連環計",
                Reading = "れんかんけい",
                Category = StratagemCategory.Defeat,
                OriginalText = "将多兵衆、不可以敵、使其自累、以殺其勢。在師中吉、承天寵也。",
                ModernTranslation = "将が多く兵が多ければ、敵にできない。自ら累を招かせ、その勢いを殺す。",
                HistoricalExample = "龐統が曹操の船を鎖で繋がせ、火攻めを可能にした故事。",
                Effect = StratagemEffectType.Composite,
                EffectValue = 50,
                CostSP = 6,
                Duration = 1
            },
            new StratagemDefinition
            {
                Number = 36,
                NameJP = "走為上",
                Reading = "そういじょう",
                Category = StratagemCategory.Defeat,
                OriginalText = "全師避敵。左次無咎、未失常也。",
                ModernTranslation = "全軍で敵を避ける。次の位置に退いても咎めなし、常を失っていない。",
                HistoricalExample = "劣勢時に撤退し、再起を図る最善の策。",
                Effect = StratagemEffectType.Escape,
                EffectValue = 100,
                CostSP = 1,
                Duration = 1
            }
        };
    }

    /// <summary>
    /// 計略定義データ
    /// </summary>
    public class StratagemDefinition
    {
        public int Number;
        public string NameJP;
        public string Reading;
        public StratagemCategory Category;
        public string OriginalText;
        public string ModernTranslation;
        public string HistoricalExample;
        public StratagemEffectType Effect;
        public int EffectValue;
        public int CostSP;
        public int Duration;
    }
}
