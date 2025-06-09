// Scripts/ApiConfig.cs
public static class ApiConfig
{
    // Base de autenticación
    public static readonly string AUTH_BASE_URL = "https://g6tzwkucx3.execute-api.us-east-1.amazonaws.com/dev";
    public static readonly string LOGIN_URL = $"{AUTH_BASE_URL}/auth/login";
    public static readonly string REGISTER_URL = $"{AUTH_BASE_URL}/auth/register";
    public static readonly string GET_CLASSROOMS = $"{AUTH_BASE_URL}/classrooms/by-teacher";
    public static readonly string CREATE_CLASSROOM = $"{AUTH_BASE_URL}/classrooms/create";
    public static string GetStudentSkins(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";
    public static string UpdateStudentSkin(string userId) => $"{AUTH_BASE_URL}/auth/student/skin/{userId}";

    // Base del juego
    public static readonly string GAME_BASE_URL = "https://0mztjazn7i.execute-api.us-east-1.amazonaws.com/dev";

    public static string GetLevel(string gameId, int level) =>
        $"{GAME_BASE_URL}/games/{gameId}/levels/{level}";

    public static string GetQuestion(string questionId) =>
        $"{GAME_BASE_URL}/games/questions/{questionId}";

    public static string SubmitLevel(string gameId, int level) =>
        $"{GAME_BASE_URL}/games/{gameId}/levels/{level}/submit";

    public static string SubmitSingle(string gameId) =>
        $"{GAME_BASE_URL}/games/{gameId}/submits";
}
