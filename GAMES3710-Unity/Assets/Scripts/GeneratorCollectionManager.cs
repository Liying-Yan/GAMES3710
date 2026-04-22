using UnityEngine;

public class GeneratorCollectionManager : MonoBehaviour
{
    [Header("Collection Settings")]
    public int totalRequired = 3;

    [Header("Subtitle Text")]
    public string multipleRemainingText = "Still need {0} more items.";
    public string oneRemainingText = "Still need 1 more item.";
    public string finishedText = "That should be everything. I can start the generator now.";

    private int collectedCount = 0;

    public int CollectedCount
    {
        get { return collectedCount; }
    }

    public int RemainingCount
    {
        get { return Mathf.Max(0, totalRequired - collectedCount); }
    }

    public void RegisterCollected()
    {
        collectedCount++;

        int remaining = RemainingCount;

        if (GuideSubtitleUI.Instance != null)
        {
            if (remaining > 1)
            {
                GuideSubtitleUI.Instance.Show(string.Format(multipleRemainingText, remaining), 3f);
            }
            else if (remaining == 1)
            {
                GuideSubtitleUI.Instance.Show(oneRemainingText, 3f);
            }
            else
            {
                GuideSubtitleUI.Instance.Show(finishedText, 3f);
            }
        }

        Debug.Log("Collected: " + collectedCount + " / " + totalRequired);
    }
}