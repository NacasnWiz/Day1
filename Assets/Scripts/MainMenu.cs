using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    Slider sensitivitySlider;

    [SerializeField]
    TMP_Text sensitivityText;

    // Start is called before the first frame update
    void Start()
    {
        sensitivitySlider.value = PlayerPrefs.GetInt("Sensitivity", 500);
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        sensitivityText.text = sensitivitySlider.value.ToString();
        PlayerPrefs.SetInt("Sensitivity", (int)sensitivitySlider.value);
    }


    public void OnPlayButtonCLicked()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MainMenu");
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void OnResumeButton()
    {

    }

}
