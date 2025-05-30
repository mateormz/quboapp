using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Problema
{
    public string enunciado;
    public string[] valoresNenufares;
}

public class LilyManager : MonoBehaviour
{
    public GameObject prefabLily; // Prefab del nenúfar Lily

    private Problema
        problemaActual; // Aquí cargaremos el problema (puedes mantener la carga JSON o asignarlo manualmente)

    private List<GameObject> nenufaresInstanciados = new List<GameObject>();

    void Start()
    {
        CargarProblema(); // Carga o asigna problemaActual
        CrearNenufares(); // Instancia los 3 nenúfares con orden aleatorio
    }

    void CargarProblema()
    {
        // Por simplicidad solo asignamos directamente el primer problema:
        problemaActual = new Problema
        {
            enunciado = "Selecciona el nenúfar equivalente a 1/2",
            valoresNenufares = new string[] { "0.5", "0.4", "0.25", "0.75" }
        };
    }

    void CrearNenufares()
    {
        int totalNenufares = 3;
        float posY = 0f;

        // 1) Calcular límites en X
        float leftX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x;
        float rightX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;
        float screenWidth = rightX - leftX;

        // 2) Ancho de un nenúfar
        float lilyWidth = prefabLily.GetComponent<SpriteRenderer>().bounds.size.x;

        // 3) Espacio (gap) total repartido en (n+1) ranuras
        float gap = (screenWidth - totalNenufares * lilyWidth) / (totalNenufares + 1);

        // 4) Preparamos la lista de opciones: el correcto + 2 distractores
        string correcto = problemaActual.valoresNenufares[0];
        List<string> distractores = new List<string>(problemaActual.valoresNenufares);
        distractores.RemoveAt(0); // quitamos el correcto
        // mezclamos y tomamos los primeros 2 distractores
        Shuffle(distractores);
        List<string> opciones = new List<string> { correcto, distractores[0], distractores[1] };
        // mezclamos las 3 opciones
        Shuffle(opciones);

        // 5) Instanciamos cada nenúfar en su posición
        for (int i = 0; i < totalNenufares; i++)
        {
            float posX = leftX + gap + lilyWidth * 0.5f + i * (lilyWidth + gap);
            Vector3 posicionMundo = new Vector3(posX, posY, 0f);

            GameObject lilyGO = Instantiate(prefabLily, posicionMundo, Quaternion.identity);
            Lily lilyScript = lilyGO.GetComponent<Lily>();

            // asignamos el TextMeshProUGUI de la instancia
            TextMeshProUGUI texto = lilyGO.GetComponentInChildren<TextMeshProUGUI>();
            lilyScript.textoValor = texto;

            // asignamos valor y si es correcto
            string valor = opciones[i];
            lilyScript.SetValor(valor);
            lilyScript.esCorrecto = (valor == correcto);
            lilyScript.manager = this;

            nenufaresInstanciados.Add(lilyGO);
        }
    }

    // método que llama Lily cuando se cliquea un nenúfar
    public void VerificarRespuesta(bool respuestaCorrecta)
    {
        if (respuestaCorrecta)
            Debug.Log("Respuesta correcta!");
        else
            Debug.Log("Respuesta incorrecta");
    }

    // Fisher–Yates shuffle
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[rnd];
            list[rnd] = tmp;
        }
    }
}