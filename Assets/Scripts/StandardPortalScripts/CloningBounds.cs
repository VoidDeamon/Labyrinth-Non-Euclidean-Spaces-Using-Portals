using UnityEngine;

//Based on https://medium.com/@limdingwen_66715/multiple-recursive-portals-and-ai-in-unity-intro-and-table-of-contents-acb097bc7f89

public class CloningBounds : MonoBehaviour
{
    public Transform referenceTransform;

    public delegate void PortalEnterHandler(Portal sender);
    public event PortalEnterHandler PortalEnter;

    public delegate void PortalExitHandler(Portal sender);
    public event PortalExitHandler PortalExit;

    public void OnTriggerEnter(Collider other)
    {
        /*if (other == null)
        {
            Debug.Log("The fuck?");
        }*/
        //Debug.Log("OnTriggerEnter");
        var portal = other.GetComponent<Portal>();
        //var portal = other.gameObject.GetComponent<Portal>();
        //var portal = other.GetComponentsInParent<Portal>()[0];
        //Debug.Log(other.gameObject);
        if (portal == null) return;
        //Debug.Log("ItsNotNull");
        //Debug.Log(portal);
        //PortalEnter?.Invoke(portal);
        //PortalEnter(portal);
        PortalEnter(other.GetComponent<Portal>());
        //Debug.Log("ITS THE EVENT");
    }

    public void OnTriggerExit(Collider other)
    {
        var portal = other.GetComponent<Portal>();
        if (portal == null) return;
        PortalExit?.Invoke(portal);
    }
}