using UnityEngine;
using System.Collections;

public class InicioAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Indicador")]
    public GameObject marca;

    [Header("Zona de detección")]
    public GameObject zonaDeteccion;

    void Start()
    {
        // Ocultar al iniciar
        if (marca != null)
            marca.SetActive(false);

        if (zonaDeteccion != null)
            zonaDeteccion.SetActive(false);

        // Esperar 2 segundos y luego iniciar audio
        StartCoroutine(IniciarAudio());
    }

    IEnumerator IniciarAudio()
    {
        // Esperar 2 segundos
        yield return new WaitForSeconds(2f);

        // Iniciar audio
        if (audioSource != null)
        {
            audioSource.Play();
            Debug.Log("Audio iniciado");
        }

        // Esperar a que termine
        yield return new WaitWhile(() => audioSource.isPlaying);

        Debug.Log("Audio terminado");

        // Activar marca
        if (marca != null)
            marca.SetActive(true);

        // Activar zona de detección
        if (zonaDeteccion != null)
            zonaDeteccion.SetActive(true);
    }
}