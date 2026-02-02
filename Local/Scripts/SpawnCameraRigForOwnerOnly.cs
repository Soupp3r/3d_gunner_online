using UnityEngine;
using FishNet.Object;

public class SpawnCameraRigForOwnerOnly : NetworkBehaviour
{
    [Header("Camera rig prefab (NOT networked)")]
    [SerializeField] private GameObject cameraRigPrefab;

    [Header("Optional: child name under player to follow")]
    [SerializeField] private string cameraTargetChildName = "CameraTarget";

    private GameObject _rigInstance;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // ONLY the local player should spawn a camera rig on this machine.
        if (!IsOwner)
            return;

        if (cameraRigPrefab == null)
        {
            Debug.LogWarning("[SpawnCameraRigForOwnerOnly] No cameraRigPrefab assigned.");
            return;
        }

        _rigInstance = Instantiate(cameraRigPrefab);

        // Hard safety: ensure the rig camera becomes the only active MainCamera on this client
        DisableAllOtherCamerasExceptRig(_rigInstance);

        // If you need to hook Cinemachine follow target, do it here
        Transform target = FindChildByName(transform, cameraTargetChildName);
        if (target != null)
        {
            // If you’re using CM vcam1, you can set Follow/LookAt/TrackingTarget here
            // (Tell me your CM version + component names and I’ll wire this perfectly.)
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (_rigInstance != null)
            Destroy(_rigInstance);
    }

    private static void DisableAllOtherCamerasExceptRig(GameObject rig)
    {
        var rigCams = rig.GetComponentsInChildren<Camera>(true);
        var allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (var cam in allCams)
        {
            if (cam == null) continue;
            bool isRigCam = false;
            for (int i = 0; i < rigCams.Length; i++)
            {
                if (rigCams[i] == cam) { isRigCam = true; break; }
            }

            cam.enabled = isRigCam;
            if (isRigCam) cam.tag = "MainCamera";
        }

        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var al in listeners)
        {
            if (al == null) continue;
            al.enabled = al.transform.IsChildOf(rig.transform);
        }
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
                return t;
        return null;
    }
}
