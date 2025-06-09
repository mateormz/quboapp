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

    private string loginUrl = "https://bdvhnjkzea.execute-api.us-east-1.amazonaws.com/dev/auth/login";

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
        // Usar clase serializable
        LoginRequestPost loginData = new LoginRequestPost
        {
            email = email,
            password = password
        };

        string json = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
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

                SceneManager.LoadScene("Main");
            }
            else
            {
                Debug.LogError("Error en login: " + request.downloadHandler.text);

                // Manejo de errores
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
}