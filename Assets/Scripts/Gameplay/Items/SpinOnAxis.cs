using UnityEngine;

public class SpinOnAxis : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 90f;

    private void Update()
    {
        transform.Rotate(axis * degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
