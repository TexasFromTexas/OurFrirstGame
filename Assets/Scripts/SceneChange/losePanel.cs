using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class losePanel : MonoBehaviour
{
    private string sceneName;
    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }
    
    public void Back()
    {
        SceneManager.LoadScene(0);
    }
    
    public void ReStart()
    {
        SceneManager.LoadScene(sceneName);
    }
}
