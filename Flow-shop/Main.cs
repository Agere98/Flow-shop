using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

// Ta bardzo brzydka i nieczytelna klasa statyczna oprócz funkcji Main zawiera głównie operacje na plikach.
// Implementacja algorytmu genetycznego znajduje się w klasie Solver.
static class FlowShop {

    static string instancesDirectory = "instances";
    static readonly string persistentData = "PersistentData";
    static readonly string instanceProperties = "InstanceGeneratorProperties.xml";
    static readonly string solverProperties = "SolverProperties.xml";
    static readonly string scheduleProperties = "Schedule.xml";

    static Solver solver;

    [Serializable]
    class Persistent {
        public int counter;
    }
    static Persistent persistent;

    static void Main(string[] args)
    {
        CheckFiles ();

        PrintUsage ();
        string mode = Console.ReadLine ();

        Debug.Listeners.Add (new TextWriterTraceListener (Console.Out));
        Debug.AutoFlush = true;

        switch (mode) {
            case "g":
                GenerateInstances ();
                break;
            case "a":
                SolveAll ();
                break;
            case "s":
                GetSchedule ();
                break;
            case "i":
                Import ();
                break;
            default:
                int id;
                if (int.TryParse (mode, out id)) {
                    string file = $@"{instancesDirectory}\{id}.in";
                    if (File.Exists (file)) {
                        SolveInstance (file);
                    } else {
                        Console.WriteLine ("Nie znaleziono instancji o podanym id");
                    }
                } else {
                    Console.WriteLine ("Nieznane polecenie");
                    return;
                }
                break;
        }

        BinarySerialize (persistent, persistentData);
    }

    static void PrintUsage()
    {
        Console.WriteLine ("g - uruchamia generator instancji");
        Console.WriteLine ("a - uruchamia rozwiazywanie wszystkich nierozwiazanych instancji");
        Console.WriteLine ("s - uruchamia rozwiazywanie instancji wedlug harmonogramu");
        Console.WriteLine ("i - importuje pliki instancji");
        Console.WriteLine ("<id> - uruchamia rozwiazywanie instancji o podanym id");
    }

    static void GenerateInstances()
    {
        InstanceGenerator instanceGenerator;
        if (File.Exists (instanceProperties)) instanceGenerator = XmlDeserialize<InstanceGenerator> (instanceProperties);
        else {
            instanceGenerator = new InstanceGenerator ();
        }

        instanceGenerator.InitCounter (persistent.counter);
        for (int i = 0; i < instanceGenerator.GeneratedInstances; i++) {
            Instance instance = instanceGenerator.Generate ();
            WriteToFile (instance);
            BinarySerialize (instance, $@"{instancesDirectory}\{instance.id}.in");
            persistent.counter++;
            BinarySerialize (persistent, persistentData);
        }
        Console.WriteLine ("Liczba wygenerowanych instancji: " + instanceGenerator.GeneratedInstances);
    }

    static void SolveInstance(string instanceFile)
    {
        Instance instance = BinaryDeserialize<Instance> (instanceFile);
        if (solver == null) {
            if (File.Exists (solverProperties)) {
                solver = XmlDeserialize<Solver> (solverProperties);
            } else {
                solver = new Solver ();
            }
        }

        solver.Init (instance);
        solver.Solve ();
        WriteToFile (solver.BestSolution, instance, solver.StartingPoint);
    }

    static void SolveAll()
    {
        string[] instances = Directory.GetFiles (instancesDirectory, "*.in");
        foreach (string instanceFile in instances) {
            string outFile = instanceFile.Replace (".in", "_out.txt");
            if (!File.Exists (outFile)) {
                SolveInstance (instanceFile);
            }
        }
    }

    static void GetSchedule()
    {
        List<ScheduledSolver> schedule;
        if (File.Exists (scheduleProperties)) schedule = XmlDeserialize<List<ScheduledSolver>> (scheduleProperties);
        else {
            schedule = new List<ScheduledSolver> { new ScheduledSolver () };
        }
        string tmp = instancesDirectory;
        foreach(ScheduledSolver scheduledSolver in schedule) {
            solver = scheduledSolver;
            instancesDirectory = scheduledSolver.OutputDirectory;
            if (!Directory.Exists (instancesDirectory)) Directory.CreateDirectory (instancesDirectory);
            for (int i = scheduledSolver.FromInstance; i <= scheduledSolver.ToInstance; i++) {
                string instanceFile = $@"{tmp}\{i}.in";
                if (File.Exists (instanceFile)) {
                    SolveInstance (instanceFile);
                }
            }
        }
        instancesDirectory = tmp;
        solver = null;
    }

