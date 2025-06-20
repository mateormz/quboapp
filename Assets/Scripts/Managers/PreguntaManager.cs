using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Data;

public class PreguntaManager : MonoBehaviour
{
    public TextMeshProUGUI textoPregunta;
    public AlternativaController[] zonasAlternativas;
    public GameObject panelPerdiste;
    public GameObject panelCargando;
    public GameObject panelVictoria;
    public GameObject mensajeIncorrectoUI;

    private PreguntaData[] preguntas;
    private int indiceActual = 0;
    private SubmitWrapper submitData = new SubmitWrapper();

    private string gameId;
    private int level;
    private string token;

    private float inicioNivel;

    void Start()
    {
        Time.timeScale = 1f;
        token = PlayerPrefs.GetString("token");

        bool esModoAsignacion = PlayerPrefs.GetInt("modo_asignacion", 0) == 1;

        if (panelCargando != null)
            panelCargando.SetActive(true);

        if (esModoAsignacion)
        {
            Debug.Log("📘 Modo asignación detectado");
            StartCoroutine(CargarPreguntasDesdeAssignmentLevel());
        }
        else
        {
            level = PlayerPrefs.GetInt("nivel_seleccionado", 1);

            if (!PlayerPrefs.HasKey("selected_game_id"))
            {
                Debug.LogWarning("⚠️ No hay gameId seleccionado. Regresando a selección de juegos.");
                SceneManager.LoadScene("Games");
                return;
            }

            gameId = PlayerPrefs.GetString("selected_game_id");
            Debug.Log("🔢 Nivel seleccionado: " + level);
            Debug.Log("🎮 Game ID: " + gameId);

            StartCoroutine(CargarPreguntasDesdeGameAPI());
        }

        inicioNivel = Time.time;
    }

    IEnumerator CargarPreguntasDesdeGameAPI()
    {
        string levelUrl = ApiConfig.GetLevel(gameId, level);
        UnityWebRequest www = UnityWebRequest.Get(levelUrl);
        www.SetRequestHeader("Authorization", token);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Error cargando nivel: {www.error}");
            yield break;
        }

        LevelResponse levelData = JsonUtility.FromJson<LevelResponse>(www.downloadHandler.text);
        List<PreguntaData> preguntasCompletas = new List<PreguntaData>();

        foreach (string qid in levelData.questions)
        {
            string questionUrl = ApiConfig.GetQuestion(qid);
            UnityWebRequest qRequest = UnityWebRequest.Get(questionUrl);
            qRequest.SetRequestHeader("Authorization", token);
            yield return qRequest.SendWebRequest();

            if (qRequest.result == UnityWebRequest.Result.Success)
            {
                PreguntaData pregunta = JsonUtility.FromJson<PreguntaData>(qRequest.downloadHandler.text);
                preguntasCompletas.Add(pregunta);
            }
            else
            {
                Debug.LogWarning($"⚠️ Error obteniendo pregunta {qid}: {qRequest.error}");
            }
        }

        preguntas = preguntasCompletas.ToArray();
        indiceActual = 0;
        if (panelCargando != null) panelCargando.SetActive(false);
        MostrarPreguntaActual();
    }

