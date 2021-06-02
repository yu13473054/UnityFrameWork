using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRawImage : RawImage
{
    private ResModuleUtility _resModuleUtility;
    [SerializeField]
    private bool _multiRes;
    [SerializeField]
    private bool _multiResNativeSize = false;
    protected override void Start()
    {
        base.Start();
        if (_multiRes && Application.isPlaying && texture)
        {
            InitResModule();
            Texture tex = _resModuleUtility.LoadTexture(texture.name);
            texture = tex;
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
                Debug.LogErrorFormat(this, "<UIRawImage> 缺少资源管理器ResModuleUtility, 名称：{0}", name);
            }
        }
    }
}
