using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    [SerializeField] private List<Piecer> piecers = new List<Piecer>();

    public int deckid;
    public bool maindeck;
    public Mat mat;
    public IReadOnlyList<Piecer> Piecers => piecers;
    public string name = "";

    public int getdex(Piecer a)
    {
        return piecers.IndexOf(a);
    }

    public void AddPiecer(Piecer newPiecer)
    {
        if (newPiecer == null)
        {
            Debug.LogWarning("Tried to add a null Piecer to the deck.");
            return;
        }

        piecers.Add(newPiecer);
        Debug.Log($"Added {newPiecer.name} to deck.");
    }

    // Remove a Piecer by reference
    public void RemovePiecer(Piecer target)
    {
        if (piecers.Remove(target))
        {
            Debug.Log($"Removed {target.name} from deck.");
        }
        else
        {
            Debug.LogWarning("Tried to remove a Piecer that wasn't in the deck.");
        }
    }

    // Remove by name
    public void RemovePiecerByName(string piecerName)
    {
        Piecer found = piecers.Find(p => p.name == piecerName);
        if (found != null)
            RemovePiecer(found);
        else
            Debug.LogWarning($"No Piecer named '{piecerName}' found in deck.");
    }

    // Get a Piecer by name
    public Piecer GetPiecer(string piecerName)
    {
        return piecers.Find(p => p.name == piecerName);
    }

    // Clone a specific Piecer
    public Piecer ClonePiecer(string piecerName)
    {
        Piecer original = GetPiecer(piecerName);
        if (original == null)
        {
            Debug.LogWarning($"No Piecer named '{piecerName}' found to clone.");
            return null;
        }

        return original.Clone();
    }

    // Clear the entire deck
    public void Clear()
    {
        piecers.Clear();
        Debug.Log("Deck cleared.");
    }

    // Shuffle deck (optional)
    public void Shuffle()
    {
        for (int i = 0; i < piecers.Count; i++)
        {
            Piecer temp = piecers[i];
            int randomIndex = Random.Range(i, piecers.Count);
            piecers[i] = piecers[randomIndex];
            piecers[randomIndex] = temp;
        }
        Debug.Log("Deck shuffled.");
    }
}
