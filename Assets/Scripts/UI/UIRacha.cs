using TMPro;
using UnityEngine;

public class UIRacha : MonoBehaviour
{
    public TextMeshProUGUI textoRacha;

    void Start()
    {
        int racha = PlayerPrefs.GetInt("streak", 1);
        textoRacha.text = "Racha: " + racha + " días";
    }
}