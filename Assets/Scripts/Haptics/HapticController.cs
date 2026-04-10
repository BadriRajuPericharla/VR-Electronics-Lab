using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticController : MonoBehaviour
{

    public static HapticController instance;

    private void Awake()
    {
        instance = this;
    }

    public void TriggerHaptics(float amplitude, float duration, XRGrabInteractable xrGrab)
    {

        foreach (var interactor in xrGrab.interactorsSelecting)
        {
            if (interactor is XRBaseControllerInteractor controllerInteractor && controllerInteractor.xrController != null)
            {
                controllerInteractor.xrController.SendHapticImpulse(amplitude, duration);
            }
        }
    }
}
