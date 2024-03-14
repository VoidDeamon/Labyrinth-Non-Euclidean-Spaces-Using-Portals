using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
MIT License

Copyright (c) 2020 Lim Ding Wen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

public class PortableObjectClone : MonoBehaviour
{
    public CloningBounds cloningBounds;
    private readonly HashSet<Portal> currentlyTouchingPortals = new HashSet<Portal>();

    private PortableObject portalableObject;
    private AbstractClone[] clones;

    private void Awake()
    {
        portalableObject = GetComponent<PortableObject>();
        portalableObject.HasTeleported += OnTeleported;

        clones = GetComponentsInChildren<AbstractClone>();
        foreach (var clone in clones)
            clone.OnCloneAwake();

        cloningBounds.PortalEnter += OnEnterPortal;
        cloningBounds.PortalExit += OnExitPortal;
    }
    private void OnDestroy()
    {
        portalableObject.HasTeleported -= OnTeleported;
        cloningBounds.PortalEnter -= OnEnterPortal;
        cloningBounds.PortalExit -= OnExitPortal;
    }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            try
            {
                if (currentlyTouchingPortals.Count != 0)
                    UpdateClones();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void OnEnterPortal(Portal sender)
    {
        // Only call OnCloneEnable for the first portal entered
        if (currentlyTouchingPortals.Count == 0)
            foreach (var clone in clones)
                clone.OnCloneEnable(sender, sender.targetPortal);

        currentlyTouchingPortals.Add(sender);

        UpdateClones(); // Force update after portal change for new transforms
    }

    private void OnExitPortal(Portal sender)
    {
        currentlyTouchingPortals.Remove(sender);

        // Only call OnCloneDisable if all portals have been exited
        if (currentlyTouchingPortals.Count == 0)
            foreach (var clone in clones)
                clone.OnCloneDisable(sender, sender.targetPortal);
    }
    private void OnTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        // OnTrigger events won't fire until next tick (Portal crossing calculations happen after OnTrigger events),
        // so if a frame is going to be rendered after this tick, we need the currentlyTouchingPortals to be correct.
        // This means manually editing it with the known teleport info. Since it is a HashMap, this should not
        // have any ill effects when OnTrigger happens next tick.

        currentlyTouchingPortals.Remove(sender);
        currentlyTouchingPortals.Add(destination);
        UpdateClones(); // Force update after portal change for new transforms
    }

    public Portal ClosestTouchingPortal
    {
        get
        {
            var currentMin = (portal: (Portal)null, distance: float.PositiveInfinity);
            var referencePosition = cloningBounds.referenceTransform.position;
            foreach (var portal in currentlyTouchingPortals)
            {
                var closestPointOnPlane = portal.plane.ClosestPointOnPlane(referencePosition);
                var distance = Vector3.Distance(closestPointOnPlane, referencePosition);
                if (distance < currentMin.distance) currentMin = (portal, distance);
            }
            return currentMin.portal;
        }
    }

    private void UpdateClones()
    {
        var closestPortal = ClosestTouchingPortal;
        if (closestPortal == null)
            throw new Exception("No touching portals found when trying to update clones.");

        foreach (var clone in clones)
            clone.OnCloneUpdate(closestPortal, closestPortal.targetPortal);
    }
}
