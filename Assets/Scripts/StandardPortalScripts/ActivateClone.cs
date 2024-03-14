using UnityEngine;

//Based on https://medium.com/@limdingwen_66715/multiple-recursive-portals-and-ai-in-unity-intro-and-table-of-contents-acb097bc7f89

public class ActivateClone : AbstractClone
{
    public override void OnCloneAwake()
    {
        //Debug.Log("OnCloneAwake");
        gameObject.SetActive(false);
    }

    public override void OnCloneEnable(Portal sender, Portal destination)
    {
        gameObject.SetActive(true);
    }

    public override void OnCloneDisable(Portal sender, Portal destination)
    {
        gameObject.SetActive(false);
    }
}