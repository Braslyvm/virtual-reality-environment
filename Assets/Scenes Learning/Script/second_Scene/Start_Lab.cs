
using System.Collections;
using UnityEngine;

public class start_labnn
    : MonoBehaviour
{
    [Header("Audio inicial")]
    public AudioSource audioSource;
    public float delay = 3f;

    [Header("Glows de las estaciones")]
    public GameObject glow1;
    public GameObject glow2;

    [Header("Triggers de las estaciones")]
    public GameObject trigger1;
    public GameObject trigger2;

    private bool activado = false;
    private Collider triggerInicial;

    private void Awake()
    {
        triggerInicial = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (other.transform.root.name != "XR Origin Hands (XR Rig)")
            return;

        activado = true;

        if (triggerInicial != null)
            triggerInicial.enabled = false;

        StartCoroutine(IniciarProceso());
    }

    private IEnumerator IniciarProceso()
    {
        yield return new WaitForSeconds(delay);

        if (audioSource != null)
            audioSource.Play();

        if (glow1 != null)
            glow1.SetActive(true);

        if (glow2 != null)
            glow2.SetActive(true);

        if (trigger1 != null)
            trigger1.SetActive(true);

        if (trigger2 != null)
            trigger2.SetActive(true);
    }
}
