using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemySprite;
    [TextArea(2, 4)]
    [SerializeField] private string enemyDescription;

    [Header("Health")]
    [SerializeField] private int healthMin;
    [SerializeField] private int healthMax;

    [Header("Attack")]
    [SerializeField] private int attackMin;
    [SerializeField] private int attackMax;

    [Header("Reward")]
    [SerializeField] private int rewardMin;
    [SerializeField] private int rewardMax;

    // Getters
    public string GetName() => enemyName;
    public Sprite GetSprite() => enemySprite;
    public string GetDescription() => enemyDescription;
    public int GetHealthMin() => healthMin;
    public int GetHealthMax() => healthMax;
    public int GetAttackMin() => attackMin;
    public int GetAttackMax() => attackMax;
    public int GetRewardMin() => rewardMin;
    public int GetRewardMax() => rewardMax;
}