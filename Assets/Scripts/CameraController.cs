using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] new Transform transform;
    Transform target;
    [SerializeField] float speed;

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }

    public void Initialize(Transform newTarget)
    {
        target = newTarget;
    }
}
