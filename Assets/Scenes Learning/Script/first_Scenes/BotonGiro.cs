using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotonGiro : MonoBehaviour
{
    public Transform cubo;

    public float velocidadCubo = 180f;

    public float distanciaPresion = 0.015f;
    public float presionNecesaria = 0.008f;

    public float giroMaximo = 90f;
    public float giroNecesario = 35f;

    public float velocidadRetorno = 8f;

    public AudioSource voz10;

    private XRGrabInteractable grab;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private Vector3 posicionManoInicial;
    private Vector3 direccionInicial;

    private Transform mano;

    private bool agarrado;
    private bool activado;
    private bool cuboGirando;
    private bool terminado;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;

        grab.selectEntered.AddListener(Agarrar);
        grab.selectExited.AddListener(Soltar);
    }

    private void Update()
    {
        if (cuboGirando && cubo != null)
        {
            cubo.Rotate(
                Vector3.up,
                velocidadCubo * Time.deltaTime,
                Space.Self
            );
        }
    }

    private void LateUpdate()
    {
        if (agarrado && mano != null)
        {
            ActualizarBoton();
        }
        else
        {
            transform.localPosition =
                Vector3.Lerp(
                    transform.localPosition,
                    posicionInicial,
                    Time.deltaTime * velocidadRetorno
                );

            transform.localRotation =
                Quaternion.Slerp(
                    transform.localRotation,
                    rotacionInicial,
                    Time.deltaTime * velocidadRetorno
                );
        }
    }

    private void Agarrar(SelectEnterEventArgs args)
    {
        if (terminado)
            return;

        mano = args.interactorObject.transform;
        agarrado = true;

        posicionManoInicial =
            transform.parent.InverseTransformPoint(
                mano.position
            );

        direccionInicial =
            Vector3.ProjectOnPlane(
                mano.forward,
                transform.parent.up
            ).normalized;
    }

    private void ActualizarBoton()
    {
        Vector3 actual =
            transform.parent.InverseTransformPoint(
                mano.position
            );

        float presion =
            posicionManoInicial.y -
            actual.y;

        presion =
            Mathf.Clamp(
                presion,
                0f,
                distanciaPresion
            );

        transform.localPosition =
            posicionInicial +
            Vector3.down * presion;


        Vector3 direccionActual =
            Vector3.ProjectOnPlane(
                mano.forward,
                transform.parent.up
            ).normalized;


        float angulo =
            Vector3.SignedAngle(
                direccionInicial,
                direccionActual,
                transform.parent.up
            );

        angulo =
            Mathf.Clamp(
                angulo,
                -giroMaximo,
                giroMaximo
            );


        transform.localRotation =
            rotacionInicial *
            Quaternion.AngleAxis(
                angulo,
                Vector3.up
            );


        if (
            presion >= presionNecesaria &&
            Mathf.Abs(angulo) >= giroNecesario &&
            !activado
        )
        {
            activado = true;
            cuboGirando = true;
        }
    }

    private void Soltar(SelectExitEventArgs args)
    {
        agarrado = false;
        mano = null;

        if (!activado)
            return;

        cuboGirando = false;
        terminado = true;

        grab.enabled = false;

        StartCoroutine(Finalizar());
    }

    private IEnumerator Finalizar()
    {
        yield return Reproducir(voz10);
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