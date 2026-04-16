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
            PositionTheLabel(other.GetComponentInParent<CircuitComponent>().transform);
            deleteLabelCanvas.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        CircuitComponent comp = other.GetComponentInParent<CircuitComponent>();
        if (comp == null) return;

        PositionTheLabel(comp.transform);

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
        if(other.GetComponentInParent<CircuitComponent>() == null) return false;

        XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();

        return interactable != null && interactable.isSelected;
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
    }
}
