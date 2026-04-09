using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TriggerRemoveComp : MonoBehaviour
{
    private Color originalColor;
    private MeshRenderer compMR;


    private void OnTriggerEnter(Collider other)
    {
        originalColor = other.GetComponentInParent<MeshRenderer>().material.color;
        CheckAndRemoveComp(other);
    }
    private void OnTriggerStay(Collider other)
    {
        CheckAndRemoveComp(other);
    }

    private void OnTriggerExit(Collider other)
    {
        XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();

        if (interactable != null && interactable.isSelected)
            compMR.material.color = originalColor;
    }



    private void CheckAndRemoveComp(Collider other)
    {
        CircuitComponent component = other.GetComponentInParent<CircuitComponent>();
        compMR = other.GetComponentInParent<MeshRenderer>();
        
        if(component != null)
        {
            XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();

            if(interactable != null && interactable.isSelected)
            {
                compMR.material.color = Color.red;
            }

            if(interactable != null && !interactable.isSelected)
            {
                RemoveComponent(interactable.gameObject);
            }
        }
    }


    private void RemoveComponent(GameObject comp)
    {
        Destroy(comp);
    }
}
