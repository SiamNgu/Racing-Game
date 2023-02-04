using UnityEngine;

[RequireComponent(typeof (BoxCollider) )]
[RequireComponent(typeof (Rigidbody) )]
public class Car : MonoBehaviour
{
    #region Inspector Fields
    [Header("Setup Variables")]
    [SerializeField] private Wheel[] wheels;
    [SerializeField] private float springHeight;

    [Header("Car Data")]
    public CarDataScriptableObject carDataScriptableObject;

    [Header("Settings")]
    public SteeringDevice steeringDevice;
    #endregion

    #region Hardcoded Variables
    private const float turnSensitivity = 25;
    private const float maxSteeringAngle = 40;
    private const float tyreGrip = 1000;
    private const float springForce = 20000;
    private const float springDamping = 3200;
    #endregion

    #region Static Variables
    private float wheelRadius;
    private Rigidbody rb;
    private AudioSource engineAudioSource;
    #endregion

    public float kmph { get; private set; } = 0;
    private float engineForceOutput;

    #region Type Declarations
    [System.Serializable] [SerializeField] private struct Wheel
    {
        public Transform transform;
        public Transform mesh;
    }
    public enum SteeringDevice
    {
        Mouse, 
        Keyboard
    }
    #endregion

    private void Reset()
    {
        #region Rigidbody Default Variables
        rb = GetComponent<Rigidbody>();
        rb.mass = 1000;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        #endregion
    }

    private void Awake()
    {
        #region Setting Static Variables
        rb = GetComponent<Rigidbody>();
        wheelRadius = wheels[0].mesh.GetComponent<MeshRenderer>().bounds.extents.y;
        engineAudioSource = Instantiate(new GameObject("Engine Audio", typeof(AudioSource)), transform).GetComponent<AudioSource>();
        #endregion

        #region Default Parameters
        //audio source
        engineAudioSource.loop = true;
        engineAudioSource.playOnAwake = false;
        engineAudioSource.pitch = 0;
        engineAudioSource.clip = carDataScriptableObject.engineSound;

        //rigidbody
        rb.centerOfMass = new Vector3(0, 0, 0);
        #endregion
    }

    private void Start()
    {
        engineAudioSource.Play();
    }

    #region Debugging
    private void OnDrawGizmos()
    {
        rb = GetComponent<Rigidbody>();
        BoxCollider carCollider = GetComponent<BoxCollider>();
        rb.centerOfMass = new Vector3(0, 0, 0);
        Debug.Log(carCollider.size.y);
        Gizmos.DrawSphere(rb.worldCenterOfMass, 0.1f);
    }
    #endregion

    private void Update()
    {
        float verticalInputAxis = Input.GetKey(KeyCode.Space) ? 0 : Input.GetAxisRaw("Vertical");
        kmph = transform.InverseTransformDirection(rb.velocity).z * 3.6f;
        float potentialEngineForce = Mathf.InverseLerp(0, carDataScriptableObject.topSpeed, carDataScriptableObject.topSpeed - kmph) * carDataScriptableObject.acceleration;

        engineForceOutput = verticalInputAxis * potentialEngineForce;

        //audio
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 2.5f * Mathf.InverseLerp(0, carDataScriptableObject.acceleration, (carDataScriptableObject.acceleration - potentialEngineForce) * Mathf.Abs(verticalInputAxis)) + .5f, Time.deltaTime * 3);

        #region Wheel Rotations
        //Looping Through Wheels
        for (int i = 0; i < 4; i++)
        {
            #region Steering for front wheels
            if (i < 2)
            {
                //Calculating and applying steer
                switch (steeringDevice)
                {
                    case SteeringDevice.Mouse:
                        SetMouseWheelSteering(wheels[i].transform);
                        break;
                    case SteeringDevice.Keyboard:
                        SetKeyboardWheelSteering(wheels[i].transform);
                        break;
                }

                //Clamping value
                bool overSteer = Mathf.Abs(PositiveAngleToAngle(wheels[i].transform.localEulerAngles.y)) > maxSteeringAngle;
                if (overSteer) wheels[i].transform.localRotation = Quaternion.Euler(0, maxSteeringAngle * Mathf.Sign(PositiveAngleToAngle(wheels[i].transform.localEulerAngles.y)), 0);
            }
            #endregion

            #region Wheel Rolling
            Vector3 wheelVelocity = wheels[i].transform.InverseTransformDirection(rb.GetPointVelocity(wheels[i].transform.position));
            float distance = wheelVelocity.z;
            float rollAngle = distance * (180f / Mathf.PI) / wheelRadius * Time.deltaTime;
            wheels[i].mesh.localRotation = Quaternion.Euler(rollAngle, 0, 0) * wheels[i].mesh.localRotation;
            #endregion
        }
        #endregion Wheel Rotations
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            #region Defining variables
            RaycastHit hit;
            Vector3 targetPos;
            bool isHit = Physics.Raycast(wheels[i].transform.position, -transform.up, out hit, springHeight);
            Vector3 wheelVel = wheels[i].transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            #endregion

            //Applying all car forces
            if (isHit)
            {
                targetPos = hit.point + wheels[i].transform.up * wheelRadius;

                #region Calculate Forces
                //Suspension
                float offset = Mathf.InverseLerp(springHeight, 0, wheels[i].transform.position.y - hit.point.y);
                float calculatedSpringForce = (springForce * offset) - (wheelVel.y * springDamping);
                Vector3 supspensionForce = wheels[i].transform.up * calculatedSpringForce;

                //Acceleration
                Vector3 accelerationForce = engineForceOutput * wheels[i].transform.forward;

                //Braking
                Vector3 brakingForce = Input.GetKey(KeyCode.Space) ? -wheels[i].transform.forward * wheelVel.z * carDataScriptableObject.brakeForce : Vector3.zero;

                //Grip
                Vector3 xGripForce = -wheels[i].transform.right * wheelVel.x * tyreGrip;
                #endregion

                //Applying force
                rb.AddForceAtPosition(supspensionForce + accelerationForce + xGripForce + brakingForce, hit.point);
            }

            //Calculating wheel position if not grounded
            else targetPos = wheels[i].transform.position + Vector3.up *(-springHeight + wheelRadius);

            //Setting wheel position
            wheels[i].mesh.position = Vector3.MoveTowards(wheels[i].mesh.position, targetPos, Time.fixedDeltaTime * 3);
        }
    }

    private float PositiveAngleToAngle(float angle)
    {
        return angle = (angle > 180) ? angle - 360 : angle;
    }

    private void SetMouseWheelSteering(Transform wheelTransform)
    {
        if (Input.GetMouseButton(0))
        {
            wheelTransform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * Time.deltaTime * 110);
        }
        else if (!Input.GetMouseButton(0))
        {
            Debug.Log("Resetting");
            wheelTransform.localRotation = Quaternion.Lerp(wheelTransform.localRotation, Quaternion.identity, Time.deltaTime * 10);
        }
    }

    private void SetKeyboardWheelSteering(Transform wheelTransform)
    {
        float calculatedAngle = PositiveAngleToAngle(wheelTransform.transform.localEulerAngles.y);
        float steerAmount = (Input.GetAxisRaw("Horizontal") == 0 && calculatedAngle != 0) ? -Mathf.Sign(calculatedAngle) * Time.deltaTime * turnSensitivity : Input.GetAxisRaw("Horizontal") * Time.deltaTime * turnSensitivity;
        wheelTransform.transform.Rotate(Vector3.up, steerAmount, Space.Self);
    }
}
