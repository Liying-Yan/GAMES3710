using UnityEngine;

public class PlayerHideState : MonoBehaviour
{
    public static PlayerHideState Instance { get; private set; }

    public bool IsHiding { get; private set; }

    /// <summary>The hiding spot the player is currently in (null when not hiding or spot is not checkable).</summary>
    public ICheckableHidingSpot CurrentSpot { get; set; }

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

    public void SetHiding(bool hiding)
    {
        IsHiding = hiding;
        if (!hiding) CurrentSpot = null;
    }
}
