using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // Include for scene management

public class Scenario
{
    public bool isUnlocked;
    public string sceneName;

    public Scenario(bool isUnlocked, string sceneName)
    {
        this.isUnlocked = isUnlocked;
        this.sceneName = sceneName;
    }
}

public class ScenarioManager : MonoBehaviour
{
    public List<Scenario> scenarios;
    public Button[] scenarioButtons;
    public Button debugUnlockButton;

    void Start()
    {
        // Initialize scenarios with their names and initially unlock only the first scenario
        scenarios = new List<Scenario>
        {
            new Scenario(true, "Scenario1"),
            new Scenario(false, "Scenario2"),
            new Scenario(false, "Scenario3")
        };
        UpdateScenariosUnlockStatus();
        debugUnlockButton.onClick.AddListener(() => DebugUnlockNextScenario());
    }

    public void UpdateScenariosUnlockStatus()
    {
        for (int i = 0; i < scenarioButtons.Length; i++)
        {
            scenarioButtons[i].interactable = scenarios[i].isUnlocked;
            scenarioButtons[i].onClick.RemoveAllListeners();
            if (scenarios[i].isUnlocked)
            {
                int scenarioIndex = i;
                scenarioButtons[i].onClick.AddListener(() => LoadScenario(scenarioIndex));
            }
        }
    }

    public void LoadScenario(int index)
    {
        if (scenarios[index].isUnlocked)
        {
            SceneManager.LoadScene(scenarios[index].sceneName);
        }
    }

    private void DebugUnlockNextScenario()
    {
        // Find the first locked scenario and unlock it
        int lockedIndex = scenarios.FindIndex(s => !s.isUnlocked);
        if (lockedIndex != -1)
        {
            scenarios[lockedIndex].isUnlocked = true;
            UpdateScenariosUnlockStatus();
        }
    }
}
