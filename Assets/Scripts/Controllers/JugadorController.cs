using UnityEngine;

public class JugadorController : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 8f;
    public float desaceleracion = 5f; // Qué tan rápido se detiene el personaje

    private Rigidbody2D rb;
    private bool enSuelo = true;

    private float direccion = 0f;         // -1, 0 o 1 según entrada del jugador
    private float velocidadActual = 0f;   // Velocidad que se va desacelerando

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Si el jugador está presionando izquierda o derecha
        if (direccion != 0f)
        {
            velocidadActual = direccion * velocidad;
        }
        else
        {
            // Desaceleración progresiva
            velocidadActual = Mathf.MoveTowards(velocidadActual, 0f, desaceleracion * Time.deltaTime);
        }

        rb.linearVelocity = new Vector2(velocidadActual, rb.linearVelocity.y);
    }

    public void MoverIzquierda()
    {
        direccion = -1f;
    }

    public void MoverDerecha()
    {
        direccion = 1f;
    }

    public void DetenerMovimiento()
    {
        direccion = 0f; // Pero sigue con inercia gracias a velocidadActual
    }

    public void Saltar()
    {
        if (enSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            enSuelo = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Suelo"))
        {
            enSuelo = true;
        }
    }
}