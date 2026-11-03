using UnityEngine;

public enum TipoCubo
{
    Verde,
    Rojo,
    Azul
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class CuboEnBanda : MonoBehaviour
{
    [Header("Configuracion de la banda")]
    public string etiquetaBanda = "Banda";

    [Header("Configuracion de la caida")]
    public float gravedadCaida = 3f;

    public TipoCubo Tipo { get; private set; }

    public bool EstaEnBanda { get; private set; } = false;

    private GestorEstacion3 gestor;

    private Rigidbody rb;
    private Collider colision;

    private Transform destino;

    private Vector3 direccion;

    private float velocidad;
    private float velocidadCaida;

    private bool cayendo = false;
    private bool finalizado = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colision = GetComponent<Collider>();

        ConfigurarMovimientoControlado();
    }

    private void ConfigurarMovimientoControlado()
    {
        // Eliminar cualquier velocidad inicial.
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Evitar impulsos producidos por colisiones.
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;

        rb.isKinematic = true;
        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Inicializar(
        GestorEstacion3 nuevoGestor,
        TipoCubo nuevoTipo,
        Transform puntoFinal,
        float nuevaVelocidad)
    {
        gestor = nuevoGestor;
        Tipo = nuevoTipo;
        destino = puntoFinal;
        velocidad = nuevaVelocidad;

        cayendo = true;
        finalizado = false;
        EstaEnBanda = false;

        velocidadCaida = 0f;

        ConfigurarMovimientoControlado();

        direccion = destino.position - transform.position;
        direccion.y = 0f;
        direccion.Normalize();

        // ASIGNAR COLOR
        Renderer renderizador =
            GetComponentInChildren<Renderer>();

        if (renderizador != null)
        {
            switch (Tipo)
            {
                case TipoCubo.Verde:
                    renderizador.material.color = Color.green;
                    break;

                case TipoCubo.Rojo:
                    renderizador.material.color = Color.red;
                    break;

                case TipoCubo.Azul:
                    renderizador.material.color = Color.blue;
                    break;
            }
        }

        Debug.Log(
            "Cubo " + Tipo + " generado. Iniciando caida."
        );
    }

    private void FixedUpdate()
    {
        if (gestor == null || destino == null || finalizado)
            return;

        // PRIMERA ETAPA: CAIDA VERTICAL
        if (cayendo)
        {
            CaerHaciaBanda();
            return;
        }

        // SEGUNDA ETAPA: TRANSPORTE
        if (EstaEnBanda)
        {
            MoverSobreBanda();
        }
    }

    private void CaerHaciaBanda()
    {
        // Aumentar progresivamente la velocidad de caida.
        velocidadCaida += gravedadCaida * Time.fixedDeltaTime;

        float desplazamiento =
            velocidadCaida * Time.fixedDeltaTime;

        Vector3 nuevaPosicion =
            rb.position + Vector3.down * desplazamiento;

        // Calcular la mitad de la altura del cubo.
        float mitadAltura = colision.bounds.extents.y;

        // Buscar la superficie de la banda debajo del cubo.
        Vector3 origenRayo =
            rb.position + Vector3.up * (mitadAltura + 0.02f);

        float distanciaRayo =
            mitadAltura * 2f + desplazamiento + 0.05f;

        RaycastHit[] impactos = Physics.RaycastAll(
            origenRayo,
            Vector3.down,
            distanciaRayo,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        bool encontroBanda = false;

        float alturaBanda = float.NegativeInfinity;

        foreach (RaycastHit impacto in impactos)
        {
            if (impacto.collider == colision)
                continue;

            if (!impacto.collider.CompareTag(etiquetaBanda))
                continue;

            if (impacto.point.y > alturaBanda)
            {
                alturaBanda = impacto.point.y;
                encontroBanda = true;
            }
        }

        // COMPROBAR SI EL CUBO LLEGO A LA BANDA
        if (encontroBanda &&
            nuevaPosicion.y - mitadAltura <= alturaBanda)
        {
            Vector3 posicionFinal = new Vector3(
                rb.position.x,
                alturaBanda + mitadAltura + 0.002f,
                rb.position.z
            );

            rb.MovePosition(posicionFinal);

            cayendo = false;
            EstaEnBanda = true;

            velocidadCaida = 0f;

            Debug.Log(
                "Cubo " + Tipo +
                " aterrizo en la banda transportadora."
            );

            return;
        }

        // Continuar cayendo verticalmente.
        rb.MovePosition(nuevaPosicion);
    }

    private void MoverSobreBanda()
    {
        if (!gestor.BandaEnMovimiento)
            return;

        Vector3 movimiento =
            direccion * velocidad * Time.fixedDeltaTime;

        Vector3 nuevaPosicion =
            rb.position + movimiento;

        rb.MovePosition(nuevaPosicion);

        // VERIFICAR SI LLEGO AL DESCARTE
        float distanciaFinal = Vector3.Dot(
            nuevaPosicion - destino.position,
            direccion
        );

        if (distanciaFinal >= 0f)
        {
            Debug.Log(
                "Cubo " + Tipo + " enviado a descarte."
            );

            MarcarFinalizado();

            Destroy(gameObject);
        }
    }

    // DETENER EL CUBO PARA RECOGERLO
    public void DetenerParaRecoger()
    {
        EstaEnBanda = false;
        cayendo = false;

        Debug.Log("Cubo detenido para el DOBOT.");
    }

    // PEGAR EL CUBO A LA PINZA
    public void PegarAPinza(Transform puntoAgarre)
    {
        EstaEnBanda = false;
        cayendo = false;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;

        rb.isKinematic = true;
        rb.useGravity = false;

        colision.enabled = false;

        transform.SetParent(puntoAgarre, true);
        transform.position = puntoAgarre.position;

        Debug.Log("DOBOT recogio el cubo.");
    }

    // SOLTAR EN LA CAJA DE CLASIFICACION
    public void SoltarEn(Transform puntoSoltar)
    {
        EstaEnBanda = false;
        cayendo = false;

        transform.SetParent(null, true);
        transform.position = puntoSoltar.position;

        colision.enabled = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Discrete;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        Debug.Log("DOBOT solto el cubo.");
    }

    // NOTIFICAR FINALIZACION
    public void MarcarFinalizado()
    {
        if (finalizado)
            return;

        finalizado = true;

        EstaEnBanda = false;
        cayendo = false;

        if (gestor != null)
        {
            gestor.NotificarCuboFinalizado();
        }
    }
}