public enum ClientPacketId
{
    None = 0,
    Create,
    Delete,
    Login,
    Logout,
    GameClose,
    CreateRoom,
    EnterRoom,
    ExitRoom,
}

public enum ServerPacketId
{
    None = 0,
    CreateResult,
    DeleteResult,
    LoginResult,
    Match,
    CreateRoomResult,
}