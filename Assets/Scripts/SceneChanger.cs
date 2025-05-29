using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    public void IrATienda()
    {
        SceneManager.LoadScene("Shop");
    }

    public void IrAMenu()
    {
        SceneManager.LoadScene("Main");
    }
}