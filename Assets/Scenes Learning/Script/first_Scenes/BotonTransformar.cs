using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotonTransformar : MonoBehaviour
{
    public SkinnedMeshRenderer cubo;

    public string blendShape = "Esfera";
    public float duracion = 1.5f;

    public float distanciaPresion = 0.015f;
    public float presionNecesaria = 0.008f;

    public float giroMaximo = 90f;
    public float giroNecesario = 35f;

    public float velocidadRetorno = 8f;

    public AudioSource voz8;
    public AudioSource voz9;

    public XRGrabInteractable siguienteBoton;

    private XRGrabInteractable grab;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private Vector3 posicionManoInicial;
    private Vector3 direccionInicial;

    private Transform mano;

    private int indice;

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

        if (cubo != null)
        {
            indice =
                cubo.sharedMesh.GetBlendShapeIndex(
                    blendShape
                );

            if (indice < 0)
                Debug.LogError(
                    "No se encontró Blend Shape: " +
                    blendShape
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
        Vector3 actual =
            transform.parent.InverseTransformPoint(
                mano.position
            );

        float presion =
            posicionManoInicial.y -
            actual.y;

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

            StartCoroutine(Transformar());
        }
    }

    private IEnumerator Transformar()
    {
        grab.enabled = false;

        if (cubo != null && indice >= 0)
        {
            float inicio =
                cubo.GetBlendShapeWeight(indice);

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