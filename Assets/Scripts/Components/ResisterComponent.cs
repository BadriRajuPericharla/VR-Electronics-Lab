using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResisterComponent : CircuitComponent
{
    [Header("Resistance")]
    public float resistance = 220f;

    [Header("Color Code Materials")]
    public Material[] digitalMaterials;
    public Material goldMaterial;

    public MeshRenderer meshRenderer;

    private const int SLOT_BAND_1 = 4;
    private const int SLOT_BAND_2 = 3;
    private const int SLOT_MULTIPLIER = 2;
    private const int SLOT_TOLERANCE = 1;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Simulate(CircuitSolver solver)
    {
        if(!IsFullyConnected()) return;

        float current = GetVoltageDrop(0, 1) / resistance;

    }

    public override void Stamp(float[,] G, float[] I, List<Node> unknownNodes)
    {
        Node nodeA = legs[0].node;
        Node nodeB = legs[1].node;

        float g = 1f / resistance;

        int indexA = unknownNodes.IndexOf(nodeA); //we get -1 if node is not found in the list i.e, node is known
        int indexB = unknownNodes.IndexOf(nodeB);


        //situation 1: when both nodes are unknown
        if (indexA >= 0 && indexB >= 0)
        {
            G[indexA, indexA] += g;
            G[indexB, indexB] += g;
            G[indexA, indexB] -= g;
            G[indexB, indexA] -= g;
        }
        //situation 2: when A is known and B unknown
        else if(indexA < 0 && indexB >= 0)
        {
            G[indexB, indexB] += g;
            I[indexB] += g * nodeA.voltage;
        }
        //situation 3: when B is known and A unknown
        else if (indexA >= 0 && indexB < 0)
        {
            G[indexA, indexA] += g;
            I[indexA] += g * nodeB.voltage;
        }
    }

    public override void ResetState()
    {
        //no reset required
    }

    public override void SetValue(float value)
    {
        resistance = value;
        AppyColorCode(value);
    }

    private void AppyColorCode(float resistance)
    {
        if(meshRenderer == null || digitalMaterials.Length < 10)
        {
            Debug.LogWarning($"Resistor Comp: meshRender is not assigned or color palatte is not assigned, materials length: {digitalMaterials.Length}");
            return;
        }

        float log = Mathf.Log10(resistance);
        int exponent = Mathf.FloorToInt(log);

        int firstDigit = Mathf.FloorToInt(resistance/Mathf.Pow(10, exponent));
        int secondDigit = Mathf.FloorToInt(resistance / Mathf.Pow(10, exponent - 1) % 10);
        

        int multiplierPower = exponent - 1;

        Material[] currentMaterials = meshRenderer.materials;


        currentMaterials[SLOT_BAND_1] = digitalMaterials[firstDigit];
        currentMaterials[SLOT_BAND_2] = digitalMaterials[secondDigit];

        if (multiplierPower >= 0 && multiplierPower < 10)
            currentMaterials[SLOT_MULTIPLIER] = digitalMaterials[multiplierPower];
        else if (multiplierPower < 0)
            currentMaterials[SLOT_MULTIPLIER] = digitalMaterials[0];

        currentMaterials[SLOT_TOLERANCE] = goldMaterial;

        meshRenderer.materials = currentMaterials;
    }
}
