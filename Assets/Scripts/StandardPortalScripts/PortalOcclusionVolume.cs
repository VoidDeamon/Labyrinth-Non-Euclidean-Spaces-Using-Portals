using System.Collections.Generic;
using UnityEngine;

//Based on https://medium.com/@limdingwen_66715/multiple-recursive-portals-and-ai-in-unity-intro-and-table-of-contents-acb097bc7f89

public class PortalOcclusionVolume : MonoBehaviour
{
    public new Collider collider;
    public Portal[] portals;

    private void Start()
    {
        if (collider == null)
        {
            collider = GetComponent<Collider>();
        }
    }

    [ContextMenu("Auto Add Portals")]
    private void EditorAutoAdd()
    {
        var col = GetComponent<Collider>();
        var allPortals = FindObjectsOfType<Portal>();
        var portalBuilder = new List<Portal>();

        foreach (var portal in allPortals)
        {
            if (col.bounds.Contains(portal.transform.position))
            {
                portalBuilder.Add(portal);
            }
        }

        portals = portalBuilder.ToArray();
    }
}