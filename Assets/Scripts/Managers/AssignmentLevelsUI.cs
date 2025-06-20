using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AssignmentLevelsUI : MonoBehaviour
{
    public Transform contenedorLevels;
    public GameObject levelItemPrefab;

    private List<LevelCustom> niveles = new();
    private string assignmentId;

    void Start()
    {
        assignmentId = PlayerPrefs.GetString("assignment_id", "");

        if (string.IsNullOrEmpty(assignmentId))
        {
            Debug.LogError("❌ No se encontró assignment_id en PlayerPrefs.");
            return;
        }

        Debug.Log("📘 assignment_id recuperado: " + assignmentId);
        StartCoroutine(CargarNivelesDesdeAPI());
    }

    IEnumerator CargarNivelesDesdeAPI()
    {
        string token = PlayerPrefs.GetString("token");
        string url = ApiConfig.GetLevelsFromAssignment(assignmentId);

        Debug.Log("🔗 Consultando niveles de la asignación: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", token);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error obteniendo niveles: " + request.downloadHandler.text);
                yield break;
            }

            LevelResponseWrapper response = JsonUtility.FromJson<LevelResponseWrapper>(request.downloadHandler.text);

            if (response == null || response.custom_levels == null || response.custom_levels.Length == 0)
            {
                Debug.LogWarning("⚠️ No se encontraron niveles en la asignación.");
                yield break;
            }

            niveles = new List<LevelCustom>(response.custom_levels);
            Debug.Log($"📦 Niveles recibidos: {niveles.Count}");
            GenerarUI();
        }
    }

    void GenerarUI()
    {
        foreach (Transform child in contenedorLevels)
            Destroy(child.gameObject);

        foreach (LevelCustom nivel in niveles)
        {
            GameObject item = Instantiate(levelItemPrefab, contenedorLevels);
            TMP_Text nombreTexto = item.transform.Find("NombreText").GetComponent<TMP_Text>();
            Button btn = item.GetComponent<Button>();

            nombreTexto.text = nivel.name;

            LevelCustom nivelCopiado = nivel;
            btn.onClick.AddListener(() => AlSeleccionarNivel(nivelCopiado));
        }
    }

    void AlSeleccionarNivel(LevelCustom nivel)
    {
        PlayerPrefs.SetString("selected_assignment_level_id", nivel.level_id);
        PlayerPrefs.SetString("selected_game_type", nivel.game_type);
        PlayerPrefs.SetString("assignment_id", nivel.assignment_id);
        PlayerPrefs.SetInt("modo_asignacion", 1); // ✅ Marcar modo asignación activo

        string preguntasJson = JsonUtility.ToJson(new PreguntasWrapper { questions_ids = nivel.questions_ids });
        PlayerPrefs.SetString("selected_assignment_questions", preguntasJson);

        if (nivel.game_type == "GameJump")
            SceneManager.LoadScene("GameJump");
        else if (nivel.game_type == "QJ_1-1")
            SceneManager.LoadScene("QJ_1-1");
        else
            Debug.LogWarning("❗ Tipo de juego desconocido: " + nivel.game_type);
    }

    [System.Serializable]
    public class LevelCustom
    {
        public string level_id;
        public string game_type;
        public string[] questions_ids;
        public string name;
        public string description;
        public string assignment_id;
    }

    [System.Serializable]
    public class LevelResponseWrapper
    {
        public LevelCustom[] custom_levels;
    }

    [System.Serializable]
    public class PreguntasWrapper
    {
        public string[] questions_ids;
    }
}