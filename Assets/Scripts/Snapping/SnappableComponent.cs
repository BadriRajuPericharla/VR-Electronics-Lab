using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public float componentHeightOffset = 0.03f;
    private XRGrabInteractable xrGrab;


    private CircuitSolver solver;
    private CircuitComponent component;

    private float holdReleaseTime = 1.5f;
    private Coroutine holdCoroutine;
    private bool justHoldReleased = false;

    private Vector3[] legsOriginalLocalPositions;


    private void Awake()
    {
        if (breadboardManager == null) breadboardManager = FindAnyObjectByType<BreadboardManager>();

        snapVisualizers = FindObjectsByType<SnapVisualizer>(FindObjectsSortMode.None);

        solver = FindAnyObjectByType<CircuitSolver>();
        component = GetComponent<CircuitComponent>();    

        xrGrab = GetComponent<XRGrabInteractable>();

        xrGrab.selectEntered.AddListener(OnGrabbed);
        xrGrab.selectExited.AddListener(OnReleased);
        legsOriginalLocalPositions = new Vector3[legs.Length];

        for(int i = 0; i < legs.Length; i++)
        {
            legsOriginalLocalPositions[i] = legs[i].transform.localPosition;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!isPlaced)
        {
            justHoldReleased = false;
        }

        if (isPlaced)
        {
            xrGrab.trackPosition = false;
            xrGrab.trackRotation = false;

            holdCoroutine = StartCoroutine(holdToRelease());
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

        UnsnapComponent();
        
    }


    public void UnsnapComponent()
    {
        justHoldReleased = true;
        isPlaced = false;

        xrGrab.trackPosition = true;
        xrGrab.trackRotation = true;


        for(int i = 0; i < legs.Length; i++)
        {
            if (legs[i].occupiedHoleKey != "")
            {
                breadboardManager.ReleaseHole(legs[i].occupiedHoleKey);
                legs[i].occupiedHoleKey = "";
            }

            legs[i].transform.localPosition = legsOriginalLocalPositions[i];
            legs[i].ClearSnap();
        }

        if (component != null)
        {
            solver.UnRegisterComponent(component);
            HapticController.instance.TriggerHaptics(0.3f, 0.3f, xrGrab);
            AudioManager.instance.PlayDetachSound();
        }
    }



    private void OnReleased(SelectExitEventArgs args)
    {
        if(holdCoroutine != null) StopCoroutine(holdCoroutine);

        if (isPlaced)
        {
            xrGrab.trackRotation = true;
            xrGrab.trackPosition = true;
        }

        for(int i = 0; i < legs.Length; i++)
        {
            snapVisualizers[i].Hide();
        }

        StartCoroutine(TrySnapNextFrame());
    }


    private IEnumerator TrySnapNextFrame()
    {
        yield return null;

        if (justHoldReleased)
        {
            justHoldReleased = false;
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
                    AudioManager.instance.PlayAttachSound();
                }
            }

        }
    }



    public bool TrySnapAllLegs()
    {

        Vector3[] pendingSnapPoints = new Vector3[legs.Length];
        GridRegion[] pendingRegions = new GridRegion[legs.Length];
        string[] pendingHoleKeys = new string[legs.Length];

        HashSet<string> assignedHoles = new HashSet<string>();


        //checking and storing the leg data, if even one of them can't snap return false
        for (int i = 0;  i < legs.Length; i++)
        {
            if (!breadboardManager.TryGetSnapPoint(legs[i].transform.position, out pendingSnapPoints[i], out pendingRegions[i], out pendingHoleKeys[i]))
            {
                return false;
            }

            if(assignedHoles.Contains(pendingHoleKeys[i])) return false;

            assignedHoles.Add(pendingHoleKeys[i]);
        }

        Vector3 midPoint = (pendingSnapPoints[0] + pendingSnapPoints[1]) / 2f;
        midPoint += Vector3.up * componentHeightOffset;
        transform.position = midPoint;

        


        for (int i = 0;i < legs.Length; i++)
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


        HashSet<string> targetedHoles = new HashSet<string>();

        Vector3[] snapPoints = new Vector3[legs.Length];
        bool[] isLegValid = new bool[legs.Length];
        bool allLegsValid = true;

        for(int i = 0; i <legs.Length; i++)
        {
            if (breadboardManager.TryGetSnapPoint(legs[i].transform.position, out Vector3 snapPoint, out _, out string holekey))
            {
                if (targetedHoles.Contains(holekey))
                {
                    isLegValid[i] = false;
                    allLegsValid = false;
                }
                else
                {
                    isLegValid[i] = true;
                    targetedHoles.Add(holekey);
                    snapPoints[i] = snapPoint;
                }
            }
            else
            {
                isLegValid[i] = false;
                allLegsValid = false;
            }
        }

        if (allLegsValid)
            for(int i = 0;i <legs.Length;i++) snapVisualizers[i].Show(snapPoints[i]);
        else
            for (int i = 0; i < legs.Length; i++) snapVisualizers[i].Hide();

    }

}
