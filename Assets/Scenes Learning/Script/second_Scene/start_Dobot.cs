
using System.Collections;
using UnityEngine;

public class start_Dobot : MonoBehaviour
{
    [Header("Audio Voz Estacion 3 #1")]
    public AudioSource audioSource;

    [Header("Audio Voz Estacion 3 #2")]
    public AudioSource voz2;

    [Header("Glows anteriores")]
    public GameObject glow;
    public GameObject otroGlow;

    [Header("Controlador de la estacion")]
    public GestorEstacion3 gestorCorrutinas;

    [Header("Boton rojo de la banda")]
    public GameObject botonRojo;

    [Header("Glow del boton rojo")]
    public GameObject glowBotonRojo;

    [Header("Interaccion XR del boton rojo")]
    public Behaviour interactuableXR;

    private bool activado = false;

    private Collider colliderBotonRojo;

    // Desactiva y oculta inicialmente el botón rojo de la banda.
    private void Awake()
    {
        if (botonRojo != null)
        {
            colliderBotonRojo =
                botonRojo.GetComponent<Collider>();

            if (colliderBotonRojo != null)
            {
                colliderBotonRojo.enabled = false;
            }
        }

        if (interactuableXR != null)
        {
            interactuableXR.enabled = false;
        }

        OcultarGlow(glowBotonRojo);
    }

    // Inicia las narraciones y habilita el botón rojo cuando entra el jugador XR.
    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (other.transform.root.name != "XR Origin Hands (XR Rig)")
            return;

        MonoBehaviour ejecutor = null;

        if (gestorCorrutinas != null &&
            gestorCorrutinas.isActiveAndEnabled)
        {
            ejecutor = gestorCorrutinas;
        }
        else if (isActiveAndEnabled)
        {
            ejecutor = this;
        }

        if (ejecutor == null)
        {
            Debug.LogError(
                "No hay un controlador activo para reproducir las voces."
            );
            return;
        }

        activado = true;

        Debug.Log("Jugador entro por primera vez a ZonaInicio_3.");

        OcultarGlow(glow);
        OcultarGlow(otroGlow);


        ejecutor.StartCoroutine(
            ReproducirNarracionesEstacion3()
        );
    }


    // Oculta los renderizadores y las luces del objeto indicado.
    private void OcultarGlow(GameObject objeto)
    {
        if (objeto == null)
            return;

        Renderer[] renderizadores =
            objeto.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderizador in renderizadores)
        {
            renderizador.enabled = false;
        }

        Light[] luces =
            objeto.GetComponentsInChildren<Light>(true);

        foreach (Light luz in luces)
        {
            luz.enabled = false;
        }
    }


    // Muestra los renderizadores y las luces del objeto indicado.
    private void MostrarGlow(GameObject objeto)
    {
        if (objeto == null)
            return;

        objeto.SetActive(true);

        Renderer[] renderizadores =
            objeto.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderizador in renderizadores)
        {
            renderizador.enabled = true;
        }

        Light[] luces =
            objeto.GetComponentsInChildren<Light>(true);

        foreach (Light luz in luces)
        {
            luz.enabled = true;
        }
    }


    // Reproduce las dos voces de la estación 3 y habilita el botón rojo.
    private IEnumerator ReproducirNarracionesEstacion3()
    {
        if (audioSource != null)
        {
            Debug.Log("Reproduciendo Voz Estacion 3 #1");

            audioSource.Stop();
            audioSource.Play();

            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }
        else
        {
            Debug.LogWarning(
                "No se configuro la Voz #1."
            );
        }

        if (voz2 != null)
        {
            Debug.Log("Reproduciendo Voz Estacion 3 #2");

            voz2.Stop();
            voz2.Play();

            yield return new WaitWhile(
                () => voz2.isPlaying
            );
        }
        else
        {
            Debug.LogWarning(
                "No se configuro la Voz #2."
            );
        }

        ActivarBotonBanda();
    }


    // Habilita el botón rojo de la banda y muestra su glow.
    private void ActivarBotonBanda()
    {
        if (botonRojo != null)
        {
            botonRojo.SetActive(true);
        }

        if (colliderBotonRojo != null)
        {
            colliderBotonRojo.enabled = true;
        }

        if (interactuableXR != null)
        {
            interactuableXR.enabled = true;
        }

        MostrarGlow(glowBotonRojo);

        Debug.Log(
            "Voces #1 y #2 finalizadas. Boton rojo habilitado."
        );
    }
}
