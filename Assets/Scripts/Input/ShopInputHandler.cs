using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ShopInputHandler : MonoBehaviour
{
    public Transform leftHandTranform;

    [Header("Gesture Settings")]
    [SerializeField] private float palmUpThreshold = 0.8f;
    [SerializeField] private float gestureHoldDuration = 0.6f;
    [SerializeField] private float gestureCoolDown = 1f;

    public UnityEvent onShopToggleRequested;

    private InputDevice _rightHandDevice;
    private bool _prevBBtnState = false;
    private float _gestureHeldTime = 0f;
    private float _lastToggleTime = -999f;
    private bool _gestureTriggeredThisPose = false;

    private void Start()
    {
        TryGetRightHandDevice();
    }

    private void Update()
    {
        if (!_rightHandDevice.isValid)
            TryGetRightHandDevice();

        HandleControlerInput();
        HandlePalmGesture();
    }

    private void TryGetRightHandDevice()
    {
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if(devices.Count > 0 )
            _rightHandDevice = devices[0];
    }

    private void HandleControlerInput()
    {
        if (!_rightHandDevice.isValid) return;

        _rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool currentState);

        bool risingEdge = currentState && !_prevBBtnState;

        if (risingEdge)
            RequestToggle();

        _prevBBtnState = currentState;
    }


    private void HandlePalmGesture()
    {
        if (leftHandTranform == null) return;
        if (Time.time - _lastToggleTime < gestureCoolDown) return;

        float palmUpDot = Vector3.Dot(leftHandTranform.transform.up, Vector3.up);

        bool isPalmUp = palmUpDot > palmUpThreshold;

         if (isPalmUp && !_gestureTriggeredThisPose)
        {
            _gestureHeldTime += Time.deltaTime;
            if (_gestureHeldTime >= gestureHoldDuration)
            {
                RequestToggle();
                _gestureTriggeredThisPose = true;
                _gestureHeldTime = 0f;
            }
        }
        else if (!isPalmUp)
        {
            _gestureHeldTime = 0f;
            _gestureTriggeredThisPose = false;
        }
    }


    private void RequestToggle()
    {
        _lastToggleTime = Time.time;
        onShopToggleRequested?.Invoke();
    }

}
