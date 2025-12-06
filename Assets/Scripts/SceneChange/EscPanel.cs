using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscPanel : MonoBehaviour
{
    [SerializeField] private GameObject escPanel;
    
    private bool isEsc = false;
    private string sceneName;

    void Start()
    {
        escPanel.SetActive(false);
        sceneName = SceneManager.GetActiveScene().name;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isEsc== false)
            ShowPanel();
        else if(Input.GetKeyDown(KeyCode.Escape))
            HidePanel();
    }

    private void ShowPanel()
    {
        Time.timeScale = 0;
        escPanel.SetActive(true);
        isEsc = true;
    }

    private void HidePanel()
    {
        Time.timeScale = 1;
        escPanel.SetActive(false);
        isEsc = false;
    }

    public void Continue()
    {
        HidePanel();
    }

    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }

    public void ReStart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    public void Back()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
