using System.Collections.Generic;
using UnityEngine;

public static class ExtensionForList
{
    public static void Copy(this List<Vector3> pathVector, List<Vector3> currentPath)
    {
        foreach (var item in currentPath)
        {
            pathVector.Add(item);
        }
    }
}
