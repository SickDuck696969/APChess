using SQLite4Unity3d;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static PlayerDataDB;


public class InventoryDatabase
{
    private SQLiteConnection connection;

    public InventoryDatabase(string dbName = "Inventory.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<InventoryDataDB>();

        Debug.Log("Inventory DB ready at: " + path);
    }

    public void AddToInventory(string userId, int capsuleId)
    {
        var data = new InventoryDataDB
        {
            user_id = userId,
            capsuleid = capsuleId
        };

        connection.Insert(data);
    }

    public InventoryDataDB GetInventoryItem(int inventoryId)
    {
        return connection.Find<InventoryDataDB>(inventoryId);
    }

    public List<InventoryDataDB> GetPlayerInventory(string userId)
    {
        return connection
            .Table<InventoryDataDB>()
            .Where(i => i.user_id == userId)
            .ToList();
    }

    public void RemoveFromInventory(int inventoryId)
    {
        connection.Delete<InventoryDataDB>(inventoryId);
    }

    public void RemoveCapsuleFromPlayer(string userId, int capsuleId)
    {
        var item = connection
            .Table<InventoryDataDB>()
            .FirstOrDefault(i => i.user_id == userId && i.capsuleid == capsuleId);

        if (item != null)
            connection.Delete(item);
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}

public class StorageDatabase
{
    private SQLiteConnection connection;

    public StorageDatabase(string dbName = "Storage.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<StorageDataDB>();

        Debug.Log("Storage DB ready at: " + path);
    }

    public void SaveStorage(int storageid, int capsuleId)
    {
        var data = new StorageDataDB
        {
            storageid = storageid,
            capsuleid = capsuleId
        };

        connection.Insert(data);
    }

    public List<StorageDataDB> GetStorage(int storageId)
    {
        return connection
            .Table<StorageDataDB>()
            .Where(x => x.storageid == storageId)
            .ToList();
    }


    public List<StorageDataDB> GetAllStorage()
    {
        return connection.Table<StorageDataDB>().ToList();
    }

    public void DeleteAllStorage()
    {
        connection.DeleteAll<StorageDataDB>();
    }


    public void DeleteStorage(int storageId)
    {
        connection.Delete<StorageDataDB>(storageId);
    }

    public void DeleteFromStorage(int capid)
    {
        connection.Execute(
            $"DELETE FROM StorageDataDB WHERE capsuleid = {capid}"
        );
    }


    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}

public class CapsuleDatabase
{
    private SQLiteConnection connection;

    public CapsuleDatabase(string dbName = "Capsule.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<CapsuleDataDB>();

        Debug.Log("Capsule DB ready at: " + path);
    }

    public void SaveCapsule(int itemId, string type, string? variant, int fragments, bool featured)
    {
        var data = new CapsuleDataDB
        {
            itemid = itemId,
            type = type,
            variant = variant,
            fragments = fragments,
            featured = featured
        };

        connection.Insert(data);
    }

    public List<CapsuleDataDB> GetCapsulesByType(string type)
    {
        return connection
            .Table<CapsuleDataDB>()
            .Where(c => c.type == type)
            .ToList();
    }

    public CapsuleDataDB GetCapsule(int capid)
    {
        return connection.Find<CapsuleDataDB>(capid);
    }

    public List<CapsuleDataDB> GetAllCapsules()
    {
        return connection.Table<CapsuleDataDB>().ToList();
    }

    public void DeleteCapsule(int capid)
    {
        connection.Delete<CapsuleDataDB>(capid);
    }

    public void DeleteAllCapsules()
    {
        connection.DeleteAll<CapsuleDataDB>();
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}

public class PiercerDatabase
{
    private SQLiteConnection connection;

    public PiercerDatabase(string dbName = "Piercers.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<PiercerDataDB>();

        Debug.Log("Piercer DB ready at: " + path);
    }

    public void SavePiercer(string name, string alt, int piercerID)
    {
        var data = new PiercerDataDB
        {
            name = name,
            alt = alt,
            PiercerID = piercerID
        };

        connection.Insert(data);
    }

    public PiercerDataDB GetPiercer(int id)
    {
        return connection.Find<PiercerDataDB>(id);
    }

    public List<PiercerDataDB> GetAllPiercers()
    {
        return connection.Table<PiercerDataDB>().ToList();
    }

    public void DeletePiercer(int id)
    {
        connection.Delete<PiercerDataDB>(id);
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}

public class DeckDatabase
{
    private SQLiteConnection connection;

    public DeckDatabase(string dbName = "Decks.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<DeckDataDB>();
        Debug.Log("Deck DB ready at: " + path);
    }

    public void SaveDeck(
        string playerId,
        string name,
        int matId,
        int slot1,
        int slot2,
        int slot3,
        int slot4,
        int slot5,
        bool maindeck
    )
    {
        var data = new DeckDataDB
        {
            player_id = playerId,
            name = name,
            mat_id = matId,
            slot1 = slot1,
            slot2 = slot2,
            slot3 = slot3,
            slot4 = slot4,
            slot5 = slot5,
            maindeck = maindeck
        };

        connection.Insert(data);
    }

