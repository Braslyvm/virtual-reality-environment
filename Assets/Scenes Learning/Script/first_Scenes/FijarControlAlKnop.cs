using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FijarControlAlKnop : MonoBehaviour
{
    [Header("Controles")]
    public Transform leftControllerVisual;
    public Transform rightControllerVisual;

    [Header("Suavizado")]
    public float suavidadPosicion = 20f;
    public float suavidadRotacion = 12f;

    private XRSimpleInteractable interactable;

    private Transform visualActual;

    private Vector3 posicionLocalInicial;
    private Quaternion rotacionLocalInicial;

    private Vector3 offsetPosicion;
    private Quaternion offsetRotacion;

    private bool agarrado;


    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        interactable.selectEntered.AddListener(Agarrar);
        interactable.selectExited.AddListener(Soltar);
    }


    private void LateUpdate()
    {
        if (!agarrado || visualActual == null)
            return;


        Vector3 posicionObjetivo =
            transform.TransformPoint(
                offsetPosicion
            );


        Quaternion rotacionObjetivo =
            transform.rotation *
            offsetRotacion;


        float tPosicion =
            1f - Mathf.Exp(
                -suavidadPosicion *
                Time.deltaTime
            );


        float tRotacion =
            1f - Mathf.Exp(
                -suavidadRotacion *
                Time.deltaTime
            );


        visualActual.position =
            Vector3.Lerp(
                visualActual.position,
                posicionObjetivo,
                tPosicion
            );


        visualActual.rotation =
            Quaternion.Slerp(
                visualActual.rotation,
                rotacionObjetivo,
                tRotacion
            );
    }


    private void Agarrar(SelectEnterEventArgs args)
    {
        Transform interactor =
            args.interactorObject.transform;


        if (
            leftControllerVisual != null &&
            interactor.IsChildOf(
                leftControllerVisual.parent
            )
        )
        {
            visualActual =
                leftControllerVisual;
        }
        else if (
            rightControllerVisual != null &&
            interactor.IsChildOf(
                rightControllerVisual.parent
            )
        )
        {
            visualActual =
                rightControllerVisual;
        }
        else
        {
            Debug.LogWarning(
                "No se pudo detectar qué control agarró el KNOP."
            );

            return;
        }


        posicionLocalInicial =
            visualActual.localPosition;

        rotacionLocalInicial =
            visualActual.localRotation;


        offsetPosicion =
            transform.InverseTransformPoint(
                visualActual.position
            );


        offsetRotacion =
        offsetRotacion =
            Quaternion.Inverse(
                transform.rotation
            ) *
            visualActual.rotation;


        agarrado = true;
    }


    private void Soltar(SelectExitEventArgs args)
    {
        agarrado = false;


        if (visualActual == null)
            return;


        visualActual.localPosition =
            posicionLocalInicial;

        visualActual.localRotation =
            rotacionLocalInicial;


        visualActual = null;
    }
}