using System;
using System.Linq;

public class SolutionGenerator {
    
    Random random;
    int numberOfJobs;
    int minMaintenanceLength;
    int maxMaintenanceLength;
    int[] ordered;

    public SolutionGenerator(int numberOfJobs, int minMaintenanceLength, int maxMaintenanceLength)
    {
        random = new Random ();
        this.numberOfJobs = numberOfJobs;
        this.minMaintenanceLength = minMaintenanceLength;
        this.maxMaintenanceLength = maxMaintenanceLength;
        ordered = new int[numberOfJobs];
        for (int i = 0; i < numberOfJobs; i++) ordered[i] = i;
    }

    public Solution Generate()
    {
        Solution solution = new Solution
        {
            maintenanceDecision = new bool[numberOfJobs],
            maintenanceDuration = new int[numberOfJobs]
        };
        for (int i = 0; i < numberOfJobs; i++) {
            solution.maintenanceDecision[i] = (random.Next (2) == 0);
            solution.maintenanceDuration[i] = random.Next (minMaintenanceLength, maxMaintenanceLength);
        }
        solution.m1 = ordered.OrderBy (x => random.Next ()).ToArray ();
        solution.m2 = ordered.OrderBy (x => random.Next ()).ToArray ();
        return solution;
    }

    public void Generate(Solution solution)
    {
        for (int i = 0; i < numberOfJobs; i++) {
            solution.maintenanceDecision[i] = (random.Next (2) == 0);
            solution.maintenanceDuration[i] = random.Next (minMaintenanceLength, maxMaintenanceLength);
            solution.m1[i] = i;
            solution.m2[i] = i;
        }
        Shuffle (solution.m1);
        Shuffle (solution.m2);
        solution.m2 = ordered.OrderBy (x => random.Next ()).ToArray ();
    }

    void Shuffle(int[] array)
    {
        int r, t, n = array.Length;
        for (int i = 0; i < n; i++) {
            r = i + random.Next (n - i);
            t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }
}
