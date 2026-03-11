
public class JumperWireComponent : CircuitComponent
{
    public PlugSnappable plugA;
    public PlugSnappable plugB;

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
        //no simulation needed for wire
    }

    public override void ResetState()
    {
        //no reset needed
    }

    public void OnPlugSnapped()
    {
        if(plugA.isSnapped && plugB.isSnapped)
        {
            solver.RegisterComponent(this);
        }
    }

    public void OnPlugUnsnapped()
    {
        solver.UnRegisterComponent(this);
    }
}
