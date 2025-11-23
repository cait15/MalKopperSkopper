using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthbarSprite;
    [SerializeField] private float reduceSpeed = 2f;
    private float target = 1f;
    private Camera camera_cam;
    
    private static readonly Color ColorGreen = new Color32(0, 255, 59, 255); 
    private static readonly Color ColorYellow = new Color32(255, 255, 0, 255); 
    private static readonly Color ColorRed = new Color32(255, 0, 0, 255);     

    void Start()
    {
        camera_cam = Camera.main;
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth)
    {
        target = currentHealth / maxHealth;


        if (target > 0.60f) // 100% to 61%
        {
            healthbarSprite.color = ColorGreen;
        }
        else if (target > 0.30f) // 60% to 31%
        {
            healthbarSprite.color = ColorYellow;
        }
        else // 30% to 0%
        {
            healthbarSprite.color = ColorRed;
        }
    }

    void Update()
    {

        transform.rotation = Quaternion.LookRotation(transform.position - camera_cam.transform.position);
        healthbarSprite.fillAmount = Mathf.MoveTowards(healthbarSprite.fillAmount, target, reduceSpeed * Time.deltaTime);
    }
}