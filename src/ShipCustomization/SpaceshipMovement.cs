using UnityEngine;

public class SpaceshipMovement : MonoBehaviour
{
    public float thrustForce = 10f;
    public float maxSpeed = 20f;
    public float rotationSpeed = 90f;
    public float brakingForce = 5f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        if (Input.GetKey(KeyCode.W))
        {
            _rb.AddForce(transform.forward * thrustForce);
        }
        if (Input.GetKey(KeyCode.S))
        {
            _rb.AddForce(-transform.forward * brakingForce);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _rb.AddTorque(Vector3.up * -rotationSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rb.AddTorque(Vector3.up * rotationSpeed);
        }

        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
    }
}
