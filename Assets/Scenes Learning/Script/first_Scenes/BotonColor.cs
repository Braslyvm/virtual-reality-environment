using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotonColor : MonoBehaviour
{
    public Renderer cuboRenderer;

    public Color nuevoColor = Color.green;

    public float distanciaPresion = 0.015f;
    public float presionNecesaria = 0.008f;

    public float giroMaximo = 90f;
    public float giroNecesario = 35f;

    public float velocidadRetorno = 8f;

    public AudioSource voz6;
    public AudioSource voz7;

    public XRGrabInteractable siguienteBoton;

    private XRGrabInteractable grab;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private Transform mano;

    private Vector3 posicionManoInicial;
    private Vector3 direccionInicial;

    private bool agarrado;
    private bool activado;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;

        grab.selectEntered.AddListener(Agarrar);
        grab.selectExited.AddListener(Soltar);

        if (siguienteBoton != null)
            siguienteBoton.enabled = false;
    }

    private void LateUpdate()
    {
        if (agarrado && mano != null)
        {
            ActualizarBoton();
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                posicionInicial,
                Time.deltaTime * velocidadRetorno
            );

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
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
        Vector3 posicionActual =
            transform.parent.InverseTransformPoint(
                mano.position
            );

        float presion =
            posicionManoInicial.y -
            posicionActual.y;

        presion = Mathf.Clamp(
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

        angulo = Mathf.Clamp(
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

            CambiarColor();

            StartCoroutine(Finalizar());
        }
    }

    private void CambiarColor()
    {
        if (cuboRenderer == null)
            return;

        Material material = cuboRenderer.material;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", nuevoColor);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", nuevoColor);
    }

    private IEnumerator Finalizar()
    {
        grab.enabled = false;

        yield return Reproducir(voz6);
        yield return Reproducir(voz7);

        if (siguienteBoton != null)
            siguienteBoton.enabled = true;
    }

    private IEnumerator Reproducir(AudioSource audio)
    {
        if (audio == null)
            yield break;

        audio.Stop();
        audio.Play();

        yield return new WaitWhile(() => audio.isPlaying);
    }

    private void Soltar(SelectExitEventArgs args)
    {
        agarrado = false;
        mano = null;
    }
}