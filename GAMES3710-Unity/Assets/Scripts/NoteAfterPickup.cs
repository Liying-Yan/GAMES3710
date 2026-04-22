using System.Collections;
using UnityEngine;

public class NoteAfterPickup : MonoBehaviour
{
    public ItemPickup itemPickup;

    [TextArea(3, 8)]
    public string noteContent = "Maintenance Notice:\nAll maintenance tools have been moved to the basement storage room.";

    [TextArea]
    public string afterHint = "Basement storage room... maybe I can find something there.";

    private bool hasTriggered = false;

    private void Update()
    {
        if (hasTriggered) return;
        if (itemPickup == null) return;

        if (itemPickup.isPickedUp)
        {
            hasTriggered = true;

            if (GuideNoteUI.Instance != null)
            {
                GuideNoteUI.Instance.ShowNote(noteContent);
                StartCoroutine(WaitThenShowSubtitle());
            }
        }
    }

    private IEnumerator WaitThenShowSubtitle()
    {
        while (GuideNoteUI.Instance != null && GuideNoteUI.Instance.IsOpen)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        if (GuideSubtitleUI.Instance != null)
        {
            GuideSubtitleUI.Instance.Show(afterHint, 3f);
        }
    }
}