using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Problema
{
    public string enunciado;
    public string[] valoresNenufares;
}

[System.Serializable]
public class ProblemasList
{
    public Problema[] problemas;
}

public class LilyManager : MonoBehaviour
{
    public GameObject prefabLily;
    public TextMeshProUGUI textoEnunciado;

    private List<Problema> problemasDisponibles = new List<Problema>();
    private int preguntaActualIndex = 0;
    private List<GameObject> nenufaresInstanciados = new List<GameObject>();

    void Start()
    {
        CargarProblemas();
        GenerarSiguientePregunta();
    }

    void CargarProblemas()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("QuboJump/problems");
        if (jsonText != null)
        {
            ProblemasList data = JsonUtility.FromJson<ProblemasList>(WrapArray(jsonText.text));
            problemasDisponibles = new List<Problema>(data.problemas);
            Shuffle(problemasDisponibles);
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo JSON.");
        }
    }

    void GenerarSiguientePregunta()
    {
        // Limpia los nenúfares anteriores
        foreach (var lily in nenufaresInstanciados)
            Destroy(lily);
        nenufaresInstanciados.Clear();

        // Si ya no hay más preguntas
        if (preguntaActualIndex >= problemasDisponibles.Count)
        {
            textoEnunciado.text = "¡Felicidades, completaste todas las preguntas!";
            Debug.Log("Juego completado");
            return;
        }

        // Toma la siguiente pregunta
        Problema problemaActual = problemasDisponibles[preguntaActualIndex++];
        textoEnunciado.text = problemaActual.enunciado;

        // Prepara 1 correcta + 2 distractores
        List<string> opciones = new List<string> { problemaActual.valoresNenufares[0] };
        List<string> distractores = new List<string>(problemaActual.valoresNenufares);
        distractores.RemoveAt(0);
        Shuffle(distractores);
        // Toma hasta dos distractores
        for (int i = 0; i < Mathf.Min(2, distractores.Count); i++)
            opciones.Add(distractores[i]);
        Shuffle(opciones);

        // Cálculo de posiciones en X
        int totalNenufares = opciones.Count; // debería ser 3
        float posY = 0f;
        float leftX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x;
        float rightX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;
        float screenWidth = rightX - leftX;
        float lilyWidth = prefabLily.GetComponent<SpriteRenderer>().bounds.size.x;
        float gap = (screenWidth - totalNenufares * lilyWidth) / (totalNenufares + 1);

        // Instancia cada nenúfar
        for (int i = 0; i < totalNenufares; i++)
        {
            float posX = leftX + gap + lilyWidth * 0.5f + i * (lilyWidth + gap);
            Vector3 worldPos = new Vector3(posX, posY, 0f);

            GameObject lilyGO = Instantiate(prefabLily, worldPos, Quaternion.identity);
            Lily lilyScript = lilyGO.GetComponent<Lily>();

            // Asigna el TextMeshPro de la instancia
            lilyScript.textoValor = lilyGO.GetComponentInChildren<TextMeshProUGUI>();
            lilyScript.manager = this;

            // Asigna valor y marca correcto/incorrecto
            string valor = opciones[i];
            lilyScript.SetValor(valor);
            lilyScript.esCorrecto = (valor == problemaActual.valoresNenufares[0]);

            nenufaresInstanciados.Add(lilyGO);
        }
    }

    public void VerificarRespuesta(bool esCorrecta)
    {
        if (esCorrecta)
        {
            Debug.Log("¡Correcto!");
            GenerarSiguientePregunta();
        }
        else
        {
            Debug.Log("Incorrecto. Reiniciando...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    [System.Serializable]
    private class Wrapper
    {
        public Problema[] problemas;
    }

    private string WrapArray(string json)
    {
        return "{\"problemas\":" + json + "}";
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int rnd = Random.Range(i, list.Count);
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}