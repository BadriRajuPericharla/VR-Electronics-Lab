using UnityEngine;

public class ComponentLeg : MonoBehaviour
{
    public string legName;

    [HideInInspector] public Node node = null;
    [HideInInspector] public bool isSnapped = false;
    [HideInInspector] public GridRegion snappedRegion = null;
    [HideInInspector] public SnapTarget snapTarget = null;
    [HideInInspector] public string occupiedHoleKey = "";

    public void ClearSnap()
    {
        node = null;
        snappedRegion = null;
        snapTarget = null;
        isSnapped = false;

    }
}
