using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour
{

    public ScrollRect _scroll;
    public VerticalLayoutGroup _group;
    public ToggleGroup _toggleGroup;

    public Button _btn;

	void Start ()
	{
        _btn.onClick.AddListener(OnBgClick);

        for (int i = 0; i < 2; i++)
        {
            GameObject go = UIMgr.Inst.AddPrefab("Tab", _group.transform);
            Toggle toggle = go.GetComponent<Toggle>();
            toggle.isOn = i == 0;
            toggle.group = _toggleGroup;
        }

        _scroll.onValueChanged.AddListener(OnScroll);
    }

    private void OnScroll(Vector2 pos)
    {
        Debug.Log(pos);
    }

    private void OnBgClick()
    {
        GameObject go = UIMgr.Inst.AddPrefab("Tab", _group.transform);
        Toggle toggle = go.GetComponent<Toggle>();
        toggle.group = _toggleGroup;

        Debug.Log("1111111     "+((RectTransform)_group.transform).rect);
    }

    // Update is called once per frame
	void Update ()
	{
    }
}

