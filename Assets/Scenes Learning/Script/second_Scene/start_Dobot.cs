using UnityEngine;

public class TriggerAudio2 : MonoBehaviour
{
    [Header("Audio Voz #2")]
    public AudioSource audioSource;

    [Header("Glows a desactivar")]
    public GameObject glow;
    public GameObject otroGlow;

    [Header("Trigger actual")]
    public Collider triggerCollider;

    [Header("Otro Trigger a desactivar")]
    public GameObject otroTrigger;

    private bool activado = false;

    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (other.transform.root.name != "XR Origin Hands (XR Rig)")
            return;

        activado = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (glow != null)
        {
            glow.SetActive(false);
        }

        if (otroGlow != null)
        {
            otroGlow.SetActive(false);
        }

        if (otroTrigger != null)
        {
            otroTrigger.SetActive(false);
        }
    }
}