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
            isSnapped = false;
            xrGrab.enabled = true;
            leg.ClearSnap();

            if(occupiedTarget != null)
            {
                occupiedTarget.isOccupied = false;
                occupiedTarget = null;
            }
            parentWire.OnPlugUnsnapped();
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        snapVisualizer.Hide();
        StartCoroutine(TrySnapNextFrame());
    }

    private IEnumerator TrySnapNextFrame()
    {
        yield return null;

        if(breadboardManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint, out GridRegion gridRegion))
        {
            leg.transform.position = snapPoint;
            leg.transform.localRotation = Quaternion.identity;
            leg.snappedRegion = gridRegion;
            leg.snapTarget = null;
            leg.isSnapped = true;
            isSnapped = true;

            parentWire.OnPlugSnapped();
            xrGrab.enabled = false;
        }
        else if(snapTargetManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint2, out SnapTarget snapTarget))
        {
            leg.transform.position = snapPoint2;
            leg.snappedRegion = null;
            leg.snapTarget = snapTarget;
            leg.isSnapped = true;
            isSnapped = true;

            parentWire.OnPlugSnapped();
            xrGrab.enabled = false;
        }
    }

    private void Update()
    {

        if (isSnapped) return;
        if (!xrGrab.isSelected) return;

        if(breadboardManager.TryGetSnapPoint(leg.transform.position, out Vector3 snapPoint, out _))
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
