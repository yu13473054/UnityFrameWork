using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIText : Text 
{
	public string textID = "";
    public string fontName = "";

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (!Localization.isInited)
        {
            Localization.Init();
            Localization.isInited = true;
        }
#endif

        if (textID != "")
        {
            string dataText = Localization.Get(textID);
            if (!string.IsNullOrEmpty(dataText))
            {
                this.text = dataText;
            }
        }

        if (Application.isPlaying && !string.IsNullOrEmpty(fontName))//指定了字体名称
        {
            UIMod uiMod = GetComponentInParent<UIMod>();
            string moduleName = uiMod ? uiMod.resModule : "Font";//在UISystem下，就使用UI名称作为模块名，不在的话，就使用Font作为模块名
            Font mFont = ResMgr.Inst.LoadObj<Font>(fontName, moduleName);
            font = mFont;
        }
    }

    // 设置文本，所有Lua中的文字设置，都调用此方法
    public void SetText(string textID, params object[] args)
    {
        text = Localization.Get(textID, args);
    }


}
