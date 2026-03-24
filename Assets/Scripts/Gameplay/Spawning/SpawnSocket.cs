using UnityEngine;

public class SpawnSocket : MonoBehaviour
{
    [SerializeField] private RunnerLane lane = RunnerLane.Middle;

    public RunnerLane Lane => lane;
}