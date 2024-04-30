using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    public TextMeshProUGUI textScore;
    public GameObject missionCompleteUI;

    private int currentKill = 0;
    public int totalKills = 4;

    public float timer = 1.4f;

    private void Start()
    {
        missionCompleteUI.SetActive(false);
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        totalKills = enemyList.Length;
      
        textScore.text = $"{currentKill}/{totalKills}";
    }

    public void AddScore(int _kill)
    {
        currentKill += _kill;
        textScore.text = $"{currentKill}/{totalKills}";

        if(currentKill == totalKills)
        {
            //mission complete
            StartCoroutine(StartCountdown(timer));
        }
    }

    IEnumerator StartCountdown(float time)
    {
        yield return new WaitForSeconds(time);
        missionCompleteUI.SetActive(true);

    }
}
