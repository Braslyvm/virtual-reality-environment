
using UnityEngine;

public enum TipoBotonEstacion
{
    Rojo,
    Verde
}

public class BotonEstacionVR : MonoBehaviour
{
    [Header("Configuracion del boton")]
    public TipoBotonEstacion tipoBoton;

    [Header("Controlador principal")]
    public GestorEstacion3 gestor;

    [Header("Glow del boton")]
    public GameObject glowBoton;

    [Header("Interaccion XR")]
    public Behaviour interactuableXR;

    [Header("Siguiente boton a habilitar")]
    public BotonEstacionVR siguienteBoton;

    private Collider colliderBoton;

    private bool habilitado;
    private bool presionado = false;

    private void Awake()
    {
        colliderBoton = GetComponent<Collider>();

        // El boton rojo lo habilitara TriggerAudio2.
        // El boton verde inicia deshabilitado.
        habilitado = tipoBoton == TipoBotonEstacion.Rojo;

        if (!habilitado)
        {
            if (colliderBoton != null)
                colliderBoton.enabled = false;

            if (interactuableXR != null)
                interactuableXR.enabled = false;

            if (glowBoton != null)
                glowBoton.SetActive(false);
        }
    }

    public void Presionar()
    {
        Debug.Log("SE EJECUTO PRESIONAR DESDE XR");
        if (!habilitado || presionado)
            return;

        if (gestor == null)
        {
            Debug.LogError("Falta asignar GestorEstacion3.");
            return;
        }

        if (tipoBoton == TipoBotonEstacion.Rojo)
        {
            gestor.EncenderBanda();

            Debug.Log("Boton rojo: banda encendida.");
        }
        else
        {
            gestor.IniciarDispensador();

            Debug.Log("Boton verde: dispensador iniciado.");
        }

        presionado = true;
        habilitado = false;

        if (glowBoton != null)
            glowBoton.SetActive(false);

        if (siguienteBoton != null)
            siguienteBoton.Habilitar();
    }

    public void Habilitar()
    {
        if (presionado)
            return;

        habilitado = true;

        if (colliderBoton != null)
            colliderBoton.enabled = true;

        if (interactuableXR != null)
            interactuableXR.enabled = true;

        if (glowBoton != null)
            glowBoton.SetActive(true);
    }
}
