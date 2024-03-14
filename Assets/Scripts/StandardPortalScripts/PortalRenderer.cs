using UnityEngine;

public class PortalRenderer : MonoBehaviour
{
    public Camera portalCamera;
    public int maxRecursions = 2;

    public int debugTotalRenderCount;

    private Camera mainCamera;
    //private Portal[] allPortals;
    private PortalOcclusionVolume[] occlusionVolumes;

    private void Start()
    {
        //Debug.Log("Start being called");
        mainCamera = Camera.main;
        //allPortals = FindObjectsOfType<Portal>();
        occlusionVolumes = FindObjectsOfType<PortalOcclusionVolume>();
    }

    private void OnPreRender()
    {
        //Debug.Log("OnPreRender being called");
        debugTotalRenderCount = 0;
        /*
        foreach (var portal in allPortals)
        {
            portal.RenderViewthroughRecursive(
                mainCamera.transform.position,
                mainCamera.transform.rotation,
                out _,
                out _,
                out var renderCount,
                portalCamera,
                0,
                maxRecursions);

            debugTotalRenderCount += renderCount;
        }*/
        PortalOcclusionVolume currentOcclusionVolume = null;
        foreach (var occlusionVolume in occlusionVolumes)
        {
            if (occlusionVolume.collider.bounds.Contains(mainCamera.transform.position))
            {
                currentOcclusionVolume = occlusionVolume;
                break;
            }
        }
        if (currentOcclusionVolume != null)
        {
            var cameraPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

            foreach (var portal in currentOcclusionVolume.portals)
            {
                if (!portal.ShouldRender(cameraPlanes)) continue;
                portal.RenderViewthroughRecursive(mainCamera.transform.position, mainCamera.transform.rotation, out _, out _, out var renderCount, portalCamera, 0, maxRecursions);
                debugTotalRenderCount += renderCount;
            }
        }
    }

    private void OnPostRender()
    {
        //Debug.Log("OnPostRender being called");
        RenderTexturePool.Instance.ReleaseAllTextures();
    }
}