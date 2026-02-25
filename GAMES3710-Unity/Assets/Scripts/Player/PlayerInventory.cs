using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private HashSet<ItemPickup> _keys = new HashSet<ItemPickup>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKey(ItemPickup key)
    {
        _keys.Add(key);
    }

    public bool HasKey(ItemPickup key)
    {
        return _keys.Contains(key);
    }

    public void RemoveKey(ItemPickup key)
    {
        _keys.Remove(key);
    }

    public void Clear()
    {
        _keys.Clear();
    }
}
