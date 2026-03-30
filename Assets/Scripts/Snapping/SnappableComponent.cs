using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnappableComponent : MonoBehaviour
{
    [Header("References")]
    public BreadboardManager breadboardManager;
    public SnapVisualizer[] snapVisualizers;

    [Header("Legs")]
    public ComponentLeg[] legs;

    public bool isPlaced = false;
    private XRGrabInteractable xrGrab;


    private CircuitSolver solver;
    private CircuitComponent component;

    private float holdReleaseTime = 10f;
    private bool justHoldRleased = false;


    private void Awake()
    {
        solver = FindAnyObjectByType<CircuitSolver>();
        component = GetComponent<CircuitComponent>();    

        xrGrab = GetComponent<XRGrabInteractable>();

        xrGrab.selectEntered.AddListener(OnGrabbed);
        xrGrab.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isPlaced)
        {
            StartCoroutine(holdToRelease());
        }
    }

    private IEnumerator holdToRelease()
    {
        float timer = 0;

        while(timer < holdReleaseTime)
        {
            if (!xrGrab.isSelected) yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        justHoldRleased = true;
        isPlaced = false;


        foreach (ComponentLeg leg in legs)
        {
            if (leg.occupiedHoleKey != "")
            {
                breadboardManager.ReleaseHole(leg.occupiedHoleKey);
                leg.occupiedHoleKey = "";
            }

            leg.ClearSnap();
        }

        if (component != null)
        {
            solver.UnRegisterComponent(component);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        for(int i = 0; i < legs.Length; i++)
        {
            snapVisualizers[i].Hide();
        }

        StartCoroutine(TrySnapNextFrame());
    }


    private IEnumerator TrySnapNextFrame()
    {
        yield return null;

        if (justHoldRleased)
        {
            justHoldRleased = false;
            yield break;
        }



        if (TrySnapAllLegs())
        {
            isPlaced = true;


            if (component != null)
            {

                if(component is BatteryComponent battery)
                {
                    battery.OnPlaced(); 
                }
                else
                {
                    solver.RegisterComponent(component);
                }
            }

        }
    }



    public bool TrySnapAllLegs()
    {




        Vector3[] pendingSnapPoints = new Vector3[legs.Length];
        GridRegion[] pendingRegions = new GridRegion[legs.Length];
        string[] pendingHoleKeys = new string[legs.Length];


        //checking and storing the leg data, if even one of them can't snap return false
        for (int i = 0;  i < legs.Length; i++)
        {
            if (!breadboardManager.TryGetSnapPoint(legs[i].transform.position, out pendingSnapPoints[i], out pendingRegions[i], out pendingHoleKeys[i]))
            {
                return false;
            }
        }

        Vector3 midPoint = (pendingSnapPoints[0] + pendingSnapPoints[1]) / 2f;
        midPoint += Vector3.up * 0.03f;
        transform.position = midPoint;
        //transform.localRotation = Quaternion.identity;


        for(int i = 0;i < legs.Length; i++)
        {
            legs[i].transform.position = pendingSnapPoints[i];
            legs[i].isSnapped = true;
            legs[i].snappedRegion = pendingRegions[i];
            legs[i].occupiedHoleKey = pendingHoleKeys[i];
            breadboardManager.OccupyHole(pendingHoleKeys[i]);
        }


        return true;
    }


    private void Update()
    {
        if (isPlaced) return;
        if (!xrGrab.isSelected) return;

        int snapCount = 0;
        Vector3[] snapPoints = new Vector3[2];
        for(int i = 0; i <legs.Length; i++)
        {
            if (breadboardManager.TryGetSnapPoint(legs[i].transform.position, out Vector3 snapPoint, out _, out _))
            {
                snapCount++;
                snapPoints[i] = snapPoint;
            }
            else
            {
                snapVisualizers[i].Hide();
            }
        }

        if(snapCount >= 2)
        {
            for(int i = 0; i < legs.Length; i++)
            {
                snapVisualizers[i].Show(snapPoints[i]);
            }
        }

    }


}
