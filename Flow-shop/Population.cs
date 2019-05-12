public class Population {

    public int Count { get; private set; }
    public int Capacity { get; private set; }
    public int SolutionSize { get; set; }

    Solution[] population;
    Solution[] offspring;
    Pool<Solution> solutions;
    bool[] persistenceMarkers;

    public Population(int capacity)
    {
        population = new Solution[capacity];
        offspring = new Solution[capacity];
        solutions = new Pool<Solution> (capacity);
        persistenceMarkers = new bool[capacity];
        Count = 0;
        Capacity = capacity;
    }

    public Solution this[int i] => population[i];

    public void Add(Solution solution)
    {
        population[Count++] = solution;
    }

    public void Persist(int index)
    {
        persistenceMarkers[index] = true;
    }

    public void NextGeneration()
    {
        int count = 0;
        for (int i = 0; i < Count; i++) {
            if (persistenceMarkers[i]) {
                offspring[count++] = population[i];
                persistenceMarkers[i] = false;
            } else solutions.Push (population[i]);
        }
        var tmp = population;
        population = offspring;
        offspring = tmp;
        Count = count;
    }

    public Solution New()
    {
        Solution solution = solutions.TryPop ();
        if (solution == null) {
            solution = new Solution (SolutionSize);
        }
        return solution;
    }
}
