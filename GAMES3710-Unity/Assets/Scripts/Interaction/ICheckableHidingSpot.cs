using UnityEngine;

public interface ICheckableHidingSpot
{
    /// <summary>Whether this hiding spot can be checked by enemies (Inspector toggle).</summary>
    bool CanBeChecked { get; }

    /// <summary>Where the enemy should navigate to before triggering the check.</summary>
    Transform CheckTarget { get; }

    /// <summary>Called by the enemy upon arrival – play reveal animation, then game over.</summary>
    void OnEnemyCheck();
}