IEnumerator CargarPreguntasDesdeAssignmentLevel()
    {
        string levelId = PlayerPrefs.GetString("selected_assignment_level_id", "");
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogError("❌ No se encontró el level_id para la asignación.");
            yield break;
        }

        string url = ApiConfig.GetQuestionsFromAssignmentLevel(levelId);
        Debug.Log("🔗 Consultando preguntas del nivel de asignación: " + url);

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", token);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error al obtener preguntas del nivel de asignación: " + req.downloadHandler.text);
            yield break;
        }

        AssignmentLevelQuestionResponse data = JsonUtility.FromJson<AssignmentLevelQuestionResponse>(req.downloadHandler.text);
        if (data == null || data.questions == null || data.questions.Count == 0)
        {
            Debug.LogWarning("⚠️ No se encontraron preguntas en el nivel de asignación.");
            yield break;
        }

        preguntas = data.questions.ToArray();
        indiceActual = 0;
        if (panelCargando != null) panelCargando.SetActive(false);
        MostrarPreguntaActual();
    }

    public void MostrarPreguntaActual()
    {
        if (preguntas == null || preguntas.Length == 0)
        {
            textoPregunta.text = "No hay preguntas disponibles.";
            return;
        }

        if (indiceActual >= preguntas.Length)
        {
            StartCoroutine(FinalizarJuego());
            return;
        }

        PreguntaData p = preguntas[indiceActual];
        textoPregunta.text = p.text;

        for (int i = 0; i < zonasAlternativas.Length; i++)
        {
            if (zonasAlternativas[i] != null && i < p.options.Length)
            {
                bool esCorrecta = i == p.correctIndex;
                zonasAlternativas[i].Configurar(p.options[i], esCorrecta, this, i);
            }
        }
    }

    public void Responder(bool correcta, int indexSeleccionado)
    {
        var pregunta = preguntas[indiceActual];
        submitData.responses.Add(new SubmitResponse(pregunta.question_id, indexSeleccionado));

        if (correcta)
        {
            CurrencyManager.Instance.SumarMonedas(5);
        }
        else
        {
            mensajeIncorrectoUI.SetActive(true);
            StartCoroutine(MostrarMensajeIncorrecto()); 
        }

        indiceActual++;
        MostrarPreguntaActual();
    }

    IEnumerator MostrarMensajeIncorrecto()
    {
        yield return new WaitForSeconds(1f);
        mensajeIncorrectoUI.SetActive(false);
    }
    IEnumerator FinalizarJuego()
    {
        if (panelCargando != null) panelCargando.SetActive(true);

        bool esModoAsignacion = PlayerPrefs.GetInt("modo_asignacion", 0) == 1;

        float tiempoFinal = Time.time;
        float duracion = tiempoFinal - inicioNivel;
        submitData.level_time = Mathf.RoundToInt(duracion).ToString();
        
        string json = JsonUtility.ToJson(submitData);

        string url;
        if (esModoAsignacion)
        {
            string levelId = PlayerPrefs.GetString("selected_assignment_level_id", "");
            url = ApiConfig.SubmitAssignmentLevel(levelId);
            Debug.Log("📤 Enviando respuestas al endpoint de asignaciones: " + url);
        }
        else
        {
            url = ApiConfig.SubmitLevel(gameId, level);
            Debug.Log("📤 Enviando respuestas al endpoint de juegos: " + url);
        }

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", token);

        yield return req.SendWebRequest();

        if (panelCargando != null) panelCargando.SetActive(false);

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Error enviando respuestas: {req.error}");
            Perder();
        }
        else
        {
            Debug.Log("✅ Envío exitoso: " + req.downloadHandler.text);

            SubmitResult resultado = JsonUtility.FromJson<SubmitResult>(req.downloadHandler.text);

            if (resultado.passed)
            {
                Debug.Log("🎉 ¡Nivel aprobado!");
                Time.timeScale = 0f;
                if (panelVictoria != null) panelVictoria.SetActive(true);
            }
            else
            {
                Debug.Log("🚫 Nivel no aprobado.");

                PlayerPrefs.SetString("feedback_session_id", resultado.sessionId);
                PlayerPrefs.Save();
                Debug.Log("🧠 SessionId guardado para feedback: " + resultado.sessionId);

                Perder();
            }
        }
    }

    void Perder()
    {
        Time.timeScale = 0f;
        if (panelPerdiste != null) panelPerdiste.SetActive(true);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("modo_asignacion", 0);
        SceneManager.LoadScene("Main");
    }
}

[System.Serializable]
public class AssignmentLevelQuestionResponse
{
    public List<PreguntaData> questions;
}