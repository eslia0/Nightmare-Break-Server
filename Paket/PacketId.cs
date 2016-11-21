public enum ClientPacketId
{
    None = 0,
    Create,
    Delete,
    Login,
    Logout,
    GameClose,
    CreateCharacter,
    DeleteCharacter,
    SelectCharacter,
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
    CreateCharacterResult,
    DeleteChracterResult,
    SelectCharacterResult,
    Match,
    CreateRoomResult,
    EnterRoomResult,
    ExitRoomResult,
}