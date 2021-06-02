using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImage : Image
{
    private Material _defaultMat;
    private ResModuleUtility _resModuleUtility;
    [SerializeField]
    private float _colorByGrayFactor = 1;
    private bool _isGray;
    [SerializeField]
    private bool _multiRes = false;
    [SerializeField]
    private bool _multiResNativeSize = false;

    protected override void Start()
    {
        base.Start();
        if (_multiRes && Application.isPlaying && GameMain.Inst.lngType != LngType.CN && sprite)
        {
            InitResModule();
            Sprite loadSprite = _resModuleUtility.LoadSprite(sprite.name);
            sprite = loadSprite;
            if (_multiResNativeSize)
                SetNativeSize();
        }
    }


    private void InitResModule()
    {
        if (!_resModuleUtility)
        {
            _resModuleUtility = GetComponentInParent<ResModuleUtility>();
            if (!_resModuleUtility)
            {
                Debug.LogErrorFormat(this, "<UIImage> 缺少资源管理器ResModuleUtility, 名称：{0}", name);
            }
        }
    }

    public void SetType(int value)
    {
        type = (Type) value;
    }

    public bool isGray
    {
        set
        {
            if (_isGray == value) return; //防止反复设置
            _isGray = value;
            if (value)
            {
                if(!defaultMaterial) _defaultMat = material;
                InitResModule();
                material = _resModuleUtility.LoadMaterial("UI_Mat_SpriteGray");
                color = new Color(color.r * _colorByGrayFactor, color.g * _colorByGrayFactor, color.b * _colorByGrayFactor, color.a);
            }
            else
            {
                if (defaultMaterial) material = _defaultMat;
                color = new Color(color.r / _colorByGrayFactor, color.g / _colorByGrayFactor, color.b / _colorByGrayFactor, color.a);
            }
        }
    }
}
