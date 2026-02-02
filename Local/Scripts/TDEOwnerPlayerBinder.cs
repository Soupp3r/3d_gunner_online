using UnityEngine;
using FishNet.Object;
using MoreMountains.TopDownEngine;

public class TDEOwnerPlayerBinder : NetworkBehaviour
{
    [SerializeField] private Character character; // drag your TDE Character here

    [SerializeField] private string localPlayerId = "Player1";
    [SerializeField] private string remotePlayerId = "Remote";

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (character == null)
            character = GetComponentInChildren<Character>();

        if (character == null)
            return;

        if (IsOwner)
        {
            character.PlayerID = localPlayerId;
            character.CharacterType = Character.CharacterTypes.Player;

            // Make sure TDE input is configured for this device
            if (InputManager.HasInstance)
            {
                InputManager.Instance.PlayerID = localPlayerId;
                InputManager.Instance.ControlsModeDetection();
            }
        }
        else
        {
            // Remote players should not consume local input
            character.PlayerID = remotePlayerId;
            character.CharacterType = Character.CharacterTypes.AI; // or Player if your net driver needs it, but ID must not match
        }
    }
}
