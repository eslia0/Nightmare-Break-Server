public enum ClientPacketId
{
    None = 0,
    CreateAccount,
    DeleteAccount,
    Login,
    Logout,
    GameClose,
    CreateCharacter,
    DeleteCharacter,
    SelectCharacter,
    RequestCharacterStatus,
    RequestRoomList,
    SkillUp,
    EquipUpgrade,
    CreateRoom,
    EnterRoom,
    ExitRoom,
}

public enum ServerPacketId
{
    None = 0,
    CreateAccountResult,
    DeleteAccountResult,
    LoginResult,
    LogoutResult,
    CreateCharacterResult,
    DeleteChracterResult,
    SelectCharacterResult,
    RoomList,
    CharacterStatus,
    SkillUpResult,
    EquipUpgradeResult,
    CreateRoomResult,
    EnterRoomResult,
    ExitRoomResult,
    Match,
}