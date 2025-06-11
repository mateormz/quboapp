using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TMP_Text mensajeErrorText;

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        mensajeErrorText.text = "";
    }

    void OnLoginClicked()
    {
        mensajeErrorText.text = "";
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            mensajeErrorText.text = "Por favor ingresa tu correo y contraseña.";
            return;
        }

        StartCoroutine(LoginRequest(email, password));
    }

    IEnumerator LoginRequest(string email, string password)
    {
        mensajeErrorText.text = "Cargando...";

        LoginRequestPost loginData = new LoginRequestPost { email = email, password = password };
        string json = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(ApiConfig.LOGIN_URL, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

                PlayerPrefs.SetString("token", response.token);
                PlayerPrefs.SetString("user_id", response.user_id);
                PlayerPrefs.SetString("role", response.role);

                // 1. Actualizar racha
                yield return StartCoroutine(ActualizarStreak(response.user_id, response.token));

                // 2. Obtener datos adicionales del usuario
                yield return StartCoroutine(ObtenerDatosDelUsuario(response.user_id, response.token));

                SceneManager.LoadScene("Main");
            }
            else
            {
                try
                {
                    ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                    mensajeErrorText.text = error.error ?? "Error desconocido al iniciar sesión.";
                }
                catch
                {
                    mensajeErrorText.text = "Error inesperado en el servidor.";
                }
            }
        }
    }

    IEnumerator ActualizarStreak(string user_id, string token)
    {
        string url = ApiConfig.UPDATE_USER_STREAK(user_id);

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            request.SetRequestHeader("Authorization", token);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    StreakResponse datos = JsonUtility.FromJson<StreakResponse>(request.downloadHandler.text);
                    PlayerPrefs.SetInt("streak", datos.streak);
                    PlayerPrefs.SetString("lastLoginDate", datos.last_login_date);
                    Debug.Log("✅ Racha actualizada: " + datos.streak);
                }
                catch
                {
                    Debug.LogWarning("⚠️ No se pudo parsear la respuesta del streak.");
                }
            }
            else
            {
                Debug.LogError("❌ Error actualizando racha: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator ObtenerDatosDelUsuario(string user_id, string token)
    {
        string url = ApiConfig.GET_USER_BY_ID(user_id); // ejemplo: auth/users/get/{user_id}

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                UsuarioDatos datos = JsonUtility.FromJson<UsuarioDatos>(request.downloadHandler.text);

                PlayerPrefs.SetInt("monedas", datos.qu_coin);
                PlayerPrefs.SetString("skinSeleccionada", datos.skinSeleccionada);
                PlayerPrefs.SetString("skinsDesbloqueadas", string.Join(",", datos.skinsDesbloqueadas));
                PlayerPrefs.SetString("nombre", datos.name);
                PlayerPrefs.SetString("apellido", datos.lastName);

                Debug.Log("✅ Datos del usuario cargados correctamente.");
            }
            else
            {
                Debug.LogError("❌ Error obteniendo datos del usuario: " + request.downloadHandler.text);
            }
        }
    }

    [System.Serializable]
    public class LoginRequestPost
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
        public string user_id;
        public string role;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string error;
    }

    [System.Serializable]
    public class UsuarioDatos
    {
        public string name;
        public string lastName;
        public string email;
        public string role;
        public int qu_coin;
        public string skinSeleccionada;
        public string[] skinsDesbloqueadas;
        public string classroom_id;
    }

    [System.Serializable]
    public class StreakResponse
    {
        public string message;
        public int streak;
        public string last_login_date;
    }
}