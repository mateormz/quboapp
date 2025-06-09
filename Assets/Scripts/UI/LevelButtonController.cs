using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButtonController : MonoBehaviour
{
    public TextMeshProUGUI levelNumberText;
    public GameObject lockIcon;

    [SerializeField] private int levelNumber;
    [SerializeField] private bool isUnlocked;

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

        SceneManager.LoadScene("GameJump");
    }

}
