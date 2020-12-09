using System.Collections.Generic;
using UnityEngine;

public static class ExtensionForList
{
    public static int GetNearEnemy(this List<Player> enemies, Player searcher)
    {
        if(enemies.Count == 0 || enemies == null)
        {
            return -1;
        }

        var searcherVector2 = new Vector2(searcher.Transform.position.x, searcher.Transform.position.z);
        var distance = (searcherVector2 - Vector2.zero).magnitude;

        var left = 0;
        var right = enemies.Count - 1;
        var i = (left + right) / 2;

        while((right - left) != 1)
        {
            if (enemies[i].Magnitude <= distance)
            {
                left = i;
            }
            else
            {
                right = i;
            }
        }

        var nearEnemy = (enemies[right].Magnitude - distance) > (distance - enemies[left].Magnitude) ? left : right;

        return nearEnemy;
    }

    public static void Copy(this List<Vector3> pathVector, List<Vector3> currentPath)
    {
        foreach (var item in currentPath)
        {
            pathVector.Add(item);
        }
    }
}
