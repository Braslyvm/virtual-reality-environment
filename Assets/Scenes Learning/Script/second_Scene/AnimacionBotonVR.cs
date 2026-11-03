
using UnityEngine;

public class AnimacionBotonVR : MonoBehaviour
{
    [Header("Parte del boton que se movera")]
    public Transform botonVisual;

    [Header("Configuracion de la animacion")]
    public float profundidad = 0.01f;
    public float velocidad = 0.08f;

    [Header("Direccion del hundimiento")]
    public Vector3 direccion = Vector3.down;

    private Vector3 posicionOriginal;
    private Vector3 posicionHundida;

    private bool hundido = false;

    private void Awake()
    {
        if (botonVisual == null)
        {
            Debug.LogWarning("No se asigno el boton visual.");
            return;
        }

        posicionOriginal = botonVisual.localPosition;

        posicionHundida = posicionOriginal +
            direccion.normalized * profundidad;
    }

    private void Update()
    {
        if (botonVisual == null)
            return;

        Vector3 destino;

        if (hundido)
        {
            destino = posicionHundida;
        }
        else
        {
            destino = posicionOriginal;
        }

        botonVisual.localPosition = Vector3.MoveTowards(
            botonVisual.localPosition,
            destino,
            velocidad * Time.deltaTime
        );
    }
    public void Hundirse()
    {
        hundido = true;

        Debug.Log("Boton hundiendose.");
    }
    public void Regresar()
    {
        hundido = false;

        Debug.Log("Boton regresando.");
    }

    private void OnDisable()
    {
        hundido = false;

        if (botonVisual != null)
        {
            botonVisual.localPosition = posicionOriginal;
        }
    }
}
