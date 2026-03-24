using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnTable", menuName = "Runner/Spawn Table")]
public class SpawnTable : ScriptableObject
{
    [SerializeField] private List<SpawnItemDefinition> items = new List<SpawnItemDefinition>();

    public SpawnItemDefinition PickItem(float score, RunnerLane lane)
    {
        return PickItemInternal(score, lane, false, RunnerItemType.Coin);
    }

    public SpawnItemDefinition PickItemByType(float score, RunnerLane lane, RunnerItemType itemType)
    {
        return PickItemInternal(score, lane, true, itemType);
    }

    private SpawnItemDefinition PickItemInternal(float score, RunnerLane lane, bool filterByType, RunnerItemType itemType)
    {
        int totalWeight = 0;

        foreach (SpawnItemDefinition item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (filterByType && item.ItemType != itemType)
            {
                continue;
            }

            if (item.IsEligible(score, lane))
            {
                totalWeight += item.Weight;
            }
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        int runningWeight = 0;

        foreach (SpawnItemDefinition item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (filterByType && item.ItemType != itemType)
            {
                continue;
            }

            if (!item.IsEligible(score, lane))
            {
                continue;
            }

            runningWeight += item.Weight;
            if (roll < runningWeight)
            {
                return item;
            }
        }

        return null;
    }
}