using System;
using ThirtySixStratagems.Data.Models;

namespace ThirtySixStratagems.Core
{
    /// <summary>
    /// グローバルイベントバス
    /// ゲーム内のイベント通知を一元管理
    /// </summary>
    public static class EventBus
    {
        // ========== ゲーム状態イベント ==========

        /// <summary>ゲーム状態が変更された</summary>
        public static event Action<GameState> OnGameStateChanged;

        /// <summary>ゲームが開始された</summary>
        public static event Action OnGameStarted;

        /// <summary>ゲームが終了した</summary>
        public static event Action<GameEndReason> OnGameEnded;

        // ========== ターンイベント ==========

        /// <summary>ターンが開始された</summary>
        public static event Action<int> OnTurnStarted;

        /// <summary>ターンが終了した</summary>
        public static event Action<int> OnTurnEnded;

        /// <summary>フェーズが変更された</summary>
        public static event Action<TurnPhase> OnPhaseChanged;

        /// <summary>勢力のターンが開始された</summary>
        public static event Action<string> OnFactionTurnStarted;

        /// <summary>勢力のターンが終了した</summary>
        public static event Action<string> OnFactionTurnEnded;

        // ========== 戦闘イベント ==========

        /// <summary>戦闘が開始された</summary>
        public static event Action<BattleEventArgs> OnBattleStarted;

        /// <summary>戦闘が終了した</summary>
        public static event Action<BattleResultEventArgs> OnBattleEnded;

        /// <summary>領地が占領された</summary>
        public static event Action<TerritoryConqueredEventArgs> OnTerritoryConquered;

        // ========== 計略イベント ==========

        /// <summary>計略が発動された</summary>
        public static event Action<StratagemEventArgs> OnStratagemExecuted;

        /// <summary>計略が成功した</summary>
        public static event Action<StratagemEventArgs> OnStratagemSucceeded;

        /// <summary>計略が失敗した</summary>
        public static event Action<StratagemEventArgs> OnStratagemFailed;

        // ========== 外交イベント ==========

        /// <summary>同盟が締結された</summary>
        public static event Action<AllianceEventArgs> OnAllianceFormed;

        /// <summary>同盟が破棄された</summary>
        public static event Action<AllianceEventArgs> OnAllianceBroken;

        /// <summary>宣戦布告された</summary>
        public static event Action<DiplomacyEventArgs> OnWarDeclared;

        // ========== キャラクターイベント ==========

        /// <summary>武将が登用された</summary>
        public static event Action<CharacterEventArgs> OnCharacterRecruited;

        /// <summary>武将が離反した</summary>
        public static event Action<CharacterEventArgs> OnCharacterDefected;

        /// <summary>武将が捕獲された</summary>
        public static event Action<CharacterEventArgs> OnCharacterCaptured;

        /// <summary>武将が死亡した</summary>
        public static event Action<CharacterEventArgs> OnCharacterDied;

        // ========== 軍隊イベント ==========

        /// <summary>軍が移動を開始した</summary>
        public static event Action<ArmyEventArgs> OnArmyMoveStarted;

        /// <summary>軍が移動を完了した</summary>
        public static event Action<ArmyEventArgs> OnArmyMoveCompleted;

        /// <summary>軍が作成された</summary>
        public static event Action<ArmyEventArgs> OnArmyCreated;

        /// <summary>軍が解散された</summary>
        public static event Action<ArmyEventArgs> OnArmyDisbanded;

        // ========== リソースイベント ==========

        /// <summary>リソースが変化した</summary>
        public static event Action<ResourceEventArgs> OnResourceChanged;

        // ========== UIイベント ==========

        /// <summary>領地が選択された</summary>
        public static event Action<string> OnTerritorySelected;

        /// <summary>武将が選択された</summary>
        public static event Action<string> OnCharacterSelected;

        /// <summary>軍が選択された</summary>
        public static event Action<string> OnArmySelected;

        /// <summary>通知が表示された</summary>
        public static event Action<NotificationEventArgs> OnNotificationShown;

        // ========== イベント発火メソッド ==========

        #region Game State Events

        public static void GameStateChanged(GameState state)
        {
            OnGameStateChanged?.Invoke(state);
        }

        public static void GameStarted()
        {
            OnGameStarted?.Invoke();
        }

        public static void GameEnded(GameEndReason reason)
        {
            OnGameEnded?.Invoke(reason);
        }

        #endregion

        #region Turn Events

        public static void TurnStarted(int turn)
        {
            OnTurnStarted?.Invoke(turn);
        }

        public static void TurnEnded(int turn)
        {
            OnTurnEnded?.Invoke(turn);
        }

        public static void PhaseChanged(TurnPhase phase)
        {
            OnPhaseChanged?.Invoke(phase);
        }

        public static void FactionTurnStarted(string factionId)
        {
            OnFactionTurnStarted?.Invoke(factionId);
        }

        public static void FactionTurnEnded(string factionId)
        {
            OnFactionTurnEnded?.Invoke(factionId);
        }

        #endregion

        #region Battle Events

        public static void BattleStarted(BattleEventArgs args)
        {
            OnBattleStarted?.Invoke(args);
        }

        public static void BattleEnded(BattleResultEventArgs args)
        {
            OnBattleEnded?.Invoke(args);
        }

        public static void TerritoryConquered(TerritoryConqueredEventArgs args)
        {
            OnTerritoryConquered?.Invoke(args);
        }

        #endregion

