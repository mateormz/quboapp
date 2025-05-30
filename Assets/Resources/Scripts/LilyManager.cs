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
    public GameObject playerPrefab;
    public TextMeshProUGUI textoEnunciado;

    [Header("Layout")]
    public int filasCount = 5;       // Cuántas filas generar
    public float espacioVertical = 5f; // Separación vertical entre filas

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

    /// <summary>
    /// Genera todas las filas de nenúfares comenzando siempre 2f sobre el fondo.
    /// </summary>
    void GenerarMapaCompleto(int filasCount)
    {
        int max = Mathf.Min(filasCount, problemasDisponibles.Count);
        generatedRowsCount = max;

        // límites horizontales
        var cam = Camera.main;
        float leftX = cam.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = cam.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float screenWidth = rightX - leftX;
        float lilyW = prefabLily.GetComponent<SpriteRenderer>().bounds.size.x;

        // borde inferior del mundo
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        // altura de la primera fila de nenúfares: bottomY + 2f
        float firstLilyY = bottomY + 2.5f;

        for (int f = 0; f < max; f++)
        {
            float posY = firstLilyY + f * espacioVertical;
            Problema prob = problemasDisponibles[f];

            // slots = 3 (1 correcta + 2 distractores)
            int totalSlots = Mathf.Min(3, prob.valoresNenufares.Length);
            float gap = (screenWidth - totalSlots * lilyW) / (totalSlots + 1);

            var fila = new List<Lily>();
            for (int i = 0; i < totalSlots; i++)
            {
                float posX = leftX + gap + lilyW * .5f + i * (lilyW + gap);
                GameObject go = Instantiate(prefabLily, new Vector3(posX, posY, 0f), Quaternion.identity);
                var script = go.GetComponent<Lily>();
                script.manager = this;
                script.textoValor = go.GetComponentInChildren<TextMeshProUGUI>();

                // inicialmente vacío
                script.SetValor("");
                script.esCorrecto = false;

                fila.Add(script);
            }
            filasLily.Add(fila);
        }
    }

    /// <summary>
    /// Genera el césped inicial, el agua y el césped final según el mapa.
    /// </summary>
    void GenerarEntorno()
    {
        var cam = Camera.main;
        float leftX = cam.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = cam.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float centerX = (leftX + rightX) / 2f;
        float width = rightX - leftX;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;

        const float grassH = 1.5f;
        const float scale = 0.1f;

        // 1) Césped inicial [bottomY .. bottomY+1.5]
        var grassStart = Instantiate(prefabCesped,
            new Vector3(centerX, bottomY + grassH / 2f, 0f),
            Quaternion.identity);
        var srG1 = grassStart.GetComponent<SpriteRenderer>();
        srG1.drawMode = SpriteDrawMode.Tiled;
        srG1.size = new Vector2(width / scale, grassH / scale);
        grassStart.transform.localScale = Vector3.one * scale;

        // 2) Agua [bottomY+1.5 .. lastLilyY+0.5]
        float firstLilyY = bottomY + 2f;
        float lastLilyY = firstLilyY + (generatedRowsCount - 1) * espacioVertical;
        float waterBot = bottomY + grassH;
        float waterTop = lastLilyY + 1.5f;
        float waterH = waterTop - waterBot;

        var agua = Instantiate(prefabAgua,
            new Vector3(centerX, waterBot + waterH / 2f, 1f),
            Quaternion.identity);
        var srA = agua.GetComponent<SpriteRenderer>();
        srA.drawMode = SpriteDrawMode.Tiled;
        srA.size = new Vector2(width / scale, waterH / scale);
        agua.transform.localScale = Vector3.one * scale;

        // 3) Césped final [waterTop .. waterTop+1.5]
        var grassEnd = Instantiate(prefabCesped,
            new Vector3(centerX, waterTop + grassH / 2f, 0f),
            Quaternion.identity);
        var srG2 = grassEnd.GetComponent<SpriteRenderer>();
        srG2.drawMode = SpriteDrawMode.Tiled;
        srG2.size = new Vector2(width / scale, grassH / scale);
        grassEnd.transform.localScale = Vector3.one * scale;

        // ——— Ajuste de cámara: que deje de seguir y se fije para mostrar todo grassEnd ———

        // Al final de GenerarEntorno()
        var cf = Camera.main.GetComponent<CameraFollow>();
        if (cf != null)
            cf.SetStopFollow(grassEnd.transform, grassH);

    }



    void SpawnPlayerEnInicio()
    {
        var cam = Camera.main;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        // jugador en X=0, Y mitad césped inicial
        float spawnY = bottomY + (1.5f / 2f);
        var playerGO = Instantiate(playerPrefab, new Vector3(0f, spawnY, 0f), Quaternion.identity);
        playerGO.transform.localScale = Vector3.one * 0.2f;
        playerMovement = playerGO.GetComponent<PlayerMovement>();

        var camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
            camFollow.SetTarget(playerGO.transform);
    }

    void MostrarEnunciado(int f)
    {
        if (f < problemasDisponibles.Count)
            textoEnunciado.text = problemasDisponibles[f].enunciado;
    }

    void ActivarFila(int f)
    {
        if (f < 0 || f >= filasLily.Count) return;
        var prob = problemasDisponibles[f];

        var opciones = new List<string> { prob.valoresNenufares[0] };
        var distract = new List<string>(prob.valoresNenufares);
        distract.RemoveAt(0);
        Shuffle(distract);
        for (int i = 0; i < Mathf.Min(2, distract.Count); i++)
            opciones.Add(distract[i]);
        Shuffle(opciones);

        var fila = filasLily[f];
        for (int i = 0; i < fila.Count; i++)
        {
            fila[i].SetValor(opciones[i]);
            fila[i].esCorrecto = opciones[i] == prob.valoresNenufares[0];
        }
    }

    public void OnLilyClicked(Lily lily)
    {
        foreach (var fila in filasLily)
            foreach (var l in fila)
                l.GetComponent<Collider2D>().enabled = false;

        playerMovement.MoveTo(lily.transform.position, () =>
        {
            if (lily.esCorrecto)
            {
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
    [System.Serializable] class Wrapper { public Problema[] problemas; }
    string WrapArray(string j) => "{\"problemas\":" + j + "}";

    void Shuffle<T>(List<T> list)
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
