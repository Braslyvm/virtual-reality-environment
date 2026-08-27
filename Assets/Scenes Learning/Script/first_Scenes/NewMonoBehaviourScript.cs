using UnityEngine;

public class ProbarTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name == "XR Origin Hands (XR Rig)")
        {
            Debug.Log("¡XR Origin Hands (XR Rig) entró al trigger!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.name == "XR Origin Hands (XR Rig)")
        {
            Debug.Log("¡XR Origin Hands (XR Rig) salió del trigger!");
        }
    }
}