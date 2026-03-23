using System.Collections.Generic;
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
    public Material bulbMaterial;
    public Light glowLight;
    public ParticleSystem blownParticles;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip popSound;
    public AudioClip humSound;

    public enum LEDState {Off, Glowing, Blown};
    public LEDState currentState = LEDState.Off;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");


    private void Start()
    {
        if(bulbRender != null)
        {
            bulbMaterial = bulbRender.material;
        }

        currentState = LEDState.Off;
        ledResistance = forwardVoltage / maxSafeCurrent;
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
            Debug.Log("LED Blown");
            SetState(LEDState.Blown);
        }
        else
        {
            Debug.Log("LED is Glowing");
            SetState(LEDState.Glowing);
        }
    }

    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes)
    {
        //
    }

    public override void ResetState()
    {
        currentState = LEDState.Off;
        ApplyOff();
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
        if(bulbMaterial != null)
        {
            bulbMaterial.DisableKeyword("_EMISSION");
            bulbMaterial.SetColor(EmissionColor, Color.black);
            bulbMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        if(glowLight != null)
        {
            glowLight.enabled = false;
        }

        StopHum();
    }

    private void ApplyGlow()
    {
        if (bulbMaterial != null)
        {
            bulbMaterial.EnableKeyword("_EMISSION");
            bulbMaterial.SetColor(EmissionColor, ledColor * 2f);
            bulbMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        if (glowLight != null)
        {
            glowLight.enabled = true;
            glowLight.color = ledColor;
            glowLight.intensity = 15f;
        }

        PlayHum();
    }

    private void ApplyBlown()
    {
        if(blownParticles != null)
        {
            blownParticles.Play();
        }

        if (bulbMaterial != null)
        {
            bulbMaterial.EnableKeyword("_EMISSION");
            bulbMaterial.SetColor(EmissionColor, Color.white * 5f);
        }
        StopHum();

        if(audioSource != null && popSound != null)
        {
            audioSource.PlayOneShot(popSound);
        }


        Invoke(nameof(ApplyOff), 0.1f);
    }

    private void PlayHum()
    {
        if (audioSource == null || humSound == null) return;
        if (audioSource.isPlaying) return;


        audioSource.clip = humSound;
        audioSource.loop = true;
        audioSource.Play();

    }

    private void StopHum()
    {
        if(audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

}
