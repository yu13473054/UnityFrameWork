using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RaycastChecker
{

    [MenuItem("Assets/PVP.功能/UI/取消勾选Raycast Target")]
    private static void CheckRaycastTarget() {
        var gameObjects = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        bool rst = false;
        foreach (var prefab in gameObjects) {
            if (prefab == null) {
                continue;
            }
            var process = Checker(prefab);
            if (process)
            {
                rst = true;
                EditorUtility.SetDirty(prefab);
            }
        }
        if (rst)
        {
            AssetDatabase.SaveAssets();
        }
        // EditorUtility.DisplayDialog("提示", $"Raycat Target 纠正完成！", "确定");
    }
    
     private static bool Checker(GameObject prefab) {
        var process = false;
        var graphicList = prefab.GetComponentsInChildren<Graphic>(true).Where(e => e.raycastTarget);
        foreach (var g in graphicList) {
            if (!NeedTogOnComponent(g))
            {
                g.raycastTarget = false;
                process = true;
            }
        }

        return process;
    }

     private static bool NeedTogOnComponent(Graphic g)
     {
         //字体统一不勾选
         if (g is Text || g is TextMeshProUGUI)
             return false;
         //同级有Selectable对象，保持勾选
         if (g.GetComponent<Selectable>())
             return true;
         var parentT = g.transform.parent;
         if (!parentT)
             return false;
         var parentG = parentT.GetComponent<Graphic>();
         if (!parentG)
             return false;
         //直接父级有Selectable对象,并没有raycast组件，保持勾选
         if (!parentG.raycastTarget && 
             (parentT.GetComponent<Selectable>() || parentT.GetComponent<ScrollRect>()))
         {
             return true;
         }
         return false;
     }
}
