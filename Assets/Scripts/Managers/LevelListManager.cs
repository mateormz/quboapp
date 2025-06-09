using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Data; // Usa tus clases LevelProgressWrapper y Entry

public class LevelListManager : MonoBehaviour
{
    public Transform contenedorLevels;
    public GameObject levelButtonPrefab;

    public string gameId = "a3d59a39-c738-450f-8f56-af0bd0ef4302";

    private int levelDesbloqueado = 0;

    void Start()
    {
        StartCoroutine(CargarNivelesDesbloqueados());
    }

    IEnumerator CargarNivelesDesbloqueados()
    {
        string userId = PlayerPrefs.GetString("user_id");
        Debug.Log("🔐 UserId usado: " + userId);
        string token = PlayerPrefs.GetString("token");
        Debug.Log("🔐 Token usado: " + token);


        string url = $"https://g6tzwkucx3.execute-api.us-east-1.amazonaws.com/dev/auth/users/get/{userId}";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener datos del usuario: " + www.error);
            yield break;
        }

        string rawJson = www.downloadHandler.text;

        // Buscar manualmente el bloque de levelProgress por su juego
        int start = rawJson.IndexOf(gameId);
        if (start != -1)
        {
            int colon = rawJson.IndexOf(':', start);
            int comma = rawJson.IndexOfAny(new[] { ',', '}' }, colon);
            string levelStr = rawJson.Substring(colon + 1, comma - colon - 1).Trim();
            int.TryParse(levelStr, out levelDesbloqueado);
        }

        StartCoroutine(CrearBotones());
    }

    IEnumerator CrearBotones()
    {
        string userId = PlayerPrefs.GetString("user_id");
        string token = PlayerPrefs.GetString("token");
        Debug.Log("🔐 Token usado: " + token);

        string url = "https://0mztjazn7i.execute-api.us-east-1.amazonaws.com/dev/games";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", token);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener juegos: " + req.error);
            yield break;
        }

        GameListWrapper wrapper = JsonUtility.FromJson<GameListWrapper>(req.downloadHandler.text);
        int totalLevels = 0;

        foreach (GameData game in wrapper.games)
        {
            if (game.game_id == gameId)
            {
                totalLevels = game.level_count;
                break;
            }
        }

        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, contenedorLevels);
            LevelButtonController controller = buttonObj.GetComponent<LevelButtonController>();

            if (controller != null)
            {
                bool desbloqueado = i <= levelDesbloqueado;
                Debug.Log($"Asignando nivel {i} al botón");
                controller.Configurar(i, desbloqueado);

                // 👉 Asegúrate de limpiar listeners anteriores por si acaso
                Button button = buttonObj.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(controller.AlHacerClick);
            }
        }


    }

    [System.Serializable]
    public class GameListWrapper
    {
        public List<GameData> games;
    }

    [System.Serializable]
    public class GameData
    {
        public string game_id;
        public string name;
        public int level_count;
    }
}
