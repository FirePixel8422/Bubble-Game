using Unity.Netcode;
using UnityEngine;



[System.Serializable]
public struct PlayerIdDataArray : INetworkSerializable
{
    [SerializeField] private ulong[] networkIds;

    [SerializeField] private int playerCount;


    public PlayerIdDataArray(int maxPlayerCount)
    {
        networkIds = new ulong[maxPlayerCount];

        playerCount = 0;
    }


    public void AddPlayer(ulong addedNetworkId)
    {
        networkIds[playerCount] = addedNetworkId;

        playerCount += 1;
    }
    public void RemovePlayer(ulong removedNetworkId)
    {
        int removedGameId = NetworkIdToGameId(removedNetworkId);

        playerCount -= 1;

        for (int i = removedGameId; i < playerCount; i++)
        {
            //move down all the networkIds in the array by 1.
            networkIds[i] = networkIds[i + 1]; 
        }
    }

    public int GetPlayerGameId(ulong networkId)
    {
        return NetworkIdToGameId(networkId);
    }

    private int NetworkIdToGameId(ulong toConvertNetworkId)
    {
        Debug.Log("Attempting Conversion of clientNetworkId: " + toConvertNetworkId + ", playerCount = " + playerCount);

        for (int i = 0; i < playerCount; i++)
        {
            if (networkIds[i] == toConvertNetworkId)
            {
                Debug.Log("clientNetworkId: " + toConvertNetworkId + " converted to gameId: " + i);

                return i;
            }
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("Cant Convert Id: " + toConvertNetworkId + ", networkIds are: + " + networkIds[0] + ", " + networkIds[1] + ", " + networkIds[2] + ", " + networkIds[3]);
#endif
        return -1;
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref networkIds);
        serializer.SerializeValue(ref playerCount);
    }
}