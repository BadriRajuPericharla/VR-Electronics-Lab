using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TriggerRemoveComp : MonoBehaviour
{
    [Header("Delete Label")]
    public GameObject deleteLabelCanvas;
    public float labelHeightOffset = 0.2f;
    private Camera cam;

    private void Start()
    {
        deleteLabelCanvas.SetActive(false);
        cam = Camera.main;
    }


    private void OnTriggerEnter(Collider other)
    {

        if (IsHeldComp(other))
        {
            CircuitComponent comp = GetHeldCircuitComponent(other);
            if (comp == null) return;

            Transform compTf = comp is JumperWireComponent ? other.transform : comp.transform;
            PositionTheLabel(compTf);
            if (deleteLabelCanvas != null) deleteLabelCanvas.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        CircuitComponent comp = GetHeldCircuitComponent(other);
        if (comp == null) return;


        Transform compTf = comp is JumperWireComponent ? other.transform : comp.transform;
        PositionTheLabel(compTf);

        XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();

        if (interactable != null && !interactable.isSelected)
        {
            RemoveComponent(comp.gameObject);
            deleteLabelCanvas.SetActive(false);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        deleteLabelCanvas.SetActive(false);
    }

    private bool IsHeldComp(Collider other)
    {

        if(other.GetComponentInParent<CircuitComponent>() != null)
        {
            XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();
            return interactable != null && interactable.isSelected;
        }

        if (other.GetComponentInParent<PlugSnappable>() != null)
        {
            XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();
            return interactable != null && interactable.isSelected;
        }
        return false;
    }

    private CircuitComponent GetHeldCircuitComponent(Collider other)
    {
        CircuitComponent comp = other.GetComponentInParent<CircuitComponent>();
        if (comp != null) return comp;

        PlugSnappable plug = other.GetComponentInParent<PlugSnappable>();
        if (plug != null) return plug.parentWire;

        return null;
    }


    private void PositionTheLabel(Transform componentTf)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || deleteLabelCanvas == null) return;
        deleteLabelCanvas.transform.position = componentTf.position + labelHeightOffset * Vector3.up;
        deleteLabelCanvas.transform.LookAt(cam.transform);
        deleteLabelCanvas.transform.Rotate(0f, 180f, 0f);
    }

    private void RemoveComponent(GameObject comp)
    {
        CircuitComponent circuitComp = comp.GetComponent<CircuitComponent>();
        if (circuitComp == null) circuitComp = comp.GetComponentInChildren<CircuitComponent>();

        BreadboardManager bbm = FindAnyObjectByType<BreadboardManager>();
        CircuitSolver solver = FindAnyObjectByType<CircuitSolver>();

        if (circuitComp != null)
        {
            // Release any occupied breadboard holes
            if (bbm != null && circuitComp.legs != null)
            {
                foreach (ComponentLeg leg in circuitComp.legs)
                {
                    if (leg != null && !string.IsNullOrEmpty(leg.occupiedHoleKey))
                    {
                        bbm.ReleaseHole(leg.occupiedHoleKey);
                        leg.occupiedHoleKey = "";
                    }
                }
            }
            if (solver != null) solver.UnRegisterComponent(circuitComp);
        }

        // Jumper wires: handle plugs
        PlugSnappable[] plugs = comp.GetComponentsInChildren<PlugSnappable>();
        foreach (PlugSnappable plug in plugs)
        {
            if (plug == null) continue;
            if (plug.leg != null && bbm != null && !string.IsNullOrEmpty(plug.leg.occupiedHoleKey))
            {
                bbm.ReleaseHole(plug.leg.occupiedHoleKey);
                plug.leg.occupiedHoleKey = "";
            }
            if (plug.parentWire != null && solver != null)
            {
                solver.UnRegisterComponent(plug.parentWire);
            }
        }

        Destroy(comp);
        AudioManager.instance?.PlayComponentDestroySound();
    }
}