    public bool UpdateDeckSlot(string playerId, int deckId, int slotNumber, int newValue, string name, bool maindeck)
    {
        if (slotNumber < 0 || slotNumber > 5)
        {
            Debug.LogError("Invalid slot number. Must be 1–5.");
            return false;
        }

        var deck = connection.Table<DeckDataDB>()
            .FirstOrDefault(d => d.deck_id == deckId && d.player_id == playerId);

        deck.name = name;
        deck.maindeck = maindeck;

        if (deck == null)
        {
            Debug.LogError("Deck not found for player.");
            return false;
        }

        switch (slotNumber)
        {
            case 0: deck.mat_id = newValue; break;
            case 1: deck.slot1 = newValue; break;
            case 2: deck.slot2 = newValue; break;
            case 3: deck.slot3 = newValue; break;
            case 4: deck.slot4 = newValue; break;
            case 5: deck.slot5 = newValue; break;
        }

        connection.Update(deck);
        return true;
    }


    public DeckDataDB GetDeck(int deckId)
    {
        return connection.Find<DeckDataDB>(deckId);
    }

    public List<DeckDataDB> GetPlayerDecks(string playerId)
    {
        return connection
            .Table<DeckDataDB>()
            .Where(d => d.player_id == playerId)
            .ToList();
    }

    public void DeleteDeck(int deckId)
    {
        connection.Delete<DeckDataDB>(deckId);
    }

    public void DeleteAllDecks()
    {
        connection.DeleteAll<DeckDataDB>();
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}


public class PlayerDatabase
{
    private SQLiteConnection connection;

    public PlayerDatabase(string dbName = "playerdata.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<PlayerDataDB>();
        Debug.Log("Player DB ready at: " + path);
    }
    public async Task<bool> SetCurrencyAsync(string userId, int gems, int fragments)
    {
        return await Task.Run(() =>
        {
            try
            {
                var player = connection.Find<PlayerDataDB>(userId);
                if (player == null) return false;

                player.gems = gems;
                player.fragments = fragments;

                connection.Update(player);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("SetCurrencyAsync failed: " + e);
                return false;
            }
        });
    }

    public async Task<bool> SavePlayerAsync(PlayerDataDB data)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (string.IsNullOrEmpty(data.createwhen))
                    data.createwhen = System.DateTime.UtcNow.ToString("o");

                connection.InsertOrReplace(data);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("SavePlayerAsync failed: " + e);
                return false;
            }
        });
    }

    public PlayerDataDB GetPlayer(string userId)
    {
        return connection.Find<PlayerDataDB>(userId);
    }

    public async Task<PlayerDataDB> GetPlayerByLoginAsync(string username, string password)
    {
        return await Task.Run(() =>
        {
            try
            {
                return connection
                    .Table<PlayerDataDB>()
                    .FirstOrDefault(p =>
                        p.username == username &&
                        p.password == password
                    );
            }
            catch (System.Exception e)
            {
                Debug.LogError("GetPlayerByLoginAsync failed: " + e);
                return null;
            }
        });
    }

    public async Task<PlayerDataDB> GetPlayerByggLoginAsync(string sub)
    {
        return await Task.Run(() =>
        {
            try
            {
                return connection
                    .Table<PlayerDataDB>()
                    .FirstOrDefault(p =>
                        p.sub == sub
                    );
            }
            catch (System.Exception e)
            {
                Debug.LogError("GetPlayerByggLoginAsync failed: " + e);
                return null;
            }
        });
    }

    public async Task<List<PlayerDataDB>> GetAllPlayersAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                return connection.Table<PlayerDataDB>().ToList();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return new List<PlayerDataDB>();
            }
        });
    }

    public async Task<bool> DeletePlayerAsync(string userId)
    {
        return await Task.Run(() =>
        {
            try
            {
                connection.Delete<PlayerDataDB>(userId);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        });
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }


}

public class BannedDatabase
{
    private SQLiteConnection connection;

    public BannedDatabase(string dbName = "Banned.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<BannedDataDB>();

        Debug.Log("Banned DB ready at: " + path);
    }

    /// <summary>
    /// Create or replace a ban case
    /// </summary>
    public void BanUser(
        string userId,
        int durationDays,
        string caseTitle,
        string statement,
        bool pending = true
    )
    {
        var data = new BannedDataDB
        {
            user_id = userId,
            date_since = System.DateTime.UtcNow.ToString("o"),
            duration_days = durationDays,
            Case = caseTitle,
            Statement = statement,
            status = pending ? "pending" : "active",
            pending = pending
        };

        connection.InsertOrReplace(data);
    }

    public BannedDataDB GetBan(string userId)
    {
        return connection.Find<BannedDataDB>(userId);
    }

    public List<BannedDataDB> GetAllBans()
    {
        return connection.Table<BannedDataDB>().ToList();
    }

    public List<BannedDataDB> GetPendingBans()
    {
        return connection
            .Table<BannedDataDB>()
            .Where(b => b.pending)
            .ToList();
    }

    public List<BannedDataDB> GetActiveBans()
    {
        return connection
            .Table<BannedDataDB>()
            .Where(b => !b.pending && b.status == "active")
            .ToList();
    }

