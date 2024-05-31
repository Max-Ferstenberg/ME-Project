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

    void Awake()
    {
        // Initialize scenarios with their names and initially unlock only the first scenario
        scenarios = new List<Scenario>
        {
            new Scenario(true, "Scenario1"),
            new Scenario(false, "Scenario2A"),
            new Scenario(false, "Scenario2B"),
            new Scenario(false, "Scenario3A"),
            new Scenario(false, "Scenario3B")
        };
    }

    void Start()
    {
        Debug.Log("Initialized scenarios:");
        foreach (var scenario in scenarios)
        {
            Debug.Log($"Scenario: {scenario.sceneName}, Unlocked: {scenario.isUnlocked}");
        }

        UpdateScenariosUnlockStatus();
    }

    public void UpdateScenariosUnlockStatus()
    {
        Debug.Log("Updating scenario unlock statuses...");

        if (scenarioButtons == null)
        {
            Debug.LogError("Scenario buttons are not assigned!");
            return;
        }

        if (scenarios == null)
        {
            Debug.LogError("Scenarios list is not initialized!");
            return;
        }

        for (int i = 0; i < scenarioButtons.Length; i++)
        {
            if (i < scenarios.Count)
            {
                Debug.Log($"Setting button {i} interactable to {scenarios[i].isUnlocked}");
                if (scenarioButtons[i] != null)
                {
                    scenarioButtons[i].interactable = scenarios[i].isUnlocked;
                    scenarioButtons[i].onClick.RemoveAllListeners();
                    if (scenarios[i].isUnlocked)
                    {
                        int scenarioIndex = i;
                        scenarioButtons[i].onClick.AddListener(() => LoadScenario(scenarioIndex));
                    }
                }
                else
                {
                    Debug.LogError($"Scenario button {i} is not assigned!");
                }
            }
            else
            {
                if (scenarioButtons[i] != null)
                {
                    scenarioButtons[i].gameObject.SetActive(false); // Hide unused buttons
                }
                else
                {
                    Debug.LogError($"Scenario button {i} is not assigned!");
                }
            }
        }
    }

    public void LoadScenario(int index)
    {
        if (index < scenarios.Count && scenarios[index].isUnlocked)
        {
            Debug.Log($"Loading scenario: {scenarios[index].sceneName}");
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.FadeToScene(scenarios[index].sceneName);
            }
            else
            {
                Debug.LogError("FadeManager instance is not found.");
            }
        }
        else
        {
            Debug.LogError($"Attempted to load an invalid or locked scenario at index {index}");
        }
    }

    public void UnlockScenarioBasedOnCharacter(string characterName)
    {
        Debug.Log($"Unlocking scenario based on character: {characterName}");
        switch (characterName)
        {
            case "Dr. Hana Lane":
                UnlockScenario("Scenario2A");
                break;
            case "Dr. Aaron Pearson":
                UnlockScenario("Scenario2B");
                break;
            case "Sara":
                UnlockScenario("Scenario3A");
                break;
            case "Li-Mei":
                UnlockScenario("Scenario3B");
                break;
            default:
                Debug.LogError("Unknown character name: " + characterName);
                break;
        }
        UpdateScenariosUnlockStatus();
    }

    private void UnlockScenario(string sceneName)
    {
        Debug.Log($"Attempting to unlock scenario: {sceneName}");
        var scenario = scenarios.Find(s => s.sceneName == sceneName);
        if (scenario != null)
        {
            scenario.isUnlocked = true;
            Debug.Log($"Unlocked scenario: {sceneName}");
        }
        else
        {
            Debug.LogError($"Scenario not found: {sceneName}");
        }
    }
}
