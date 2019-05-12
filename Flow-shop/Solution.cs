public class Solution {
    
    public int[] m1;
    public int[] m2;
    public bool[] maintenanceDecision;
    public int[] maintenanceDuration;

    public Solution() { }

    public Solution(int size)
    {
        m1 = new int[size];
        m2 = new int[size];
        maintenanceDecision = new bool[size];
        maintenanceDuration = new int[size];
    }

    public void Copy(Solution solution)
    {
        for (int i = 0; i < m1.Length; i++) {
            m1[i] = solution.m1[i];
            m2[i] = solution.m2[i];
            maintenanceDecision[i] = solution.maintenanceDecision[i];
            maintenanceDuration[i] = solution.maintenanceDuration[i];
        }
    }
}
