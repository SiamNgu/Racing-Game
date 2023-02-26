using System;
using UnityEngine;
using UnityEngine.UI;

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
    private const float tyreGrip = 900;
    private const float springForce = 10000;
    private const float springDamping = 1500;
    private const float steerSpeed  = 50;
    private const float steerCenteringSpeed = 10;
    private const float decelerationAmount = 30;
    private const float revSpeed = 1;
    #endregion

    #region Static Variables
    private float wheelRadius;
    private Rigidbody rb;
    private AudioSource engineAudioSource;
    #endregion

    public float kmph { get; private set; } = 0;
    private float engineForceOutput;
    public float rpm { get; private set; } = 0;

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
        float betweenWheels = (wheels[0].transform.localPosition.z + wheels[2].transform.localPosition.z) * 0.5f;
        rb.centerOfMass = new Vector3(0, .17f, betweenWheels);
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
        float betweenWheels = (wheels[0].transform.localPosition.z + wheels[2].transform.localPosition.z) * 0.5f;
        rb.centerOfMass = new Vector3(0, .1f, betweenWheels);
        Gizmos.DrawSphere(rb.worldCenterOfMass, 0.17f);
    }
    #endregion

    private void Update()
    {
        kmph = MpsToKph( transform.InverseTransformDirection(rb.velocity).z );

        rpm = Mathf.Lerp(rpm, Mathf.Abs(Input.GetAxisRaw("Vertical")), Time.deltaTime * 1 * revSpeed);
        float verticalInputAxis = Input.GetKey(KeyCode.Space) ? 0 : Input.GetAxisRaw("Vertical");
        float distToTopSpeed = Mathf.InverseLerp(0, carDataScriptableObject.topSpeed, carDataScriptableObject.topSpeed - kmph);
        float potentialEngineForce = distToTopSpeed * carDataScriptableObject.acceleration  ;
        engineForceOutput = verticalInputAxis * potentialEngineForce;

        //audio
        engineAudioSource.pitch = rpm;
        engineAudioSource.volume = Mathf.SmoothStep(engineAudioSource.volume, Mathf.Abs(Input.GetAxisRaw("Vertical")), Time.deltaTime * 5);

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

                //Deceleration
                Vector3 decelerationForce = Input.GetAxisRaw("Vertical") == 0 ? -wheels[i].transform.forward * wheelVel.z * decelerationAmount : Vector3.zero;

                //Braking
                Vector3 brakingForce = Input.GetKey(KeyCode.Space) ? -wheels[i].transform.forward * wheelVel.z * carDataScriptableObject.brakeForce : Vector3.zero;

                //Grip
                Vector3 xGripForce = -wheels[i].transform.right * wheelVel.x * tyreGrip;
                #endregion

                //Applying force
                rb.AddForceAtPosition(supspensionForce + accelerationForce + xGripForce + brakingForce + decelerationForce, hit.point);
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
            wheelTransform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * Time.deltaTime * steerSpeed);
        }
        else if (!Input.GetMouseButton(0))
        {
            wheelTransform.localRotation = Quaternion.Lerp(wheelTransform.localRotation, Quaternion.identity, Time.deltaTime * steerCenteringSpeed);
        }
    }

    private void SetKeyboardWheelSteering(Transform wheelTransform)
    {
        float calculatedAngle = PositiveAngleToAngle(wheelTransform.transform.localEulerAngles.y);
        float steerAmount = (Input.GetAxisRaw("Horizontal") == 0 && calculatedAngle != 0) ? -Mathf.Sign(calculatedAngle) * Time.deltaTime * turnSensitivity : Input.GetAxisRaw("Horizontal") * Time.deltaTime * turnSensitivity;
        wheelTransform.transform.Rotate(Vector3.up, steerAmount, Space.Self);
    }

    private float MpsToKph(float mps)
    {
        return mps * 3.6f;
    }
}
