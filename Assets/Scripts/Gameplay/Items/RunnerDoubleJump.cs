using UnityEngine;

public class RunnerDoubleJump : RunnerPickup
{
    public override void Collect(PlayerRunnerController player)
    {
        if (player == null)
        {
            return;
        }

        player.ApplyDoubleJump();
        gameObject.SetActive(false);
    }
}
