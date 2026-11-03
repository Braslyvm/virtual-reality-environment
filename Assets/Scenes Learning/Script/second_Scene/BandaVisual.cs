
using UnityEngine;

public class BandaVisual : MonoBehaviour
{
    [Header("Controlador")]
    public GestorEstacion3 gestor;

    [Header("Superficie de la banda")]
    public Renderer superficie;

    [Header("Movimiento de la textura")]
    public Vector2 direccionTextura =
        new Vector2(0, 1);

    public float velocidadTextura = 0.25f;

    [Header("Rodillos opcionales")]
    public Transform[] rodillos;

    public Vector3 ejeRodillos = Vector3.right;
    public float velocidadRodillos = 180f;

    private Material materialBanda;
    private string propiedadTextura;
    private Vector2 desplazamiento;

    private void Start()
    {
        if (superficie == null)
            return;

        materialBanda = superficie.material;

        if (materialBanda.HasProperty("_BaseMap"))
        {
            propiedadTextura = "_BaseMap";
        }
        else if (materialBanda.HasProperty("_MainTex"))
        {
            propiedadTextura = "_MainTex";
        }
    }

    private void Update()
    {
        if (gestor == null || !gestor.BandaEnMovimiento)
            return;

        // Animacion de textura.
        if (materialBanda != null &&
            !string.IsNullOrEmpty(propiedadTextura))
        {
            desplazamiento +=
                direccionTextura *
                velocidadTextura *
                Time.deltaTime;

            materialBanda.SetTextureOffset(
                propiedadTextura,
                desplazamiento
            );
        }

        // Animacion de rodillos.
        if (rodillos != null)
        {
            foreach (Transform rodillo in rodillos)
            {
                if (rodillo == null)
                    continue;

                rodillo.Rotate(
                    ejeRodillos,
                    velocidadRodillos * Time.deltaTime,
                    Space.Self
                );
            }
        }
    }
}
