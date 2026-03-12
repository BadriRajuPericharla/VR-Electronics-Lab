using System.Collections.Generic;
using UnityEngine;

public class CircuitSolver : MonoBehaviour
{
    [Header("Ref")]
    public BreadboardManager breadboardManager;

    [Header("Voltage")]
    public float supplyVoltage = 5f;
    public float solvedCurrent = 0f;

    private Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();
    private List<CircuitComponent> components = new List<CircuitComponent>();


    int nextNodeId;



    public void Solve()
    {
        nodeMap.Clear();
        nextNodeId = 0;


        //creating nodeMap
        foreach (CircuitComponent comp in components)
        {
            foreach (ComponentLeg leg in comp.legs)
            {
                if (!leg.isSnapped) continue;

                string key = GetKeyNode(leg);
                if (!nodeMap.ContainsKey(key))
                {
                    nodeMap[key] = new Node(nextNodeId++);
                }
                leg.node = nodeMap[key];
            }
        }

        MergeWireNodes();

        //setting battery
        foreach (CircuitComponent comp in components)
        {
            if (comp is BatteryComponent battery)
            {
                if (battery.legs[0].node != null && battery.legs[1].node != null)
                {
                    battery.legs[0].node.voltage = supplyVoltage;
                    battery.legs[1].node.voltage = 0f;
                }
            }
        }

        CalculateVoltages();

        foreach (CircuitComponent comp in components)
        {
            comp.Simulate(this);
        }
    }

    public void RegisterComponent(CircuitComponent comp)
    {
        if (!components.Contains(comp))
            components.Add(comp);
        Solve();
    }

    public void UnRegisterComponent(CircuitComponent comp)
    {
        components.Remove(comp);

        foreach (ComponentLeg leg in comp.legs)
        {
            leg.node = null;
        }

        comp.ResetState();
        Solve();
    }

    private string GetKeyNode(ComponentLeg leg)
    {
        if (leg.snapTarget != null)
        {
            return leg.snapTarget.nodeKey;
        }


        GridRegion legRegion = leg.snappedRegion;

        Vector3 legLocalPos = breadboardManager.breadboardRoot.InverseTransformPoint(leg.transform.position);
        Vector3 legPosRelToRegion = legLocalPos - legRegion.localOrigin;

        if (legRegion.connectivityType == GridRegion.ConnectivityType.rowsConnected)
        {
            int col;
            if (legRegion.isSegmentedCol)
            {
                col = 0;
                float minDistance = Mathf.Infinity;
                for (int c = 0; c < legRegion.columns; c++)
                {
                    float colZPos = legRegion.GetColumnXOffset(c);
                    float distance = Mathf.Abs(legPosRelToRegion.z - colZPos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        col = c;
                    }
                }
            }
            else
            {
                col = Mathf.RoundToInt(legPosRelToRegion.z / legRegion.columnSpacing);
                col = Mathf.Clamp(col, 0, legRegion.columns - 1);
            }

            return $"{legRegion.regionName}_col_{col}";
        }
        else
        {
            int col = Mathf.RoundToInt(legPosRelToRegion.z / legRegion.columnSpacing);
            col = Mathf.Clamp(col, 0, legRegion.columns - 1);


            if (legRegion.isSegmentedCol)
            {
                int segmentIndex = col / legRegion.segmentedColSize;
                return $"{legRegion.regionName}_seg_{segmentIndex}";
            }
            else
            {
                return $"{legRegion.regionName}_rail";
            }
        }
    }

    private void CalculateVoltages()
    {
        float totalResistance = 0f;
        float totalForwardVoltage = 0f;
        float highVoltage = -1f;
        float lowVoltage = -1f;
        bool batteryOn = false;

        foreach (CircuitComponent comp in components)
        {
            if (comp is ResisterComponent resister)
            {
                totalResistance += resister.resistance;
            }

            if (comp is LEDComponent led)
            {
                totalForwardVoltage += led.forwardVoltage;
            }

            if (comp is BatteryComponent battery)
            {
                batteryOn = battery.isPowerOn;

                if (battery.legs[0].node != null)
                {
                    highVoltage = battery.legs[0].node.voltage;
                }

                if (battery.legs[1].node != null)
                {
                    lowVoltage = battery.legs[1].node.voltage;
                }
                
            }

        }

        if (!batteryOn)
        {
            return;
        }

        if (highVoltage < 0f || lowVoltage < 0f)
        {
            return;
        }

        float availableVoltage = highVoltage - lowVoltage;
        if (availableVoltage <= 0f)
        {
            return;
        }

        if (totalResistance <= 0f)
        {
            solvedCurrent = 999f;
            return;
        }

        float voltageAcrossResistor = availableVoltage - totalForwardVoltage;
        if (voltageAcrossResistor <= 0f)
        {
            solvedCurrent = 0f;
            return;
        }

        solvedCurrent = voltageAcrossResistor / totalResistance;


        foreach (var kvp in nodeMap)
        {
            if (kvp.Value.voltage == -1f)
            {
                kvp.Value.voltage = lowVoltage + totalForwardVoltage;
            }
        }
    }

    private void MergeWireNodes()
    {
        foreach (CircuitComponent comp in components)
        {
            if (comp is JumperWireComponent wireComponent)
            {
                if (wireComponent.legs[0].node == null) continue;
                if (wireComponent.legs[1].node == null) continue;

                Node nodeA = wireComponent.legs[0].node;
                Node nodeB = wireComponent.legs[1].node;

                if (nodeA == null || nodeB == null) continue;
                if (nodeA == nodeB) continue;

                MergeIntoOneNode(nodeA, nodeB);
            }
        }
    }

    private void MergeIntoOneNode(Node keepNode, Node removeNode)
    {
        if (keepNode == null || removeNode == null) return;

        foreach(CircuitComponent comp in components)
        {
            foreach(ComponentLeg leg in comp.legs)
            {
                if (leg.node == removeNode)
                {
                    leg.node = keepNode;
                }
            }
        }

        //update nodeMap

        foreach(var key in new List<string>(nodeMap.Keys))
        {
            if (nodeMap[key] == removeNode)
            {
                nodeMap[key] = keepNode;
            }
        }
    }
}
