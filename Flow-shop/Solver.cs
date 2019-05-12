//#define DEBUG

using System;
using System.Diagnostics;
using System.Xml.Serialization;

public class Solver {

    public long TimeLimit { get; set; } = 30000;
    public int GenerationLimit { get; set; } = 1000000;
    public int InitialPopulationSize { get; set; } = 100;
    [XmlIgnore] public int MinMaintenanceLength { get; private set; }
    public int MaxMaintenanceLength { get; set; } = 100;
    public int AgeLimit { get; set; } = 10000;
    public int SelectionLimit { get; set; } = 60;
    public double SelectionWeight { get; set; } = 2;
    public double CrossoverRate { get; set; } = 0.9;
    public double MutationRate { get; set; } = 0.15;
    public bool UseTwoPointCrossover { get; set; } = true;

    [XmlIgnore] public int StartingPoint { get; private set; }
    [XmlIgnore] public Solution BestSolution { get; private set; }
    [XmlIgnore] public int BestScore { get; private set; }

    Solution localBest;
    int localScore;

    Instance instance;
    Random random;
    Population population;
    SolutionGenerator generator;
    Roulette roulette;
    int age;

    int[] buffer;

    double selectionTime;
    double crossoverTime;
    double mutationTime;

    public void Solve()
    {
        Stopwatch stopwatch = new Stopwatch ();
        Stopwatch secondaryStopwatch = new Stopwatch ();
        stopwatch.Start ();
        Console.WriteLine ($"Wyszukiwanie rozwiazan instancji {instance.id}");
        Debug.Indent ();

        int generationCounter = 0;
        Init ();
        Debug.WriteLine ($"Minimalny czas przerw technicznych dla instancji: {MinMaintenanceLength}");

        GeneratePopulation (InitialPopulationSize);
        GetBestSolution ();
        StartingPoint = localScore;

        Debug.WriteLine ("Wygenerowano populacje poczatkowa");
        Debug.WriteLine ($"Czas: {ElapsedTime (stopwatch.Elapsed)}");
        Debug.WriteLine ($"Najlepsze rozwiazanie losowe: {StartingPoint}");

        int selected;
        while (generationCounter < GenerationLimit && stopwatch.ElapsedMilliseconds < TimeLimit) {

            secondaryStopwatch.Restart ();
            selected = Selection ();
            selectionTime += secondaryStopwatch.Elapsed.TotalMilliseconds;

            secondaryStopwatch.Restart ();
            Crossover (selected);
            crossoverTime += secondaryStopwatch.Elapsed.TotalMilliseconds;

            secondaryStopwatch.Restart ();
            Mutation (selected);
            mutationTime += secondaryStopwatch.Elapsed.TotalMilliseconds;

            generationCounter++;

            Debug.WriteLineIf (generationCounter % 1000 == 0, $"Pokolenie {generationCounter}:\t{BestScore}\t{localScore}");
        }
        GetBestSolution ();

        stopwatch.Stop ();
        secondaryStopwatch.Stop ();
        Debug.Unindent ();
        Console.WriteLine ($"Zakonczono wyszukiwanie rozwiazan instancji {instance.id}");
        Debug.WriteLine ($"Najlepsze znalezione rozwiazanie: {BestScore}");
        Debug.WriteLine ($"Czas: {ElapsedTime (stopwatch.Elapsed)}");
        Debug.WriteLine ($"Liczba pokolen: {generationCounter}");
        Debug.WriteLine ($"Sredni czas selekcji: {selectionTime / generationCounter} ms");
        Debug.WriteLine ($"Sredni czas krzyzowania: {crossoverTime / generationCounter} ms");
        Debug.WriteLine ($"Sredni czas mutacji: {mutationTime / generationCounter} ms");
        Debug.WriteLine ("");
    }

