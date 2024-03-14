using System.Collections.Generic;
using UnityEngine;

//Based on https://medium.com/@limdingwen_66715/multiple-recursive-portals-and-ai-in-unity-intro-and-table-of-contents-acb097bc7f89

public abstract class AbstractClone : MonoBehaviour
{
    // Called when awake.
    public virtual void OnCloneAwake() { }

    // Called when the clone object is enabled.
    public virtual void OnCloneEnable(Portal sender, Portal destination) { }

    // Called on FixedUpdate and PortalChange. May be called multiple times, so do not rely on state and fixedDeltaTime.
    // Update your clone object here.
    public virtual void OnCloneUpdate(Portal sender, Portal destination) { }

    // Called when the clone object is disabled.
    public virtual void OnCloneDisable(Portal sender, Portal destination) { }
}