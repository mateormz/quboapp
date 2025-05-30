using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PreguntaManager : MonoBehaviour
{
    public TextMeshProUGUI textoPregunta;
    public AlternativaController[] zonasAlternativas;
    public GameObject panelPerdiste; // Asignar desde el editor

    private PreguntaData[] preguntas;
    private int indiceActual = 0;

    void Start()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        CargarPreguntasDesdeJSON();
        MostrarPreguntaActual();
    }

    void CargarPreguntasDesdeJSON()
    {
        TextAsset json = Resources.Load<TextAsset>("preguntas"); // sin extensión
        if (json != null)
        {
            PreguntasWrapper wrapper = JsonUtility.FromJson<PreguntasWrapper>(json.text);
            preguntas = wrapper.preguntas;

            // Mezclar las preguntas al azar
            ShuffleArray(preguntas);
        }
        else
        {
            Debug.LogError("No se encontró el archivo preguntas.json en Resources");
            preguntas = new PreguntaData[0];
        }
    }

    void ShuffleArray(PreguntaData[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            PreguntaData temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public void MostrarPreguntaActual()
    {
        if (indiceActual >= preguntas.Length)
        {
            textoPregunta.text = "¡Completaste todas las preguntas!";
            StartCoroutine(FinalizarJuego());
            return;
        }

        PreguntaData p = preguntas[indiceActual];
        textoPregunta.text = p.pregunta;

        for (int i = 0; i < zonasAlternativas.Length; i++)
        {
            zonasAlternativas[i].Configurar(p.alternativas[i], i == p.indiceCorrecta, this);
        }
    }

    public void Responder(bool correcta)
    {
        if (correcta)
        {
            CurrencyManager.Instance.SumarMonedas(5);
            indiceActual++;
            MostrarPreguntaActual();
        }
        else
        {
            Debug.Log("Incorrecta");
            Perder();
        }
    }

    void Perder()
    {
        Time.timeScale = 0f; // Detiene el movimiento de todo
        panelPerdiste.SetActive(true); // Muestra el panel
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f; // Reanudar tiempo antes de cambiar de escena
        SceneManager.LoadScene("Main"); // Cambia a tu menú principal
    }

    IEnumerator FinalizarJuego()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Main");
    }
}