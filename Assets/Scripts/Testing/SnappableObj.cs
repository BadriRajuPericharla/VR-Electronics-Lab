using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnappableObj : MonoBehaviour
{
    private XRGrabInteractable xrGrab;
    public bool isGrabbed = false;
    public BreadboardManager breadboardManager;
    public SnapVisualizer snapVisualizer;

    private GridRegion snapRegion = null;

    private void Awake()
    {
        xrGrab = GetComponent<XRGrabInteractable>();

        xrGrab.selectEntered.AddListener(onGrabbed);
        xrGrab.selectExited.AddListener(onReleased);
    }

    void onGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void onReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        snapVisualizer.Hide();

        if (breadboardManager.TryGetSnapPoint(transform.position, out Vector3 snapPoint, out GridRegion region, out string holeKey))
        {
            transform.position = snapPoint;
            xrGrab.enabled = false;
            snapRegion = region;
        }
    }


    private void Update()
    {
        if (!isGrabbed) return;

        if (breadboardManager.TryGetSnapPoint(transform.position, out Vector3 snapPoint, out GridRegion region, out string holeKey))
        {
            snapVisualizer.Show(snapPoint);
        }
        else
        {
            snapVisualizer.Hide();
        }
    }
}
