using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Called when this player object is spawned on the network
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("This is MY player prefab.");
        }
        else
        {
            Debug.Log("Spawned another player's prefab.");
        }
    }
}
