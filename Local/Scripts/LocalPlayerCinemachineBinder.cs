using UnityEngine;
using System.Collections;
using System.Reflection;
using FishNet.Object;

public class LocalPlayerCinemachineBinder : NetworkBehaviour
{
    [Header("Target on the player to follow")]
    [SerializeField] private string cameraTargetChildName = "Camera Target";
    [SerializeField] private Transform explicitTarget; // optional override

    [Header("Retry (handles spawn timing)")]
    [SerializeField] private bool retryUntilAssigned = true;
    [SerializeField] private float retrySeconds = 0.1f;

    [Header("Optional")]
    [SerializeField] private bool alsoSetLookAt = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Only the local-owned player should drive the camera.
        if (!IsOwner) return;

        StartCoroutine(BindWhenReady());
    }

    private IEnumerator BindWhenReady()
    {
        while (true)
        {
            if (TryBindOnce())
                yield break;

            if (!retryUntilAssigned)
                yield break;

            yield return new WaitForSeconds(retrySeconds);
        }
    }

    private bool TryBindOnce()
    {
        // 1) Find the follow target on THIS player
        Transform target = explicitTarget != null ? explicitTarget : FindChildByName(transform, cameraTargetChildName);
        if (target == null) return false;

        // 2) Find CinemachineBrain (prefer Camera.main)
        var mainCam = Camera.main;
        if (mainCam == null) return false;

        Component brain = mainCam.GetComponent("CinemachineBrain");
        if (brain == null) return false;

        // 3) Get ActiveVirtualCamera from brain (works across CM2/CM3)
        var brainType = brain.GetType();
        var avcProp = brainType.GetProperty("ActiveVirtualCamera", BindingFlags.Public | BindingFlags.Instance);
        if (avcProp == null) return false;

        object activeVcam = avcProp.GetValue(brain);
        if (activeVcam == null) return false;

        // Most Cinemachine vcams are Components, so we can get their Transform
        var activeComp = activeVcam as Component;
        if (activeComp == null) return false;

        // 4) Assign Follow / TrackingTarget on the active vcam subtree
        bool assigned = AssignOnComponents(activeComp.transform, target);
        return assigned;
    }

    private bool AssignOnComponents(Transform vcamRoot, Transform target)
    {
        bool didAssign = false;

        Component[] comps = vcamRoot.GetComponentsInChildren<Component>(true);
        for (int i = 0; i < comps.Length; i++)
        {
            var c = comps[i];
            if (c == null) continue;

            System.Type t = c.GetType();

            // CM3: TrackingTarget
            PropertyInfo trackingProp = t.GetProperty("TrackingTarget", BindingFlags.Public | BindingFlags.Instance);
            if (trackingProp != null && trackingProp.PropertyType == typeof(Transform) && trackingProp.CanWrite)
            {
                trackingProp.SetValue(c, target);
                didAssign = true;
                continue;
            }

            // CM2-style: Follow (+ optional LookAt)
            PropertyInfo followProp = t.GetProperty("Follow", BindingFlags.Public | BindingFlags.Instance);
            if (followProp != null && followProp.PropertyType == typeof(Transform) && followProp.CanWrite)
            {
                followProp.SetValue(c, target);
                didAssign = true;

                if (alsoSetLookAt)
                {
                    PropertyInfo lookAtProp = t.GetProperty("LookAt", BindingFlags.Public | BindingFlags.Instance);
                    if (lookAtProp != null && lookAtProp.PropertyType == typeof(Transform) && lookAtProp.CanWrite)
                        lookAtProp.SetValue(c, target);
                }
            }
        }

        return didAssign;
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
                return t;

        return null;
    }
}
