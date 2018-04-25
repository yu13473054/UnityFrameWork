using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSTool : MonoBehaviour
{
    public int sampleCount = 5;   //取样次数

    private int _frame;
    float _lastTime;
    private string fpsText;
    private GUIStyle style;

	void Start ()
	{
	    _frame = sampleCount;
        _lastTime = Time.realtimeSinceStartup;

        //初始化Style
        style = new GUIStyle();
        style.fontSize = 30;
    }
	
	void Update ()
	{
	    _frame++;
	    if (_frame >= 5)
	    {
	        _frame = 0;
	        float currTime = Time.realtimeSinceStartup;
	        float deltaTime = currTime - _lastTime;
	        float fps = sampleCount/deltaTime;
            if (fps >= 20)
                style.normal.textColor = Color.green;
            else if (fps > 10)
                style.normal.textColor = Color.yellow;
            else
                style.normal.textColor = Color.red;

            fpsText = fps.ToString("0.00");

            _lastTime = currTime;
	    }
	}

    void OnGUI()
    {
        GUILayout.Label(fpsText,style);
    }
}
