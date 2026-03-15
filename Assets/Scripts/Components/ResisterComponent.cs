using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResisterComponent : CircuitComponent
{
    [Header("Resistance")]
    public float resistance = 220f;

    public override void Simulate(CircuitSolver solver)
    {
        if(!IsFullyConnected()) return;

        float current = GetVoltageDrop(0, 1) / resistance;

    }

    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes)
    {
        Node nodeA = legs[0].node;
        Node nodeB = legs[1].node;

        float g = 1f / resistance;

        int indexA = unknownNodes.IndexOf(nodeA); //we get -1 if node is not found in the list i.e, node is known
        int indexB = unknownNodes.IndexOf(nodeB);


        //situation 1: when both nodes are unknown
        if (indexA >= 0 && indexB >= 0)
        {
            G[indexA, indexA] += g;
            G[indexB, indexB] += g;
            G[indexA, indexB] -= g;
            G[indexB, indexA] -= g;
        }
        //situation 2: when A is known and B unknown
        else if(indexA < 0 && indexB >= 0)
        {
            G[indexB, indexB] += g;
            I[indexB] += g * nodeA.voltage;
        }
        //situation 3: when B is known and A unknown
        else if (indexA >= 0 && indexB < 0)
        {
            G[indexA, indexA] += g;
            I[indexA] += g * nodeB.voltage;
        }
    }

    public override void ResetState()
    {
        //no reset required
    }
}
