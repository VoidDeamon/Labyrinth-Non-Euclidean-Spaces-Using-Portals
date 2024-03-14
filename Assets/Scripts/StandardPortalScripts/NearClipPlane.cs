using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Based on https://medium.com/@limdingwen_66715/multiple-recursive-portals-and-ai-in-unity-intro-and-table-of-contents-acb097bc7f89

public class NearClipPlane : MonoBehaviour
{
    public float nearClipPlane = 0.0001f;

    private void Start()
    {
        GetComponent<Camera>().nearClipPlane = nearClipPlane;
    }
}
