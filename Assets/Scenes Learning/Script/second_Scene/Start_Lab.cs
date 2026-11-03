using System.Collections;
using UnityEngine;

public class start_labnn : MonoBehaviour
{
    [Header("Audio inicial")]
    public AudioSource audioSource;
    public float delay = 2f;

    [Header("Glows de las estaciones")]
    public GameObject glow1;
    public GameObject glow2;

    [Header("Triggers de las estaciones")]
    public GameObject trigger1;
    public GameObject trigger2;

    private bool activado = false;

    private void Start()
    {
        StartCoroutine(IniciarProceso());
    }

    private IEnumerator IniciarProceso()
    {
        if (activado)
            yield break;

        activado = true;

        Debug.Log("Esperando para iniciar el laboratorio...");

        yield return new WaitForSeconds(delay);

        // INICIAR AUDIO
        if (audioSource != null)
        {
            if (audioSource.clip != null)
            {
                audioSource.Play();
                Debug.Log("Narracion inicial reproduciendose");
            }
            else
            {
                Debug.LogError("El AudioSource no tiene un AudioClip asignado");
            }
        }
        else
        {
            Debug.LogError("No se asigno el AudioSource");
        }

        // ACTIVAR GLOWS
        if (glow1 != null)
            glow1.SetActive(true);

        if (glow2 != null)
            glow2.SetActive(true);

        // ACTIVAR TRIGGERS
        if (trigger1 != null)
            trigger1.SetActive(true);

        if (trigger2 != null)
            trigger2.SetActive(true);

        Debug.Log("Laboratorio iniciado correctamente");
    }
}