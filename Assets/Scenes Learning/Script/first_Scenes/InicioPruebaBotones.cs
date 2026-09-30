using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InicioPruebaBotones : MonoBehaviour
{
    [Header("Objetos a desactivar")]
    public GameObject glow;
    public GameObject trigger;

    [Header("Audios")]
    public AudioSource voz4;
    public AudioSource voz5;

    [Header("Primer botón")]
    public XRSimpleInteractable boton1;

    private bool iniciado = false;

    private void Start()
    {
        if (boton1 != null)
            boton1.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (iniciado)
            return;

        if (other.transform.root.name != "XR Origin Hands (XR Rig)")
            return;

        iniciado = true;

        Debug.Log("Iniciando prueba de botones.");

        if (glow != null)
            glow.SetActive(false);

        StartCoroutine(IniciarSecuencia());
    }

    private IEnumerator IniciarSecuencia()
    {
        yield return Reproducir(voz4);
        yield return Reproducir(voz5);

        if (boton1 != null)
        {
            boton1.enabled = true;
            Debug.Log("Botón 1 habilitado.");
        }

        yield return new WaitForSeconds(0.1f);

        if (trigger != null)
        {
            trigger.SetActive(false);
            Debug.Log("Trigger desactivado.");
        }
    }

    private IEnumerator Reproducir(AudioSource audio)
    {
        if (audio == null)
        {
            Debug.LogWarning("AudioSource no asignado.");
            yield break;
        }

        audio.Stop();
        audio.Play();

        yield return new WaitWhile(() => audio.isPlaying);
    }
}