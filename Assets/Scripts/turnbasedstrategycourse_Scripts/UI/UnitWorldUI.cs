using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitWorldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image ManaBarImage;
    [SerializeField] private GameObject ManaBar;
    [SerializeField] private StatSystem StatSystem;

    private void Start()
    {
        StatSystem.OnDamaged += HealthSystem_OnDamaged;
        StatSystem.OnMPUsed += ManaSystem_OnUsed;

        UpdateHealthBar();

        if (StatSystem.IsManaCharacter())
            ManaBar.SetActive(false);
        else
            ManaBar.SetActive(true);
    }

    private void UpdateHealthBar()
    {
        healthBarImage.fillAmount = StatSystem.GetHealthNormalized();
    }

    private void UpdateManaBar()
    {
        ManaBarImage.fillAmount = StatSystem.GetManaNormalized();
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

    private void ManaSystem_OnUsed(object sender, EventArgs e)
    {
        UpdateManaBar();
    }

}
