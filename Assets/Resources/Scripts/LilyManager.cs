using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Data;
using System.Linq;


[System.Serializable]
public class Problema
{
    public string enunciado;
    public string[] valoresNenufares;
    public int correctIndex;
}


public class LilyManager : MonoBehaviour
{
    public GameObject prefabLily;
    public GameObject prefabAgua;
    public GameObject prefabCesped;
    public GameObject playerPrefab;

    public GameObject panelPregunta;
    public GameObject panelFinish;
    public TextMeshProUGUI textoFinal;
    public TextMeshProUGUI textoEnunciado;

    public int filasCount = 5;
    public float espacioVertical = 5f;

    private List<Problema> problemasDisponibles = new List<Problema>();
    private List<List<Lily>> filasLily = new List<List<Lily>>();
    private int filaActual = 0;
    private int generatedRowsCount = 0;
    [SerializeField] private GameObject mensajeIncorrectoUI;

    private PlayerMovement playerMovement;
    private AudioSource audioSource;
    private Vector3 grassEndPos;

    public AudioClip musicaVictoria;

    void Start()
    {
        panelFinish.SetActive(false);
        panelPregunta.SetActive(true);
        StartCoroutine(CargarPreguntasDesdeAPI());
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    IEnumerator CargarPreguntasDesdeAPI()
    {
        string token = PlayerPrefs.GetString("token");
        int level = PlayerPrefs.GetInt("nivel_seleccionado", 1);
        string gameId = PlayerPrefs.GetString("selected_game_id");

        string levelUrl = ApiConfig.GetLevel(gameId, level);
        UnityWebRequest www = UnityWebRequest.Get(levelUrl);
        www.SetRequestHeader("Authorization", token);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error cargando nivel: " + www.error);
            yield break;
        }

        LevelResponse levelData = JsonUtility.FromJson<LevelResponse>(www.downloadHandler.text);
        List<Problema> preguntasCompletas = new List<Problema>();

        foreach (string qid in levelData.questions)
        {
            string questionUrl = ApiConfig.GetQuestion(qid);
            UnityWebRequest qRequest = UnityWebRequest.Get(questionUrl);
            qRequest.SetRequestHeader("Authorization", token);
            yield return qRequest.SendWebRequest();

            if (qRequest.result == UnityWebRequest.Result.Success)
            {
                PreguntaData p = JsonUtility.FromJson<PreguntaData>(qRequest.downloadHandler.text);

                List<string> opcionesOriginales = new List<string>(p.options);
                string respuestaCorrecta = opcionesOriginales[p.correctIndex];

                List<string> primerasTres = opcionesOriginales.Take(3).ToList();
                if (!primerasTres.Contains(respuestaCorrecta))
                {
                    Debug.LogWarning($"❌ Pregunta descartada: respuesta correcta '{respuestaCorrecta}' no está en las 3 primeras.");
                    continue;
                }

                System.Random rng = new System.Random();
                List<string> opcionesMezcladas = primerasTres.OrderBy(x => rng.Next()).ToList();
                int nuevoCorrectIndex = opcionesMezcladas.IndexOf(respuestaCorrecta);

                Problema nuevo = new Problema
                {
                    enunciado = p.text,
                    valoresNenufares = opcionesMezcladas.ToArray(),
                    correctIndex = nuevoCorrectIndex
                };

                preguntasCompletas.Add(nuevo);
            }
        }

        problemasDisponibles.Clear();
        problemasDisponibles.AddRange(preguntasCompletas);

        GenerarMapaCompleto(filasCount);
        GenerarEntorno();
        SpawnPlayerEnInicio();
        MostrarEnunciado(0);
        ActivarFila(0);
    }



