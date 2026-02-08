using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private HashSet<ItemType> _items = new HashSet<ItemType>();

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

    public void AddItem(ItemType item)
    {
        _items.Add(item);
    }

    public bool HasItem(ItemType item)
    {
        return _items.Contains(item);
    }

    public void RemoveItem(ItemType item)
    {
        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
