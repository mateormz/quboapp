using UnityEngine;

[System.Serializable]
public class PreguntaData
{
    public string pregunta;
    public string[] alternativas; // 3 opciones
    public int indiceCorrecta; // 0, 1 o 2
}