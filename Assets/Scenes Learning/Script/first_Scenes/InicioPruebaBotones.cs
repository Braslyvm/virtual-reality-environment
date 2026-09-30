using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InicioPruebaBotones : MonoBehaviour
{
    public GameObject glow;
    public Collider triggerInicio;

    public AudioSource voz4;
    public AudioSource voz5;

    public XRGrabInteractable boton1;

    private bool iniciado;

    private void Start()
    {
        if (triggerInicio == null)
            triggerInicio = GetComponent<Collider>();

        if (boton1 != null)
            boton1.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (iniciado)
            return;

        if (!other.CompareTag("Player"))
            return;

        iniciado = true;

        if (glow != null)
            glow.SetActive(false);

        if (triggerInicio != null)
            triggerInicio.enabled = false;

        StartCoroutine(IniciarSecuencia());
    }

    private IEnumerator IniciarSecuencia()
    {
        yield return Reproducir(voz4);

        yield return Reproducir(voz5);

        if (boton1 != null)
            boton1.enabled = true;
    }

    private IEnumerator Reproducir(AudioSource audio)
    {
        if (audio == null)
            yield break;

        audio.Stop();
        audio.Play();

        yield return new WaitWhile(() => audio.isPlaying);
    }
}