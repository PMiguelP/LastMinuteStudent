using UnityEngine;

public abstract class RunnerHazard : MonoBehaviour
{
    public abstract void Hit(PlayerRunnerController player);
}