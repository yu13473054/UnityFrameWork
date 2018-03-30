using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ExecuteInEditMode]
public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void OnEnable()
    {
        string input = "UpdateHost=http://192.168.3.102:8080/Resources/\nLoginHost=http://192.168.3.102:8090/";
        Regex regex = new Regex(@"UpdateHost=(?>([^\n]+))\n+LoginHost=(?>([^\n]+))");
        Stopwatch sw = Stopwatch.StartNew();
        Match ismatch = regex.Match(input);
        Debug.Log(sw.ElapsedTicks + "   "+ismatch.Groups[2]+"    "+ismatch.Groups[1]);


        sw = Stopwatch.StartNew();
        string[] strings = input.Split('\n');
        string[] result2 = null;
        for (int i = 0; i < strings.Length; i++)
        {
            result2 = strings[i].Split('=');
        }

        int appVer = 1;
        float remoteVer = 1.11f;
        Debug.Log("<ResourceUpdate> 当前资源版本：" + appVer + remoteVer);

        Debug.Log(sw.ElapsedTicks+"   1111   "+strings[0]+ strings[1]+ result2[0]+result2[1]);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
