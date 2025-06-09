// Scripts/ApiConfig.cs
public static class ApiConfig
{
    public static readonly string AUTH_BASE_URL = "https://bdvhnjkzea.execute-api.us-east-1.amazonaws.com/dev";

    public static readonly string LOGIN_URL = $"{AUTH_BASE_URL}/auth/login";
    public static readonly string REGISTER_URL = $"{AUTH_BASE_URL}/auth/register";
    public static readonly string GET_CLASSROOMS = $"{AUTH_BASE_URL}/classrooms/by-teacher";
    public static readonly string CREATE_CLASSROOM = $"{AUTH_BASE_URL}/classrooms/create";
    public static string GetStudentSkins(string userId) => $"{AUTH_BASE_URL}/auth/student/skins/{userId}";
    public static string UpdateStudentSkin(string userId) => $"{AUTH_BASE_URL}/auth/student/skin/{userId}";
}