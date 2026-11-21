
using UnityEngine;
using System.Collections;

public class TriggerCubos : MonoBehaviour
{
    [Header("Cubos")]
    public Transform cuboGrande;
    public Transform cuboMediano;
    public Transform cuboPequeno;

    [Header("Siguiente paso")]
    public AudioSource audioSiguiente;
    public GameObject indicador2;
    public GameObject trigger2;

    [Header("Sonido de logro")]
    public AudioSource Logro;

    [Header("Configuraci�n")]
    public float tolerancia = 0.15f;
    public float tiempoEspera = 2f;

    private bool completado = false;
    private bool comprobando = false;

    private void Update()
    {
        if (completado || comprobando)
            return;

        if (PilaCorrecta())
        {
            StartCoroutine(EsperarConfirmacion());
        }
    }

    IEnumerator EsperarConfirmacion()
    {
        comprobando = true;

        yield return new WaitForSeconds(tiempoEspera);

        if (PilaCorrecta())
        {
            ActivarSiguientePaso();
        }
        else
        {
            Debug.Log("La pila se movi� o se cay�. Volviendo a comprobar.");
        }

        comprobando = false;
    }

    bool PilaCorrecta()
    {
        bool medianoSobreGrande =
            cuboMediano.position.y > cuboGrande.position.y;

        bool pequenoSobreMediano =
            cuboPequeno.position.y > cuboMediano.position.y;

        float distanciaGrandeMediano = Vector2.Distance(
            new Vector2(cuboGrande.position.x, cuboGrande.position.z),
            new Vector2(cuboMediano.position.x, cuboMediano.position.z)
        );

        float distanciaMedianoPequeno = Vector2.Distance(
            new Vector2(cuboMediano.position.x, cuboMediano.position.z),
            new Vector2(cuboPequeno.position.x, cuboPequeno.position.z)
        );

        bool alineados =
            distanciaGrandeMediano < tolerancia &&
            distanciaMedianoPequeno < tolerancia;

        return medianoSobreGrande &&
               pequenoSobreMediano &&
               alineados;
    }

    // Reproduce los sonidos y activa el indicador y trigger del siguiente paso.
    void ActivarSiguientePaso()
    {
        completado = true;

        Debug.Log("�CUBOS APILADOS CORRECTAMENTE!");

        if (Logro != null)
        {
            Logro.Play();
        }

        if (audioSiguiente != null)
        {
            audioSiguiente.Play();
        }

        if (indicador2 != null)
        {
            indicador2.SetActive(true);
        }

        if (trigger2 != null)
        {
            trigger2.SetActive(true);
        }
    }
}