    public void Init(Instance instance)
    {
        this.instance = instance;
        buffer = new int[instance.numberOfJobs];

        double d = 0;
        foreach (int op in instance.op2) {
            d += op;
        }
        MinMaintenanceLength = (int)Math.Ceiling (1.5d * d / instance.numberOfJobs);
        if (MaxMaintenanceLength < MinMaintenanceLength) MaxMaintenanceLength = MinMaintenanceLength;
    }

    void Init()
    {
        random = new Random ();
        roulette = new Roulette ();
        age = 0;

        if (InitialPopulationSize < SelectionLimit) InitialPopulationSize = SelectionLimit;
    }

    void GeneratePopulation(int size)
    {
        int n = SelectionLimit + 1;
        n *= 3;
        if (n < InitialPopulationSize) n = InitialPopulationSize;

        population = new Population (n)
        {
            SolutionSize = instance.numberOfJobs
        };

        generator = new SolutionGenerator (instance.numberOfJobs, MinMaintenanceLength, MaxMaintenanceLength);
        for (int i = 0; i < size; i++) {
            population.Add (generator.Generate ());
        }

        BestSolution = new Solution (instance.numberOfJobs);
        BestScore = 0;
    }

    void RandomizePopulation(int size)
    {
        for (int i = 0; i < size; i++) {
            generator.Generate (population[i]);
        }
    }

    int GetBestSolution()
    {
        int best = 0, score = ObjectiveFunction (population[0]);
        for (int i = 1; i < population.Count; i++) {
            int f = ObjectiveFunction (population[i]);
            if (f < score) {
                best = i;
                score = f;
            }
        }

        localScore = score;
        localBest = population[best];
        if (localScore < BestScore || BestScore == 0) {
            BestSolution.Copy (localBest);
            BestScore = localScore;
        }
        return best;
    }

    int Selection()
    {
        roulette.Clear ();

        int best = 0, score = ObjectiveFunction (population[0]);
        for (int i = 0; i < population.Count; i++) {
            int f = ObjectiveFunction (population[i]);
            if (f < score) {
                best = i;
                score = f;
            }
            roulette.Add (Math.Pow (1d / f, SelectionWeight));
        }

        if (population[best] == localBest) age++;
        else age = 0;

        localScore = score;
        localBest = population[best];
        if (localScore < BestScore || BestScore == 0) {
            BestSolution.Copy (localBest);
            BestScore = localScore;
        }

        if (age < AgeLimit) {
            for (int i = 0; i < SelectionLimit; i++) {
                int r = roulette.Get ();
                population.Persist (r);
            }
            population.Persist (best);
            population.NextGeneration ();
        } else {
            age = 0;
            RandomizePopulation (population.Count);
            return Selection ();
        }
        return population.Count;
    }

    void Crossover(int size)
    {
        if (CrossoverRate == 0) return;
        double r;
        int j;
        for (int i = 0; i < size; i++) {
            r = random.NextDouble ();
            if (r >= CrossoverRate) continue;
            j = random.Next (size - 1);
            if (j >= i) j++;
            population.Add (Cross (population[i], population[j]));
        }
    }

    Solution Cross(Solution primary, Solution secondary)
    {
        Solution offspring = population.New ();
        offspring.Copy (primary);
        switch (random.Next (4)) {
            case 0:
                if (UseTwoPointCrossover)
                    TwoPointSwapOrder (offspring.m1, primary.m1, secondary.m1);
                else
                    OnePointSwapOrder (offspring.m1, primary.m1, secondary.m1);
                break;
            case 1:
                if (UseTwoPointCrossover)
                    TwoPointSwapOrder (offspring.m2, primary.m2, secondary.m2);
                else
                    OnePointSwapOrder (offspring.m2, primary.m2, secondary.m2);
                break;
            case 2:
                TwoPointSwap (offspring.maintenanceDuration, primary.maintenanceDuration, secondary.maintenanceDuration);
                break;
            case 3:
                TwoPointSwap (offspring.maintenanceDecision, primary.maintenanceDecision, secondary.maintenanceDecision);
                break;
            default:
                break;
        }
        return offspring;
    }

