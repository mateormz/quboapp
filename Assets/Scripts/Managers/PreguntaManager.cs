using UnityEngine;
using TMPro;

public class PreguntaManager : MonoBehaviour
{
    public TextMeshProUGUI textoPregunta;
    public AlternativaController[] zonasAlternativas;

    private PreguntaData[] preguntas;
    private int indiceActual = 0;

    void Start()
    {
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
        }
        else
        {
            Debug.LogError("No se encontró el archivo preguntas.json en Resources");
            preguntas = new PreguntaData[0];
        }
    }

    public void MostrarPreguntaActual()
    {
        if (indiceActual >= preguntas.Length)
        {
            textoPregunta.text = "¡Completaste todas las preguntas!";
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
        }
    }
}