    /// <summary>
    /// Approves a pending ban and starts the timer
    /// </summary>
    public void ApproveBan(string userId)
    {
        var ban = GetBan(userId);
        if (ban == null) return;

        ban.pending = false;
        ban.status = "active";
        ban.date_since = System.DateTime.UtcNow.ToString("o");

        connection.Update(ban);
    }

    /// <summary>
    /// Rejects a ban request but keeps the record
    /// </summary>
    public void RejectBan(string userId, string reason = "Rejected")
    {
        var ban = GetBan(userId);
        if (ban == null) return;

        ban.status = "rejected";
        ban.pending = false;
        ban.Statement += "\n\n[REJECTED]: " + reason;

        connection.Update(ban);
    }

    /// <summary>
    /// Removes the ban completely
    /// </summary>
    public void UnbanUser(string userId)
    {
        connection.Delete<BannedDataDB>(userId);
    }

    /// <summary>
    /// Checks if a user is currently banned
    /// </summary>
    public bool IsUserBanned(string userId)
    {
        var ban = GetBan(userId);
        if (ban == null) return false;

        if (ban.pending || ban.status != "active")
            return false;

        // Permanent ban
        if (ban.duration_days < 0)
            return true;

        var since = System.DateTime.Parse(ban.date_since);
        bool active =
            since.AddDays(ban.duration_days) > System.DateTime.UtcNow;

        if (!active)
        {
            ban.status = "expired";
            connection.Update(ban);
        }

        return active;
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}


public class MatchDatabase
{
    private SQLiteConnection connection;

    public MatchDatabase(string dbName = "Matches.sqlite")
    {
        string path = Path.Combine(Application.persistentDataPath, dbName);

        connection = new SQLiteConnection(
            path,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );

        connection.CreateTable<MatchDataDB>();

        Debug.Log("Match DB ready at: " + path);
    }

    public void SaveMatch(string player1Id, string player2Id, string rawLog)
    {
        var data = new MatchDataDB
        {
            player1_id = player1Id,
            player2_id = player2Id,
            raw_log = rawLog
        };

        connection.Insert(data);
    }

    public MatchDataDB GetMatch(int matchId)
    {
        return connection.Find<MatchDataDB>(matchId);
    }

    public List<MatchDataDB> GetMatchesByPlayer(string userId)
    {
        return connection.Table<MatchDataDB>()
            .Where(m => m.player1_id == userId || m.player2_id == userId)
            .ToList();
    }

    public List<MatchDataDB> GetAllMatches()
    {
        return connection.Table<MatchDataDB>().ToList();
    }

    public void DeleteMatch(int matchId)
    {
        connection.Delete<MatchDataDB>(matchId);
    }

    public void DeleteAllMatches()
    {
        connection.DeleteAll<MatchDataDB>();
    }

    public void Close()
    {
        connection?.Close();
        connection = null;
    }
}

public class DeckDataDB
{
    [PrimaryKey, AutoIncrement]
    public int deck_id { get; set; }

    public string player_id { get; set; }
    public string name { get; set; }
    public int mat_id { get; set; }

    public int slot1 { get; set; }
    public int slot2 { get; set; }
    public int slot3 { get; set; }
    public int slot4 { get; set; }
    public int slot5 { get; set; }
    public bool maindeck {  get; set; }
}


public class StorageDataDB
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    public int storageid { get; set; }
    public int capsuleid { get; set; }
}


public class InventoryDataDB
{
    [PrimaryKey, AutoIncrement]
    public int inventoryid { get; set; }

    public string user_id { get; set; }
    public int capsuleid { get; set; }
}


public class CapsuleDataDB
{
    [PrimaryKey, AutoIncrement]
    public int capid { get; set; }

    public int itemid { get; set; }
    public string type { get; set; }
    [AllowNull]
    public string variant {  get; set; }
    public int fragments { get; set; }

    public bool featured { get; set; }
}


public class PiercerDataDB
{
    [PrimaryKey]
    public int ID { get; set; }

    public string name { get; set; }
    public string alt { get; set; }
    public int PiercerID { get; set; }
}

public class BannedDataDB
{
    [PrimaryKey]
    public string user_id { get; set; }
    public string date_since { get; set; }
    public int duration_days { get; set; }
    public string Case { get; set; }
    public string Statement { get; set; }
    public string status { get; set; }
    public bool pending { get; set; }
}


public class PlayerDataDB
{
    [PrimaryKey]
    public string user_id { get; set; }

    public string username { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string bday { get; set; }
    public int rankpoint { get; set; }
    public int rating { get; set; }
    public string createwhen { get; set; }
    public string History { get; set; }
    public string profilePicBase64 { get; set; }
    public int gems { get; set; }
    public int fragments { get; set; }
    public bool virgin { get; set; }
    public string inventoryid { get; set; }
    public bool admin { get; set; }
    public string sub {  get; set; }

    public class MatchDataDB
    {
        [PrimaryKey, AutoIncrement]
        public int match_id { get; set; }

        public string player1_id { get; set; }
        public string player2_id { get; set; }

        public string raw_log { get; set; }
    }
}
