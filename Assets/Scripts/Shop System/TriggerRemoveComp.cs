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
            deleteLabelCanvas.SetActive(true);
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
        deleteLabelCanvas.transform.position = componentTf.position + labelHeightOffset * Vector3.up;

        deleteLabelCanvas.transform.LookAt(cam.transform);
        deleteLabelCanvas.transform.Rotate(0f, 180f, 0f);
    }

    private void RemoveComponent(GameObject comp)
    {
        Destroy(comp);
        AudioManager.instance.PlayComponentDestroySound();
    }
}
