using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PetShopController : MonoBehaviour
{
    public TMP_Text textoMonedas;
    public GameObject skinItemPrefab;
    public Transform contenedorSkins;

    private List<Skin> skinsDisponibles = new();
    private List<string> skinsDesbloqueadas = new();
    private string skinSeleccionada;

    void Start()
    {
        textoMonedas.text = "Monedas: " + CurrencyManager.Instance.GetMonedas();
        CurrencyManager.Instance.OnMonedasActualizadas += ActualizarTextoMonedas;

        StartCoroutine(CargarSkinsDesdeAPI());
    }

    IEnumerator CargarSkinsDesdeAPI()
    {
        string user_id = PlayerPrefs.GetString("user_id");
        string token = PlayerPrefs.GetString("token");

        // 1. Obtener lista de skins
        using (UnityWebRequest request = UnityWebRequest.Get(ApiConfig.GET_SKINS_URL))
        {
            request.SetRequestHeader("Authorization", token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                SkinsResponse response = JsonUtility.FromJson<SkinsResponse>("{\"skins\":" + request.downloadHandler.text + "}");
                skinsDisponibles = new List<Skin>(response.skins);
                skinsDisponibles.Sort((a, b) => ExtraerNumero(a.skin_id).CompareTo(ExtraerNumero(b.skin_id)));
            }
            else
            {
                Debug.LogError("Error obteniendo skins: " + request.downloadHandler.text);
                yield break;
            }
        }

        // 2. Obtener skin seleccionada y skins desbloqueadas
        using (UnityWebRequest userRequest = UnityWebRequest.Get(ApiConfig.GET_USER_SKINS_URL(user_id)))
        {
            userRequest.SetRequestHeader("Authorization", token);
            yield return userRequest.SendWebRequest();

            if (userRequest.result == UnityWebRequest.Result.Success)
            {
                UserSkinsResponse userData = JsonUtility.FromJson<UserSkinsResponse>(userRequest.downloadHandler.text);
                skinsDesbloqueadas = new List<string>(userData.skins_unlocked);
                skinSeleccionada = userData.skin_selected;
            }
            else
            {
                Debug.LogError("Error obteniendo skins del usuario: " + userRequest.downloadHandler.text);
                yield break;
            }
        }

        GenerarUI();
    }

    void GenerarUI()
    {
        foreach (Transform child in contenedorSkins)
            Destroy(child.gameObject);

        foreach (Skin skin in skinsDisponibles)
        {
            GameObject item = Instantiate(skinItemPrefab, contenedorSkins);
            TMP_Text priceText = item.transform.Find("PriceText").GetComponent<TMP_Text>();
            Button btn = item.GetComponent<Button>();
            Image img = item.transform.Find("SkinImage").GetComponent<Image>();

            StartCoroutine(CargarImagenDesdeURL(skin.image_url, img));

            bool desbloqueada = skinsDesbloqueadas.Contains(skin.skin_id);

            if (desbloqueada)
            {
                if (skin.skin_id == skinSeleccionada)
                {
                    priceText.text = "Equipado";
                    btn.interactable = false;
                }
                else
                {
                    priceText.text = "Equipar";
                    btn.interactable = true;
                }
            }
            else
            {
                priceText.text = skin.price + " qu";
                btn.interactable = CurrencyManager.Instance.GetMonedas() >= skin.price;
            }

            string idSkin = skin.skin_id;
            btn.onClick.AddListener(() => ComprarSkin(idSkin, skin.price, desbloqueada));
        }
    }

    void ComprarSkin(string skinId, int precio, bool yaDesbloqueada)
    {
        string user_id = PlayerPrefs.GetString("user_id");
        string token = PlayerPrefs.GetString("token");

        if (yaDesbloqueada)
        {
            StartCoroutine(SeleccionarSkin(skinId, user_id, token));
        }
        else if (CurrencyManager.Instance.GetMonedas() >= precio)
        {
            StartCoroutine(DesbloquearSkin(skinId, precio, user_id, token));
        }
    }

    IEnumerator SeleccionarSkin(string skinId, string user_id, string token)
    {
        string url = ApiConfig.UPDATE_SKIN_SELECTED(user_id);
        var data = JsonUtility.ToJson(new SkinSeleccionada { skin_selected = skinId });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Skin seleccionada: " + skinId);
                skinSeleccionada = skinId;
                PlayerPrefs.SetString("skinSeleccionada", skinId);
                GenerarUI();
            }
            else
            {
                Debug.LogError("Error al seleccionar skin: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator DesbloquearSkin(string skinId, int precio, string user_id, string token)
    {
        string url = ApiConfig.UNLOCK_SKIN(user_id);
        var data = JsonUtility.ToJson(new SkinDesbloqueo { skin_id = skinId });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(data);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                CurrencyManager.Instance.RestarMonedas(precio);
                skinsDesbloqueadas.Add(skinId);
                Debug.Log("Skin desbloqueada: " + skinId);
                GenerarUI();
            }
            else
            {
                Debug.LogError("Error al desbloquear skin: " + request.downloadHandler.text);
            }
        }
    }

    IEnumerator CargarImagenDesdeURL(string url, Image img)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    void ActualizarTextoMonedas(int nuevaCantidad)
    {
        textoMonedas.text = "Monedas: " + nuevaCantidad;
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMonedasActualizadas -= ActualizarTextoMonedas;
    }

    [System.Serializable] public class Skin
    {
        public string skin_id;
        public string name;
        public int price;
        public string image_url;
    }

    [System.Serializable] public class SkinsResponse
    {
        public Skin[] skins;
    }

    [System.Serializable] public class UserSkinsResponse
    {
        public string skin_selected;
        public string[] skins_unlocked;
        public int qu_coin;
    }

    [System.Serializable] public class SkinSeleccionada
    {
        public string skin_selected;
    }

    [System.Serializable] public class SkinDesbloqueo
    {
        public string skin_id;
    }

    int ExtraerNumero(string id)
    {
        string numeroStr = System.Text.RegularExpressions.Regex.Match(id, @"\d+").Value;
        return int.TryParse(numeroStr, out int numero) ? numero : int.MaxValue;
    }
}