        #region Stratagem Events

        public static void StratagemExecuted(StratagemEventArgs args)
        {
            OnStratagemExecuted?.Invoke(args);
        }

        public static void StratagemSucceeded(StratagemEventArgs args)
        {
            OnStratagemSucceeded?.Invoke(args);
        }

        public static void StratagemFailed(StratagemEventArgs args)
        {
            OnStratagemFailed?.Invoke(args);
        }

        #endregion

        #region Diplomacy Events

        public static void AllianceFormed(AllianceEventArgs args)
        {
            OnAllianceFormed?.Invoke(args);
        }

        public static void AllianceBroken(AllianceEventArgs args)
        {
            OnAllianceBroken?.Invoke(args);
        }

        public static void WarDeclared(DiplomacyEventArgs args)
        {
            OnWarDeclared?.Invoke(args);
        }

        #endregion

        #region Character Events

        public static void CharacterRecruited(CharacterEventArgs args)
        {
            OnCharacterRecruited?.Invoke(args);
        }

        public static void CharacterDefected(CharacterEventArgs args)
        {
            OnCharacterDefected?.Invoke(args);
        }

        public static void CharacterCaptured(CharacterEventArgs args)
        {
            OnCharacterCaptured?.Invoke(args);
        }

        public static void CharacterDied(CharacterEventArgs args)
        {
            OnCharacterDied?.Invoke(args);
        }

        #endregion

        #region Army Events

        public static void ArmyMoveStarted(ArmyEventArgs args)
        {
            OnArmyMoveStarted?.Invoke(args);
        }

        public static void ArmyMoveCompleted(ArmyEventArgs args)
        {
            OnArmyMoveCompleted?.Invoke(args);
        }

        public static void ArmyCreated(ArmyEventArgs args)
        {
            OnArmyCreated?.Invoke(args);
        }

        public static void ArmyDisbanded(ArmyEventArgs args)
        {
            OnArmyDisbanded?.Invoke(args);
        }

        #endregion

        #region Resource Events

        public static void ResourceChanged(ResourceEventArgs args)
        {
            OnResourceChanged?.Invoke(args);
        }

        #endregion

        #region UI Events

        public static void TerritorySelected(string territoryId)
        {
            OnTerritorySelected?.Invoke(territoryId);
        }

        public static void CharacterSelected(string characterId)
        {
            OnCharacterSelected?.Invoke(characterId);
        }

        public static void ArmySelected(string armyId)
        {
            OnArmySelected?.Invoke(armyId);
        }

        public static void NotificationShown(NotificationEventArgs args)
        {
            OnNotificationShown?.Invoke(args);
        }

        #endregion

        /// <summary>
        /// 全イベントをクリア（シーン遷移時などに使用）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnGameStateChanged = null;
            OnGameStarted = null;
            OnGameEnded = null;
            OnTurnStarted = null;
            OnTurnEnded = null;
            OnPhaseChanged = null;
            OnFactionTurnStarted = null;
            OnFactionTurnEnded = null;
            OnBattleStarted = null;
            OnBattleEnded = null;
            OnTerritoryConquered = null;
            OnStratagemExecuted = null;
            OnStratagemSucceeded = null;
            OnStratagemFailed = null;
            OnAllianceFormed = null;
            OnAllianceBroken = null;
            OnWarDeclared = null;
            OnCharacterRecruited = null;
            OnCharacterDefected = null;
            OnCharacterCaptured = null;
            OnCharacterDied = null;
            OnArmyMoveStarted = null;
            OnArmyMoveCompleted = null;
            OnArmyCreated = null;
            OnArmyDisbanded = null;
            OnResourceChanged = null;
            OnTerritorySelected = null;
            OnCharacterSelected = null;
            OnArmySelected = null;
            OnNotificationShown = null;
        }
    }

    // ========== イベント引数クラス ==========

    public class BattleEventArgs
    {
        public string AttackerArmyId;
        public string DefenderArmyId;
        public string TerritoryId;
    }

    public class BattleResultEventArgs : BattleEventArgs
    {
        public string VictorFactionId;
        public int AttackerLosses;
        public int DefenderLosses;
        public bool TerritoryConquered;
    }

    public class TerritoryConqueredEventArgs
    {
        public string TerritoryId;
        public string PreviousOwnerId;
        public string NewOwnerId;
    }

    public class StratagemEventArgs
    {
        public string StratagemId;
        public string CasterFactionId;
        public string CasterCharacterId;
        public string TargetId;
        public bool Success;
        public string FailureReason;
    }

    public class AllianceEventArgs
    {
        public string FactionId1;
        public string FactionId2;
        public int Duration;
    }

    public class DiplomacyEventArgs
    {
        public string DeclarerFactionId;
        public string TargetFactionId;
    }

    public class CharacterEventArgs
    {
        public string CharacterId;
        public string PreviousFactionId;
        public string NewFactionId;
    }

    public class ArmyEventArgs
    {
        public string ArmyId;
        public string FactionId;
        public string TerritoryId;
        public string TargetTerritoryId;
    }

    public class ResourceEventArgs
    {
        public string FactionId;
        public ResourceType ResourceType;
        public int PreviousValue;
        public int NewValue;
        public int Delta;
    }

    public enum ResourceType
    {
        Gold,
        Food,
        StratagemPoints,
        Soldiers
    }

    public class NotificationEventArgs
    {
        public string Title;
        public string Message;
        public NotificationType Type;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
