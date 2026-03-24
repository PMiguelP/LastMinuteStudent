using UnityEngine;

public abstract class RunnerPickup : MonoBehaviour
{
    public abstract void Collect(PlayerRunnerController player);
}