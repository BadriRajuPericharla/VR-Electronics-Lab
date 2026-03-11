using UnityEngine;

public class LEDTest : MonoBehaviour
{
    public MeshRenderer bulbRenderer;
    public Light glowLight;
    public Color ledColor = Color.red;

    private Material mat;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        mat = bulbRenderer.material;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) TurnOn();
        if (Input.GetMouseButtonUp(0)) TurnOff();
    }

    private void TurnOn()
    {
        Debug.Log("Turning ON");
        mat.EnableKeyword("_EMISSION");
        mat.SetColor(EmissionColor, ledColor * 2f);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        if (glowLight != null) glowLight.enabled = true;
    }

    private void TurnOff()
    {
        Debug.Log("Turning OFF");
        mat.DisableKeyword("_EMISSION");
        mat.SetColor(EmissionColor, Color.black);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        if (glowLight != null) glowLight.enabled = false;
    }
}