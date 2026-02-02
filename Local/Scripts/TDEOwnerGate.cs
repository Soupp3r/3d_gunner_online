using UnityEngine;
using FishNet.Object;

public class TDEOwnerGate : NetworkBehaviour
{
    [Header("Disable these on remote players (non-owners)")]
    [SerializeField] private Behaviour[] disableOnRemote;  // scripts
    [SerializeField] private GameObject[] disableOnRemoteObjects; // cameras, UI roots, etc.

    public override void OnStartClient()
    {
        base.OnStartClient();

        bool isLocal = IsOwner;

        // Local player: keep enabled. Remote players: disable.
        if (disableOnRemote != null)
        {
            foreach (var b in disableOnRemote)
                if (b != null) b.enabled = isLocal;
        }

        if (disableOnRemoteObjects != null)
        {
            foreach (var go in disableOnRemoteObjects)
                if (go != null) go.SetActive(isLocal);
        }
    }
}
