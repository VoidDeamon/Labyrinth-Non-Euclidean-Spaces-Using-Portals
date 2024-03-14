using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalSwitchPortal : Portal
{
    //public Portal newTargetPortal;
    public Portal[] newTargetPortal;
    public int count = 0;
    override protected void CheckForPortalCrossing()
    {
        // Clear removal queue

        objectsInPortalToRemove.Clear();
        // Check every touching object

        foreach (var portalableObject in objectsInPortal)
        {
            // If portalable object has been destroyed, remove it immediately
            if (portalableObject == null)
            {
                objectsInPortalToRemove.Add(portalableObject);
                continue;
            }
            // Check if portalable object is behind the portal using Vector3.Dot (dot product)
            // If so, they have crossed through the portal.
            var pivot = portalableObject.transform;
            var directionToPivotFromTransform = pivot.position - transform.position;
            directionToPivotFromTransform.Normalize();
            var pivotToNormalDotProduct = Vector3.Dot(directionToPivotFromTransform, normalVisible.forward);

            if (pivotToNormalDotProduct > 0) continue;
            var DirectionDotProduct = Vector3.Dot(pivot.forward, normalVisible.forward);
            if (portalableObject.GetComponent<CharacterController>() != null)
            {
                if (DirectionDotProduct >= min2DAngle && DirectionDotProduct <= max2DAngle) // Currently only effects player. Entering the rear of a portal does not cause a warp so whilst moving objects will give the game away, that may be their purpose.
                {
                    SetCount(count + 1);
                    if (count > newTargetPortal.Length - 1)
                    {
                        //count = 0;
                        SetCount(0);
                    }
                    targetPortal = newTargetPortal[count];
                    Warp(portalableObject);
                    //objectsInPortalToRemove.Add(portalableObject);
                }
                else
                {
                    Warp(portalableObject);
                }
            }
            else
            {
                Warp(portalableObject);
            }
        }

        // Remove all objects queued up for removal
        foreach (var portalableObject in objectsInPortalToRemove)
        {
            objectsInPortal.Remove(portalableObject);
        }
    }

    override public void ChangeTargetPortal(Portal newTarget)
    {
        targetPortal = newTarget;
        int i = 0;
        foreach (var portal in newTargetPortal)
        {
            //Debug.Log(portal.Equals(targetPortal));
            //Debug.Log(portal.name);
            //Debug.Log(targetPortal.name);
            if (portal.Equals(targetPortal))
            {
                //Debug.Log(i);
                SetCount(i);
                break;
            }
            i += 1;
        }
    }

    private void SetCount(int tempCount)
    {
        count = tempCount;
    }
}
