using UnityEngine;

public class TriggerInstruccion : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Objetos a desactivar")]
    public GameObject glow;
    public GameObject trigger;

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (other.transform.root.name == "XR Origin Hands (XR Rig)")
        {
            activado = true;

            if (audioSource != null)
            {
                audioSource.Play();
            }
            if (glow != null)
            {
                glow.SetActive(false);
            }
            if (trigger != null)
            {
                trigger.SetActive(false);
            }
        }
    }
}