using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instancia;

    void Awake()
    {

        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
    }
}
