using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public ScenarioManager scenarioManager;
    public Button[] scenarioButtons;

    void Start()
    {
        Debug.Log("MainMenu Start called.");
        if (scenarioManager == null || scenarioManager.scenarios == null || scenarioManager.scenarios.Count == 0)
        {
            Debug.LogError("ScenarioManager or scenarios list is not initialized!");
            return;
        }

        UpdateMenu();
    }

    public void UpdateMenu()
    {
        Debug.Log("UpdateMenu called.");

        if (scenarioManager == null)
        {
            Debug.LogError("ScenarioManager is not assigned!");
            return;
        }

        if (scenarioButtons == null || scenarioButtons.Length == 0)
        {
            Debug.LogError("Scenario buttons are not assigned or empty!");
            return;
        }

        string unlockedCharacter = PlayerPrefs.GetString("UnlockedCharacter", null);
        Debug.Log($"Retrieved unlocked character from PlayerPrefs: {unlockedCharacter}");

        if (!string.IsNullOrEmpty(unlockedCharacter))
        {
            Debug.Log($"Unlocked character found: {unlockedCharacter}");
            scenarioManager.UnlockScenarioBasedOnCharacter(unlockedCharacter);
        }
        else
        {
            Debug.Log("No unlocked character found.");
        }

        scenarioManager.UpdateScenariosUnlockStatus();

        for (int i = 0; i < scenarioButtons.Length; i++)
        {
            if (i < scenarioManager.scenarios.Count)
            {
                var scenario = scenarioManager.scenarios[i];
                scenarioButtons[i].interactable = scenario.isUnlocked;
            }
            else
            {
                scenarioButtons[i].gameObject.SetActive(false); // Hide unused buttons
            }
        }
    }

    // Method to clear PlayerPrefs manually during testing
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared manually.");
    }
}
