using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSwitchCollider : MonoBehaviour
{
    public Portal switchPortal;
    public Portal newTargetPortal;

    private void OnTriggerEnter(Collider other)
    {
        if (switchPortal != null && newTargetPortal != null)
        {
            switchPortal.ChangeTargetPortal(newTargetPortal);
            //gameObject.SetActive(false);
        }
    }
}
