using UnityEngine;
using System.Collections;

public class InicioAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Indicador")]
    public GameObject marca;

    [Header("Zona de detecci�n")]
    public GameObject zonaDeteccion;

    // Oculta los indicadores e inicia la secuencia de audio.
    void Start()
    {
        if (marca != null)
            marca.SetActive(false);

        if (zonaDeteccion != null)
            zonaDeteccion.SetActive(false);

        StartCoroutine(IniciarAudio());
    }

    // Reproduce el audio y activa la marca y la zona de detección al terminar.
    IEnumerator IniciarAudio()
    {
        yield return new WaitForSeconds(2f);

        if (audioSource != null)
        {
            audioSource.Play();
            Debug.Log("Audio iniciado");
        }

        yield return new WaitWhile(() => audioSource.isPlaying);

        Debug.Log("Audio terminado");

        if (marca != null)
            marca.SetActive(true);

        if (zonaDeteccion != null)
            zonaDeteccion.SetActive(true);
    }
}