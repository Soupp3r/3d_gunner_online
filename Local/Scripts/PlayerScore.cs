using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerScore : NetworkBehaviour
{
    public readonly SyncVar<int> Points = new SyncVar<int>();

    [Server]
    public void AddPointServer()
    {
        Points.Value += 1;
    }
}
