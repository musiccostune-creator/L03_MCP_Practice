using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SoccerBall : MonoBehaviour
{
    public Rigidbody Body { get; private set; }

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
    }

    public void ResetTo(Vector3 position)
    {
        Body.linearVelocity = Vector3.zero;
        Body.angularVelocity = Vector3.zero;
        Body.position = position;
        transform.position = position;
    }
}
