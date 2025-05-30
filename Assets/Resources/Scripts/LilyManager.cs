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
    [Header("Prefabs & UI")] public GameObject prefabLily;
    public TextMeshProUGUI textoEnunciado;
    public PlayerMovement playerMovement;

    [Header("Layout")] public float alturaBase = 0f; // Altura de la primera fila
    public float espacioVertical = 2f; // Espacio entre filas

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
            var data = JsonUtility.FromJson<ProblemasList>(WrapArray(jsonText.text));
            problemasDisponibles = new List<Problema>(data.problemas);
            Shuffle(problemasDisponibles);
        }
        else
        {
            Debug.LogError("No se pudo cargar el JSON de preguntas.");
        }
    }

    void GenerarSiguientePregunta()
    {
        // Si ya no hay más preguntas, mostramos final
        if (preguntaActualIndex >= problemasDisponibles.Count)
        {
            textoEnunciado.text = "¡Has completado todas las preguntas!";
            return;
        }

        // Tomamos la siguiente pregunta
        var prob = problemasDisponibles[preguntaActualIndex++];
        textoEnunciado.text = prob.enunciado;

        // Construimos las opciones: 1 correcta + 2 distractores
        var opciones = new List<string> { prob.valoresNenufares[0] };
        var distract = new List<string>(prob.valoresNenufares);
        distract.RemoveAt(0);
        Shuffle(distract);
        for (int i = 0; i < Mathf.Min(2, distract.Count); i++)
            opciones.Add(distract[i]);
        Shuffle(opciones);

        // Cálculo de posiciones X para tantas lilys como opciones (aquí 3)
        int total = opciones.Count;
        float leftX = Camera.main.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = Camera.main.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float screenWidth = rightX - leftX;
        float lilyW = prefabLily.GetComponent<SpriteRenderer>().bounds.size.x;
        float gap = (screenWidth - total * lilyW) / (total + 1);

        // Altura de esta fila:
        float posY = alturaBase + (preguntaActualIndex - 1) * espacioVertical;

        // Instanciamos cada nenúfar
        for (int i = 0; i < total; i++)
        {
            float posX = leftX + gap + lilyW * .5f + i * (lilyW + gap);
            var go = Instantiate(prefabLily, new Vector3(posX, posY, 0f), Quaternion.identity);
            var script = go.GetComponent<Lily>();

            // Configuramos el Lily
            script.manager = this;
            script.textoValor = go.GetComponentInChildren<TextMeshProUGUI>();
            script.SetValor(opciones[i]);
            script.esCorrecto = (opciones[i] == prob.valoresNenufares[0]);

            nenufaresInstanciados.Add(go);
        }
    }

    /// <summary>
    /// Llamado por Lily cuando se hace click.
    /// </summary>
    public void OnLilyClicked(Lily lily)
    {
        // Desactivamos todos los colliders para evitar clicks múltiples
        foreach (var l in nenufaresInstanciados)
            l.GetComponent<Collider2D>().enabled = false;

        // Saltamos a la posición del nenúfar clickeado
        playerMovement.MoveTo(
            lily.transform.position,
            () =>
            {
                if (lily.esCorrecto)
                {
                    // Destruir solo los nenúfares incorrectos
                    foreach (var l in nenufaresInstanciados)
                    {
                        if (l != lily.gameObject)
                            Destroy(l);
                    }

                    // Limpiamos la lista y la reemplazamos con el nenúfar donde estamos
                    nenufaresInstanciados.Clear();
                    nenufaresInstanciados.Add(lily.gameObject);

                    // Generamos la siguiente fila de preguntas
                    GenerarSiguientePregunta();
                }
                else
                {
                    // En caso de error, destuir este nenúfar y reiniciar
                    Destroy(lily.gameObject);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        );
    }

    // ----------------- Helpers -----------------

    [System.Serializable]
    private class Wrapper
    {
        public Problema[] problemas;
    }

    private string WrapArray(string j) => "{\"problemas\":" + j + "}";

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var tmp = list[i];
            int r = Random.Range(i, list.Count);
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}