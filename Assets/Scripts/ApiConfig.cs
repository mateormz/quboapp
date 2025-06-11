// Scripts/ApiConfig.cs
public static class ApiConfig
{
    public static readonly string AUTH_BASE_URL = "https://t7ciz97x12.execute-api.us-east-1.amazonaws.com/dev";
    public static readonly string SKINS_BASE_URL = "https://q56793cuc6.execute-api.us-east-1.amazonaws.com/dev";

    // Auth
    public static readonly string LOGIN_URL = $"{AUTH_BASE_URL}/auth/login";
    public static readonly string REGISTER_URL = $"{AUTH_BASE_URL}/auth/register";
    public static readonly string GET_CLASSROOMS = $"{AUTH_BASE_URL}/classrooms/by-teacher";
    public static readonly string CREATE_CLASSROOM = $"{AUTH_BASE_URL}/classrooms/create";
    public static string GET_USER_BY_ID(string userId) => $"{AUTH_BASE_URL}/auth/users/get/{userId}";
  
    public static string GetUserData(string userId) => $"{AUTH_BASE_URL}/auth/users/get/{userId}";

    // Skins
    public static string GET_USER_SKINS_URL(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";
    public static string UPDATE_SKIN_SELECTED(string userId) => $"{AUTH_BASE_URL}/auth/student/skin/{userId}";
    public static string UNLOCK_SKIN(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";

    public static string GET_SKINS_URL = $"{SKINS_BASE_URL}/skins";

    // Coins
    public static string UPDATE_USER_COINS(string userId) => $"{AUTH_BASE_URL}/auth/student/coins/{userId}";

    // Streak
    public static string UPDATE_USER_STREAK(string userId) => $"{AUTH_BASE_URL}/auth/student/streak/{userId}";
  

    // === Base del juego ===
    public static readonly string GAME_BASE_URL = "https://cs3veym1u9.execute-api.us-east-1.amazonaws.com/dev";

    public static string GetLevel(string gameId, int level) =>
        $"{GAME_BASE_URL}/games/{gameId}/levels/{level}";

    public static string GetQuestion(string questionId) =>
        $"{GAME_BASE_URL}/games/questions/{questionId}";

    public static string SubmitLevel(string gameId, int level) =>
        $"{GAME_BASE_URL}/games/{gameId}/levels/{level}/submit";

    public static string SubmitSingle(string gameId) =>
        $"{GAME_BASE_URL}/games/{gameId}/submits";

    // Obtener todos los juegos
    public static readonly string GET_ALL_GAMES = $"{GAME_BASE_URL}/games";

    // === Game IDs ===
    public static class GameIds
    {
        public static readonly string Qubo1 = "ed7bdef1-74c2-4a1c-a055-d5876b069734";
        public static readonly string Qubo2 = "d8e09986-6c0a-4301-a16a-89f4ae1d00c4";
    }
}
