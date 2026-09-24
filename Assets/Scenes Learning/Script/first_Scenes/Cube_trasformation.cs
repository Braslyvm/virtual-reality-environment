using System.Collections;
using UnityEngine;

public class CuboEsferaController : MonoBehaviour
{
    [Tooltip("Duración de cada transformación en segundos")]
    public float duracion = 2f;

    [Tooltip("Tiempo que permanece como cubo o esfera antes de volver a transformarse")]
    public float espera = 1f;

    public string nombreBlendShape = "Esfera";

    private SkinnedMeshRenderer smr;
    private int indice;

    public bool EsEsfera { get; private set; }

    void Awake()
    {
        smr = GetComponentInChildren<SkinnedMeshRenderer>();

        if (smr == null)
        {
            Debug.LogError("No se encontró un SkinnedMeshRenderer.");
            return;
        }

        indice = smr.sharedMesh.GetBlendShapeIndex(nombreBlendShape);

        if (indice < 0)
        {
            Debug.LogError("No se encontró el Blend Shape: " + nombreBlendShape);
            indice = 0;
        }
    }

    void Start()
    {
        // Comienza automáticamente el ciclo
        StartCoroutine(CicloAutomatico());
    }

    IEnumerator CicloAutomatico()
    {
        while (true)
        {
            // Cubo → Esfera
            yield return StartCoroutine(Animar(100f));

            EsEsfera = true;

            // Espera siendo esfera
            yield return new WaitForSeconds(espera);

            // Esfera → Cubo
            yield return StartCoroutine(Animar(0f));

            EsEsfera = false;

            // Espera siendo cubo
            yield return new WaitForSeconds(espera);
        }
    }

    IEnumerator Animar(float destino)
    {
        float inicio = smr.GetBlendShapeWeight(indice);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duracion;

            float suave = Mathf.SmoothStep(0f, 1f, t);

            smr.SetBlendShapeWeight(
                indice,
                Mathf.Lerp(inicio, destino, suave)
            );

            yield return null;
        }

        smr.SetBlendShapeWeight(indice, destino);
    }
}