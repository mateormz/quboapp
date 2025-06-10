using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PetController : MonoBehaviour
{
    public SpriteRenderer petRenderer;

    void Start()
    {
        StartCoroutine(CargarSkinSeleccionada());
        Debug.Log("Monedas: " + PlayerPrefs.GetInt("monedas"));
        Debug.Log("Skin: " + PlayerPrefs.GetString("skinSeleccionada"));
    }

    IEnumerator CargarSkinSeleccionada()
    {
        string selectedSkinId = PlayerPrefs.GetString("skinSeleccionada", "skin1");
        string token = PlayerPrefs.GetString("token");

        // Obtener lista de skins disponibles
        List<Skin> skinsDisponibles = new();
        using (UnityWebRequest skinsRequest = UnityWebRequest.Get(ApiConfig.GET_SKINS_URL))
        {
            skinsRequest.SetRequestHeader("Authorization", token);
            yield return skinsRequest.SendWebRequest();

            if (skinsRequest.result == UnityWebRequest.Result.Success)
            {
                PetSkinsResponse response = JsonUtility.FromJson<PetSkinsResponse>("{\"skins\":" + skinsRequest.downloadHandler.text + "}");
                skinsDisponibles = new List<Skin>(response.skins);

                // Buscar la skin seleccionada
                Skin skinSeleccionada = skinsDisponibles.Find(s => s.skin_id == selectedSkinId);

                if (skinSeleccionada != null)
                {
                    StartCoroutine(CargarImagenDesdeURL(skinSeleccionada.image_url));
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró la skin seleccionada. Usando la primera disponible.");
                    StartCoroutine(CargarImagenDesdeURL(skinsDisponibles[0].image_url));
                }
            }
            else
            {
                Debug.LogError("❌ Error al obtener lista de skins: " + skinsRequest.downloadHandler.text);
            }
        }
    }

    IEnumerator CargarImagenDesdeURL(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                petRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("❌ Error al cargar imagen de la skin: " + request.downloadHandler.text);
            }
        }
    }

    [System.Serializable]
    public class PetSkinsResponse
    {
        public Skin[] skins;
    }

    [System.Serializable]
    public class Skin
    {
        public string skin_id;
        public string name;
        public int price;
        public string image_url;
    }
}