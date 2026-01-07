using Unity.Netcode;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PlayerData : INetworkSerializable
{
    public string user_id;
    public string username;
    public int rating;
    public int rankpoint;
    public string email;
    public string password;
    public string bday;
    public string createwhen;
    public string History;
    public string profilePicBase64;
    public int fragments;
    public int gems;
    public bool virgin;
    public string inventoryid;
    public bool admin;

    public PlayerData()
    {
        user_id = "";
        username = "";
        email = "";
        rankpoint = 0;
        rating = 0;
        password = "";
        bday = "";
        createwhen = "";
        History = "";
        profilePicBase64 = "";
        fragments = 0;
        gems = 0;
        virgin = false;
        admin = false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref user_id);
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref email);
        serializer.SerializeValue(ref password);
        serializer.SerializeValue(ref bday);
        serializer.SerializeValue(ref createwhen);
        serializer.SerializeValue(ref History);
        serializer.SerializeValue(ref profilePicBase64);
        serializer.SerializeValue(ref fragments);
        serializer.SerializeValue(ref gems);
        serializer.SerializeValue(ref rankpoint);
        serializer.SerializeValue(ref rating);
        serializer.SerializeValue(ref virgin);
        serializer.SerializeValue(ref admin);
    }
}
