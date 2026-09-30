
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InicioPruebaBotones : MonoBehaviour
{
    [Header("Objetos a desactivar")]
    public GameObject glow;
    public GameObject trigger;

    [Header("Audios")]
    public AudioSource voz4;
    public AudioSource voz5;

    [Header("Botón")]
    public XRGrabInteractable boton1;

    private bool iniciado = false;

    private void Start()
    {
        // El botón comienza desactivado
        if (boton1 != null)
            boton1.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya se ejecutó, no volver a iniciar
        if (iniciado)
            return;

        // Detectar XR Origin Hands
        if (other.transform.root.name != "XR Origin Hands (XR Rig)")
            return;

        Debug.Log("XR Origin Hands (XR Rig) entró al trigger.");

        // Marcar inmediatamente como iniciado
        iniciado = true;

        // Apagar glow
        if (glow != null)
            glow.SetActive(false);

        // Iniciar la secuencia
        StartCoroutine(IniciarSecuencia());
    }

    private IEnumerator IniciarSecuencia()
    {
        Debug.Log("Iniciando prueba de botones.");

        // Reproducir voz 4
        yield return Reproducir(voz4);

        // Reproducir voz 5
        yield return Reproducir(voz5);

        // Activar botón
        if (boton1 != null)
        {
            boton1.enabled = true;
            Debug.Log("Botón 1 activado.");
        }

        // Esperar un pequeño momento antes de apagar el trigger
        yield return new WaitForSeconds(0.1f);

        // Desactivar el GameObject del trigger AL FINAL
        if (trigger != null)
        {
            trigger.SetActive(false);
            Debug.Log("Trigger desactivado. La prueba no volverá a iniciar.");
        }
    }

    private IEnumerator Reproducir(AudioSource audio)
    {
        if (audio == null)
        {
            Debug.LogWarning("Audio no asignado.");
            yield break;
        }

        audio.Stop();
        audio.Play();

        Debug.Log("Reproduciendo: " + audio.name);

        // Esperar hasta que termine
        yield return new WaitWhile(() => audio.isPlaying);

        Debug.Log("Terminó: " + audio.name);
    }
}
