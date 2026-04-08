using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlugSnappable : MonoBehaviour
{
    public BreadboardManager breadboardManager;
    public SnapTargetManager snapTargetManager;
    public SnapVisualizer snapVisualizer;
    public ComponentLeg leg;
    public JumperWireComponent parentWire;
    public SnapTarget occupiedTarget = null;

    public bool isSnapped = false;
    private XRGrabInteractable xrGrab;

    private float holdReleaseTime = 1.5f;
    private Coroutine holdCoroutine;
    private bool justHoldReleased = false;

    private void Awake()
    {
        xrGrab = GetComponent<XRGrabInteractable>();
        xrGrab.selectEntered.AddListener(OnGrabbed);
        xrGrab.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isSnapped)
        {
            xrGrab.trackPosition = false;
            xrGrab.trackRotation = false;

            holdCoroutine = StartCoroutine(HoldToRelease());
        }
    }

    private IEnumerator HoldToRelease()
    {
        float timer = 0f;

        while (timer < holdReleaseTime)
        {
            if (!xrGrab.isSelected) yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        UnsnapPlug();
    }

    private void UnsnapPlug()
    {
        justHoldReleased = true;

        xrGrab.trackPosition = true;
        xrGrab.trackRotation = true;

        if(leg.snapTarget != null && leg.snapTarget.isOccupied) leg.snapTarget.isOccupied = false;
        isSnapped = false;
        breadboardManager.ReleaseHole(leg.occupiedHoleKey);
        leg.occupiedHoleKey = "";
        leg.ClearSnap();

        if (occupiedTarget != null)
        {
            occupiedTarget.isOccupied = false;
            occupiedTarget = null;
        }
        parentWire.OnPlugUnsnapped();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if(holdCoroutine != null) StopCoroutine(holdCoroutine);

        xrGrab.trackPosition = true;
        xrGrab.trackRotation = true;

        snapVisualizer.Hide();
        StartCoroutine(TrySnapNextFrame());
    }

    private IEnumerator TrySnapNextFrame()
    {
        yield return null;

        if (justHoldReleased)
        {
            justHoldReleased = false;
            yield break ;
        }

        if(breadboardManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint, out GridRegion gridRegion, out string holeKey))
        {
            leg.transform.position = snapPoint;
            leg.transform.localRotation = Quaternion.identity;
            leg.snappedRegion = gridRegion;
            leg.snapTarget = null;
            leg.isSnapped = true;
            isSnapped = true;
            leg.occupiedHoleKey = holeKey;
            breadboardManager.OccupyHole(holeKey);
            parentWire.OnPlugSnapped();
        }
        else if(snapTargetManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint2, out SnapTarget snapTarget))
        {
            leg.transform.position = snapPoint2;
            leg.snappedRegion = null;
            leg.snapTarget = snapTarget;
            leg.snapTarget.isOccupied = true;
            leg.isSnapped = true;
            isSnapped = true;

            parentWire.OnPlugSnapped();
        }
    }

    private void Update()
    {

        if (isSnapped) return;
        if (!xrGrab.isSelected) return;

        if(breadboardManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint, out _, out _))
        {
            snapVisualizer.Show(snapPoint);
        }
        else if(snapTargetManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint2, out _))
        {
            snapVisualizer.Show(snapPoint2);
        }
        else
        {
            snapVisualizer.Hide();
        }
    }
}
