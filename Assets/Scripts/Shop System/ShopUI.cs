using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Transform playerHead;

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject valuePanel;
    [SerializeField] private GameObject componentPanel;

    [SerializeField] private Transform categoryBtnContainer;
    [SerializeField] private Transform componentBtnContainer;
    [SerializeField] private Transform valueBtnContainer;

    [SerializeField] private Button categoryBtnPrefab;
    [SerializeField] private Button componentBtnPreab;
    [SerializeField] private Button valueBtnPrefab;
    [SerializeField] private Button confirmBtn;

    [SerializeField] private TextMeshProUGUI selectedComponentLabel;

    [Header("Component Scriptable Objs: ")]
    [SerializeField] private List<ComponentEntrySO> allComponents = new List<ComponentEntrySO>();

    [SerializeField] private ShopSpawner shopSpawner;
    [SerializeField] private ShopInputHandler shopInputHandler;


    private bool _isOpen = false;
    private ComponentEntrySO _selectedComponent = null;
    private int _selectedIndex = -1;


    private void Start()
    {
        shopPanel.SetActive(false);
        valuePanel.SetActive(false);
        componentPanel.SetActive(false);
        confirmBtn.gameObject.SetActive(false);

        shopInputHandler.onShopToggleRequested.AddListener(ToggleShop);
        confirmBtn.onClick.AddListener(OnConfirmPressed);

        BuildCategoryBtns();
    }

    private void OnDestroy()
    {
        shopInputHandler.onShopToggleRequested.RemoveListener(ToggleShop);
    }

    private void ToggleShop()
    {
        //Debug.Log("Shop is Toggle");
        _isOpen = !_isOpen;
        shopPanel.SetActive(_isOpen);

        if (_isOpen)
        {
            PositionPanelInfrontOfThePlayer();
            ResetSelectionState();
        }
    }

    private void PositionPanelInfrontOfThePlayer()
    {
        if (playerHead == null) return;

        Vector3 flatForward = playerHead.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        shopPanel.transform.position = playerHead.position + flatForward * 1.0f;
        shopPanel.transform.rotation = Quaternion.LookRotation(flatForward);
    }


    private void ShowComponentsForCategory(ComponentCategory category)
    {
        ClearContainer(componentBtnContainer);
        ResetSelectionState();

        componentPanel.SetActive(true);

        foreach (ComponentEntrySO comp in allComponents)
        {
            if (comp.category != category) continue;

            Button btn = Instantiate(componentBtnPreab, componentBtnContainer);

            Image icon = btn.transform.Find("Icon").gameObject.GetComponent<Image>();

            if(icon != null && comp.icon != null)
            {
                icon.sprite = comp.icon;
            }

            btn.GetComponentInChildren<TextMeshProUGUI>().text = comp.componentName;
            btn.onClick.AddListener(() => OnComponentSelected(comp));
        }

    }


    private void BuildCategoryBtns()
    {
        HashSet<ComponentCategory> seenCatagories = new HashSet<ComponentCategory>();

        foreach(ComponentEntrySO component in allComponents)
        {
            if (seenCatagories.Contains(component.category)) continue;

            seenCatagories.Add(component.category);

            Button btn = Instantiate(categoryBtnPrefab, categoryBtnContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = component.category.ToString();
            btn.onClick.AddListener(() => ShowComponentsForCategory(component.category));
        }
    }

    private void BuildValueBtns(ComponentEntrySO comp)
    {
        ClearContainer(valueBtnContainer);

        for(int i = 0; i < comp.valueLabels.Count; i++)
        {
            int valueIndex = i;
            Button btn = Instantiate(valueBtnPrefab, valueBtnContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = comp.valueLabels[i];
            btn.onClick.AddListener(() => OnvalueSelected(valueIndex));
        }
    }

    private void OnvalueSelected(int index)
    {
        _selectedIndex = index;
        confirmBtn.gameObject.SetActive(true);
    }

    private void OnComponentSelected(ComponentEntrySO comp)
    {
        _selectedComponent = comp;
        _selectedIndex = -1;

        selectedComponentLabel.text = _selectedComponent.componentName;

        if (comp.HasValues)
        {
            BuildValueBtns(comp);
            valuePanel.SetActive(true);
            confirmBtn.gameObject.SetActive(false);
        }
        else
        {
            valuePanel.SetActive(false);
            confirmBtn.gameObject.SetActive(true);
        }
    }


    private void OnConfirmPressed()
    {
        if(_selectedComponent == null) return;

        float value = 0f;

        if (_selectedComponent.HasValues && _selectedIndex >= 0)
            value = _selectedComponent.values[_selectedIndex];

        shopSpawner.SpawnIntoHand(_selectedComponent.prefab, value);
        ToggleShop();
    }


    private void ClearContainer(Transform container)
    {
        foreach (Transform c in container)
            Destroy(c.gameObject);
    }


    private void ResetSelectionState()
    {
        _selectedComponent = null;
        _selectedIndex = -1;
        valuePanel.SetActive(false);
        componentPanel.SetActive(false);
        confirmBtn.gameObject.SetActive(false);
        ClearContainer(componentBtnContainer);
    }
}
