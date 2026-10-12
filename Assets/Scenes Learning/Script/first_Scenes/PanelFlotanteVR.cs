
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PanelFlotanteVR : MonoBehaviour
{
    [Header("Botones de la pantalla")]
    public Transform botonSiguiente;
    public Transform botonReiniciar;
    public Transform textoSiguiente;
    public Transform textoReiniciar;

    [Header("Escenas (arrastrar desde Project)")]
#if UNITY_EDITOR
    public SceneAsset escenaSiguiente;
    public SceneAsset escenaReiniciar;
#endif

    [SerializeField, HideInInspector]
    private string rutaSiguiente;

    [SerializeField, HideInInspector]
    private string rutaReiniciar;

    [Header("Audio")]
    public AudioSource Logro;
    public float esperaDespuesAudio = 0.3f;

    [Header("Presion")]
    public float distanciaHundimiento = 0.012f;
    public float duracionPresion = 0.15f;

    public Vector3 direccionHundimientoMundo =
        new Vector3(0f, 0f, 1f);

    [Header("Flotacion")]
    public bool flotar = true;
    public float amplitud = 0.04f;
    public float velocidad = 1.4f;

    private Vector3 posicionInicial;
    private bool ocupado = false;

    private void Awake()
    {
        posicionInicial = transform.localPosition;
        ocupado = false;

        // Identificar cada boton por separado
        botonSiguiente = BuscarObjeto(
            transform,
            "Boton_Siguiente"
        );

        botonReiniciar = BuscarObjeto(
            transform,
            "Boton_Reiniciar"
        );

        // Identificar el texto de cada boton
        textoSiguiente = botonSiguiente != null
            ? BuscarObjeto(botonSiguiente, "Texto_Siguiente")
            : null;

        textoReiniciar = botonReiniciar != null
            ? BuscarObjeto(botonReiniciar, "Texto_Reiniciar")
            : null;

        if (botonSiguiente == null)
        {
            Debug.LogError(
                "No se encontro Boton_Siguiente.",
                this
            );
        }

        if (botonReiniciar == null)
        {
            Debug.LogError(
                "No se encontro Boton_Reiniciar.",
                this
            );
        }

        if (botonSiguiente != null &&
            botonSiguiente == botonReiniciar)
        {
            Debug.LogError(
                "Los botones no pueden ser el mismo objeto.",
                this
            );
        }

        Debug.Log(
            "Panel inicializado. Siguiente: " +
            (botonSiguiente != null
                ? botonSiguiente.name
                : "NULL") +
            " | Reiniciar: " +
            (botonReiniciar != null
                ? botonReiniciar.name
                : "NULL")
        );
    }

    private Transform BuscarObjeto(
        Transform padre,
        string nombre
    )
    {
        foreach (Transform hijo in padre)
        {
            if (hijo.name == nombre)
                return hijo;

            Transform encontrado =
                BuscarObjeto(hijo, nombre);

            if (encontrado != null)
                return encontrado;
        }

        return null;
    }

    private void Update()
    {
        if (!flotar)
            return;

        float desplazamiento =
            Mathf.Sin(Time.time * velocidad) *
            amplitud;

        Vector3 movimientoMundo =
            Vector3.up * desplazamiento;

        Vector3 movimientoLocal =
            transform.parent != null
                ? transform.parent.InverseTransformVector(
                    movimientoMundo
                )
                : movimientoMundo;

        transform.localPosition =
            posicionInicial + movimientoLocal;
    }

    public bool PulsarSiguiente()
    {
        Debug.Log("Se presiono SIGUIENTE.");

        return Pulsar(
            botonSiguiente,
            textoSiguiente,
            rutaSiguiente
        );
    }

    public bool PulsarReiniciar()
    {
        Debug.Log("Se presiono REINICIAR.");

        return Pulsar(
            botonReiniciar,
            textoReiniciar,
            rutaReiniciar
        );
    }

    private bool Pulsar(
        Transform boton,
        Transform texto,
        string ruta
    )
    {
        if (ocupado)
            return false;

        if (boton == null)
        {
            Debug.LogError(
                "No se encontro el boton seleccionado.",
                this
            );

            return false;
        }

        if (string.IsNullOrEmpty(ruta) ||
            !Application.CanStreamedLevelBeLoaded(ruta))
        {
            Debug.LogError(
                "Escena no asignada o no incluida en Build: " +
                ruta,
                this
            );

            return false;
        }

        ocupado = true;

        StartCoroutine(
            ConfirmarYCambiar(
                boton,
                texto,
                ruta
            )
        );

        return true;
    }

    private IEnumerator ConfirmarYCambiar(
        Transform boton,
        Transform texto,
        string ruta
    )
    {
        Vector3 baseBoton =
            boton.localPosition;

        Vector3 baseTexto =
            texto != null
                ? texto.localPosition
                : Vector3.zero;

        Vector3 desplazamientoMundo =
            direccionHundimientoMundo.normalized *
            distanciaHundimiento;

        Vector3 desplazamientoBoton =
            boton.parent != null
                ? boton.parent.InverseTransformVector(
                    desplazamientoMundo
                )
                : desplazamientoMundo;

        bool moverTexto =
            texto != null &&
            !texto.IsChildOf(boton);

        Vector3 desplazamientoTexto =
            Vector3.zero;

        if (moverTexto)
        {
            desplazamientoTexto =
                texto.parent != null
                    ? texto.parent.InverseTransformVector(
                        desplazamientoMundo
                    )
                    : desplazamientoMundo;
        }

        float tiempo = 0f;

        float duracion =
            Mathf.Max(0.01f, duracionPresion);

        Debug.Log(
            "Hundiendo boton: " + boton.name
        );

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(
                    tiempo / duracion
                )
            );

            boton.localPosition =
                baseBoton +
                desplazamientoBoton * t;

            if (moverTexto)
            {
                texto.localPosition =
                    baseTexto +
                    desplazamientoTexto * t;
            }

            yield return null;
        }

        boton.localPosition =
            baseBoton + desplazamientoBoton;

        if (moverTexto)
        {
            texto.localPosition =
                baseTexto + desplazamientoTexto;
        }

        if (Logro != null && Logro.clip != null)
        {
            Logro.loop = false;
            Logro.Stop();
            Logro.Play();

            yield return new WaitWhile(
                () => Logro != null &&
                      Logro.isPlaying
            );
        }

        if (esperaDespuesAudio > 0f)
        {
            yield return new WaitForSeconds(
                esperaDespuesAudio
            );
        }

        Debug.Log(
            "Cargando escena: " + ruta
        );

        AsyncOperation carga =
            SceneManager.LoadSceneAsync(ruta);

        if (carga == null)
        {
            Debug.LogError(
                "No se pudo cargar la escena: " +
                ruta,
                this
            );

            boton.localPosition = baseBoton;

            if (moverTexto)
                texto.localPosition = baseTexto;

            ocupado = false;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        rutaSiguiente =
            escenaSiguiente != null
                ? AssetDatabase.GetAssetPath(
                    escenaSiguiente
                )
                : "";

        rutaReiniciar =
            escenaReiniciar != null
                ? AssetDatabase.GetAssetPath(
                    escenaReiniciar
                )
                : "";
    }
#endif
}
