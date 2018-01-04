using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderImage : Image
{
    private PolygonCollider2D _polygon = null;
    private PolygonCollider2D polygon
    {
        get
        {
            if (_polygon == null)
                _polygon = GetComponent<PolygonCollider2D>();
            return _polygon;
        }
    }
    protected PolygonColliderImage() : base()
    {
//        useLegacyMeshGeneration = true;
    }
//    protected override void OnPopulateMesh(VertexHelper vh)
//    {
//        vh.Clear();
//    }
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return polygon.OverlapPoint(eventCamera.ScreenToWorldPoint(screenPoint));
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/UI/PolygonColliderImage",false,2010)]
    static void AddPolygonColliderImage(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("PolygonColliderImage");
        PolygonColliderImage obj = go.AddComponent<PolygonColliderImage>();
        obj.GetComponent<Image>().raycastTarget = false;
        obj.GetComponent<PolygonCollider2D>().isTrigger = true;

        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            return;
        }

        string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, go.name);
        go.name = uniqueName;
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Undo.SetTransformParent(go.transform, parent.transform, "Parent " + go.name);
        GameObjectUtility.SetParentAndAlign(go, parent);

        Selection.activeGameObject = go;
    }

    protected override void Reset()
    {
        base.Reset();
        transform.localPosition = Vector3.zero;
        float w = (rectTransform.sizeDelta.x * 0.5f) + 0.1f;
        float h = (rectTransform.sizeDelta.y * 0.5f) + 0.1f;
        polygon.points = new Vector2[]
        {
            new Vector2(-w,-h),
            new Vector2(w,-h),
            new Vector2(w,h),
            new Vector2(-w,h)
          };
    }
#endif
}
