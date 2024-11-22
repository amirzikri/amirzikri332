using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadSimulationTypeScene()
    {
        SceneManager.LoadScene("Simulation Type");
    }

    public void QuitApplication()
    {
        Application.Quit();
        Debug.Log("Application Quit");
    }

    public void LoadTreeScene()
    {
        SceneManager.LoadScene("Tree");
    }

    public void LoadGraphScene()
    {
        SceneManager.LoadScene("Graph");
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }
}
