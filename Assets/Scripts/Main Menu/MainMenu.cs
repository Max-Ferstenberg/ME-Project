using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public ScenarioManager scenarioManager;
    public Button[] scenarioButtons;

    void Start()
    {
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (scenarioManager == null)
        {
            Debug.LogError("ScenarioManager is not assigned!");
            return;
        }

        scenarioManager.UpdateScenariosUnlockStatus();

        for (int i = 0; i < scenarioButtons.Length; i++)
        {
            var scenario = scenarioManager.scenarios[i];
            scenarioButtons[i].interactable = scenario.isUnlocked;
        }
    }

}
