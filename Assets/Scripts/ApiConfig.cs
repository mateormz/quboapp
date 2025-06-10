// Scripts/ApiConfig.cs
public static class ApiConfig
{
    public static readonly string AUTH_BASE_URL = "https://bdvhnjkzea.execute-api.us-east-1.amazonaws.com/dev";
    public static readonly string SKINS_BASE_URL = "https://37aiksov83.execute-api.us-east-1.amazonaws.com/dev";

    public static readonly string LOGIN_URL = $"{AUTH_BASE_URL}/auth/login";
    public static readonly string REGISTER_URL = $"{AUTH_BASE_URL}/auth/register";
    public static readonly string GET_CLASSROOMS = $"{AUTH_BASE_URL}/classrooms/by-teacher";
    public static readonly string CREATE_CLASSROOM = $"{AUTH_BASE_URL}/classrooms/create";

    public static string GET_USER_SKINS_URL(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";
    public static string UPDATE_SKIN_SELECTED(string userId) => $"{AUTH_BASE_URL}/auth/student/skin/{userId}";
    public static string UNLOCK_SKIN(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";

    public static string GET_SKINS_URL = $"{SKINS_BASE_URL}/skins";
}