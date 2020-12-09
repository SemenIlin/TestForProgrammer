using System.Collections.Generic;

public class PositionComparer : IComparer<Player>
{
    public int Compare(Player x, Player y)
    {
        if (x.Magnitude > y.Magnitude)
        {
            return 1;
        }
        else if (x.Magnitude < y.Magnitude)
        {
            return -1;
        }

        return 0;
    }
}