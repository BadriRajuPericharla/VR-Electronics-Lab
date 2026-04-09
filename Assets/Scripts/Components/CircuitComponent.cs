using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CircuitComponent : MonoBehaviour
{
    public ComponentLeg[] legs;

    public abstract void Simulate(CircuitSolver solver);
    public abstract void Stamp(float[,] G, float[] I, List<Node> unknownNodes, float deltaTime);
    public abstract void UpdateState(float deltaTime, Dictionary<string, Node> nodeMap);
    public abstract void ResetState();
    public abstract void SetValue(float value);

    public bool IsFullyConnected()
    {
        foreach(ComponentLeg leg in legs)
        {
            if(leg.node == null) return false;
        }
        return true;
    }

    protected float GetVoltageDrop(int legA, int legB)
    {
        if(legs.Length < 2)
        {
            return 0f;
        }

        if (legs[legA].node == null || legs[legB].node == null) return 0f;

        return legs[legA].node.voltage - legs[legB].node.voltage;
    }


}
