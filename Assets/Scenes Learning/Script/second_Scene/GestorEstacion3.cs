
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

    public bool BandaEnMovimiento =>
        bandaEncendida &&
        !robotTrabajando &&
        !simulacionFinalizada;

    // Enciende la banda transportadora al pulsar el botón rojo.
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

    // Inicia la generación de cubos al pulsar el botón verde.
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

        ReproducirVoz(voz3);

        StartCoroutine(GenerarCubos());
    }

    // Genera cubos de colores aleatorios con pausas entre ellos.
    private IEnumerator GenerarCubos()
    {
        for (int i = 0; i < maximoCubos; i++)
        {
            while (!BandaEnMovimiento)
            {
                if (simulacionFinalizada)
                    yield break;

                yield return null;
            }

            TipoCubo tipo =
                (TipoCubo)Random.Range(0, 3);

            CuboEnBanda cubo = Instantiate(
                prefabCubo,
                puntoGeneracion.position,
                puntoGeneracion.rotation
            );

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

    // Intenta recoger un cubo cuando un sensor lo detecta.
    public void IntentarRecoger(
        CuboEnBanda cubo,
        DobotBrazo brazo,
        TipoCubo colorSensor)
    {
        if (!BandaEnMovimiento)
            return;

        if (cubo == null || !cubo.EstaEnBanda)
            return;

        if (cubo.Tipo != colorSensor)
            return;

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

        robotTrabajando = true;

        cubo.DetenerParaRecoger();

        Debug.Log(
            "Sensor detecto cubo " + cubo.Tipo
        );

        Debug.Log(
            "Banda detenida. DOBOT trabajando."
        );

        StartCoroutine(
            ClasificarCubo(cubo, brazo)
        );
    }

    // Coordina al DOBOT para clasificar el cubo detectado.
    private IEnumerator ClasificarCubo(
        CuboEnBanda cubo,
        DobotBrazo brazo)
    {
        if (cubo == null || brazo == null)
        {
            robotTrabajando = false;
            yield break;
        }

        yield return StartCoroutine(
            brazo.RecogerYDepositar(cubo)
        );

        if (cubo != null)
        {
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

        robotTrabajando = false;

        if (!simulacionFinalizada)
        {
            Debug.Log(
                "DOBOT finalizo. Banda reanudada."
            );
        }
    }

    // Registra un cubo finalizado y comprueba si terminó la simulación.
    public void NotificarCuboFinalizado()
    {
        if (simulacionFinalizada)
            return;

        cubosFinalizados++;

        Debug.Log(
            "Cubos terminados: " +
            cubosFinalizados + "/" + maximoCubos
        );

        if (dispensadorIniciado &&
            cubosGenerados == maximoCubos &&
            cubosFinalizados >= maximoCubos)
        {
            FinalizarSimulacion();
        }
    }

    // Finaliza la simulación y reproduce la voz de cierre.
    private void FinalizarSimulacion()
    {
        if (simulacionFinalizada)
            return;

        simulacionFinalizada = true;

        bandaEncendida = false;

        Debug.Log(
            "Simulacion finalizada correctamente."
        );

        ReproducirVoz(voz4);
    }

    // Reproduce una narración evitando la superposición de voces.
    private void ReproducirVoz(AudioSource audio)
    {
        if (audio == null)
        {
            Debug.LogWarning(
                "No se asigno el AudioSource."
            );

            return;
        }

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
