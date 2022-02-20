public enum PhotonCode
{
    MAX_CCU_REACHED = 32757, //Photon에서 제공하는 CCU user수가 한계에 도달함
    SERVER_FULL = 32762, //서버 인원수가 꽉참
    GAME_FULL = 32765, //게임 인원수가 꽉참
    GAME_CLOSED = 32764, //게임이 끝남
    EXIST_ROOM = 32766, //이미 존재하는 방
    INVALID_AUTHENTICATION = 32767, //계정 접근 실패(AppId 확인)
    INVALID_ENCRYPTION_PARAMETERS = 32741, //잘못된 암호화를 전달
    EXTERNAL_HTTP_CALL_FAILED = 32744, //외부 서버 호출 실패(webRPC 요청 실패)
    OPERATION_NOTALLOWED_INCURRENT_STATE = -3 //PUN에서 상태가 JoinLobby(AutoJoinLobby = true) 또는 ConnectedToMaster(AutoJoinLobby = false)가 될 때까지 기다립니다.
}