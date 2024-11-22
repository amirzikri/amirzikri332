using System;
using System.Diagnostics;
using UnityEngine;
using TMPro; // Import TMP namespace
using System.Collections.Generic;
using System.Linq;
using System.IO;


[CreateAssetMenu(fileName = "DroneNetwork", menuName = "Network/DroneNetwork")]
public class DroneNetworkCommunication : ScriptableObject
{
    private Dictionary<Drone, List<Drone>> adjacencyList;

    [SerializeField] private TextMeshProUGUI performanceText; // Reference to TMP text for output

    private void OnEnable()
    {
        adjacencyList = new Dictionary<Drone, List<Drone>>();
    }

    public void AddNode(Drone drone)
    {
        if (!adjacencyList.ContainsKey(drone))
        {
            adjacencyList[drone] = new List<Drone>();
        }
    }

    public void AddEdge(Drone drone1, Drone drone2)
    {
        if (adjacencyList.ContainsKey(drone1) && adjacencyList.ContainsKey(drone2))
        {
            if (!adjacencyList[drone1].Contains(drone2))
            {
                adjacencyList[drone1].Add(drone2);
                adjacencyList[drone2].Add(drone1);
            }
        }
    }

    public void RemoveNode(Drone drone)
    {
        if (adjacencyList.ContainsKey(drone))
        {
            foreach (var neighbor in adjacencyList[drone])
            {
                adjacencyList[neighbor].Remove(drone);
            }
            adjacencyList.Remove(drone);
        }
    }

    public List<int> FindShortestPath(int startId, int targetId)
{
    Stopwatch stopwatch = new Stopwatch(); // Start stopwatch to measure performance time
    stopwatch.Start();

    Drone startDrone = adjacencyList.Keys.FirstOrDefault(d => d.Id == startId);
    Drone targetDrone = adjacencyList.Keys.FirstOrDefault(d => d.Id == targetId);

    if (startDrone == null || targetDrone == null)
    {
        UnityEngine.Debug.Log("One or both drones not found.");
        stopwatch.Stop(); // Stop stopwatch before returning
        UnityEngine.Debug.Log($"FindShortestPath took {stopwatch.ElapsedMilliseconds} ms");
        return null;
    }

    Queue<List<Drone>> queue = new Queue<List<Drone>>();
    HashSet<Drone> visited = new HashSet<Drone>();

    queue.Enqueue(new List<Drone> { startDrone });
    visited.Add(startDrone);

    // Initialize the CSV logger for network performance
    InitializeNetworkPerformanceCSV();

    while (queue.Count > 0)
    {
        var path = queue.Dequeue();
        var current = path.Last();

        if (current == targetDrone)
        {
            stopwatch.Stop(); // Stop stopwatch before returning the result
            UnityEngine.Debug.Log($"FindShortestPath took {stopwatch.ElapsedMilliseconds} ms");

            // Log the network performance to CSV
            LogNetworkPerformance(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), startId, targetId, stopwatch.ElapsedMilliseconds);

            return path.Select(d => d.Id).ToList();
        }

        foreach (var neighbor in GetNeighbors(current))
        {
            if (!visited.Contains(neighbor))
            {
                visited.Add(neighbor);
                var newPath = new List<Drone>(path) { neighbor };
                queue.Enqueue(newPath);
            }
        }
    }

    stopwatch.Stop(); // Stop stopwatch before returning
    UnityEngine.Debug.Log($"FindShortestPath took {stopwatch.ElapsedMilliseconds} ms");

    // Log the network performance to CSV
    LogNetworkPerformance(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), startId, targetId, stopwatch.ElapsedMilliseconds);

    return null;
}

  private StreamWriter networkPerformanceCsvWriter;
private bool isCsvInitialized = false;

private void InitializeNetworkPerformanceCSV()
{
    // Ensure the CSV writer is initialized only once
    if (isCsvInitialized)
        return;

    string filePath = Path.Combine(Application.dataPath, "NetworkPerformance.csv");

    try
    {
        // Open the CSV file in append mode, so new data is added to the existing file
        networkPerformanceCsvWriter = new StreamWriter(filePath, true); // 'true' means append mode
       
        // If it's the first time writing to the file, write the header
        if (new FileInfo(filePath).Length == 0)
        {
            networkPerformanceCsvWriter.WriteLine("Timestamp, Start Drone ID, Target Drone ID, Time Taken (ms)");
        }

        // Mark CSV as initialized
        isCsvInitialized = true;
    }
    catch (IOException e)
    {
        UnityEngine.Debug.LogError($"Error initializing CSV file: {e.Message}");
    }
}

