using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIText : Text 
{
	public int localizationID = 0;
    protected void SetBase( string str )
    {
        base.text = str;
    }

    protected override void Awake ()
	{
		base.Awake();

#if UNITY_EDITOR
        if (!Localization.isInited)
        {
            Localization.Init();
        }
#endif

        if ( localizationID != 0 )
		{
            string dataText = Localization.Get(localizationID);
            if (!string.IsNullOrEmpty(dataText))
            {
                this.text = dataText;
            }
        }
    }
}
