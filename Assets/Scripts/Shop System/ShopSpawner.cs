using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShopSpawner : MonoBehaviour
{
    [SerializeField] private Transform rightHandAttachPoint;
    [SerializeField] private XRDirectInteractor rightHandInteracter;

    public void SpawnIntoHand(GameObject prefab, float value)
    {
        if(prefab == null)
        {
            Debug.LogWarning("Component prefab is not assigned in the scriptable Obj assest");
            return;
        }

        GameObject spawned = InstantiateAtHand(prefab);
        ApplyValue(spawned, value);
        ForceGrab(spawned);
    }


    private GameObject InstantiateAtHand(GameObject prefab)
    {
        Vector3 spawnPos = rightHandAttachPoint != null ?
                            rightHandAttachPoint.position :
                            Camera.main.transform.position + Camera.main.transform.forward * 0.3f;

        Quaternion spawnRot = rightHandAttachPoint != null ?
                            rightHandAttachPoint.rotation :
                            Quaternion.identity;

        return Instantiate(prefab, spawnPos, spawnRot);
    }

    private void ApplyValue(GameObject prefab, float value)
    {
        CircuitComponent comp = prefab.GetComponent<CircuitComponent>();
        comp.SetValue(value);
    }

    private void ForceGrab(GameObject prefab)
    {
        if (rightHandInteracter == null)
        {
            Debug.LogWarning("No RightHand interactor assigned");
            return;
        }

        var interactable = prefab.GetComponent<XRGrabInteractable>();

        if (interactable == null)
        {
            Debug.LogWarning("Spawned Obj has no XRGrabInteractable");
            return;
        }

        rightHandInteracter.StartManualInteraction(interactable as IXRSelectInteractable);


    }


}
