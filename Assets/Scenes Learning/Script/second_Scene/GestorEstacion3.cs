
using System.Collections;
using UnityEngine;

public class GestorEstacion3 : MonoBehaviour
{
    [Header("Configuracion de la banda")]
    [Range(1, 10)]
    public int maximoCubos = 10;

    public float velocidadBanda = 0.35f;
    public float tiempoEntreCubos = 2f;

    [Header("Dispensador")]
    public CuboEnBanda prefabCubo;
    public Transform puntoGeneracion;
    public Transform puntoDescarte;

    [Header("Narraciones")]
    public AudioSource voz3;
    public AudioSource voz4;

    private bool bandaEncendida = false;
    private bool robotTrabajando = false;
    private bool dispensadorIniciado = false;
    private bool simulacionFinalizada = false;

    private int cubosGenerados = 0;
    private int cubosFinalizados = 0;

    // INDICA SI LA BANDA PUEDE MOVERSE
    public bool BandaEnMovimiento =>
        bandaEncendida &&
        !robotTrabajando &&
        !simulacionFinalizada;

    // BOTON ROJO: ENCENDER LA BANDA
    public void EncenderBanda()
    {
        if (simulacionFinalizada)
            return;

        if (bandaEncendida)
            return;

        bandaEncendida = true;

        Debug.Log(
            "Banda transportadora encendida."
        );
    }

    // BOTON VERDE: INICIAR DISPENSADOR
    public void IniciarDispensador()
    {
        if (!bandaEncendida)
        {
            Debug.LogWarning(
                "Primero debe encender la banda."
            );

            return;
        }

        if (dispensadorIniciado || simulacionFinalizada)
            return;

        if (prefabCubo == null ||
            puntoGeneracion == null ||
            puntoDescarte == null)
        {
            Debug.LogError(
                "Faltan referencias para generar los cubos."
            );

            return;
        }

        dispensadorIniciado = true;

        Debug.Log(
            "Dispensador iniciado."
        );

        // REPRODUCIR VOZ #3
        ReproducirVoz(voz3);

        // GENERAR CUBOS
        StartCoroutine(GenerarCubos());
    }

    // GENERACION DE CUBOS ALEATORIOS
    private IEnumerator GenerarCubos()
    {
        for (int i = 0; i < maximoCubos; i++)
        {
            // ESPERAR SI LA BANDA ESTA DETENIDA
            while (!BandaEnMovimiento)
            {
                if (simulacionFinalizada)
                    yield break;

                yield return null;
            }

            // ESCOGER COLOR ALEATORIO
            TipoCubo tipo =
                (TipoCubo)Random.Range(0, 3);

            // GENERAR CUBO
            CuboEnBanda cubo = Instantiate(
                prefabCubo,
                puntoGeneracion.position,
                puntoGeneracion.rotation
            );

            // CONFIGURAR CUBO
            cubo.Inicializar(
                this,
                tipo,
                puntoDescarte,
                velocidadBanda
            );

            cubosGenerados++;

            Debug.Log(
                "Cubo " + cubosGenerados +
                " generado. Color: " + tipo
            );

            // ESPERAR ANTES DEL SIGUIENTE CUBO
            float tiempo = 0f;

            while (tiempo < tiempoEntreCubos)
            {
                if (simulacionFinalizada)
                    yield break;

                if (BandaEnMovimiento)
                {
                    tiempo += Time.deltaTime;
                }

                yield return null;
            }
        }

        Debug.Log(
            "El dispensador termino de generar los cubos."
        );
    }

