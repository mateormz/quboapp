using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelector : MonoBehaviour
{
    [Header("ID del juego (de ApiConfig.GameIds)")]
    public string gameId;

    [Header("Escena de niveles a cargar")]
    public string escenaDeNiveles;

    public void SeleccionarJuego()
    {
        PlayerPrefs.SetString("selected_game_id", gameId);
        SceneManager.LoadScene(escenaDeNiveles);
    }
}
