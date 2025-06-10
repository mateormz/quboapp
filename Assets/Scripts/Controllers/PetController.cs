using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PetController : MonoBehaviour
{
    public SpriteRenderer petRenderer;

    void Start()
    {
        StartCoroutine(CargarSkinDesdeBackend());
    }

    IEnumerator CargarSkinDesdeBackend()
    {
        string user_id = PlayerPrefs.GetString("user_id");
        string token = PlayerPrefs.GetString("token");

        string selectedSkinId = null;

        // 1. Obtener la skin seleccionada del usuario
        using (UnityWebRequest userRequest = UnityWebRequest.Get(ApiConfig.GET_USER_SKINS_URL(user_id)))
        {
            userRequest.SetRequestHeader("Authorization", token);
            yield return userRequest.SendWebRequest();

            if (userRequest.result == UnityWebRequest.Result.Success)
            {
                PetUserSkinData userData = JsonUtility.FromJson<PetUserSkinData>(userRequest.downloadHandler.text);
                selectedSkinId = userData.skin_selected;
            }
            else
            {
                Debug.LogError("Error al obtener skin seleccionada: " + userRequest.downloadHandler.text);
                yield break;
            }
        }

        // 2. Obtener la lista de skins disponibles
        List<Skin> skinsDisponibles = new();
        using (UnityWebRequest skinsRequest = UnityWebRequest.Get(ApiConfig.GET_SKINS_URL))
        {
            skinsRequest.SetRequestHeader("Authorization", token);
            yield return skinsRequest.SendWebRequest();

            if (skinsRequest.result == UnityWebRequest.Result.Success)
            {
                PetSkinsResponse response = JsonUtility.FromJson<PetSkinsResponse>("{\"skins\":" + skinsRequest.downloadHandler.text + "}");
                skinsDisponibles = new List<Skin>(response.skins);

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
                Debug.LogError("Error al obtener lista de skins: " + skinsRequest.downloadHandler.text);
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
    public class PetUserSkinData
    {
        public string skin_selected;
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