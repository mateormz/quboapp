using System.Collections;
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
        StartCoroutine(CargarJuegoConRetraso());
    }

    public void IrALevelsGame1()
    {
        SceneManager.LoadScene("LevelsGame1");
    }

    public void IrAFeedback()
    {
        SceneManager.LoadScene("FeedbackScene");
    }

    public void IrAAssignments()
    {
        SceneManager.LoadScene("Assignments");
    }

    IEnumerator CargarJuegoConRetraso()
    {
        yield return new WaitForSecondsRealtime(0.1f); // <- Esto ignora Time.timeScale
        Time.timeScale = 1f;
        SceneManager.LoadScene("QJ_1-1");
    }


}
