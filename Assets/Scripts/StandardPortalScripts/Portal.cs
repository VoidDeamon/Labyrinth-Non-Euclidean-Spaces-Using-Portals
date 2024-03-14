using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal targetPortal;
    public Transform normalVisible;
    public Transform normalInvisible;
    public Renderer viewThroughRenderer;
    private Material viewThroughMaterial;
    private Camera mainCamera;
    public Plane plane;
    private Vector4 vectorPlane;
    protected HashSet<PortableObject> objectsInPortal = new HashSet<PortableObject>();
    protected HashSet<PortableObject> objectsInPortalToRemove = new HashSet<PortableObject>();
    public Portal[] visiblePortals;
    public Texture viewthroughDefaultTexture;
    public bool ShouldRender(Plane[] cameraPlanes) => viewThroughRenderer.isVisible && GeometryUtility.TestPlanesAABB(cameraPlanes, viewThroughRenderer.bounds);
    public int maxRecursionsOverride = -1;
    public float min2DAngle = -1.5f; // avoiding floating point errors, really would be -1
    public float max2DAngle = 1.5f; // avoiding floating point errors, really would be 1

    private void OnDrawGizmos()
    {
        // Linked portals
        if (targetPortal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPortal.transform.position);
        }
        // Visible portals
        Gizmos.color = Color.blue;
        foreach (var visiblePortal in visiblePortals)
        {
            Gizmos.DrawLine(transform.position, visiblePortal.transform.position);
        }
    }

    public static Vector3 TransformPositionBetweenPortals(Portal sender, Portal target, Vector3 position)
    {
        return
            target.normalInvisible.TransformPoint(
                sender.normalVisible.InverseTransformPoint(position));
    }

    public static Quaternion TransformRotationBetweenPortals(Portal sender, Portal target, Quaternion rotation)
    {
        return
            target.normalInvisible.rotation *
            Quaternion.Inverse(sender.normalVisible.rotation) *
            rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        var portalableObject = other.GetComponent<PortableObject>();
        if (portalableObject)
        {
            objectsInPortal.Add(portalableObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var portalableObject = other.GetComponent<PortableObject>();
        if (portalableObject)
        {
            for (int i = 0; i < portalableObject.originalMaterials.Length; i++)
            {
                portalableObject.originalMaterials[i].SetVector("sliceCentre", Vector3.zero);
                portalableObject.originalMaterials[i].SetVector("sliceNormal", Vector3.zero);
            }
            objectsInPortal.Remove(portalableObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        viewThroughMaterial = viewThroughRenderer.material;
        mainCamera = Camera.main;
        plane = new Plane(normalVisible.forward, transform.position);
        vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        StartCoroutine(WaitForFixedUpdateLoop());
    }

    private IEnumerator WaitForFixedUpdateLoop()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            yield return waitForFixedUpdate;
            try
            {
                CheckForPortalCrossing();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    void HandleClipping(Vector3 portalCamPos)
    {
        const float showDst = 0.075f;

        foreach (var portableObject in objectsInPortal)
        {
            portableObject.SetSliceOffsetDst(showDst);
        }
    }

    protected virtual void CheckForPortalCrossing()
    {
        objectsInPortalToRemove.Clear();

        foreach (var portalableObject in objectsInPortal)
        {
            if (portalableObject == null)
            {
                objectsInPortalToRemove.Add(portalableObject);
                continue;
            }

            var pivot = portalableObject.transform;
            var directionToPivotFromTransform = pivot.position - transform.position;
            directionToPivotFromTransform.Normalize();
            var pivotToNormalDotProduct = Vector3.Dot(directionToPivotFromTransform, normalVisible.forward);


            UpdateSliceParams(portalableObject); // TEST CODE


            if (pivotToNormalDotProduct > 0) continue;
            var DirectionDotProduct = Vector3.Dot(pivot.forward, normalVisible.forward);
            if (portalableObject.GetComponent<CharacterController>() != null) 
            {
                if (DirectionDotProduct >= min2DAngle && DirectionDotProduct <= max2DAngle) // Currently only effects player. Entering the rear of a portal does not cause a warp so whilst moving objects will give the game away, that may be their purpose. Beware of setting this exactly, it seems setting it to -1 min, 1 max may be the cause of a bug where you sometimes walk through the portal. Set to -1.5 min and 1.5 max instead.
                {
                    Warp(portalableObject);
                }
                else
                {
                    objectsInPortalToRemove.Add(portalableObject);
                }
            }
            else
            {
                Warp(portalableObject);
            }
        }

        //Update Slice for portable object (not clones)
        /*foreach (var portalableObject in objectsInPortal)
        {
            UpdateSliceParams(portalableObject);
        }*/

        foreach (var portalableObject in objectsInPortalToRemove)
        {
            objectsInPortal.Remove(portalableObject);
            /*for (int i = 0; i < portalableObject.originalMaterials.Length; i++)
            {
                //portalableObject.originalMaterials[i].SetVector("sliceCentre", Vector3.zero);
                //portalableObject.originalMaterials[i].SetVector("sliceNormal", Vector3.zero);
            }*/
        }
    }

    protected void Warp(PortableObject portalableObject)
    {
        var newPosition = TransformPositionBetweenPortals(this, targetPortal, portalableObject.transform.position);
        var newRotation = TransformRotationBetweenPortals(this, targetPortal, portalableObject.transform.rotation);
        portalableObject.transform.SetPositionAndRotation(newPosition, newRotation);

        // Scaling portableObject
        Vector3 scale = new Vector3(targetPortal.transform.lossyScale.x / this.transform.lossyScale.x, targetPortal.transform.lossyScale.y / this.transform.lossyScale.y, targetPortal.transform.lossyScale.z / this.transform.lossyScale.z);
        portalableObject.transform.localScale = new Vector3(portalableObject.transform.localScale.x * scale.y, portalableObject.transform.localScale.y * scale.y, portalableObject.transform.localScale.z * scale.y); // transforms by y to keep proportions (not necessarily necessary, but alternative is not desired) - portals should be proportional to each other

        var porb = portalableObject.GetComponent<Rigidbody>();
        if (porb != null)
        {
            porb.velocity = targetPortal.normalInvisible.TransformDirection(this.normalVisible.InverseTransformDirection(porb.velocity));
        }

        portalableObject.OnHasTeleported(this, targetPortal, newPosition, newRotation);


        targetPortal.UpdateSliceParams(portalableObject);// TEST CODE


        objectsInPortalToRemove.Add(portalableObject);
    }

    public void RenderViewthroughRecursive(Vector3 refPosition, Quaternion refRotation, out RenderTexturePool.PoolItem temporaryPoolItem, out Texture originalTexture, out int debugRenderCount, Camera portalCamera, int currentRecursion, int maxRecursions)
    {
        debugRenderCount = 1;
        // Calculate virtual camera position, rotation, and scale
        var scale = targetPortal.transform.lossyScale.x / this.transform.lossyScale.x;
        if (scale != 1.0f)
        {
            var newPosVector = this.transform.position - refPosition;

            newPosVector = this.normalVisible.InverseTransformDirection(newPosVector);
            Vector3 scaleVec = new Vector3(1.0f, 1.0f, scale);
            newPosVector.Scale(scaleVec);
            newPosVector = this.normalVisible.TransformDirection(newPosVector);

            refPosition = this.transform.position - newPosVector;
        }

        var virtualPosition = TransformPositionBetweenPortals(this, targetPortal, refPosition);
        var virtualRotation = TransformRotationBetweenPortals(this, targetPortal, refRotation);
        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);

        var targetViewThroughPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix))
            * targetPortal.vectorPlane;

        var obliqueProjectionMatrix = mainCamera.CalculateObliqueMatrix(targetViewThroughPlaneCameraSpace);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        var visiblePortalResourcesList = new List<VisiblePortalResources>();

        var cameraPlanes = GeometryUtility.CalculateFrustumPlanes(portalCamera);

        var actualMaxRecursions = targetPortal.maxRecursionsOverride >= 0 ? targetPortal.maxRecursionsOverride : maxRecursions;

        if (currentRecursion < actualMaxRecursions)
        {
            foreach (var visiblePortal in targetPortal.visiblePortals)
            {
                if (!visiblePortal.ShouldRender(cameraPlanes)) continue;

                visiblePortal.RenderViewthroughRecursive(
                    virtualPosition,
                    virtualRotation,
                    out var visiblePortalTemporaryPoolItem,
                    out var visiblePortalOriginalTexture,
                    out var visiblePortalRenderCount,
                    portalCamera,
                    currentRecursion + 1,
                    maxRecursions);

                visiblePortalResourcesList.Add(new VisiblePortalResources()
                {
                    OriginalTexture = visiblePortalOriginalTexture,
                    PoolItem = visiblePortalTemporaryPoolItem,
                    VisiblePortal = visiblePortal
                });

                debugRenderCount += visiblePortalRenderCount;
            }
        }
        else
        {
            foreach (var visiblePortal in targetPortal.visiblePortals)
            {
                visiblePortal.ShowViewthroughDefaultTexture(out var visiblePortalOriginalTexture);

                visiblePortalResourcesList.Add(new VisiblePortalResources()
                {
                    OriginalTexture = visiblePortalOriginalTexture,
                    VisiblePortal = visiblePortal
                });
            }
        }

        temporaryPoolItem = RenderTexturePool.Instance.GetTexture();

        portalCamera.targetTexture = temporaryPoolItem.Texture;
        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);
        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        HandleClipping(portalCamera.transform.position);

        portalCamera.Render();

        foreach (var resources in visiblePortalResourcesList)
        {
            resources.VisiblePortal.viewThroughMaterial.mainTexture = resources.OriginalTexture;

            if (resources.PoolItem != null)
            {
                RenderTexturePool.Instance.ReleaseTexture(resources.PoolItem);
            }
        }

        originalTexture = viewThroughMaterial.mainTexture;
        viewThroughMaterial.mainTexture = temporaryPoolItem.Texture;
    }
    private struct VisiblePortalResources
    {
        public Portal VisiblePortal;
        public RenderTexturePool.PoolItem PoolItem;
        public Texture OriginalTexture;
    }

    private void ShowViewthroughDefaultTexture(out Texture originalTexture)
    {
        originalTexture = viewThroughMaterial.mainTexture;
        viewThroughMaterial.mainTexture = viewthroughDefaultTexture;
    }

    private void OnDestroy()
    {
        Destroy(viewThroughMaterial);
    }

    public virtual void ChangeTargetPortal(Portal newTarget)
    {
        targetPortal = newTarget;
    }

    public void UpdateSliceParams(PortableObject portableObject)
    {
        Vector3 sliceNormal = transform.forward;
        Vector3 slicePos = transform.position;
        for (int i = 0; i < portableObject.originalMaterials.Length; i++)
        {
            portableObject.originalMaterials[i].SetVector("sliceCentre", slicePos);
            portableObject.originalMaterials[i].SetVector("sliceNormal", sliceNormal);
        }
    }
}