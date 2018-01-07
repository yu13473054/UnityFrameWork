using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class MyToggle : MonoBehaviour
{
    [SerializeField]
    private GameObject _normalGo;
    [SerializeField]
    private GameObject _selectedGo;

    private Graphic _normalGraphic;
    private Graphic _selectedGraphic;
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        _normalGraphic = _normalGo.GetComponent<Graphic>();
        _selectedGraphic = _selectedGo.GetComponent<Graphic>();

        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    void Start()
    {
        OnValueChanged(_toggle.isOn);
    }

    private void OnValueChanged(bool isOn)
    {
        _normalGo.SetActive(!isOn);
        _selectedGo.SetActive(isOn);
        if (isOn)
        {
            _toggle.targetGraphic = _selectedGraphic;
        }
        else
        {
            _toggle.targetGraphic = _normalGraphic;
        }
    }


}
