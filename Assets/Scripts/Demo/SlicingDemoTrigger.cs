using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicingDemoTrigger : MonoBehaviour
{
    public GameObject demoSphere;
    private Vector3 demoSpherePosition;
    private Quaternion demoSphereRotation;

    private void Awake()
    {
        demoSpherePosition = demoSphere.transform.position;
        demoSphereRotation = demoSphere.transform.rotation;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //demoSphere.transform.SetPositionAndRotation(new Vector3(6f,0f,126.75f), demoSphere.transform.rotation);
            demoSphere.transform.SetPositionAndRotation(demoSpherePosition, demoSphereRotation);
        }
    }
}
