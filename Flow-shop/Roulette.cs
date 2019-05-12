using System;
using System.Collections.Generic;

public class Roulette {

    double sum;
    List<double> slots;
    Random random;

    public int Count { get { return slots.Count; } }

    public Roulette()
    {
        sum = 0;
        slots = new List<double> ();
        random = new Random ();
    }

    public void Add(double probability)
    {
        sum += probability;
        slots.Add (sum);
    }

    public int Get()
    {
        double r = random.NextDouble () * sum;
        int i = slots.BinarySearch (r);
        if (i >= 0) return i + 1;
        else return ~i;
    }

    public void Clear()
    {
        sum = 0;
        slots.Clear ();
    }
}
