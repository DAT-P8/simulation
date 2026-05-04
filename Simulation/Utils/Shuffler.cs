using System;
using System.Collections.Generic;

namespace Simulation.Utils;

public class Shuffler
{
    public static List<T> Shuffle<T>(Random rng, List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
        return list;
    }
}
