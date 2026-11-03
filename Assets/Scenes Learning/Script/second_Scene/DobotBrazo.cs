
using System.Collections;
using UnityEngine;

public class DobotBrazo : MonoBehaviour
{
    [Header("Articulaciones del DOBOT")]
    public Transform j1;
    public Transform j2;
    public Transform j3;
    public Transform j4;

    [Header("Dedos de la pinza")]
    public Transform dedoA;
    public Transform dedoB;

    [Header("Puntos de trabajo")]
    public Transform puntoAgarre;
    public Transform puntoSoltar;

    [Header("Ejes locales de rotacion")]
    public Vector3 ejeJ1 = Vector3.forward;
    public Vector3 ejeJ2 = Vector3.right;
    public Vector3 ejeJ3 = Vector3.right;
    public Vector3 ejeJ4 = Vector3.forward;

    [Header("Configuracion de la pinza")]
    public Vector3 ejeDedos = Vector3.right;

    public float anguloApertura = 25f;

    // La pinza ya no cierra completamente a cero.
    public float anguloCierre = 12f;

    public float duracionPinza = 0.7f;
    public float pausaAntesCerrar = 0.4f;
    public float pausaAgarre = 0.3f;

    [Header("Velocidad del robot")]
    public float duracionMovimiento = 1.6f;
    public float duracionAproximacion = 2f;

    [Header("Posturas del robot")]
    public Vector4 reposo = Vector4.zero;

    public Vector4 sobreSensor =
        new Vector4(30, 20, -35, 0);

    public Vector4 tomar =
        new Vector4(30, 45, -60, 0);

    public Vector4 sobreCaja =
        new Vector4(-45, 20, -35, 0);

    public Vector4 entregar =
        new Vector4(-45, 45, -60, 0);

    [Header("Ajuste automatico de la pinza")]
    public bool usarAjusteAutomatico = true;

    public float distanciaMaximaAgarre = 0.018f;
    public float distanciaMaximaSoltar = 0.08f;

    public float tiempoMaximoAjuste = 10f;
    public float velocidadAjuste = 35f;

    [Range(1, 12)]
    public int iteracionesPorFrame = 5;

    [Header("Acercamiento al cubo")]
    public float alturaPreAgarre = 0.06f;

    [Header("Limites de articulaciones")]
    public float limiteJ1 = 140f;
    public float limiteJ2 = 90f;
    public float limiteJ3 = 120f;

    [Header("Seguridad del agarre")]
    public bool mantenerCuboAlineado = true;

    [Header("Guia visual del cubo")]
    public float tamanoCuboGuia = 0.05f;

    private CuboEnBanda cuboEnPinza;

    private Quaternion inicioJ1;
    private Quaternion inicioJ2;
    private Quaternion inicioJ3;
    private Quaternion inicioJ4;

    private Quaternion inicioDedoA;
    private Quaternion inicioDedoB;

    private float anguloActualJ1;
    private float anguloActualJ2;
    private float anguloActualJ3;
    private float anguloActualJ4;

    public bool UltimaRecogidaExitosa { get; private set; }

    public bool Configurado =>
        j1 != null &&
        j2 != null &&
        j3 != null &&
        j4 != null &&
        puntoAgarre != null &&
        puntoSoltar != null;

    private void Awake()
    {
        if (!Configurado)
        {
            Debug.LogError(
                "Faltan referencias del DOBOT: " +
                gameObject.name
            );
            return;
        }

        inicioJ1 = j1.localRotation;
        inicioJ2 = j2.localRotation;
        inicioJ3 = j3.localRotation;
        inicioJ4 = j4.localRotation;

        if (dedoA != null)
            inicioDedoA = dedoA.localRotation;

        if (dedoB != null)
            inicioDedoB = dedoB.localRotation;

        AplicarPostura(reposo);
        AbrirPinza(true);
    }

    // DIBUJAR UNA GUIA DEL TAMANO DEL CUBO
    private void OnDrawGizmosSelected()
    {
        if (puntoAgarre == null)
            return;

        Matrix4x4 matrizAnterior = Gizmos.matrix;

        Gizmos.color = Color.cyan;

        Gizmos.matrix = Matrix4x4.TRS(
            puntoAgarre.position,
            puntoAgarre.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            Vector3.one * tamanoCuboGuia
        );

        Gizmos.matrix = matrizAnterior;
    }

