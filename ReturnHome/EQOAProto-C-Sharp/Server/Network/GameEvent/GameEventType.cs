﻿namespace ReturnHome.Server.Network.GameEvent
{
    public enum GameEventType
    {
        AllegianceUpdateAborted = 0x0003,
        PopupString = 0x0004,
        PlayerDescription = 0x0013,
        AllegianceUpdate = 0x0020,
        FriendsListUpdate = 0x0021,
        InventoryPutObjInContainer = 0x0022,
        WieldObject = 0x0023,
        CharacterTitle = 0x0029,
        UpdateTitle = 0x002B,
        CloseGroundContainer = 0x0052,
        VendorInfoEvent = 0x0062,
        ApproachVendor = 0x0062,
        StartBarber = 0x0075,
        InventoryServerSaveFailed = 0x00A0,
        FellowshipQuit = 0x00A3,
        FellowshipDismiss = 0x00A4,
        BookDataResponse = 0x00B4,
        BookModifyPageResponse = 0x00B5,
        BookAddPageResponse = 0x00B6,
        BookDeletePageResponse = 0x00B7,
        BookPageDataResponse = 0x00B8,
        GetInscriptionResponse = 0x00C3,
        IdentifyObjectResponse = 0x00C9,
        ChannelBroadcast = 0x0147,
        ChannelList = 0x0148,
        ChannelIndex = 0x0149,
        ViewContents = 0x0196,
        InventoryPutObjectIn3D = 0x019A,
        AttackDone = 0x01A7,
        MagicRemoveSpell = 0x01A8,
        VictimNotification = 0x01AC,
        KillerNotification = 0x01AD,
        AttackerNotification = 0x01B1,
        DefenderNotification = 0x01B2,
        EvasionAttackerNotification = 0x01B3,
        EvasionDefenderNotification = 0x01B4,
        CombatCommenceAttack = 0x01B8,
        UpdateHealth = 0x01C0,
        QueryAgeResponse = 0x01C3,
        UseDone = 0x01C7,
        AllegianceAllegianceUpdateDone = 0x01C8,
        FellowshipFellowUpdateDone = 0x01C9,
        FellowshipFellowStatsDone = 0x01CA,
        ItemAppraiseDone = 0x01CB,
        Emote = 0x01E2,
        PingResponse = 0x01EA,
        SetSquelchDB = 0x01F4,
        RegisterTrade = 0x01FD,
        OpenTrade = 0x01FE,
        CloseTrade = 0x01FF,
        AddToTrade = 0x0200,
        RemoveFromTrade = 0x0201,
        AcceptTrade = 0x0202,
        DeclineTrade = 0x0203,
        ResetTrade = 0x0205,
        TradeFailure = 0x0207,
        ClearTradeAcceptance = 0x0208,
        HouseProfile = 0x021D,
        HouseData = 0x0225,
        HouseStatus = 0x0226,
        UpdateRentTime = 0x0227,
        UpdateRentPayment = 0x0228,
        HouseUpdateRestrictions = 0x0248,
        UpdateHAR = 0x0257,
        HouseTransaction = 0x0259,
        QueryItemManaResponse = 0x0264,
        AvailableHouses = 0x0271,
        CharacterConfirmationRequest = 0x0274,
        CharacterConfirmationDone = 0x0276,
        AllegianceLoginNotification = 0x027A,
        AllegianceInfoResponse = 0x027C,
        JoinGameResponse = 0x0281,
        StartGame = 0x0282,
        MoveResponse = 0x0283,
        OpponentTurn = 0x0284,
        OpponentStalemate = 0x0285,
        WeenieError = 0x028A,
        WeenieErrorWithString = 0x028B,
        GameOver = 0x028C,
        SetTurbineChatChannels = 0x0295,
        AdminQueryPluginList = 0x02AE,
        AdminQueryPlugin = 0x02B1,
        AdminQueryPluginResponse = 0x02B3,
        SalvageOperationsResult = 0x02B4,
        Tell = 0x02BD,
        FellowshipFullUpdate = 0x02BE,
        FellowshipDisband = 0x02BF,
        FellowshipUpdateFellow = 0x02C0,
        MagicUpdateSpell = 0x02C1,
        MagicUpdateEnchantment = 0x02C2,
        MagicRemoveEnchantment = 0x02C3,
        MagicUpdateMultipleEnchantments = 0x02C4,
        MagicRemoveMultipleEnchantments = 0x02C5,
        MagicPurgeEnchantments = 0x02C6,
        MagicDispelEnchantment = 0x02C7,
        MagicDispelMultipleEnchantments = 0x02C8,
        MiscPortalStormBrewing = 0x02C9,
        MiscPortalStormImminent = 0x02CA,
        MiscPortalStorm = 0x02CB,
        MiscPortalstormSubsided = 0x02CC,
        CommunicationTransientString = 0x02EB,
        MagicPurgeBadEnchantments = 0x0312,
        SendClientContractTrackerTable = 0x0314,
        SendClientContractTracker = 0x0315,
    }
}
