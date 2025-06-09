using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Data;

[System.Serializable]
public class LevelResponse
{
    public string game_id;
    public int level_number;
    public string[] questions;
}

public class PreguntaManager : MonoBehaviour
{
    public TextMeshProUGUI textoPregunta;
    public AlternativaController[] zonasAlternativas;
    public GameObject panelPerdiste;
    public GameObject panelCargando;
    public GameObject panelVictoria;

    private PreguntaData[] preguntas;
    private int indiceActual = 0;
    private SubmitWrapper submitData = new SubmitWrapper();

    private string gameId = "a3d59a39-c738-450f-8f56-af0bd0ef4302";
    private int level = 1;
    private string token;

    void Start()
    {
        Time.timeScale = 1f;

        // Recuperar token de PlayerPrefs
        token = PlayerPrefs.GetString("token");

        if (panelCargando != null) panelCargando.SetActive(true);
        StartCoroutine(CargarPreguntasDesdeAPI());
    }

    IEnumerator CargarPreguntasDesdeAPI()
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

        indiceActual++;
        MostrarPreguntaActual();
    }

    IEnumerator FinalizarJuego()
    {
        if (panelCargando != null) panelCargando.SetActive(true);

        string url = ApiConfig.SubmitLevel(gameId, level);
        string json = JsonUtility.ToJson(submitData);

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
        SceneManager.LoadScene("Main");
    }
}
