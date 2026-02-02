using UnityEngine;
using FishNet.Object;

public class LocalOnlyMinimalCameras : NetworkBehaviour
{
    [SerializeField] private string minimalCamerasName = "Minimal3DCameras(Clone)";

    public override void OnStartClient()
    {
        base.OnStartClient();

        var cams = transform.Find(minimalCamerasName);
        if (cams == null) return;

        cams.gameObject.SetActive(IsOwner);
    }
}
