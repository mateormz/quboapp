using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelButtonController : MonoBehaviour
{
    public TextMeshProUGUI levelNumberText;
    public GameObject lockIcon;

    [SerializeField] private int levelNumber;
    [SerializeField] private bool isUnlocked;

    private string gameId;

    public void Configurar(int level, bool unlocked)
    {
        levelNumber = level;
        isUnlocked = unlocked;

        levelNumberText.text = "Nivel " + level.ToString();
        lockIcon.SetActive(!unlocked);
        GetComponent<Button>().interactable = unlocked;

        Debug.Log($"[Configurar] Nivel asignado: {levelNumber}, desbloqueado: {unlocked}");
    }

    public void AlHacerClick()
    {
        Debug.Log("Se hizo click correctamente");
        Debug.Log($"Valor de isUnlocked: {isUnlocked}");
        Debug.Log($"Nivel actual: {levelNumber}");

        if (!isUnlocked)
        {
            Debug.Log("Este botón está bloqueado, no hace nada.");
            return;
        }

        PlayerPrefs.SetInt("nivel_seleccionado", levelNumber);
        Debug.Log($"🔢 Nivel seleccionado para jugar: {levelNumber}");

        gameId = PlayerPrefs.GetString("selected_game_id", ApiConfig.GameIds.Qubo1);

        if (gameId == ApiConfig.GameIds.Qubo1)
        {
            SceneManager.LoadScene("GameJump");
        }
        else if (gameId == ApiConfig.GameIds.Qubo2)
        {
            StartCoroutine(CargarJuegoConRetraso());
        }
        else
        {
            Debug.LogWarning("❗ gameId desconocido, no se puede cargar escena");
        }
    }

    IEnumerator CargarJuegoConRetraso()
    {
        yield return new WaitForSecondsRealtime(0.1f); // Esto ignora Time.timeScale
        Time.timeScale = 1f;
        SceneManager.LoadScene("QJ_1-1");
    }
}
