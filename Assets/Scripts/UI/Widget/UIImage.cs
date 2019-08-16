using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImage : Image
{
    private Material _defaultMat;
    private string _resModule;

    protected override void Awake()
    {
        base.Awake();
        UIMod uiMod = GetComponentInParent<UIMod>();
        if (uiMod)
            _resModule = uiMod.resModule;
    }

    public bool isGray
    {
        set
        {
            if (value)
            {
                if(!defaultMaterial) _defaultMat = material;
                if (!string.IsNullOrEmpty(_resModule))
                    material = ResMgr.Inst.LoadMaterial("UI_UIGrayScale", _resModule);
            }
            else
                if (defaultMaterial) material = _defaultMat;
        }
    }
}
