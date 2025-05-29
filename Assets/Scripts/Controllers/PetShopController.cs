using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetShopController : MonoBehaviour
{
    public SkinsData skinsData;
    public TMP_Text textoMonedas;
    public GameObject skinItemPrefab;    // Prefab para cada skin en la tienda
    public Transform contenedorSkins;    // Contenedor (Content) del Scroll View

    void Start()
    {
        textoMonedas.text = "Monedas: " + CurrencyManager.Instance.GetMonedas();
        CurrencyManager.Instance.OnMonedasActualizadas += ActualizarTextoMonedas;

        // Limpiar skins antiguos si es necesario
        foreach (Transform child in contenedorSkins)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < skinsData.skinsCompletas.Length; i++)
        {
            GameObject item = Instantiate(skinItemPrefab, contenedorSkins);
            Image img = item.transform.Find("SkinImage").GetComponent<Image>();
            TMP_Text priceText = item.transform.Find("PriceText").GetComponent<TMP_Text>();
            Button btn = item.GetComponent<Button>();

            img.sprite = skinsData.skinsCompletas[i];
            priceText.text = skinsData.preciosSkins[i] + " qu";

            int capturedIndex = i;
            btn.onClick.AddListener(() => ComprarSkin(capturedIndex));

            bool desbloqueada = PlayerPrefs.GetInt("skin_" + i, i == 0 ? 1 : 0) == 1;
            btn.interactable = desbloqueada || CurrencyManager.Instance.GetMonedas() >= skinsData.preciosSkins[i];
        }
    }

    public void ComprarSkin(int index)
    {
        bool desbloqueada = PlayerPrefs.GetInt("skin_" + index, index == 0 ? 1 : 0) == 1;

        if (desbloqueada)
        {
            PlayerPrefs.SetInt("skinSeleccionada", index);
            return;
        }

        int precio = skinsData.preciosSkins[index];
        if (CurrencyManager.Instance.GetMonedas() >= precio)
        {
            CurrencyManager.Instance.RestarMonedas(precio);
            PlayerPrefs.SetInt("skin_" + index, 1);
            PlayerPrefs.SetInt("skinSeleccionada", index);
            VerificarBotonesPorMonedas(CurrencyManager.Instance.GetMonedas());
        }
    }

    void ActualizarTextoMonedas(int nuevaCantidad)
    {
        textoMonedas.text = "Monedas: " + nuevaCantidad;
        VerificarBotonesPorMonedas(nuevaCantidad);
    }

    void VerificarBotonesPorMonedas(int monedasActuales)
    {
        // Ahora verifica los botones dentro del contenedor dinámico
        foreach (Transform child in contenedorSkins)
        {
            Button btn = child.GetComponent<Button>();
            int index = child.GetSiblingIndex();
            bool desbloqueada = PlayerPrefs.GetInt("skin_" + index, index == 0 ? 1 : 0) == 1;
            btn.interactable = desbloqueada || monedasActuales >= skinsData.preciosSkins[index];
        }
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMonedasActualizadas -= ActualizarTextoMonedas;
    }
}
