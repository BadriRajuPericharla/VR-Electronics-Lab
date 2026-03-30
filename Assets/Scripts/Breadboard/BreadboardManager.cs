using System.Collections.Generic;
using UnityEngine;

public class BreadboardManager : MonoBehaviour
{
    [Header("Breadboard Reference")]
    public Transform breadboardRoot;

    [Header("Grid Regions")]
    public GridRegion topPowerRail;
    public GridRegion mainAreaUpper;
    public GridRegion mainAreaLower;
    public GridRegion bottomPowerRail;

    [Header("Debug")]
    public bool showGizmos = true;

    private List<GridRegion> allRegions;
    private Dictionary<string, bool> occupiedHoles = new Dictionary<string, bool>();

    private void Start()
    {
        InitializeAllRegions();
        InitilizeHoles();
    }

    private void InitializeAllRegions()
    {
        topPowerRail.CalculateBounds(breadboardRoot);
        mainAreaUpper.CalculateBounds(breadboardRoot);
        mainAreaLower.CalculateBounds(breadboardRoot);
        bottomPowerRail.CalculateBounds(breadboardRoot);

        allRegions = new List<GridRegion>
        {
            topPowerRail,
            mainAreaUpper,
            mainAreaLower,
            bottomPowerRail
        };
    }

    private void InitilizeHoles()
    {
        foreach(GridRegion region in allRegions)
        {
            for (int row = 0; row < region.rows; row++)
            {
                for (int col = 0; col < region.columns; col++)
                {
                    Vector2Int gridCoord = new Vector2Int(col, row);
                    string holeKey = $"{region.regionName}_{gridCoord.x}_{gridCoord.y}";
                    occupiedHoles[holeKey] = false;
                }
            }
        }
    }

    public bool TryGetSnapPoint(Vector3 legWorldPos, out Vector3 snapPoint, out GridRegion snapRegion)
    {
        Vector3 legLocalPos = breadboardRoot.InverseTransformPoint(legWorldPos);

        foreach (GridRegion region in allRegions)
        {
            if (region.localBounds.Contains(legLocalPos))
            {
                Vector2Int gridCoord = CalculateNearestCoord(legLocalPos, region);
                Vector3 localHolePos = GetLocalHolePosition(gridCoord, region);

                float distance = Vector3.Distance(localHolePos, legLocalPos);



                string holeKey = $"{region.regionName}_{gridCoord.x}_{gridCoord.y}";
                occupiedHoles.TryGetValue(holeKey, out bool isOccupied);

                if (distance <= region.snapRange && !isOccupied)
                {
                    snapPoint = breadboardRoot.TransformPoint(localHolePos);
                    snapRegion = region;
                    return true;
                }
            }
        }

        snapPoint = Vector3.zero;
        snapRegion = null;
        return false;
    }

    private Vector2Int CalculateNearestCoord(Vector3 legLocalPos, GridRegion region)
    {
        Vector3 legPosRelToRegion = legLocalPos - region.localOrigin;

        //Rows along x-axis (to match the breadboard)
        int row = Mathf.RoundToInt(legPosRelToRegion.x / region.rowSpacing);
        row = Mathf.Clamp(row, 0, region.rows - 1);

        //Columns along z-axis (to match the breadboard)
        int col;
        if (region.isSegmentedCol)
        {
            col = 0;
            float minDistance = Mathf.Infinity;

            for (int c = 0; c < region.columns; c++)
            {
                float colZPos = region.GetColumnXOffset(c);
                float distance = Mathf.Abs(colZPos - legPosRelToRegion.z);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    col = c;
                }
            }
        }
        else
        {
            col = Mathf.RoundToInt(legPosRelToRegion.z / region.columnSpacing);
            col = Mathf.Clamp(col, 0, region.columns - 1);
        }

        return new Vector2Int(col, row);
    }

    private Vector3 GetLocalHolePosition(Vector2Int gridCoord, GridRegion region)
    {
        // SWAPPED: To match the breadboard's orientation
        float x = region.localOrigin.x + gridCoord.y * region.rowSpacing;  // Rows along X
        float z = region.localOrigin.z + region.GetColumnXOffset(gridCoord.x);  // Columns along Z
        float y = region.localOrigin.y;

        return new Vector3(x, y, z);
    }

    public void ReleaseHole(string holeKey)
    {
        if(occupiedHoles.ContainsKey(holeKey))
        {
            occupiedHoles[holeKey] = false;
        }
    }

    public void OccupiyHole(string holeKey)
    {
        if (occupiedHoles.ContainsKey(holeKey))
        {
            occupiedHoles[holeKey] = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (breadboardRoot == null) return;
        if (!Application.isPlaying) return;

        Color[] colors = { Color.red, Color.green, Color.cyan, Color.blue };
        int colorIndex = 0;

        foreach (GridRegion region in allRegions)
        {
            // Draw grid holes
            Gizmos.color = colors[colorIndex];

            for (int row = 0; row < region.rows; row++)
            {
                for (int col = 0; col < region.columns; col++)
                {
                    Vector2Int gridCoord = new Vector2Int(col, row);
                    Vector3 localHolePos = GetLocalHolePosition(gridCoord, region);
                    Vector3 worldPos = breadboardRoot.TransformPoint(localHolePos);


                    Gizmos.DrawWireSphere(worldPos, region.snapRange);
                }
            }

            colorIndex++;
        }
    }
}