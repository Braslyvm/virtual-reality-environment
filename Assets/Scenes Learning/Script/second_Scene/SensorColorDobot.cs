
using UnityEngine;

public class SensorColorDobot : MonoBehaviour
{
    [Header("Color que debe detectar")]
    public TipoCubo colorDetectado;

    [Header("Controlador de la estacion")]
    public GestorEstacion3 gestor;

    [Header("Brazo robotico asignado")]
    public DobotBrazo brazoAsignado;

    private void OnTriggerEnter(Collider other)
    {
        RevisarCubo(other);
    }

    private void OnTriggerStay(Collider other)
    {
        RevisarCubo(other);
    }

    private void RevisarCubo(Collider other)
    {
        CuboEnBanda cubo =
            other.GetComponentInParent<CuboEnBanda>();

        if (cubo == null || gestor == null)
            return;

        if (!cubo.EstaEnBanda)
            return;

        if (cubo.Tipo != colorDetectado)
            return;

        gestor.IntentarRecoger(
            cubo,
            brazoAsignado,
            colorDetectado
        );
    }
}
