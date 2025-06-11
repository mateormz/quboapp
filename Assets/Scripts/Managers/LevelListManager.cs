using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelListManager : MonoBehaviour
{
    public Transform contenedorLevels;
    public GameObject levelButtonPrefab;

    private string gameId;
    private int levelDesbloqueado = 0;


    void Start()
    {
        Debug.Log("Iniciando...");
        gameId = PlayerPrefs.GetString("selected_game_id", ApiConfig.GameIds.Qubo1); // Por defecto Qubo1
        Debug.Log("gameId recuperado: " + gameId);
        
        levelDesbloqueado = PlayerPrefs.GetInt("nivel_desbloqueado_" + gameId, 0);
        Debug.Log("Nivel desbloqueado..." + levelDesbloqueado);

        int totalLevels = PlayerPrefs.GetInt("total_levels_" + gameId, 0);
        Debug.Log("Total niveles..." + totalLevels);

        StartCoroutine(CrearBotones(totalLevels));
    }

    IEnumerator CrearBotones(int totalLevels)
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, contenedorLevels);
            LevelButtonController controller = buttonObj.GetComponent<LevelButtonController>();

            if (controller != null)
            {
                bool desbloqueado = i <= levelDesbloqueado;
                Debug.Log($"🎮 Creando botón para nivel {i} - {(desbloqueado ? "desbloqueado" : "bloqueado")}");
                controller.Configurar(i, desbloqueado);

                Button button = buttonObj.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(controller.AlHacerClick);
            }
        }

        yield return null;
    }
}