    void OnePointSwapOrder(int[] target, int[] primary, int[] secondary)
    {
        int p1 = random.Next (0, target.Length + 1);
        for (int i = 0; i < p1; i++) {
            buffer[primary[i]] = 0;
        }
        for (int i = p1; i < target.Length; i++) {
            buffer[primary[i]] = 1;
        }
        for (int i = 0, j = p1; i < secondary.Length; i++) {
            if (buffer[secondary[i]] == 1) {
                target[j++] = secondary[i];
            }
        }
    }

    void TwoPointSwapOrder(int[] target, int[] primary, int[] secondary)
    {
        int p1 = random.Next (0, target.Length + 1), p2 = random.Next (0, target.Length + 1);
        if (p1 > p2) p1 ^= p2 ^= p1 ^= p2;
        for (int i = 0; i < p1; i++) {
            buffer[primary[i]] = 0;
        }
        for (int i = p1; i < p2; i++) {
            buffer[primary[i]] = 1;
        }
        for (int i = p2; i < target.Length; i++) {
            buffer[primary[i]] = 0;
        }
        for (int i = 0, j = p1; i < secondary.Length; i++) {
            if (buffer[secondary[i]] == 1) {
                target[j++] = secondary[i];
            }
        }
    }

    void TwoPointSwap<T>(T[] target, T[] primary, T[] secondary)
    {
        int p1 = random.Next (0, target.Length + 1), p2 = random.Next (0, target.Length + 1);
        if (p1 > p2) p1 ^= p2 ^= p1 ^= p2;
        for (int i = p1; i < p2; i++) {
            target[i] = secondary[i];
        }
    }

    void Mutation(int size)
    {
        if (MutationRate == 0) return;
        double r;
        for (int i = 0; i < size; i++) {
            r = random.NextDouble ();
            if (r >= MutationRate) continue;

            Solution solution = population[i];
            if (solution == localBest) continue;
            switch (random.Next (5)) {
                case 0:
                    RandomSwap (solution.m1);
                    break;
                case 1:
                    RandomSwap (solution.m2);
                    break;
                case 2:
                    RandomSwap (solution.maintenanceDuration);
                    break;
                case 3:
                    RandomChange (solution.maintenanceDuration, MinMaintenanceLength, MaxMaintenanceLength);
                    break;
                case 4:
                    RandomChange (solution.maintenanceDecision);
                    break;
                default:
                    break;
            }
        }
    }

    void RandomSwap(int[] array)
    {
        int p1 = random.Next (0, array.Length), p2 = random.Next (0, array.Length);
        int tmp = array[p1];
        array[p1] = array[p2];
        array[p2] = tmp;
    }

    void RandomChange(int[] array, int minValue, int maxValue)
    {
        int r = random.Next (0, array.Length);
        array[r] = random.Next (minValue, maxValue);
    }

    void RandomChange(bool[] array)
    {
        int r = random.Next (0, array.Length);
        array[r] = !array[r];
    }

    public int ObjectiveFunction(Solution solution)
    {
        int f = 0;
        double accumulatedPenalty = 0;
        int job, time = 0;

        for (int i = 0; i < instance.numberOfJobs; i++) {
            job = solution.m1[i];
            time += instance.op1[job];
            buffer[job] = time;
            f += time;
        }

        time = 0;
        for (int i = 0; i < instance.numberOfJobs; i++) {
            if (solution.maintenanceDecision[i]) {
                time += solution.maintenanceDuration[i];
                accumulatedPenalty = 0;
            }
            job = solution.m2[i];
            if (time < buffer[job]) {
                time = buffer[job];
            }
            time += (int)Math.Ceiling (instance.op2[job] * (1d + accumulatedPenalty));
            accumulatedPenalty += 0.1d;
            f += time;
        }
        return f;
    }

    string ElapsedTime(TimeSpan ts)
    {
        return string.Format ("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
    }
}
