using UnityEngine;
using UnityEngine.UI;

public class Car : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Text speedText;


    [SerializeField] private Vector3 centerOfMass;

    [SerializeField] private Wheel[] wheels;


    [SerializeField] private float springHeight;
    [SerializeField] private float springForce;
    [SerializeField] private float springDamping;
    [SerializeField] private float brakeAmount;
    [SerializeField] private float topSpeed;
    [SerializeField] private float acceleration;

    private const float turnSensitivity = 30;
    private const float maxSteeringAngle = 40;
    private const float tyreSlip = 1000;

    private float wheelRadius;

    private float timeSinceAccel;
    private float engineForce;

    [System.Serializable] [SerializeField] private struct Wheel
    {
        public Transform transform;
        public Transform mesh;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        wheelRadius = wheels[0].mesh.GetComponent<MeshRenderer>().bounds.extents.y;
    }
    private void OnDrawGizmos()
    {
        rb = GetComponent<Rigidbody>();
        Gizmos.DrawSphere(rb.worldCenterOfMass, 0.1f);
        Gizmos.DrawRay(wheels[0].transform.position, -transform.up);
    }

    private void Update()
    {
        float kmph = transform.InverseTransformDirection(rb.velocity).z * 3.6f;
        timeSinceAccel = Input.GetAxisRaw("Vertical") != 0 ? timeSinceAccel += Time.deltaTime * 0.1f : 0;
        engineForce = Input.GetAxisRaw("Vertical") * Mathf.InverseLerp(0, topSpeed, topSpeed - kmph) * acceleration;

        speedText.text = Mathf.Floor(kmph).ToString() + "km/h";

        for (int i = 0; i < 4; i++)
        {
            Vector3 wheelVelocity = wheels[i].transform.InverseTransformDirection(rb.GetPointVelocity(wheels[i].transform.position));
            float distance = wheelVelocity.z;
            float rollAngle = distance * (180f / Mathf.PI) / wheelRadius * Time.deltaTime;
            if (i < 2)
            {
                float calculatedAngle = wheels[i].transform.localEulerAngles.y;
                calculatedAngle = (calculatedAngle > 180) ? calculatedAngle - 360 : calculatedAngle;
                float steerAmount = (Input.GetAxisRaw("Horizontal") == 0 && calculatedAngle != 0) ? -Mathf.Sign(calculatedAngle) * Time.deltaTime * turnSensitivity : Input.GetAxisRaw("Horizontal") * Time.deltaTime * turnSensitivity;
                wheels[i].transform.Rotate(Vector3.up, steerAmount, Space.Self);
                calculatedAngle = wheels[i].transform.localEulerAngles.y;
                calculatedAngle = (calculatedAngle > 180) ? calculatedAngle - 360 : calculatedAngle;
                bool overSteer = Mathf.Abs(calculatedAngle) > maxSteeringAngle;
                if (overSteer) wheels[i].transform.localRotation = Quaternion.Euler(0, maxSteeringAngle * Mathf.Sign(calculatedAngle), 0);
            }
            wheels[i].mesh.localRotation = Quaternion.Euler(rollAngle, 0, 0) * wheels[i].mesh.localRotation;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            RaycastHit hit;
            Vector3 targetPos;
            bool isHit = Physics.Raycast(wheels[i].transform.position, -transform.up, out hit, springHeight);
            if (isHit)
            {
                targetPos = hit.point + wheels[i].transform.up * wheelRadius;
                Vector3 wheelVel = wheels[i].transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

                //Suspension
                float offset = Mathf.InverseLerp(springHeight, 0, wheels[i].transform.position.y - hit.point.y);
                float calculatedSpringForce = (springForce * offset) - (wheelVel.y * springDamping);
                Vector3 supspensionForce = wheels[i].transform.up * calculatedSpringForce;

                //Acceleration
                Vector3 accelerationForce = engineForce * wheels[i].transform.forward;

                //Braking
                Vector3 brakingForce = Input.GetKey(KeyCode.B) ? -wheels[i].transform.forward * wheelVel.z * brakeAmount : Vector3.zero;

                //Grip
                Vector3 xGripForce = -wheels[i].transform.right * wheelVel.x * tyreSlip;

                //Applying force
                rb.AddForceAtPosition(supspensionForce + accelerationForce + xGripForce + brakingForce, hit.point);
            }
            else targetPos = wheels[i].transform.position + Vector3.up *(-springHeight + wheelRadius);
            wheels[i].mesh.position = Vector3.MoveTowards(wheels[i].mesh.position, targetPos, Time.fixedDeltaTime * 3);
        }
    }
}
