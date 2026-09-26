using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject MainBG;
    public GameObject SetBG;
    public GameObject DiffBG;
    public GameObject SoundSettings;
    public GameObject GameplaySettings;

    public void openSetBG()
    {
        MainBG.SetActive(false);
        SetBG.SetActive(true);
        DiffBG.SetActive(false);
        SoundSettings.SetActive(false);
        GameplaySettings.SetActive(false);
    }

    public void openDiffBG()
    {
        MainBG.SetActive(false);
        SetBG.SetActive(false);
        DiffBG.SetActive(true);
        SoundSettings.SetActive(false);
    }

    public void openMainBG()
    {
        MainBG.SetActive(true);
        SetBG.SetActive(false);
        DiffBG.SetActive(false);
        SoundSettings.SetActive(false);
    }

    public void openSoundSettings()
    {
        SetBG.SetActive(false);
        SoundSettings.SetActive(true);
    }
    public void openGameplaySettings()
    {
        SetBG.SetActive(false);
        GameplaySettings.SetActive(true);
    }
    public void quitGame()
    {
        Application.Quit();
    }
}
