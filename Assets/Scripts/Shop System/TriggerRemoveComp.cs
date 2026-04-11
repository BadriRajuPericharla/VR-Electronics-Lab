using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TriggerRemoveComp : MonoBehaviour
{
    //private Color[] originalColors;
    //private MeshRenderer compMR;


    private void OnTriggerEnter(Collider other)
    {
        //GetAllMeshApplyClolor(other.gameObject);
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
        {
            //ApplyOrgColors(other.gameObject);

        }
    }



    private void CheckAndRemoveComp(Collider other)
    {
        CircuitComponent component = other.GetComponentInParent<CircuitComponent>();
        //compMR = other.GetComponentInParent<MeshRenderer>();

        if (component != null)
        {
            XRGrabInteractable interactable = other.GetComponentInParent<XRGrabInteractable>();

            //if (interactable != null && interactable.isSelected)
            //{
            //    compMR.material.color = Color.red;
            //}

            if (interactable != null && !interactable.isSelected)
            {
                RemoveComponent(interactable.gameObject);
            }
        }
    }

    private void GetAllMeshApplyClolor(GameObject go)
    {
        GameObject parentObj = go.GetComponentInParent<GameObject>();
        GameObject[] allObjs = parentObj.GetComponentsInChildren<GameObject>();


        for (int i = 0; i < allObjs.Length; i++)
        {
            MeshRenderer mr = allObjs[i].GetComponent<MeshRenderer>();

            if (mr != null)
            {
                //originalColors[i] = mr.material.color;
                mr.material.color = Color.red;
            }

        }
    }
    private void ApplyOrgColors(GameObject go)
    {
        GameObject parentObj = go.GetComponentInParent<GameObject>();
        GameObject[] allObjs = parentObj.GetComponentsInChildren<GameObject>();


        for (int i = 0; i < allObjs.Length; i++)
        {
            MeshRenderer mr = allObjs[i].GetComponent<MeshRenderer>();

            if (mr != null)
            {
                //mr.material.color = originalColors[i];
            }

        }
    }


    private void RemoveComponent(GameObject comp)
    {
        Destroy(comp);
    }
}
