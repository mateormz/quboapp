// Scripts/ApiConfig.cs
public static class ApiConfig
{
    // === Base de autenticación ===
    public static readonly string AUTH_BASE_URL = "https://g6tzwkucx3.execute-api.us-east-1.amazonaws.com/dev";
    public static readonly string LOGIN_URL = $"{AUTH_BASE_URL}/auth/login";
    public static readonly string REGISTER_URL = $"{AUTH_BASE_URL}/auth/register";
    public static readonly string GET_CLASSROOMS = $"{AUTH_BASE_URL}/classrooms/by-teacher";
    public static readonly string CREATE_CLASSROOM = $"{AUTH_BASE_URL}/classrooms/create";

    public static string GetStudentSkins(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";
    public static string UpdateStudentSkin(string userId) => $"{AUTH_BASE_URL}/auth/student/skin/{userId}";

    // Obtener datos de un usuario específico
    public static string GetUserData(string userId) => $"{AUTH_BASE_URL}/auth/users/get/{userId}";

    // === Base del juego ===
    public static readonly string GAME_BASE_URL = "https://0mztjazn7i.execute-api.us-east-1.amazonaws.com/dev";

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
        public static readonly string Qubo1 = "a3d59a39-c738-450f-8f56-af0bd0ef4302";
        public static readonly string Qubo2 = "89ecea64-e70b-474c-b51f-dbeaa44605bc";
    }

    // === Base para Feedback ===
    public static readonly string FEEDBACK_BASE_URL = "https://xbtj4m5148.execute-api.us-east-1.amazonaws.com/dev";
    
    // Endpoint para obtener el feedback
    // El método POST no debería tener el session_id en la URL, lo manejamos en el body
    public static string GET_FEEDBACK() => $"{FEEDBACK_BASE_URL}/feedback/generate"; // Eliminamos el ?session_id={sessionId} pero lo pasamos en el body
}
