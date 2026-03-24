using UnityEngine;

public class RunnerObstacle : RunnerHazard
{
    [SerializeField] private bool causesStumble = true;

    public override void Hit(PlayerRunnerController player)
    {
        if (player == null)
        {
            return;
        }

        player.HandleHazardHit(causesStumble);
    }
}