    static void Import()
    {
        string[] files = Directory.GetFiles (instancesDirectory, "*_in.txt");
        foreach (string instanceFile in files) {
            string inFile = instanceFile.Replace ("_in.txt", ".in");
            if (!File.Exists (inFile)) {
                StreamReader file = new StreamReader (instanceFile);
                int id, size, a, b, i = 0;
                string s = file.ReadLine ().Trim (new char[] { '*', ' ' });
                int.TryParse (s, out id);
                s = file.ReadLine ();
                int.TryParse (s, out size);
                Instance instance = new Instance (id, size);
                while ((s = file.ReadLine ()) != "*** EOF ***") {
                    string[] nums = s.Split (new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse (nums[0], out a);
                    int.TryParse (nums[1], out b);
                    instance.op1[i] = a;
                    instance.op2[i] = b;
                    i++;
                }
                BinarySerialize (instance, inFile);
            }
        }
    }

    static void CheckFiles()
    {
        if (Directory.Exists (instancesDirectory)) {
            if (File.Exists (persistentData)) {
                persistent = BinaryDeserialize<Persistent> (persistentData);
            } else persistent = new Persistent ();
        } else {
            Directory.CreateDirectory (instancesDirectory);
            persistent = new Persistent ();
        }

        if (!File.Exists (instanceProperties)) XmlSerialize (new InstanceGenerator (), instanceProperties);
        if (!File.Exists (solverProperties)) XmlSerialize (new Solver (), solverProperties);
        if (!File.Exists (scheduleProperties)) XmlSerialize (new List<ScheduledSolver> { new ScheduledSolver { OutputDirectory = instancesDirectory } }, scheduleProperties);
    }

    static void XmlSerialize<T>(T serializable, string fileName)
    {
        XmlSerializer serializer = new XmlSerializer (typeof (T));
        FileStream fileStream = new FileStream (fileName, FileMode.Create, FileAccess.Write);
        serializer.Serialize (fileStream, serializable);
        fileStream.Close ();
    }

    static T XmlDeserialize<T>(string fileName)
    {
        XmlSerializer serializer = new XmlSerializer (typeof (T));
        FileStream fileStream = new FileStream (fileName, FileMode.Open, FileAccess.Read);
        T serializable = (T)serializer.Deserialize (fileStream);
        fileStream.Close ();
        return serializable;
    }

    static void BinarySerialize<T>(T serializable, string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter ();
        FileStream fileStream = new FileStream (fileName, FileMode.Create, FileAccess.Write);
        bf.Serialize (fileStream, serializable);
        fileStream.Close ();
    }

    static T BinaryDeserialize<T>(string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter ();
        FileStream fileStream = new FileStream (fileName, FileMode.Open, FileAccess.Read);
        T serializable = (T)bf.Deserialize (fileStream);
        fileStream.Close ();
        return serializable;
    }

    static void WriteToFile(Solution solution, Instance instance, int randomScore)
    {
        StringBuilder m1 = new StringBuilder ("M1: ");
        StringBuilder m2 = new StringBuilder ("M2: ");
        int f = 0;
        double accumulatedPenalty = 0;
        int job, time = 0, maint = 0, maintSum = 0, idle = 0, idleSum = 0;
        int[] buffer = new int[instance.numberOfJobs];

        for (int i = 0; i < instance.numberOfJobs; i++) {
            job = solution.m1[i];
            m1.Append ($"op1_{job + 1}, {time}, {instance.op1[job]}; ");
            time += instance.op1[job];
            buffer[job] = time;
            f += time;
        }

        time = 0;
        for (int i = 0; i < instance.numberOfJobs; i++) {
            if (solution.maintenanceDecision[i]) {
                m2.Append ($"maint{++maint}_M2, {time}, {solution.maintenanceDuration[i]}; ");
                maintSum += solution.maintenanceDuration[i];
                time += solution.maintenanceDuration[i];
                accumulatedPenalty = 0;
            }
            job = solution.m2[i];
            if (time < buffer[job]) {
                m2.Append ($"idle{++idle}_M2, {time}, {buffer[job] - time}; ");
                idleSum += buffer[job] - time;
                time = buffer[job];
            }
            int real = (int)Math.Ceiling (instance.op2[job] * (1d + accumulatedPenalty));
            m2.Append ($"op2_{job + 1}, {time}, {instance.op2[job]}, {real}; ");
            time += real;
            accumulatedPenalty += 0.1d;
            f += time;
        }

        List<string> text = new List<string>
        {
            $"**** {instance.id} ****",
            $"{f}, {randomScore}",
            m1.ToString (),
            m2.ToString (),
            "0, 0",
            $"{maint}, {maintSum}",
            "0, 0",
            $"{idle}, {idleSum}",
            "*** EOF ***"
        };
        File.WriteAllLines ($@"{instancesDirectory}\{instance.id}_out.txt", text);
    }

    static void WriteToFile(Instance instance)
    {
        List<string> text = new List<string>
        {
            $"**** {instance.id} ****",
            instance.numberOfJobs.ToString()
        };
        for (int i = 0; i < instance.numberOfJobs; i++) {
            text.Add ($"{instance.op1[i]}; {instance.op2[i]};");
        }
        text.Add ("*** EOF ***");
        File.WriteAllLines ($@"{instancesDirectory}\{instance.id}_in.txt", text);
    }
}
