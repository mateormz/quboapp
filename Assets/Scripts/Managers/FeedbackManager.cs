using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using Data;
using static ApiConfig;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private GameObject feedbackCardPrefab; // El prefab de FeedbackCard
    [SerializeField] private Transform contentParent; // El Content dentro de HorizontalScroll

    // Asignamos el session_id proporcionado directamente
    private string sessionId;
    private string token;
 // Session ID de ejemplo para probar

    // Usar ApiConfig para la URL
    private string feedbackUrl;

    [System.Serializable]
    public class Guide
    {
        public string[] steps;
        public string[] tips;
        public string concept;
    }

    [System.Serializable]
    public class Feedback
    {
        public string question_id;
        public string topic;
        public string text;
        public Guide guide;
    }

    [System.Serializable]
    public class FeedbackResponse
    {
        public string session_id;
        public Feedback[] feedback;
    }

    // NUEVA clase para serializar correctamente el body del POST
    [System.Serializable]
    public class FeedbackRequest
    {
        public string session_id;
        public FeedbackRequest(string sessionId)
        {
            this.session_id = sessionId;
        }
    }

    void Start()
    {
        sessionId = PlayerPrefs.GetString("feedback_session_id", "");
        token = PlayerPrefs.GetString("token", "");

        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogError("❌ No se encontró el session_id en PlayerPrefs.");
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("❌ No se encontró el token en PlayerPrefs.");
            return;
        }

        feedbackUrl = ApiConfig.GET_FEEDBACK();
        StartCoroutine(FetchFeedback());
    }


    IEnumerator FetchFeedback()
    {
        // Crear objeto de solicitud con el sessionId
        var requestBody = new FeedbackRequest(sessionId);
        var postData = JsonUtility.ToJson(requestBody);
        Debug.Log("POST body: " + postData);

        var request = new UnityWebRequest(feedbackUrl, "POST");

        // Convierte el JSON en un arreglo de bytes
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Token de autenticación provisional
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", token); // Si necesitas usar "Bearer", cambia a $"Bearer {token}"

        // Enviar la solicitud
        yield return request.SendWebRequest();

        // Revisar el resultado
        if (request.result == UnityWebRequest.Result.Success)
        {
            FeedbackResponse response = JsonUtility.FromJson<FeedbackResponse>(request.downloadHandler.text);
            Debug.Log("Feedback response: " + request.downloadHandler.text);
            foreach (var fb in response.feedback)
            {
                CreateCard(fb);
            }
        }
        else
        {
            Debug.LogError("Error fetching feedback: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    void CreateCard(Feedback fb)
    {
        GameObject card = Instantiate(feedbackCardPrefab, contentParent);

        // Buscar los textos dentro del Content del Scroll Vertical
        var preguntaText = card.transform.Find("VerticalScroll/Viewport/Content/PreguntaText")?.GetComponent<TMP_Text>();
        var stepsText = card.transform.Find("VerticalScroll/Viewport/Content/StepsText")?.GetComponent<TMP_Text>();
        var tipsText = card.transform.Find("VerticalScroll/Viewport/Content/TipsText")?.GetComponent<TMP_Text>();
        var conceptText = card.transform.Find("VerticalScroll/Viewport/Content/ConceptText")?.GetComponent<TMP_Text>();

        if (preguntaText != null)
            preguntaText.text = fb.text;

        if (stepsText != null)
            stepsText.text = "• " + string.Join("\n• ", fb.guide.steps);

        if (tipsText != null)
            tipsText.text = "• " + string.Join("\n• ", fb.guide.tips);

        if (conceptText != null)
            conceptText.text = fb.guide.concept;
    }


}
