namespace ThirtySixStratagems.Core
{
    /// <summary>
    /// ゲーム定数
    /// </summary>
    public static class Constants
    {
        // シーン名
        public static class Scenes
        {
            public const string Title = "TitleScene";
            public const string MainMenu = "MainMenuScene";
            public const string Game = "GameScene";
            public const string Battle = "BattleScene";
        }

        // ゲームバランス
        public static class Balance
        {
            // 計略
            public const int DefaultMaxStratagemPoints = 10;
            public const int StratagemPointRecoveryBase = 2;
            public const int StratagemBaseSuccessRate = 70;

            // 戦闘
            public const float WinnerLossRate = 0.15f;      // 勝者の損害率
            public const float LoserLossRate = 0.40f;       // 敗者の損害率
            public const float DefenseBonus = 1.2f;         // 防御側ボーナス
            public const float MoraleImpact = 0.5f;         // 士気の戦闘力への影響

            // 経済
            public const int BaseIncomePerEconomy = 10;     // 経済力1あたりの収入
            public const int PopulationIncomeBonus = 1000;  // 人口ボーナス基準
            public const int FoodConsumptionPerSoldier = 1; // 兵士1人あたりの食料消費

            // 徴兵
            public const int RecruitmentPopulationRatio = 10; // 徴兵可能人口比率
            public const int RecruitmentCostPerSoldier = 10;  // 兵士1人あたりの徴兵コスト

            // 士気
            public const int MaxMorale = 100;
            public const int MoraleRecoveryPerTurn = 5;
            public const int MoraleLossOnDefeat = 20;
            public const int MoraleLossNoSupply = 10;

            // 忠誠
            public const int MaxLoyalty = 100;
            public const int LoyaltyLossOnDefeat = 5;
            public const int LoyaltyThresholdForDefection = 20;
        }

        // UI
        public static class UI
        {
            public const float TooltipDelay = 0.5f;
            public const float FadeTransitionDuration = 0.3f;
            public const float NotificationDuration = 3f;
        }

        // セーブデータ
        public static class Save
        {
            public const string SaveFolderName = "Saves";
            public const string SaveFileExtension = ".json";
            public const int MaxSaveSlots = 10;
            public const int SaveVersion = 1;
        }

        // 音量
        public static class Audio
        {
            public const float DefaultBGMVolume = 0.7f;
            public const float DefaultSEVolume = 0.8f;
            public const string BGMVolumeKey = "BGMVolume";
            public const string SEVolumeKey = "SEVolume";
        }

        // タグ・レイヤー
        public static class Tags
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string Territory = "Territory";
            public const string Army = "Army";
        }

        // PlayerPrefsキー
        public static class PlayerPrefsKeys
        {
            public const string Language = "Language";
            public const string ScreenWidth = "ScreenWidth";
            public const string ScreenHeight = "ScreenHeight";
            public const string Fullscreen = "Fullscreen";
            public const string QualityLevel = "QualityLevel";
        }
    }
}
