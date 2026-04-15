using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapTargetManager : MonoBehaviour
{
    public List<SnapTarget> SnapTargets;

    public bool TryGetSnapPoint(Vector3 legWorldPos, out Vector3 snapPoint, out SnapTarget snapTarget)
    {
        foreach(SnapTarget st in SnapTargets)
        {
            if (st.isOccupied) continue;

            float distance = Vector3.Distance(legWorldPos, st.transform.position);
            if(distance <= st.snapRange)
            {
                snapPoint = st.transform.position;
                snapTarget = st;
                return true;
            }
        }

        snapPoint = Vector3.zero;
        snapTarget = null;
        return false;
    }

}