    void GenerarMapaCompleto(int filasCount)
    {
        int max = Mathf.Min(filasCount, problemasDisponibles.Count);
        generatedRowsCount = max;

        var cam = Camera.main;
        float leftX = cam.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = cam.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float screenWidth = rightX - leftX;
        float lilyW = prefabLily.GetComponent<SpriteRenderer>().bounds.size.x;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float firstLilyY = bottomY + 2.5f;

        for (int f = 0; f < max; f++)
        {
            float posY = firstLilyY + f * espacioVertical;
            var prob = problemasDisponibles[f];
            // Mostramos solo 3 nenúfares, aunque vengan más opciones
            int totalSlots = Mathf.Min(3, prob.valoresNenufares.Length);
            float gap = (screenWidth - totalSlots * lilyW) / (totalSlots + 1);
            var fila = new List<Lily>();

            for (int i = 0; i < totalSlots; i++)
            {
                float posX = leftX + gap + lilyW * .5f + i * (lilyW + gap);
                var go = Instantiate(prefabLily, new Vector3(posX, posY, 0f), Quaternion.identity);
                var script = go.GetComponent<Lily>();
                script.manager = this;
                script.textoValor = go.GetComponentInChildren<TextMeshProUGUI>();
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
        float leftX = cam.ViewportToWorldPoint(new Vector3(0, .5f, 0)).x;
        float rightX = cam.ViewportToWorldPoint(new Vector3(1, .5f, 0)).x;
        float centerX = (leftX + rightX) / 2f;
        float width = rightX - leftX;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;

        const float grassH = 1.5f;
        const float scale = 0.1f;

        // césped inicio
        var grassStart = Instantiate(prefabCesped,
            new Vector3(centerX, bottomY + grassH / 2f, 0f),
            Quaternion.identity);
        var srG1 = grassStart.GetComponent<SpriteRenderer>();
        srG1.drawMode = SpriteDrawMode.Tiled;
        srG1.size = new Vector2(width / scale, grassH / scale);
        grassStart.transform.localScale = Vector3.one * scale;

        // agua entre nenúfares
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

        // 3) Césped final (meta) – ahora triple de alto [waterTop .. waterTop + 1.5 * 3]
        float finalGrassH = grassH * 3f;
        var grassEnd = Instantiate(prefabCesped,
            new Vector3(centerX, waterTop + finalGrassH / 2f, 0f),
            Quaternion.identity);
        var srG2 = grassEnd.GetComponent<SpriteRenderer>();
        srG2.drawMode = SpriteDrawMode.Tiled;
        srG2.size = new Vector2(width / scale, finalGrassH / scale);
        grassEnd.transform.localScale = Vector3.one * scale;

        // guardamos su posición para el “win routine”
        grassEndPos = grassEnd.transform.position;

        // ajuste de cámara si necesitas fijarla al final
        var cf = cam.GetComponent<CameraFollow>();
        if (cf != null) cf.SetStopFollow(grassEnd.transform, finalGrassH);
    }

    void SpawnPlayerEnInicio()
    {
        var cam = Camera.main;
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float spawnY = bottomY + (1.5f / 2f);
        var playerGO = Instantiate(playerPrefab, new Vector3(0f, spawnY, 0f), Quaternion.identity);
        playerGO.transform.localScale = Vector3.one * 0.2f;
        playerMovement = playerGO.GetComponent<PlayerMovement>();
        var cf = cam.GetComponent<CameraFollow>();
        if (cf != null) cf.SetTarget(playerGO.transform);
    }

    void MostrarEnunciado(int f)
    {
        if (f < problemasDisponibles.Count)
            textoEnunciado.text = problemasDisponibles[f].enunciado;
    }

    void ActivarFila(int f)
    {
        if (f < 0 || f >= filasLily.Count) return;
        var problema = problemasDisponibles[f];
        var fila = filasLily[f];

        for (int i = 0; i < fila.Count && i < problema.valoresNenufares.Length; i++)
        {
            bool esCorrecto = (i == problema.correctIndex);
            fila[i].SetValor(problema.valoresNenufares[i]);
            fila[i].esCorrecto = esCorrecto;
            Debug.Log($"🌱 Nenúfar {i}: {problema.valoresNenufares[i]} - esCorrecto: {esCorrecto}");
        }
    }

    public void OnLilyClicked(Lily lily)
    {
        if (filaActual < 0 || filaActual >= filasLily.Count || !filasLily[filaActual].Contains(lily))
            return;

        foreach (var l in filasLily[filaActual])
            l.GetComponent<Collider2D>().enabled = false;

        Debug.Log($"👆 Click en nenúfar con texto: {lily.textoValor.text}, esCorrecto: {lily.esCorrecto}");

        playerMovement.MoveTo(lily.transform.position, () =>
        {
            StartCoroutine(ProcesarRespuesta(lily));
        });
    }


    IEnumerator ProcesarRespuesta(Lily lily)
    {
        var current = filasLily[filaActual];

        if (!lily.esCorrecto)
        {
            Debug.Log("❌ Nenúfar incorrecto: se eliminan los otros.");
            CurrencyManager.Instance.SumarMonedas(1);

            // Mostrar mensaje
            mensajeIncorrectoUI.SetActive(true);
            yield return new WaitForSeconds(1f);
            mensajeIncorrectoUI.SetActive(false);
        }
        else
        {
            Debug.Log("✅ Nenúfar correcto: se destruyen los demás.");
            CurrencyManager.Instance.SumarMonedas(5);
        }

        foreach (var l in current)
            if (l != lily)
                Destroy(l.gameObject);

        filasLily[filaActual] = new List<Lily> { lily };

        bool wasLast = filaActual == filasLily.Count - 1;
        filaActual++;

        if (wasLast)
            StartCoroutine(WinRoutine());
        else
        {
            MostrarEnunciado(filaActual);
            ActivarFila(filaActual);
        }
    }

    IEnumerator WinRoutine()
    {
        panelPregunta.SetActive(false);
        if (musicaVictoria != null)
        {
            audioSource.clip = musicaVictoria;
            audioSource.volume = 0.3f; // valor entre 0 (silencio) y 1 (máximo)
            audioSource.Play();
        }

        // 1) un segundo de espera antes de saltar
        yield return new WaitForSeconds(1f);

        // 2) salto al centro del césped final
        playerMovement.MoveTo(grassEndPos, null);
        yield return new WaitForSeconds(playerMovement.jumpDuration);

        // 3) baile: 4 pasos, cada paso 0.5s, desplazando ±0.5u y rotando 360°
        Vector3 start = grassEndPos;
        int steps = 4;
        float stepDur = 0.5f;
        for (int i = 0; i < steps; i++)
        {
            float dir = (i % 2 == 0) ? 1f : -1f;
            Vector3 targetPos = start + Vector3.right * 0.5f * dir;
            float elapsed = 0f;
            Quaternion startRot = playerMovement.transform.rotation;
            while (elapsed < stepDur)
            {
                float t = elapsed / stepDur;
                playerMovement.transform.position = Vector3.Lerp(start, targetPos, t);
                playerMovement.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360f * dir, t));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // fijar al final del paso
            playerMovement.transform.position = targetPos;
            start = targetPos;
        }

        // 4) finalmente, el player queda de cabeza (rotación 180°)
        playerMovement.transform.rotation = Quaternion.Euler(0, 0, 180f);

        // 5) mostramos el panel de “ganaste”
        Ganar();
    }

    public void Perder()
    {
        Time.timeScale = 0f;
        panelPregunta.SetActive(false);
        textoFinal.text = "PERDISTE!";
        panelFinish.SetActive(true);
    }

    public void Ganar()
    {
        Time.timeScale = 0f;
        CurrencyManager.Instance.SumarMonedas(50);
        panelPregunta.SetActive(false);
        textoFinal.text = "GANASTE!";
        panelFinish.SetActive(true);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Helpers
    [System.Serializable]
    class Wrapper
    {
        public Problema[] problemas;
    }

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