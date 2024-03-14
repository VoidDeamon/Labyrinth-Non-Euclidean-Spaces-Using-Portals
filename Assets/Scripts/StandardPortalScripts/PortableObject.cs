using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortableObject : MonoBehaviour
{
    public Material[] originalMaterials { get; set; }
    private void Awake()
    {
        originalMaterials = GetMaterials(gameObject);
    }
    public Material[] GetMaterials(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }
    public void SetSliceOffsetDst(float dst)
    {
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            originalMaterials[i].SetFloat("sliceOffsetDst", dst);
        }
    }
    

    public delegate void HasTeleportedHandler(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation);
    public event HasTeleportedHandler HasTeleported;
    public void OnHasTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        HasTeleported?.Invoke(sender, destination, newPosition, newRotation);
    }
}
