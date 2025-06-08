using TMPro;
using UnityEngine;

public class UIRacha : MonoBehaviour
{
    public TextMeshProUGUI textoRacha;

    void Start()
    {
        int racha = PlayerPrefs.GetInt("currentStreak", 1);
        textoRacha.text = "Racha: " + racha + " días";
    }
}