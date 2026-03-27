using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BatteryComponent : CircuitComponent
{
    [Header("Switch")]
    public bool isPowerOn = false;
    public XRSimpleInteractable simpleInteractable;

    public CircuitSolver solver;

    //[Header("Light Visuals")]
    //public Light powerLight;
    //public Color onColor = Color.green;
    //public Color offColor = Color.gray;


    private void Start()
    {
        solver = FindAnyObjectByType<CircuitSolver>();

        isPowerOn = true;
        OnPlaced();

        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnSwitchFlip);
        }
    }

    private void OnSwitchFlip(SelectEnterEventArgs args)
    {
        isPowerOn = !isPowerOn;
        //ApplyVisuals();

        if(solver != null)
        {
            solver.Solve();
        }
        
    }

    //private void ApplyVisuals()
    //{
    //    if(powerLight != null)
    //    {
    //        powerLight.color = isPowerOn ? onColor : offColor;
    //        powerLight.intensity = isPowerOn ? 1.5f : 0.2f;
    //    }
    //}

    public override void Simulate(CircuitSolver circuitSolver)
    {
        //battery can't simulate, it is just a power source
    }

    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes)
    {
        //no need to stamp battery into matrix, already voltages of battery set in editor
    }

    public override void ResetState()
    {
        //ApplyVisuals();
    }

    public void OnPlaced()
    {
        isPowerOn = true;
        //ApplyVisuals();
        
        foreach(ComponentLeg leg in legs)
        {
            leg.isSnapped = true;
            leg.snapTarget = leg.GetComponent<SnapTarget>();
        }

        solver.RegisterComponent(this);
    }
}
