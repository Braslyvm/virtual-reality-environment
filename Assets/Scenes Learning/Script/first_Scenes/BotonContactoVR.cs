
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BotonContactoVR : MonoBehaviour
{
    [Header("Panel")]
    public PanelFlotanteVR panel;

    [Header("Controles VR")]
    public Collider controlIzquierdo;
    public Collider controlDerecho;

    private bool activado = false;

    private void Awake()
    {
        BoxCollider colliderBoton =
            GetComponent<BoxCollider>();

        colliderBoton.isTrigger = true;

        if (panel == null)
        {
            panel = GetComponentInParent<PanelFlotanteVR>();
        }
    }

    private void OnEnable()
    {
        activado = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        bool esControl =
            (controlIzquierdo != null &&
             other == controlIzquierdo) ||
            (controlDerecho != null &&
             other == controlDerecho);

        if (!esControl)
            return;

        if (panel == null)
        {
            Debug.LogError(
                "No se asigno PanelFlotanteVR en " + name
            );
            return;
        }

        bool aceptado = false;

        if (panel.botonSiguiente != null &&
            (transform == panel.botonSiguiente ||
             transform.IsChildOf(panel.botonSiguiente)))
        {
            Debug.Log("CONTACTO CON BOTON SIGUIENTE");

            aceptado = panel.PulsarSiguiente();
        }
        else if (panel.botonReiniciar != null &&
                 (transform == panel.botonReiniciar ||
                  transform.IsChildOf(panel.botonReiniciar)))
        {
            Debug.Log("CONTACTO CON BOTON REINICIAR");

            aceptado = panel.PulsarReiniciar();
        }
        else
        {
            Debug.LogError(
                "El script esta en un objeto que no " +
                "corresponde a ninguno de los botones: " +
                gameObject.name
            );
        }

        if (aceptado)
        {
            activado = true;

            Debug.Log(
                "Boton activado correctamente: " +
                gameObject.name
            );
        }
    }
}