    // LLAMADO POR LOS SENSORES
    public void IntentarRecoger(
        CuboEnBanda cubo,
        DobotBrazo brazo,
        TipoCubo colorSensor)
    {
        // VERIFICAR QUE LA BANDA ESTE FUNCIONANDO
        if (!BandaEnMovimiento)
            return;

        if (cubo == null || !cubo.EstaEnBanda)
            return;

        // VERIFICAR COLOR
        if (cubo.Tipo != colorSensor)
            return;

        // VERIFICAR ROBOT
        if (brazo == null)
        {
            Debug.LogError(
                "El sensor no tiene un DOBOT asignado."
            );

            return;
        }

        if (!brazo.Configurado)
        {
            Debug.LogError(
                "El DOBOT no tiene todas sus referencias configuradas."
            );

            return;
        }

        // DETENER LA BANDA
        robotTrabajando = true;

        // DETENER EL CUBO
        cubo.DetenerParaRecoger();

        Debug.Log(
            "Sensor detecto cubo " + cubo.Tipo
        );

        Debug.Log(
            "Banda detenida. DOBOT trabajando."
        );

        // INICIAR CLASIFICACION
        StartCoroutine(
            ClasificarCubo(cubo, brazo)
        );
    }

    // CLASIFICAR CUBO CON EL DOBOT
    private IEnumerator ClasificarCubo(
        CuboEnBanda cubo,
        DobotBrazo brazo)
    {
        if (cubo == null || brazo == null)
        {
            robotTrabajando = false;
            yield break;
        }

        // ESPERAR HASTA QUE TERMINE EL DOBOT
        yield return StartCoroutine(
            brazo.RecogerYDepositar(cubo)
        );

        // COMPROBAR SI EL CUBO SIGUE EXISTIENDO
        if (cubo != null)
        {
            // CLASIFICACION CORRECTA
            if (brazo.UltimaRecogidaExitosa)
            {
                Debug.Log(
                    "DOBOT clasifico correctamente el cubo " +
                    cubo.Tipo
                );

                cubo.MarcarFinalizado();
            }
            else
            {
                // EL ROBOT NO CONSIGUIO ALCANZARLO
                Debug.LogWarning(
                    "El DOBOT no pudo alcanzar el cubo " +
                    cubo.Tipo +
                    ". Se retira el cubo de esta prueba."
                );

                cubo.MarcarFinalizado();

                Destroy(cubo.gameObject);
            }
        }
        else
        {
            Debug.LogWarning(
                "El cubo desaparecio durante la clasificacion."
            );
        }

        // LIBERAR LA BANDA
        robotTrabajando = false;

        if (!simulacionFinalizada)
        {
            Debug.Log(
                "DOBOT finalizo. Banda reanudada."
            );
        }
    }

    // NOTIFICAR FINALIZACION DE UN CUBO
    public void NotificarCuboFinalizado()
    {
        if (simulacionFinalizada)
            return;

        cubosFinalizados++;

        Debug.Log(
            "Cubos terminados: " +
            cubosFinalizados + "/" + maximoCubos
        );

        // COMPROBAR SI TERMINARON TODOS LOS CUBOS
        if (dispensadorIniciado &&
            cubosGenerados == maximoCubos &&
            cubosFinalizados >= maximoCubos)
        {
            FinalizarSimulacion();
        }
    }

    // FINALIZAR LA SIMULACION
    private void FinalizarSimulacion()
    {
        if (simulacionFinalizada)
            return;

        simulacionFinalizada = true;

        bandaEncendida = false;

        Debug.Log(
            "Simulacion finalizada correctamente."
        );

        // REPRODUCIR VOZ #4
        ReproducirVoz(voz4);
    }

    // REPRODUCCION DE AUDIOS
    private void ReproducirVoz(AudioSource audio)
    {
        if (audio == null)
        {
            Debug.LogWarning(
                "No se asigno el AudioSource."
            );

            return;
        }

        // EVITAR SUPERPOSICION DE VOCES
        if (voz3 != null && voz3 != audio)
        {
            voz3.Stop();
        }

        if (voz4 != null && voz4 != audio)
        {
            voz4.Stop();
        }

        audio.Stop();
        audio.Play();
    }
}
