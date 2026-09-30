using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotonGiro : MonoBehaviour
{
    [Header("Cubo")]
    public Transform cubo;
    public float velocidadCubo = 180f;

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
    public AudioSource voz10;

    private XRSimpleInteractable interactable;

    private Transform mano;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private float alturaManoInicial;
    private Vector3 direccionGiroInicial;

    private bool seleccionado;
    private bool presionado;
    private bool giroBloqueado;
    private bool activado;
    private bool cuboGirando;
    private bool terminado;

    private float tiempoBloqueado;


    private void Awake()
    {
        interactable =
            GetComponent<XRSimpleInteractable>();

        posicionInicial =
            transform.position;

        rotacionInicial =
            transform.rotation;

        interactable.selectEntered.AddListener(
            Agarrar
        );

        interactable.selectExited.AddListener(
            Soltar
        );
    }


    private void Update()
    {
        if (cuboGirando && cubo != null)
        {
            cubo.Rotate(
                Vector3.up,
                velocidadCubo *
                Time.deltaTime,
                Space.World
            );
        }


        if (seleccionado && mano != null)
        {
            ActualizarBoton();
        }
        else
        {
            transform.position =
                Vector3.Lerp(
                    transform.position,
                    posicionInicial,
                    Time.deltaTime *
                    velocidadRetorno
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotacionInicial,
                    Time.deltaTime *
                    velocidadRetorno
                );
        }
    }


    private void Agarrar(
        SelectEnterEventArgs args
    )
    {
        if (terminado)
            return;

        mano =
            args.interactorObject.transform;

        seleccionado = true;
        presionado = false;
        giroBloqueado = false;

        tiempoBloqueado = 0f;

        alturaManoInicial =
            mano.position.y;
    }


    private void ActualizarBoton()
    {
        if (!presionado)
        {
            float bajada =
                alturaManoInicial -
                mano.position.y;

            bajada =
                Mathf.Clamp(
                    bajada,
                    0f,
                    recorridoPresion
                );


            transform.position =
                posicionInicial +
                Vector3.down *
                bajada;


            transform.rotation =
                rotacionInicial;


            if (
                bajada >=
                presionNecesaria
            )
            {
                presionado = true;

                transform.position =
                    posicionInicial +
                    Vector3.down *
                    recorridoPresion;

                direccionGiroInicial =
                    ObtenerDireccionHorizontal();

                Debug.Log(
                    "Boton 3 presionado. Giro desbloqueado."
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


            if (!activado)
            {
                tiempoBloqueado +=
                    Time.deltaTime;


                if (
                    tiempoBloqueado >=
                    tiempoBloqueadoNecesario
                )
                {
                    activado = true;
                    cuboGirando = true;

                    Debug.Log(
                        "Cubo girando. Suelta el boton para detenerlo."
                    );
                }
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


        if (
            giroPermitido >=
            giroNecesario
        )
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


        if (
            direccion.sqrMagnitude <
            0.001f
        )
        {
            direccion =
                Vector3.ProjectOnPlane(
                    mano.forward,
                    Vector3.up
                );
        }


        return direccion.normalized;
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

            return;
        }


        cuboGirando = false;
        terminado = true;

        Debug.Log(
            "Cubo detenido."
        );


        StartCoroutine(
            Finalizar()
        );
    }


    private IEnumerator Finalizar()
    {
        yield return Reproducir(
            voz10
        );

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
}