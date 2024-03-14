using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Based on https://medium.com/@limdingwen_66715/multiple-recursive-portals-and-ai-in-unity-intro-and-table-of-contents-acb097bc7f89

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private PortableObject portableObject;
    public float moveSpeed = 5;
    public float turnSpeed = 5;
    private float turnRotation;
    public Transform playerCamera;
    private float verticalRotationAbsolute;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        portableObject = GetComponent<PortableObject>();
        portableObject.HasTeleported += PortableObjectOnHasTeleported;
    }
    private void PortableObjectOnHasTeleported(Portal sender, Portal destination, Vector3 newposition, Quaternion newrotation)
    {
        // For character controller to update
        Physics.SyncTransforms();
    }
    private void OnDestroy()
    {
        portableObject.HasTeleported -= PortableObjectOnHasTeleported;
    }

    // Update is called once per frame
    private void Update()
    {
        turnRotation += Input.GetAxis("Mouse X");

        verticalRotationAbsolute += Input.GetAxis("Mouse Y") * -turnSpeed;
        verticalRotationAbsolute = Mathf.Clamp(verticalRotationAbsolute, -89, 89);
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }*/
    }

    void FixedUpdate()
    {
        // Turn player
        transform.Rotate(Vector3.up * turnRotation * turnSpeed);
        turnRotation = 0;
        // Turn player (up/down)
        playerCamera.localRotation = Quaternion.Euler(verticalRotationAbsolute, 0, 0);

        // Move player
        characterController.SimpleMove(transform.forward * Input.GetAxis("Vertical") * moveSpeed + transform.right * Input.GetAxis("Horizontal") * moveSpeed);
    }
}
