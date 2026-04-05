
using System.Collections.Generic;

public class JumperWireComponent : CircuitComponent
{
    public PlugSnappable plugA;
    public PlugSnappable plugB;
    public float wireResistance = 0.0001f;
    private CircuitSolver solver;

    private void Awake()
    {
        solver = FindAnyObjectByType<CircuitSolver>();

        legs = new ComponentLeg[2];
        if(plugA != null) legs[0] = plugA.leg;
        if(plugB != null) legs[1] = plugB.leg;
    }

    public override void Simulate(CircuitSolver solver)
    {
        if (!IsFullyConnected()) return;

        float current = GetVoltageDrop(0, 1) / wireResistance;
    }

    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes)
    {
        //Node nodeA = legs[0].node;
        //Node nodeB = legs[1].node; 
        //float g = 1f / wireResistance;

        //int indexA = unknownNodes.IndexOf(nodeA); //we get -1 if node is not found in the list i.e, node is known
        //int indexB = unknownNodes.IndexOf(nodeB);


        ////situation 1: when both nodes are unknown
        //if (indexA >= 0 && indexB >= 0)
        //{
        //    G[indexA, indexA] += g;
        //    G[indexB, indexB] += g;
        //    G[indexA, indexB] -= g;
        //    G[indexB, indexA] -= g;
        //}
        ////situation 2: when A is known and B unknown
        //else if (indexA < 0 && indexB >= 0)
        //{
        //    G[indexB, indexB] += g;
        //    I[indexB] += g * nodeA.voltage;
        //}
        ////situation 3: when B is known and A unknown
        //else if (indexA >= 0 && indexB < 0)
        //{
        //    G[indexA, indexA] += g;
        //    I[indexA] += g * nodeB.voltage;
        //}
    }

    public override void ResetState()
    {
        //no reset needed
    }

    public override void SetValue(float value)
    {
        
    }

    public void OnPlugSnapped()
    {
        if(plugA.isSnapped && plugB.isSnapped)
        {
            solver.RegisterComponent(this);
        }
        AudioManager.instance.PlayAttachSound();
    }

    public void OnPlugUnsnapped()
    {
        solver.UnRegisterComponent(this);
        AudioManager.instance.PlayDetachSound();
    }


}
