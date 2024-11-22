using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshPro UI

public class NetworkUIManager : MonoBehaviour
{
    public DroneNetworkCommunication network1;
    public DroneNetworkCommunication network2;
    public TMP_InputField startIdInput;  // Input field for start drone ID
    public TMP_InputField targetIdInput; // Input field for target drone ID
    public TMP_Text outputText;          // Text to display results

    // Button click to find shortest path
    public void OnFindShortestPathButtonClicked()
    {
        if (int.TryParse(startIdInput.text, out int startId) && int.TryParse(targetIdInput.text, out int targetId))
        {
            var path = network1.FindShortestPath(startId, targetId); // Use network1 as an example
            if (path != null && path.Count > 0)
            {
                outputText.text = $"Shortest path: {string.Join(" -> ", path)}";
            }
            else
            {
                outputText.text = "No path found!";
            }
        }
        else
        {
            outputText.text = "Invalid input IDs!";
        }
    }
}
