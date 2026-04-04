using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewComponent", menuName = "VR Lab/Component Entry")]

public class ComponentEntrySO : ScriptableObject
{
    public string componentName;
    public ComponentCategory category;
    public Sprite icon;

    public GameObject prefab;

    public List<string> valueLabels = new List<string>();
    public List<float> values = new List<float>();

    public bool HasValues => valueLabels != null && values.Count > 0;
}


public enum ComponentCategory
{
    Power,
    Passive,
    Active,
    Wires
}
