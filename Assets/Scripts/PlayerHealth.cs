using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public float maxHealth = 100f;
    public float timer = 1.4f;
    [HideInInspector] public bool isDead = false;
    public GameObject gameOverUI;
    public GameObject healthContainer;
    public Image healthBar; // Reference to the health bar image component
    public Color fullHealthColor; // Color for full health
    public Color midHealthColor; // Color for mid health
    public Color depletedHealthColor; // Color for depleted health

    private void Start()
    {
        gameOverUI.SetActive(false);
        healthBar.color = fullHealthColor;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        // Clamp health to ensure it's not below 0
        health = Mathf.Clamp(health, 0f, maxHealth);

        // Update health bar fill amount based on current health
        float healthPercentage = health / maxHealth;
        healthBar.fillAmount = healthPercentage;

        // Update health bar color based on current health
        if (health >= 100f)
        {
            healthBar.color = fullHealthColor;
        }
        else if (health >= 65f)
        {
            healthBar.color = midHealthColor;
        }
        else
        {
            healthBar.color = depletedHealthColor;
        }

        if (health <= 0f)
        {
            isDead = true;
            GetComponent<PlayerScript>().enabled = false;
            GetComponent<Animator>().SetBool("Death", true);
            StartCoroutine(StartCountdown(timer));

           
        }
    }

    IEnumerator StartCountdown(float time)
    {
        yield return new WaitForSeconds(time);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            enemy.GetComponent<NPCController>().enabled = false;
            enemy.GetComponent<NavMeshAgent>().enabled = false;

        }
        gameOverUI.SetActive(true);
        healthContainer.SetActive(false);
    }
}
