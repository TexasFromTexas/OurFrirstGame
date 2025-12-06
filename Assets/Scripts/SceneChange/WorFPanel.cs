using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorFPanel : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    [SerializeField] private HealthSystem_New PlayerHp;
    [SerializeField] private List<HealthSystem_New> EnemiesHp;

    void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }
    void Update()
    {
        if (PlayerHp.GetCurrentHealth() <= 0)
        {
            Time.timeScale = 0;
            losePanel.SetActive(true);
        } ;
        if (isWin())
        {
            Time.timeScale = 0;
            winPanel.SetActive(true);
        }
    }

    private bool isWin()
    {
        foreach (var enemyHp in EnemiesHp)
        {
            if(enemyHp == null)
                return true;
            if (enemyHp.GetCurrentHealth() >= 0)
                return false;
        }

        return true;
    }
}
