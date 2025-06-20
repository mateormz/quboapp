using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Data;

public class UserDataLoader : MonoBehaviour
{
    private readonly List<string> gameIds = new List<string>
    {
        ApiConfig.GameIds.Qubo1,
        ApiConfig.GameIds.Qubo2
    };

    void Start()
    {
        StartCoroutine(ObtenerDatosUsuario());
    }

    IEnumerator ObtenerDatosUsuario()
    {
        string userId = PlayerPrefs.GetString("user_id");
        string token = PlayerPrefs.GetString("token");

        string url = ApiConfig.GetUserData(userId);
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error al obtener datos del usuario: " + www.error);
            yield break;
        }

        string rawJson = www.downloadHandler.text;
        Debug.Log("📥 JSON recibido: " + rawJson);

        foreach (string gameId in gameIds)
        {
            int nivelDesbloqueado = 0;

            int start = rawJson.IndexOf(gameId);
            if (start != -1)
            {
                int colon = rawJson.IndexOf(':', start);
                int comma = rawJson.IndexOfAny(new[] { ',', '}' }, colon);
                string levelStr = rawJson.Substring(colon + 1, comma - colon - 1).Trim();
                int.TryParse(levelStr, out nivelDesbloqueado);
            }

            Debug.Log($"🔓 Nivel desbloqueado para {gameId}: {nivelDesbloqueado}");
            PlayerPrefs.SetInt("nivel_desbloqueado_" + gameId, nivelDesbloqueado);
        }

        StartCoroutine(ObtenerCantidadDeNiveles());
    }

    IEnumerator ObtenerCantidadDeNiveles()
    {
        string token = PlayerPrefs.GetString("token");
        string url = ApiConfig.GET_ALL_GAMES;

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", token);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error al obtener juegos: " + req.error);
            yield break;
        }

        Debug.Log("📦 Respuesta de juegos: " + req.downloadHandler.text);

        GameListWrapper wrapper = JsonUtility.FromJson<GameListWrapper>(req.downloadHandler.text);
        foreach (GameData game in wrapper.games)
        {
            if (gameIds.Contains(game.game_id))
            {
                Debug.Log($"🎮 Juego: {game.name}, Niveles: {game.level_count}");
                PlayerPrefs.SetInt("total_levels_" + game.game_id, game.level_count);
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