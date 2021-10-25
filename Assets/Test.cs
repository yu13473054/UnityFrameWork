using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public RedPointer pointer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            RedPointMgr.Inst.SetPointNum("Root.Main.Btn", 1);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            RedPointMgr.Inst.SetPointNum("Root.Login.Notice", 2);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            int id = RedPointMgr.Inst.AddDynamicFullKey("Root.Login1.Notice");
            RedPointMgr.Inst.SetPointNum(id, 10);
            pointer.ChangeId(id, true);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            int id = RedPointMgr.Inst.GetIdByKey("Root");
            Debug.Log(id);
        }

    }
}
