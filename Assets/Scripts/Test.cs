using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AA;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
        UnityEngine.Object obj = Resources.Load("LoginPanel");

	    GameObject parent = GameObject.Find("Canvas");

	    GameObject go  = Instantiate(obj) as GameObject;
        go.transform.SetParent(parent.transform,false);
//	    go.transform.parent = parent.transform;
        go.transform.localPosition = Vector3.zero;

//	    Stopwatch sw = new Stopwatch();
//        sw.Start();
//	    Type type = typeof (BB);
//	    string n = type.Name;
//	    string name = type.FullName;
//        sw.Stop();
//        Debug.Log(sw.ElapsedMilliseconds+"   "+sw.ElapsedTicks);
//	    Debug.Log(name+"    "+n+"    "+type.ToString());
//	    type = typeof (CC);
//        Debug.Log(name+"    "+n+"    "+type.ToString());

	}

    // Update is called once per frame
	void Update () {
		
	}
}

namespace AA
{
    public class BB
    {
        public void Method()
        {
            
        }
    }
}

public class CC
{
    public void Method()
    {
    }

}

