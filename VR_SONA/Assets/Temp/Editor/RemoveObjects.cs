using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class RemoveObjectsByName : MonoBehaviour
{
    [MenuItem("Tools/Remove SM_Ceiling, SM_Chandelier, SM_Lamp, SM_Pillar Objects")]
    private static void RemoveSpecifiedObjects()
    {
        int count = 0;
        List<GameObject> toDelete = new List<GameObject>();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.hideFlags == HideFlags.None &&
                (obj.name.StartsWith("SM_Ceiling") ||
                 obj.name.StartsWith("SM_Chandelier") ||
                 obj.name.StartsWith("SM_Pillar") ||
                 obj.name.StartsWith("SM_Lamp")))
            {
                toDelete.Add(obj);
            }
        }

        foreach (GameObject obj in toDelete)
        {
            Undo.DestroyObjectImmediate(obj);
            count++;
        }

        Debug.Log($"삭제 완료: SM_Ceiling, SM_Chandelier, SM_Lamp, SM_Pillar로 시작하는 오브젝트 {count}개 삭제됨.");
    }
}
