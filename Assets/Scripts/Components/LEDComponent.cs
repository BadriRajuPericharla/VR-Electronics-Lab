using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LEDComponent : CircuitComponent
{
    [Header("LED Properties")]
    public float forwardVoltage = 2.0f;
    public float maxSafeCurrent = 0.030f;
    float ledResistance;
    public Color ledColor = Color.red;


    [Header("Visuals")]
    public MeshRenderer bulbRender;
    public Material bulbGlowMaterial;
    public Material ledMat;
    public Light glowLight;
    public ParticleSystem[] blownParticles;

    [Header("LED Colors")]
    public Color infraredColor;
    public Color redColor;
    public Color orangeColor;
    public Color yellowColor;
    public Color greenColor;
    public Color blueColor;
    public Color whiteColor;
    public Color ultraVioletColor;


    public Mesh ledTopPartBrokenMesh;
    public Mesh ledBottomBrokenPartMesh;
    public Material blownMaterial;




    public enum LEDState {Off, Glowing, Blown};
    public LEDState currentState = LEDState.Off;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");


    private void Start()
    {
        currentState = LEDState.Off;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.LeftShift))
    //    {
    //        SetState(LEDState.Blown);
    //    }
    //}

    public override void UpdateState(float deltaTime, Dictionary<string, Node> nodeMap)
    {
        //no need to update
    }

    public override void Simulate(CircuitSolver solver)
    {
        if (currentState == LEDState.Blown) return;

        //if two legs are not connected then LED will not glow
        if (!IsFullyConnected())
        {
            SetState(LEDState.Off);
            return;
        }

        //here we are passing leg[0] and leg[1] because this method automatically takes legs from circuitComponent
        float voltageDrop = GetVoltageDrop(0, 1);

        if (voltageDrop < forwardVoltage)
        {
            if(voltageDrop < 0)
                Debug.Log("Circuit polarity is wrong!, Try change polarity");

            else
                Debug.Log($"voltage: {voltageDrop} is not enough");

            SetState(LEDState.Off);
            return;
        }


        float current = GetVoltageDrop(0, 1) / ledResistance;

        if (current > maxSafeCurrent)
        {
            Debug.Log($"[LED {GetInstanceID()}] Blown");
            SetState(LEDState.Blown);
        }
        else
        {
            Debug.Log($"[LED {GetInstanceID()}] Glowing");
            SetState(LEDState.Glowing);
        }
    }

    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes, float deltaTime)
    {
        //
    }

    public override void ResetState()
    {
        if (currentState == LEDState.Blown) return;
        currentState = LEDState.Off;
        ApplyOff();
    }

    public override void SetValue(float value)
    {
        if (bulbRender != null)
            bulbGlowMaterial = bulbRender.material;

        if (ledMat != null)
            ledMat = new Material(ledMat);



        forwardVoltage = value;
        if (forwardVoltage <= 2.2f && forwardVoltage >= 1.8f)
            maxSafeCurrent = 0.030f;
        else if (forwardVoltage >= 3.0f && forwardVoltage <= 3.6f)
            maxSafeCurrent = 0.025f;
        else
            maxSafeCurrent = 0.050f;

            
        ledResistance = forwardVoltage / maxSafeCurrent;
        AssignColorBasedOnVoltage(forwardVoltage);
    }

    private void AssignColorBasedOnVoltage(float fwdVoltage)
    {
        switch(fwdVoltage)
        {
            case 1.2f:
                AssignColor(infraredColor);
                break;
            case 2f:
                AssignColor(redColor);
                break;
            case 2.0001f:
                AssignColor(orangeColor);
                break;
            case 2.1f:
                AssignColor(yellowColor);
                break;
            case 3f:
                AssignColor(greenColor);
                break;
            case 3.2f:
                AssignColor(blueColor);
                break;
            case 3.2001f:
                AssignColor(whiteColor);
                break;
            case 3.5f:
                AssignColor(ultraVioletColor);
                break;
        }
    }

    private void AssignColor(Color color)
    {
        ledMat.color = color;
        this.ledColor = color;

        MeshRenderer ledTopRenderer = transform.Find("Exposy_Case").GetComponent<MeshRenderer>();
        MeshRenderer ledBottomRenderer = transform.Find("Flat_Spot").GetComponent<MeshRenderer>();

        if (ledTopRenderer != null && ledBottomRenderer != null)
        {
            ledTopRenderer.material = ledMat;
            ledBottomRenderer.material = ledMat;
        }
    }

    private void SetState(LEDState newState)
    {
        if (currentState == LEDState.Blown) return;

        currentState = newState;

        switch(newState)
        {
            case LEDState.Glowing:
                ApplyGlow();
                break;
            case LEDState.Off:
                ApplyOff();
                break;
            case LEDState.Blown:
                ApplyBlown();
                break;
        }
    }


    private void ApplyOff()
    {
        if(bulbGlowMaterial != null)
        {
            bulbGlowMaterial.DisableKeyword("_EMISSION");
            bulbGlowMaterial.SetColor(EmissionColor, Color.black);
            bulbGlowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        if(glowLight != null)
        {
            glowLight.enabled = false;
        }

        AudioManager.instance.StopLEDHumSound();
    }

    private void ApplyGlow()
    {
        if (bulbGlowMaterial != null)
        {
            bulbGlowMaterial.EnableKeyword("_EMISSION");
            bulbGlowMaterial.SetColor(EmissionColor, ledColor * 2f);
            bulbGlowMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        if (glowLight != null)
        {
            glowLight.enabled = true;
            glowLight.color = ledColor;
            glowLight.intensity = 2f;
        }

        AudioManager.instance.PlayLEDHumSound();
    }

    private void ApplyBlown()
    {
        if(blownParticles != null)
        {
            foreach(ParticleSystem effect in blownParticles)
                effect.Play();
        }

        MeshFilter topMeshFilter = transform.Find("Exposy_Case").GetComponent<MeshFilter>();
        MeshFilter bottomMeshFilter = transform.Find("Flat_Spot").GetComponent<MeshFilter>();
        MeshRenderer mr = transform.Find("Exposy_Case").GetComponent<MeshRenderer>();
        MeshRenderer mr2 = transform.Find("Flat_Spot").GetComponent<MeshRenderer>();

        if (topMeshFilter != null && bottomMeshFilter != null)
        {
            topMeshFilter.mesh = ledTopPartBrokenMesh;
            bottomMeshFilter.mesh = ledBottomBrokenPartMesh;
            mr.material = blownMaterial;
            mr2.material = blownMaterial;
        }

        AudioManager.instance.StopLEDHumSound();
        AudioManager.instance.PlayLEDBlownSound();


        Invoke(nameof(ApplyOff), 0.1f);
    }


}
