using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShopSpawner : MonoBehaviour
{
    [SerializeField] private Transform rightHandAttachPoint;
    [SerializeField] private XRDirectInteractor rightHandInteracter;
                  
    public void SpawnInFrontOfCam(GameObject prefab, float value)
    {
        if(prefab == null)
        {
            Debug.LogWarning("Component prefab is not assigned in the scriptable Obj assest");
            return;
        }

        GameObject spawned = InstantiateInFront(prefab);
        ApplyValue(spawned, value);
    }


    private GameObject InstantiateInFront(GameObject prefab)
    {
        Camera cam = Camera.main;
        if (cam == null) return Instantiate(prefab);
        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 0.3f;
        Quaternion spawnRot = Quaternion.identity;
        return Instantiate(prefab, spawnPos, spawnRot);
    }

    private void ApplyValue(GameObject prefab, float value)
    {
        if (prefab == null) return;
        CircuitComponent comp = prefab.GetComponent<CircuitComponent>();
        if (comp != null) comp.SetValue(value);
    }

}