private void LogNetworkPerformance(string timestamp, int startId, int targetId, long timeTaken)
{
    // Ensure the StreamWriter is open
    if (networkPerformanceCsvWriter != null)
    {
        try
        {
            // Write the network performance data (e.g., time taken to find the path)
            networkPerformanceCsvWriter.WriteLine($"{timestamp}, {startId}, {targetId}, {timeTaken}");
            networkPerformanceCsvWriter.Flush(); // Make sure it's written to the file
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError($"Error writing to CSV: {e.Message}");
        }
    }
    else
    {
        UnityEngine.Debug.LogError("StreamWriter is not initialized.");
    }
}

private void CloseNetworkPerformanceCSV()
{
    // Ensure the file is properly closed after logging is complete
    if (networkPerformanceCsvWriter != null)
    {
        networkPerformanceCsvWriter.Close();
        networkPerformanceCsvWriter = null; // Reset the writer
        isCsvInitialized = false; // Reset initialization flag
    }
}




    public List<Drone> GetNodes()
    {
        return adjacencyList.Keys.ToList();
    }

    public List<Drone> GetNeighbors(Drone drone)
    {
        if (adjacencyList.ContainsKey(drone))
        {
            return adjacencyList[drone];
        }
        return new List<Drone>();
    }

    public void CalculatePerformance()
{
    int totalNodes = adjacencyList.Count;
    int totalEdges = adjacencyList.Values.Sum(neighbors => neighbors.Count) / 2;
    float averageDegree = totalNodes > 0 ? (float)totalEdges * 2 / totalNodes : 0;

    Stopwatch stopwatch = new Stopwatch();

    // Test BFS performance
    float bfsTime = 0;
    if (totalNodes > 1)
    {
        var firstNode = adjacencyList.Keys.First();
        var lastNode = adjacencyList.Keys.Last();

        stopwatch.Start();
        var path = FindShortestPath(firstNode.Id, lastNode.Id);
        stopwatch.Stop();

        bfsTime = stopwatch.ElapsedMilliseconds;
    }

    // Update TMP text with the performance metrics
    if (performanceText != null)
    {
        performanceText.text = $"Performance Metrics:\n" +
                               $"- Total Nodes: {totalNodes}\n" +
                               $"- Total Edges: {totalEdges}\n" +
                               $"- Avg Degree: {averageDegree:F2}\n" +
                               $"- BFS Time: {bfsTime:F2} ms";
    }

    // Output results to the console
    UnityEngine.Debug.Log($"Performance Metrics:");
    UnityEngine.Debug.Log($"- Total Nodes: {totalNodes}");
    UnityEngine.Debug.Log($"- Total Edges: {totalEdges}");
    UnityEngine.Debug.Log($"- Avg Degree: {averageDegree:F2}");
    UnityEngine.Debug.Log($"- BFS Time: {bfsTime:F2} ms");

    // Save the performance metrics to a CSV file
    SavePerformanceToCSV(totalNodes, totalEdges, averageDegree, bfsTime);
}

private void SavePerformanceToCSV(int totalNodes, int totalEdges, float averageDegree, float bfsTime)
{
    // Define the file path
    string filePath = Application.persistentDataPath + "/performanceMetrics.csv";

    // Check if the file exists
    bool fileExists = File.Exists(filePath);

    // Open the file and append or create the file
    using (StreamWriter writer = new StreamWriter(filePath, true))
    {
        // If it's the first entry (new file), write the header
        if (!fileExists)
        {
            writer.WriteLine("Timestamp,Total Nodes,Total Edges,Avg Degree,BFS Time (ms)");
        }

        // Write the performance data to the CSV file
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        writer.WriteLine($"{timestamp},{totalNodes},{totalEdges},{averageDegree:F2},{bfsTime:F2}");
    }

    UnityEngine.Debug.Log($"Performance data saved to: {filePath}");
}
}
