using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapacitorComponent : CircuitComponent
{


    [Header("Capacitor Properties")]
    public float capacitance = 0.001f;
    private float V_prev = 0f;

    private CircuitSolver solver;

    private void Start()
    {
        solver = GetComponent<CircuitSolver>();
    }

    public void OnSnapped()
    {
        solver.SetHasDynamicComponents(true);
        solver.RegisterComponent(this);
    }

    public void OnUnsnapped()
    {
        solver.UnRegisterComponent(this);

        solver.SetHasDynamicComponents(false);
    }


    public override void Simulate(CircuitSolver solver)
    {
        //nothing to visualize
    }
    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes, float deltaTime)
    {
        if (deltaTime <= 0f) return;


        Node nodeA = legs[0].node;
        Node nodeB = legs[1].node;


        if(nodeA == null || nodeB == null) return;

        float G_eq = capacitance / deltaTime;
        float I_eq = G_eq * V_prev;

        int indexA = unknownNodes.IndexOf(nodeA);
        int indexB = unknownNodes.IndexOf(nodeB);

        //situation 1: when both nodes are unknown
        if (indexA >= 0 && indexB >= 0)
        {
            G[indexA, indexA] += G_eq;
            G[indexB, indexB] += G_eq;
            G[indexA, indexB] -= G_eq;
            G[indexB, indexA] -= G_eq;
        }
        //situation 2: when A is known and B unknown
        else if (indexA < 0 && indexB >= 0)
        {
            G[indexB, indexB] += G_eq;
            I[indexB] += G_eq * nodeA.voltage;
        }
        //situation 3: when B is known and A unknown
        else if (indexA >= 0 && indexB < 0)
        {
            G[indexA, indexA] += G_eq;
            I[indexA] += G_eq * nodeB.voltage;
        }

        if(indexA >= 0) I[indexA] -= I_eq;
        if(indexB >= 0) I[indexB] += I_eq;
    }

    public override void SetValue(float value)
    {
        capacitance = value;
    }

    public override void UpdateState(float deltaTime, Dictionary<string, Node> nodeMap)
    {
        if (legs[0].node == null || legs[1].node == null) return;

        V_prev = legs[0].node.voltage - legs[1].node.voltage;
        
    }

    public override void ResetState()
    {
        V_prev = 0f;
    }
}
