using UnityEngine;
using FishNet.Object;
using MoreMountains.TopDownEngine;

public class TDENukeDuplicateManagers : NetworkBehaviour
{
    [SerializeField] private Transform minimal3DCamerasRoot;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (minimal3DCamerasRoot == null)
            return;

        // Only keep camera rig for the local owned player
        minimal3DCamerasRoot.gameObject.SetActive(IsOwner);

        // If this rig contains extra GUI/Input singletons, remove them to prevent singleton overrides
        if (!IsOwner)
        {
            foreach (var gm in minimal3DCamerasRoot.GetComponentsInChildren<GUIManager>(true))
                Destroy(gm);

            foreach (var im in minimal3DCamerasRoot.GetComponentsInChildren<InputManager>(true))
                Destroy(im);
        }
    }
}
