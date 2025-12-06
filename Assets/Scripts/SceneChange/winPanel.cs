using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class winPanel : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene(0);
    }

    public void Next()
    {
        SceneManager.LoadScene(2);
    }
}
