using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


public class Mission
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int ExperienceReward { get; private set; }
    public int GoldReward { get; private set; }
    public Card CardReward { get; private set; } // Let's assume we have a Card class
    public Dictionary<string, int> StatRewards { get; private set; }
    public Dictionary<string, int> AttackRewards { get; private set; }
    public bool IsCompleted { get; private set; }

    // Constructor for the mission
    public Mission(string name, string description, int experienceReward, int goldReward, Card cardReward, Dictionary<string, int> statRewards, Dictionary<string, int> attackRewards)
    {
        Name = name;
        Description = description;
        ExperienceReward = experienceReward;
        GoldReward = goldReward;
        CardReward = cardReward;
        StatRewards = statRewards;
        AttackRewards = attackRewards;
        IsCompleted = false;
    }

    // Method to mark the mission as completed
    public void CompleteMission()
    {
        IsCompleted = true;
    }
}
