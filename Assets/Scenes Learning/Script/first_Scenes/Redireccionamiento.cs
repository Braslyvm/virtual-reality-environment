
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ReiniciarCubo : MonoBehaviour
{
    [Header("Objetos")]
    public Transform cuboPequeno;
    public GameObject trigger;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private Rigidbody rb;
    private XRGrabInteractable grab;
    private BoxCollider limite;

    // Configura el cubo y guarda su posición y rotación inicial.
    private void Start()
    {
        if (cuboPequeno == null)
            cuboPequeno = transform;

        if (trigger == null)
        {
            Debug.LogError("Asigna el Trigger en el Inspector.");
            enabled = false;
            return;
        }

        posicionInicial = cuboPequeno.position;
        rotacionInicial = cuboPequeno.rotation;

        rb = cuboPequeno.GetComponent<Rigidbody>();
        grab = cuboPequeno.GetComponent<XRGrabInteractable>();

        limite = trigger.GetComponent<BoxCollider>();

        if (limite == null)
        {
            Debug.LogError("El Trigger necesita un Box Collider.");
            enabled = false;
        }
    }

    // Reinicia el cubo si sale del área mientras no está siendo agarrado.
    private void FixedUpdate()
    {
        if (cuboPequeno == null)
            return;

        if (grab != null && grab.isSelected)
            return;

        if (!EstaDentroDelTrigger())
        {
            Reiniciar();
        }
    }

    private bool EstaDentroDelTrigger()
    {
        Vector3 posicionLocal =
            limite.transform.InverseTransformPoint(
                cuboPequeno.position
            );

        Vector3 diferencia = posicionLocal - limite.center;
        Vector3 mitad = limite.size * 0.5f;

        return
            Mathf.Abs(diferencia.x) <= mitad.x &&
            Mathf.Abs(diferencia.y) <= mitad.y &&
            Mathf.Abs(diferencia.z) <= mitad.z;
    }

    // Detiene las físicas y restaura la posición inicial del cubo.
    private void Reiniciar()
    {
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;

            rb.position = posicionInicial;
            rb.rotation = rotacionInicial;
        }
        else
        {
            cuboPequeno.SetPositionAndRotation(
                posicionInicial,
                rotacionInicial
            );
        }

        Debug.Log("Cubo reiniciado: " + cuboPequeno.name);
    }
}
