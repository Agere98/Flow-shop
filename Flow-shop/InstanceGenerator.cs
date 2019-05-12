using System;

public class InstanceGenerator {

    public int GeneratedInstances { get; set; } = 1;
    public int NumberOfJobs { get; set; } = 50;
    public int MinOperationLength { get; set; } = 5;
    public int MaxOperationLength { get; set; } = 40;
    
    int counter;
    Random random;

    public Instance Generate()
    {
        Instance instance = new Instance (counter, NumberOfJobs);
        if (random == null) random = new Random ();
        for (int i = 0; i < NumberOfJobs; i++) {
            instance.op1[i] = random.Next (MinOperationLength, MaxOperationLength);
            instance.op2[i] = random.Next (MinOperationLength, MaxOperationLength);
        }
        counter++;
        return instance;
    }

    public void InitCounter(int value)
    {
        counter = value;
    }
}
