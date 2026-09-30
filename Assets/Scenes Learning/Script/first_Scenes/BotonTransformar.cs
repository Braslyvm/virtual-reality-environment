using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotonTransformar : MonoBehaviour
{
    [Header("Transformacion")]
    public SkinnedMeshRenderer cubo;
    public string blendShape = "Esfera";
    public float duracion = 1.5f;

    [Header("Presion")]
    public float recorridoPresion = 0.025f;
    public float presionNecesaria = 0.018f;

    [Header("Giro")]
    public float giroMaximo = 70f;
    public float giroNecesario = 40f;
    public bool giroHorario = true;

    [Header("Confirmacion")]
    public float tiempoBloqueadoNecesario = 0.30f;

    [Header("Retorno")]
    public float velocidadRetorno = 10f;

    [Header("Audio")]
    public AudioSource voz8;
    public AudioSource voz9;

    [Header("Siguiente")]
    public XRSimpleInteractable siguienteBoton;

    private XRSimpleInteractable interactable;

    private Transform mano;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private float alturaManoInicial;
    private Vector3 direccionGiroInicial;

    private int indice;

    private bool seleccionado;
    private bool presionado;
    private bool giroBloqueado;
    private bool activado;

    private float tiempoBloqueado;


    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        interactable.selectEntered.AddListener(Agarrar);
        interactable.selectExited.AddListener(Soltar);

        if (siguienteBoton != null)
            siguienteBoton.enabled = false;

        if (cubo != null && cubo.sharedMesh != null)
        {
            indice = cubo.sharedMesh.GetBlendShapeIndex(blendShape);

            if (indice < 0)
                Debug.LogError("No se encontro el Blend Shape: " + blendShape);
        }
    }


    private void Update()
    {
        if (seleccionado && mano != null)
        {
            ActualizarBoton();
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                posicionInicial,
                Time.deltaTime * velocidadRetorno
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionInicial,
                Time.deltaTime * velocidadRetorno
            );
        }
    }


    private void Agarrar(SelectEnterEventArgs args)
    {
        if (activado)
            return;

        mano = args.interactorObject.transform;

        seleccionado = true;
        presionado = false;
        giroBloqueado = false;

        tiempoBloqueado = 0f;

        alturaManoInicial = mano.position.y;
    }


    private void ActualizarBoton()
    {
        if (!presionado)
        {
            float bajada =
                alturaManoInicial -
                mano.position.y;

            bajada = Mathf.Clamp(
                bajada,
                0f,
                recorridoPresion
            );

            transform.position =
                posicionInicial +
                Vector3.down * bajada;

            transform.rotation =
                rotacionInicial;


            if (bajada >= presionNecesaria)
            {
                presionado = true;

                transform.position =
                    posicionInicial +
                    Vector3.down *
                    recorridoPresion;

                direccionGiroInicial =
                    ObtenerDireccionHorizontal();

                Debug.Log(
                    "Boton 2 presionado. Giro desbloqueado."
                );
            }

            return;
        }


        transform.position =
            posicionInicial +
            Vector3.down *
            recorridoPresion;


        if (giroBloqueado)
        {
            AplicarRotacionBloqueada();

            tiempoBloqueado +=
                Time.deltaTime;


            if (
                tiempoBloqueado >= tiempoBloqueadoNecesario &&
                !activado
            )
            {
                activado = true;

                Debug.Log(
                    "Boton 2 confirmado."
                );

                StartCoroutine(
                    TransformarYFinalizar()
                );
            }

            return;
        }


        Vector3 direccionActual =
            ObtenerDireccionHorizontal();


        if (
            direccionActual.sqrMagnitude < 0.01f ||
            direccionGiroInicial.sqrMagnitude < 0.01f
        )
            return;


        float angulo =
            Vector3.SignedAngle(
                direccionGiroInicial,
                direccionActual,
                Vector3.up
            );


        float giroPermitido;


        if (giroHorario)
        {
            giroPermitido =
                Mathf.Clamp(
                    -angulo,
                    0f,
                    giroMaximo
                );

            transform.rotation =
                Quaternion.AngleAxis(
                    -giroPermitido,
                    Vector3.up
                ) *
                rotacionInicial;
        }
        else
        {
            giroPermitido =
                Mathf.Clamp(
                    angulo,
                    0f,
                    giroMaximo
                );

            transform.rotation =
                Quaternion.AngleAxis(
                    giroPermitido,
                    Vector3.up
                ) *
                rotacionInicial;
        }


        if (giroPermitido >= giroNecesario)
        {
            giroBloqueado = true;
            tiempoBloqueado = 0f;

            AplicarRotacionBloqueada();

            Debug.Log(
                "Giro bloqueado durante " +
                tiempoBloqueadoNecesario +
                " segundos."
            );
        }
    }


    private void AplicarRotacionBloqueada()
    {
        if (giroHorario)
        {
            transform.rotation =
                Quaternion.AngleAxis(
                    -giroNecesario,
                    Vector3.up
                ) *
                rotacionInicial;
        }
        else
        {
            transform.rotation =
                Quaternion.AngleAxis(
                    giroNecesario,
                    Vector3.up
                ) *
                rotacionInicial;
        }
    }


    private Vector3 ObtenerDireccionHorizontal()
    {
        Vector3 direccion =
            mano.position -
            transform.position;

        direccion =
            Vector3.ProjectOnPlane(
                direccion,
                Vector3.up
            );


        if (direccion.sqrMagnitude < 0.001f)
        {
            direccion =
                Vector3.ProjectOnPlane(
                    mano.forward,
                    Vector3.up
                );
        }

        return direccion.normalized;
    }


    private IEnumerator TransformarYFinalizar()
    {
        if (
            cubo != null &&
            indice >= 0
        )
        {
            float inicio =
                cubo.GetBlendShapeWeight(
                    indice
                );

            float tiempo = 0f;


            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        tiempo / duracion
                    );

                float suave =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );


                cubo.SetBlendShapeWeight(
                    indice,
                    Mathf.Lerp(
                        inicio,
                        100f,
                        suave
                    )
                );

                yield return null;
            }


            cubo.SetBlendShapeWeight(
                indice,
                100f
            );
        }


        yield return Reproducir(voz8);
        yield return Reproducir(voz9);


        if (siguienteBoton != null)
            siguienteBoton.enabled = true;


        while (seleccionado)
            yield return null;


        interactable.enabled = false;
    }


    private IEnumerator Reproducir(
        AudioSource audio
    )
    {
        if (audio == null)
            yield break;

        audio.Stop();
        audio.Play();

        yield return new WaitWhile(
            () => audio.isPlaying
        );
    }


    private void Soltar(
        SelectExitEventArgs args
    )
    {
        seleccionado = false;
        mano = null;


        if (!activado)
        {
            presionado = false;
            giroBloqueado = false;
            tiempoBloqueado = 0f;
        }
    }
}