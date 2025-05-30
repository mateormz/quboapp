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
    [Header("Prefabs & UI")]
    public GameObject prefabLily;
    public GameObject prefabAgua;
    public GameObject prefabCesped;
    public GameObject metaPrefab;
    public GameObject playerPrefab;
    public TextMeshProUGUI textoEnunciado;

    [Header("Layout")]
    public int filasCount = 5;          // Cuántas filas (preguntas) generar
    public float alturaBase = 0f;       // Y de la primera fila
    public float espacioVertical = 2f;  // Distancia vertical entre filas

    private List<Problema> problemasDisponibles = new List<Problema>();
    private List<List<Lily>> filasLily = new List<List<Lily>>();
    private int filaActual = 0;
    private int generatedRowsCount = 0;
    private PlayerMovement playerMovement;

    void Start()
    {
        CargarProblemas();
        GenerarMapaCompleto(filasCount);
        GenerarEntorno();
        SpawnPlayerEnInicio();
        MostrarEnunciado(0);
        ActivarFila(0);
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
        else Debug.LogError("No se pudo cargar el JSON de problemas.");
    }

    void GenerarMapaCompleto(int filasCount)
    {
        int max = Mathf.Min(filasCount, problemasDisponibles.Count);
        generatedRowsCount = max;

        // horizontales
        float leftX = Camera.main.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = Camera.main.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float screenWidth = rightX - leftX;
        float lilyW = prefabLily.GetComponent<SpriteRenderer>().bounds.size.x;

        for (int f = 0; f < max; f++)
        {
            float posY = alturaBase + f * espacioVertical;
            Problema prob = problemasDisponibles[f];

            // slots siempre 3 (correcto + 2 distractores)
            int totalSlots = Mathf.Min(3, prob.valoresNenufares.Length);
            float gap = (screenWidth - totalSlots * lilyW) / (totalSlots + 1);

            var fila = new List<Lily>();
            for (int i = 0; i < totalSlots; i++)
            {
                float posX = leftX + gap + lilyW * .5f + i * (lilyW + gap);
                GameObject go = Instantiate(prefabLily, new Vector3(posX, posY, 0f), Quaternion.identity);
                Lily script = go.GetComponent<Lily>();
                script.manager = this;
                script.textoValor = go.GetComponentInChildren<TextMeshProUGUI>();

                // Inicialmente vacío
                script.SetValor("");
                script.esCorrecto = false;

                fila.Add(script);
            }

            filasLily.Add(fila);
        }
    }

    void GenerarEntorno()
    {
        var cam = Camera.main;
        // 1) Cálculo de bordes
        float leftX = cam.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = cam.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float centerX = (leftX + rightX) / 2f;
        float width = rightX - leftX;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;

        // 2) Césped de inicio: de bottomY hasta Y = 1.5f
        float grassHeight = 1.5f;
        GameObject grassStart = Instantiate(
            prefabCesped,
            new Vector3(centerX, bottomY + grassHeight / 2f, 0f),
            Quaternion.identity
        );
        var srGrass1 = grassStart.GetComponent<SpriteRenderer>();
        srGrass1.drawMode = SpriteDrawMode.Tiled;
        srGrass1.size = new Vector2(width, grassHeight);
        grassStart.transform.localScale = Vector3.one;

        // 3) Agua inferior: desde top de césped hasta 0.5f antes de la primera fila
        float firstLilyY = alturaBase;
        float waterTop = firstLilyY - 0.5f;
        float waterBottom = bottomY + grassHeight;
        float waterH = Mathf.Max(0, waterTop - waterBottom);
        GameObject aguaInf = Instantiate(
            prefabAgua,
            new Vector3(centerX, waterBottom + waterH / 2f, 1f),
            Quaternion.identity
        );
        var srWaterInf = aguaInf.GetComponent<SpriteRenderer>();
        srWaterInf.drawMode = SpriteDrawMode.Tiled;
        srWaterInf.size = new Vector2(width, waterH);
        aguaInf.transform.localScale = Vector3.one;

        // 4) Agua superior: franja de altura espacioVertical justo encima de la última fila
        float lastLilyY = alturaBase + (generatedRowsCount - 1) * espacioVertical;
        float water2Bottom = lastLilyY + 0.5f;
        float water2H = espacioVertical;
        GameObject aguaSup = Instantiate(
            prefabAgua,
            new Vector3(centerX, water2Bottom + water2H / 2f, 1f),
            Quaternion.identity
        );
        var srWaterSup = aguaSup.GetComponent<SpriteRenderer>();
        srWaterSup.drawMode = SpriteDrawMode.Tiled;
        srWaterSup.size = new Vector2(width, water2H);
        aguaSup.transform.localScale = Vector3.one;

        // 5) Césped/meta final: de water2Top hasta +1.5f
        float grassEndBottom = water2Bottom + water2H;
        GameObject grassEnd = Instantiate(
            prefabCesped,
            new Vector3(centerX, grassEndBottom + grassHeight / 2f, 0f),
            Quaternion.identity
        );
        var srGrass2 = grassEnd.GetComponent<SpriteRenderer>();
        srGrass2.drawMode = SpriteDrawMode.Tiled;
        srGrass2.size = new Vector2(width, grassHeight);
        grassEnd.transform.localScale = Vector3.one;

        // 6) Meta justo encima del césped final
        GameObject meta = Instantiate(
            metaPrefab,
            new Vector3(centerX, grassEndBottom + grassHeight + 0.5f, 0f),
            Quaternion.identity
        );
        meta.transform.localScale = Vector3.one * 0.1f;
    }

    void SpawnPlayerEnInicio()
    {
        var cam = Camera.main;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float spawnY = bottomY + (1.5f / 2f);
        GameObject playerGO = Instantiate(playerPrefab, new Vector3(0f, spawnY, 0f), Quaternion.identity);
        playerGO.transform.localScale = Vector3.one * 0.2f;
        playerMovement = playerGO.GetComponent<PlayerMovement>();

        var camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
            camFollow.target = playerGO.transform;
    }



    void MostrarEnunciado(int f)
    {
        if (f < problemasDisponibles.Count)
            textoEnunciado.text = problemasDisponibles[f].enunciado;
    }

    void ActivarFila(int f)
    {
        if (f < 0 || f >= filasLily.Count) return;
        Problema prob = problemasDisponibles[f];

        // Preparamos opciones
        var opciones = new List<string> { prob.valoresNenufares[0] };
        var distract = new List<string>(prob.valoresNenufares);
        distract.RemoveAt(0);
        Shuffle(distract);
        for (int i = 0; i < Mathf.Min(2, distract.Count); i++)
            opciones.Add(distract[i]);
        Shuffle(opciones);

        // Asignamos a la fila
        var fila = filasLily[f];
        for (int i = 0; i < fila.Count; i++)
        {
            fila[i].SetValor(opciones[i]);
            fila[i].esCorrecto = opciones[i] == prob.valoresNenufares[0];
        }
    }

    public void OnLilyClicked(Lily lily)
    {
        // Desactivar colliders
        foreach (var fila in filasLily)
            foreach (var l in fila)
                l.GetComponent<Collider2D>().enabled = false;

        playerMovement.MoveTo(lily.transform.position, () =>
        {
            if (lily.esCorrecto)
            {
                // eliminamos los siblings incorrectos
                var current = filasLily[filaActual];
                foreach (var l in current)
                    if (l != lily) Destroy(l.gameObject);
                filasLily[filaActual] = new List<Lily> { lily };

                filaActual = Mathf.Min(filaActual + 1, filasLily.Count - 1);
                MostrarEnunciado(filaActual);
                ActivarFila(filaActual);
            }
            else
            {
                Destroy(lily.gameObject);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        });
    }

    // Helpers
    private void ScaleToWidth(GameObject go, float width)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        float w = sr.bounds.size.x;
        go.transform.localScale = new Vector3(width / w, go.transform.localScale.y, 1);
    }

    private void ScaleToArea(GameObject go, float width, float height)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        float w = sr.bounds.size.x;
        float h = sr.bounds.size.y;
        go.transform.localScale = new Vector3(width / w, height / h, 1);
    }

    [System.Serializable]
    private class Wrapper { public Problema[] problemas; }
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
