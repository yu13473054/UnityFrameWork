using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour {

//	    RectTransform rect;

    public ScrollRect scrollRect;

	// Use this for initialization
	void Start ()
	{
//	    rect = GetComponent<RectTransform>();
        scrollRect.onValueChanged.AddListener(OnScroll);
	}

    private void OnScroll(Vector2 pos)
    {

    }

    // Update is called once per frame
	void Update () {
//		Debug.Log(rect.rect+"    "+rect.sizeDelta+"   "+rect.anchoredPosition);
	}
}
