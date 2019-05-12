[System.Serializable]
public class Instance {

    public readonly int id;
    public readonly int numberOfJobs;
    public readonly int[] op1;
    public readonly int[] op2;

    public Instance(int id, int numberOfJobs)
    {
        this.id = id;
        this.numberOfJobs = numberOfJobs;
        op1 = new int[numberOfJobs];
        op2 = new int[numberOfJobs];
    }
}