    // MANTENER EL CUBO SUJETO A LA PINZA
    private void LateUpdate()
    {
        if (!mantenerCuboAlineado ||
            cuboEnPinza == null ||
            puntoAgarre == null)
        {
            return;
        }

        Transform objeto = cuboEnPinza.transform;

        if (objeto.parent != puntoAgarre)
        {
            objeto.SetParent(puntoAgarre, true);
        }

        objeto.SetPositionAndRotation(
            puntoAgarre.position,
            puntoAgarre.rotation
        );

        Rigidbody cuerpo =
            cuboEnPinza.GetComponent<Rigidbody>();

        if (cuerpo != null && !cuerpo.isKinematic)
        {
            cuerpo.collisionDetectionMode =
                CollisionDetectionMode.Discrete;

            cuerpo.isKinematic = true;
            cuerpo.useGravity = false;
        }
    }

    // SECUENCIA PRINCIPAL
    public IEnumerator RecogerYDepositar(CuboEnBanda cubo)
    {
        UltimaRecogidaExitosa = false;
        cuboEnPinza = null;

        if (!Configurado || cubo == null)
            yield break;

        Debug.Log("DOBOT iniciando clasificacion.");

        // 1. ABRIR COMPLETAMENTE LOS DEDOS
        yield return AnimarPinza(true, duracionPinza);

        // 2. GIRAR HACIA EL SENSOR
        yield return MoverHasta(sobreSensor);

        if (cubo == null)
            yield break;

        if (usarAjusteAutomatico)
        {
            // 3. POSICIONARSE PRIMERO SOBRE EL CUBO
            Vector3 posicionSuperior =
                cubo.transform.position +
                Vector3.up * alturaPreAgarre;

            yield return AjustarPinzaHasta(
                posicionSuperior,
                distanciaMaximaAgarre
            );

            if (cubo == null)
                yield break;

            // 4. BAJAR HACIA EL CENTRO DEL CUBO
            yield return AjustarPinzaHasta(
                cubo.transform.position,
                distanciaMaximaAgarre
            );
        }
        else
        {
            // MODO MANUAL CON POSTURAS
            yield return MoverHasta(
                tomar,
                duracionAproximacion
            );
        }

        if (cubo == null)
            yield break;

        // 5. VERIFICAR QUE LLEGO CORRECTAMENTE
        float distanciaAgarre = Vector3.Distance(
            puntoAgarre.position,
            cubo.transform.position
        );

        if (distanciaAgarre > distanciaMaximaAgarre)
        {
            Debug.LogWarning(
                "DOBOT no alcanzo el cubo. Distancia: " +
                distanciaAgarre.ToString("F3")
            );

            yield return MoverHasta(reposo);
            yield break;
        }

        // 6. MANTENER LA PINZA ABIERTA UN MOMENTO
        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaAntesCerrar)
        );

        // 7. CERRAR SIN APLASTAR EL CUBO
        yield return AnimarPinza(false, duracionPinza);

        if (cubo == null)
            yield break;

        // 8. SUJETAR EL CUBO
        cubo.PegarAPinza(puntoAgarre);

        cuboEnPinza = cubo;

        if (mantenerCuboAlineado)
        {
            cubo.transform.SetParent(puntoAgarre, true);

            cubo.transform.SetPositionAndRotation(
                puntoAgarre.position,
                puntoAgarre.rotation
            );
        }

        Debug.Log("Cubo sujeto correctamente.");

        yield return new WaitForSeconds(
            Mathf.Max(0f, pausaAgarre)
        );

        // 9. LEVANTAR EL CUBO
        yield return MoverHasta(sobreSensor);

        // 10. GIRAR HACIA LA ZONA DE ENTREGA
        yield return MoverHasta(sobreCaja);

        // 11. BAJAR LENTAMENTE
        yield return MoverHasta(
            entregar,
            duracionAproximacion
        );

        // 12. AJUSTAR PUNTO DE ENTREGA
        if (usarAjusteAutomatico)
        {
            yield return AjustarPinzaHasta(
                puntoSoltar.position,
                distanciaMaximaSoltar
            );
        }

        float distanciaEntrega = Vector3.Distance(
            puntoAgarre.position,
            puntoSoltar.position
        );

        if (distanciaEntrega > distanciaMaximaSoltar)
        {
            Debug.LogWarning(
                "El DOBOT no alcanzo exactamente " +
                "el punto de entrega. Distancia: " +
                distanciaEntrega.ToString("F3")
            );
        }

        // 13. ABRIR LOS DEDOS SUAVEMENTE
        yield return AnimarPinza(true, duracionPinza);

        if (cubo == null)
        {
            cuboEnPinza = null;
            yield break;
        }

        // 14. SOLTAR DESDE LA POSICION REAL
        cuboEnPinza = null;

        cubo.SoltarEn(puntoAgarre);

        Rigidbody cuerpoLiberado =
            cubo.GetComponent<Rigidbody>();

        if (cuerpoLiberado != null &&
            !cuerpoLiberado.isKinematic)
        {
            cuerpoLiberado.linearVelocity = Vector3.zero;
            cuerpoLiberado.angularVelocity = Vector3.zero;
        }

        UltimaRecogidaExitosa = true;

        Debug.Log("Cubo liberado.");

        yield return new WaitForSeconds(0.3f);

        // 15. LEVANTAR EL BRAZO
        yield return MoverHasta(sobreCaja);

        // 16. REGRESAR A REPOSO
        yield return MoverHasta(reposo);
    }

    // AJUSTE AUTOMATICO DE LA PINZA
    private IEnumerator AjustarPinzaHasta(
        Vector3 objetivo,
        float tolerancia)
    {
        float tiempo = 0f;

        int iteraciones =
            Mathf.Max(1, iteracionesPorFrame);

        while (tiempo < tiempoMaximoAjuste)
        {
            float distancia = Vector3.Distance(
                puntoAgarre.position,
                objetivo
            );

            if (distancia <= tolerancia)
                yield break;

            float pasoMaximo =
                velocidadAjuste *
                Time.deltaTime /
                iteraciones;

            for (int i = 0; i < iteraciones; i++)
            {
                AjustarArticulacion(
                    j3,
                    inicioJ3,
                    ejeJ3,
                    objetivo,
                    ref anguloActualJ3,
                    limiteJ3,
                    pasoMaximo
                );

                AjustarArticulacion(
                    j2,
                    inicioJ2,
                    ejeJ2,
                    objetivo,
                    ref anguloActualJ2,
                    limiteJ2,
                    pasoMaximo
                );

                AjustarArticulacion(
                    j1,
                    inicioJ1,
                    ejeJ1,
                    objetivo,
                    ref anguloActualJ1,
                    limiteJ1,
                    pasoMaximo
                );
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning(
            "Ajuste terminado. Distancia restante: " +
            Vector3.Distance(
                puntoAgarre.position,
                objetivo
            ).ToString("F3")
        );
    }

    // CORREGIR UNA ARTICULACION
    private void AjustarArticulacion(
        Transform articulacion,
        Quaternion rotacionInicial,
        Vector3 ejeLocal,
        Vector3 objetivo,
        ref float anguloActual,
        float limite,
        float pasoMaximo)
    {
        if (articulacion == null ||
            ejeLocal.sqrMagnitude < 0.001f)
        {
            return;
        }

        Vector3 ejeMundo =
            articulacion.TransformDirection(
                ejeLocal.normalized
            );

        Vector3 haciaPinza =
            puntoAgarre.position -
            articulacion.position;

        Vector3 haciaObjetivo =
            objetivo -
            articulacion.position;

        haciaPinza = Vector3.ProjectOnPlane(
            haciaPinza,
            ejeMundo
        );

        haciaObjetivo = Vector3.ProjectOnPlane(
            haciaObjetivo,
            ejeMundo
        );

        if (haciaPinza.sqrMagnitude < 0.000001f ||
            haciaObjetivo.sqrMagnitude < 0.000001f)
        {
            return;
        }

        float correccion = Vector3.SignedAngle(
            haciaPinza,
            haciaObjetivo,
            ejeMundo
        );

        correccion = Mathf.Clamp(
            correccion,
            -pasoMaximo,
            pasoMaximo
        );

        anguloActual = Mathf.Clamp(
            anguloActual + correccion,
            -Mathf.Abs(limite),
            Mathf.Abs(limite)
        );

        articulacion.localRotation = CalcularRotacion(
            rotacionInicial,
            anguloActual,
            ejeLocal
        );
    }

    private Quaternion CalcularRotacion(
        Quaternion original,
        float angulo,
        Vector3 eje)
    {
        if (eje.sqrMagnitude < 0.001f)
            eje = Vector3.forward;

        return original *
            Quaternion.AngleAxis(
                angulo,
                eje.normalized
            );
    }

    // APLICAR POSTURA
    private void AplicarPostura(Vector4 postura)
    {
        j1.localRotation = CalcularRotacion(
            inicioJ1, postura.x, ejeJ1
        );

        j2.localRotation = CalcularRotacion(
            inicioJ2, postura.y, ejeJ2
        );

        j3.localRotation = CalcularRotacion(
            inicioJ3, postura.z, ejeJ3
        );

        j4.localRotation = CalcularRotacion(
            inicioJ4, postura.w, ejeJ4
        );

        anguloActualJ1 = postura.x;
        anguloActualJ2 = postura.y;
        anguloActualJ3 = postura.z;
        anguloActualJ4 = postura.w;
    }

    // MOVIMIENTO SUAVE
    private IEnumerator MoverHasta(
        Vector4 postura,
        float duracion = -1f)
    {
        Quaternion desdeJ1 = j1.localRotation;
        Quaternion desdeJ2 = j2.localRotation;
        Quaternion desdeJ3 = j3.localRotation;
        Quaternion desdeJ4 = j4.localRotation;

        Quaternion hastaJ1 = CalcularRotacion(
            inicioJ1, postura.x, ejeJ1
        );

        Quaternion hastaJ2 = CalcularRotacion(
            inicioJ2, postura.y, ejeJ2
        );

        Quaternion hastaJ3 = CalcularRotacion(
            inicioJ3, postura.z, ejeJ3
        );

        Quaternion hastaJ4 = CalcularRotacion(
            inicioJ4, postura.w, ejeJ4
        );

        float duracionReal = Mathf.Max(
            0.01f,
            duracion > 0f
                ? duracion
                : duracionMovimiento
        );

        float tiempo = 0f;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime / duracionReal;

            float suavizado = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(tiempo)
            );

            j1.localRotation = Quaternion.Slerp(
                desdeJ1,
                hastaJ1,
                suavizado
            );

            j2.localRotation = Quaternion.Slerp(
                desdeJ2,
                hastaJ2,
                suavizado
            );

            j3.localRotation = Quaternion.Slerp(
                desdeJ3,
                hastaJ3,
                suavizado
            );

            j4.localRotation = Quaternion.Slerp(
                desdeJ4,
                hastaJ4,
                suavizado
            );

            yield return null;
        }

        AplicarPostura(postura);
    }

    // APERTURA INMEDIATA
    private void AbrirPinza(bool abierta)
    {
        Vector3 eje =
            ejeDedos.sqrMagnitude > 0.001f
                ? ejeDedos.normalized
                : Vector3.right;

        float apertura = Mathf.Max(
            anguloApertura,
            anguloCierre
        );

        float angulo = abierta
            ? apertura
            : Mathf.Clamp(anguloCierre, 0f, apertura);

        if (dedoA != null)
        {
            dedoA.localRotation =
                inicioDedoA *
                Quaternion.AngleAxis(
                    angulo,
                    eje
                );
        }

        if (dedoB != null)
        {
            dedoB.localRotation =
                inicioDedoB *
                Quaternion.AngleAxis(
                    -angulo,
                    eje
                );
        }
    }

    // ANIMACION DE LOS DEDOS
    private IEnumerator AnimarPinza(
        bool abierta,
        float duracion)
    {
        Vector3 eje =
            ejeDedos.sqrMagnitude > 0.001f
                ? ejeDedos.normalized
                : Vector3.right;

        float apertura = Mathf.Max(
            anguloApertura,
            anguloCierre
        );

        float angulo = abierta
            ? apertura
            : Mathf.Clamp(anguloCierre, 0f, apertura);

        Quaternion desdeA =
            dedoA != null
                ? dedoA.localRotation
                : Quaternion.identity;

        Quaternion desdeB =
            dedoB != null
                ? dedoB.localRotation
                : Quaternion.identity;

        Quaternion hastaA =
            inicioDedoA *
            Quaternion.AngleAxis(
                angulo,
                eje
            );

        Quaternion hastaB =
            inicioDedoB *
            Quaternion.AngleAxis(
                -angulo,
                eje
            );

        duracion = Mathf.Max(
            0.01f,
            duracion
        );

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(
                    tiempo / duracion
                )
            );

            if (dedoA != null)
            {
                dedoA.localRotation =
                    Quaternion.Slerp(
                        desdeA,
                        hastaA,
                        t
                    );
            }

            if (dedoB != null)
            {
                dedoB.localRotation =
                    Quaternion.Slerp(
                        desdeB,
                        hastaB,
                        t
                    );
            }

            yield return null;
        }

        AbrirPinza(abierta);
    }
}
