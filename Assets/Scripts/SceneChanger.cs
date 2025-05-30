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
    
    public void IrAGames()
    {
        SceneManager.LoadScene("Games");
    }

    public void IrAJuego1()
    {
        SceneManager.LoadScene("GameJump");
    }

    public void IrAJuego2()
    {
        SceneManager.LoadScene("QJ_1-1");
        Time.timeScale = 1f;
    }
}
