using System.Collections.Generic;
using UnityEngine;

public class CircuitSolver : MonoBehaviour
{
    [Header("Ref")]
    public BreadboardManager breadboardManager;

    [Header("Voltage")]
    public float supplyVoltage = 5f;

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
                if (battery.isPowerOn && battery.legs[0].node != null && battery.legs[1].node != null)
                {
                    battery.legs[0].node.voltage = supplyVoltage;
                    battery.legs[1].node.voltage = 0f;
                }
            }
        }

        if (IsCircuitValid())
        {
            SolveNodalAnalysis();
            foreach (CircuitComponent comp in components)
            {
                comp.Simulate(this);
            }
        }
        else
        {
            foreach(CircuitComponent comp in components)
            {
                comp.ResetState();
            }
        }


            
    }

    public void RegisterComponent(CircuitComponent comp)
    {
        if (!components.Contains(comp))
        {
            components.Add(comp);
            Solve();
        }

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

    public void SolveNodalAnalysis()
    {
        //pre-step for LED
        foreach (CircuitComponent comp in components)
        {
            if(comp is LEDComponent led)
            {
                Node anode = led.legs[0].node;
                Node cathode = led.legs[1].node;

                if (anode == null || cathode == null) continue;

                //case: 1 cathode is known & anode is unknown
                if(cathode.voltage != -1f && anode.voltage == -1f)
                {
                    if(supplyVoltage - cathode.voltage >= led.forwardVoltage)
                    {
                        anode.voltage = cathode.voltage + led.forwardVoltage;
                    }
                }
                //case: 1 cathode is unknown & anode is known
                else if (cathode.voltage == -1f && anode.voltage != -1f)
                {
                    if(anode.voltage >= led.forwardVoltage)
                    {
                        cathode.voltage = anode.voltage - led.forwardVoltage; //cathode should be lower than anode
                    }
                }
            }
        }



        List<Node> unknownNodes = new List<Node>();

        //step 1: get the unknown nodes
        foreach (var kvp in nodeMap)
        {
            if (kvp.Value.voltage == -1f)
            {
                unknownNodes.Add(kvp.Value);
            }
        }


        //setp 2: Create G-Matrix and I-Array
        float[,] G = new float[unknownNodes.Count, unknownNodes.Count];
        float[] I = new float[unknownNodes.Count];

        //step 3: Stamp the components
        foreach (CircuitComponent comp in components)
        {
            comp.Stamp(G, I, unknownNodes);
        }

        

        //step 4: solve the matrix
        float[] voltagesArray = SolveGaussian(G, I);

        //setp 5: assign voltages to unknown nodes
        for (int i = 0; i < unknownNodes.Count; i++)
        {
            unknownNodes[i].voltage = voltagesArray[i];
        }

    }

    public bool IsCircuitValid()
    {
        bool hasResistor = false;
        foreach(CircuitComponent comp in components)
        {
            if (comp is ResisterComponent) hasResistor = true;
        }

        if(!hasResistor) return false;

        foreach (CircuitComponent comp in components)
        {
            if(comp is BatteryComponent b && b.isPowerOn)
            {

                //if (HasCompletePath(b.legs[0].node, b.legs[1].node))
                //{
                //    return true;
                //}
                bool result = HasCompletePath(b.legs[0].node, b.legs[1].node);
                Debug.Log($"IsCircuitValid: hasResistor={hasResistor}, pathComplete={result}");
                return result;
            }
        }
        return false;

    }

    public bool HasCompletePath(Node startNode, Node endNode)
    {
        HashSet<Node> visited = new HashSet<Node>();
        Queue<Node> hasToCheck = new Queue<Node>();

        hasToCheck.Enqueue(startNode);
        visited.Add(startNode);

        while(hasToCheck.Count > 0)
        {
            Node currentNode = hasToCheck.Dequeue();

            if (currentNode == endNode)
            {
                return true;
            }

            foreach(CircuitComponent comp in components)
            {
                if(comp is BatteryComponent) continue;

                for(int i = 0; i < comp.legs.Length; i++)
                {
                    for(int j = 0; j < comp.legs.Length; j++)
                    {
                        if (i == j) continue;
                        if (comp.legs[i].node == currentNode)
                        {
                            Node nextNode = comp.legs[j].node;
                            if (nextNode == null) continue;

                            if (!visited.Contains(nextNode))
                            {
                                hasToCheck.Enqueue(nextNode);
                                visited.Add(nextNode);
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    //This is matrix solving function, copy pasted from claude., basic idea is
    //1. take one equation
    //2. get factor by deviding one quotient(factor is 2/1.2 or 3.2/4  0.5 + 2B + 3.2C = 4 | 3 + 1.2B + 4C = 1 ) this goes on until one unknown is remaining in the equation
    //3. now multiply that factor with one equation to remove that variable, this goes on until one equation is left
    //4. substitute the value got by solving
    //5. add to the list
    private float[] SolveGaussian(float[,] G, float[] I)
    {
        int n = I.Length;

        // Forward elimination
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (G[i, i] == 0) continue;
                float factor = G[j, i] / G[i, i];

                for (int k = i; k < n; k++)
                {
                    G[j, k] -= factor * G[i, k];
                }

                I[j] -= factor * I[i];
            }
        }

        // Back substitution
        float[] result = new float[n];

        for (int i = n - 1; i >= 0; i--)
        {
            result[i] = I[i];

            for (int j = i + 1; j < n; j++)
            {
                result[i] -= G[i, j] * result[j];
            }

            if (G[i, i] == 0) continue;
            result[i] /= G[i, i];
        }

        return result;
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
