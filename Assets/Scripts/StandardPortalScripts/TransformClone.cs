using System.Collections.Generic;
using UnityEngine;

public class TransformClone : AbstractClone
{
    public Transform target;
    public Material[] cloneMaterials { get; set; }

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
    public override void OnCloneUpdate(Portal sender, Portal destination)
    {
        cloneMaterials = GetMaterials(gameObject);
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Vector3 scale = new Vector3(destination.transform.lossyScale.x / sender.transform.lossyScale.x, destination.transform.lossyScale.y / sender.transform.lossyScale.y, destination.transform.lossyScale.z / sender.transform.lossyScale.z);
        transform.localScale = new Vector3(transform.localScale.x * scale.y, transform.localScale.y * scale.y, transform.localScale.z * scale.y);

        var xscale = destination.transform.lossyScale.x / sender.transform.lossyScale.x;
        var yscale = destination.transform.lossyScale.x / sender.transform.lossyScale.x;
        var newPosVector = sender.transform.position - target.transform.position;
        newPosVector = sender.normalVisible.InverseTransformDirection(newPosVector);
        Vector3 scaleVec = new Vector3(1.0f, 1.0f, xscale);
        newPosVector.Scale(scaleVec);
        newPosVector = sender.normalVisible.TransformDirection(newPosVector);
        var tempPosition = sender.transform.position - newPosVector;




        Vector3 cloneSliceNormal = destination.transform.forward;
        // Calculate slice centre
        Vector3 cloneSlicePos = destination.transform.position;

        for (int i = 0; i < cloneMaterials.Length; i++)
        {
            cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
        }



        transform.SetPositionAndRotation(Portal.TransformPositionBetweenPortals(sender, destination, tempPosition), Portal.TransformRotationBetweenPortals(sender, destination, target.rotation));
    }
}