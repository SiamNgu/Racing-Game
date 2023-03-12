using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (BoxCollider) )]
[RequireComponent(typeof (Rigidbody) )]
public class CarScript : MonoBehaviour
{
    #region Inspector Fields
    [Header("Setup Variables")]
    [SerializeField] private float springHeight;
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private CarDatasScriptableObject carsData;
    [Header("Settings")]
    public string steeringDevice;
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
    private Transform[] wheelReferences;
    private Rigidbody rb;
    #endregion

    public float kmph { get; private set; } = 0;
    private float engineForceOutput;
    public float rpm { get; private set; } = 0;
    private void Reset()
    {
        #region Rigidbody Default Variables
        rb = GetComponent<Rigidbody>();
        rb.mass = 1000;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        #endregion
    }

    public void SpawnCar()
    {
        #region Initializing Data
        wheelReferences = new Transform[4];
        CarData carToSpawn = DataBetweenScenes.carSelected;
        string[] namingWheel = { "FR", "FL", "RR", "RL" };
        Vector3[] positionWheel = {
            carToSpawn.FRWheelPos,
            new Vector3(carToSpawn.RLWheelPos.x, carToSpawn.FRWheelPos.y, carToSpawn.FRWheelPos.z),
            new Vector3(carToSpawn.FRWheelPos.x, carToSpawn.RLWheelPos.y, carToSpawn.RLWheelPos.z),
            carToSpawn.RLWheelPos
        };
        #endregion

        #region Instantiating Wheels
        for (int i = 0; i < 4; i++)
        {
            //Instantiating wheel transforms
            Transform instantiatedWheelTransform = new GameObject(namingWheel[i]).transform;
            instantiatedWheelTransform.transform.parent = transform;
            instantiatedWheelTransform.position = positionWheel[i];
            wheelReferences[i] = instantiatedWheelTransform;

            //Istantiating wheel meshes
            GameObject instantiatedMesh;
            if (namingWheel[i][1] == 'R')
            {
                instantiatedMesh = Instantiate(carToSpawn.leftFacingWheel, instantiatedWheelTransform);
                instantiatedMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }
            else
            {
                instantiatedMesh = Instantiate(carToSpawn.leftFacingWheel, instantiatedWheelTransform);
            }
            instantiatedMesh.transform.localPosition = Vector3.zero;
            instantiatedMesh.name = namingWheel + " Mesh";
        }
        #endregion
        //Instantiating car
        Instantiate(carToSpawn.car, transform).name = carToSpawn.name;
        GetComponent<BoxCollider>().size = carToSpawn.colliderSize;
    }

    private void Awake()
    {
        SpawnCar();

        #region Setting Static Variables
        rb = GetComponent<Rigidbody>();
        wheelRadius = wheelReferences[0].GetChild(0).GetComponent<MeshRenderer>().bounds.extents.y;
        #endregion

        #region Default Parameters
        //audio source
        engineAudioSource.loop = true;
        engineAudioSource.playOnAwake = false;
        engineAudioSource.pitch = 0;
        engineAudioSource.clip = DataBetweenScenes.carSelected.engineSound;
        //rigidbody
        float betweenWheels = (wheelReferences[0].transform.localPosition.z + wheelReferences[2].transform.localPosition.z) * 0.5f;
        rb.centerOfMass = new Vector3(0, .17f, betweenWheels);
        #endregion

        steeringDevice = SettingDataClasses.steeringDevices[PlayerPrefs.GetInt("Steering Device")];
    }

    private void Start()
    {
        engineAudioSource.Play();
    }

    private void Update()
    {
        kmph = MpsToKph( transform.InverseTransformDirection(rb.velocity).z );

        rpm = Mathf.Lerp(rpm, Mathf.Abs(Input.GetAxisRaw("Vertical")), Time.deltaTime * 1 * revSpeed);
        float verticalInputAxis = Input.GetKey(KeyCode.Space) ? 0 : Input.GetAxisRaw("Vertical");
        float distToTopSpeed = Mathf.InverseLerp(0, DataBetweenScenes.carSelected.topSpeed, DataBetweenScenes.carSelected.topSpeed - kmph);
        float potentialEngineForce = distToTopSpeed * DataBetweenScenes.carSelected.acceleration;
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
                    case "Mouse":
                        SetMouseWheelSteering(wheelReferences[i].transform);
                        break;
                    case "Keyboard":
                        SetKeyboardWheelSteering(wheelReferences[i].transform);
                        break;
                }

                //Clamping value
                bool overSteer = Mathf.Abs(PositiveAngleToAngle(wheelReferences[i].transform.localEulerAngles.y)) > maxSteeringAngle;
                if (overSteer) wheelReferences[i].transform.localRotation = Quaternion.Euler(0, maxSteeringAngle * Mathf.Sign(PositiveAngleToAngle(wheelReferences[i].transform.localEulerAngles.y)), 0);
            }
            #endregion

            #region Wheel Rolling
            Vector3 wheelVelocity = wheelReferences[i].transform.InverseTransformDirection(rb.GetPointVelocity(wheelReferences[i].transform.position));
            float distance = wheelVelocity.z;
            float rollAngle = distance * (180f / Mathf.PI) / wheelRadius * Time.deltaTime;
            wheelReferences[i].GetChild(0).localRotation = Quaternion.Euler(rollAngle, 0, 0) * wheelReferences[i].GetChild(0).localRotation;
            #endregion
        }
        #endregion Wheel Rotations
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < wheelReferences.Length; i++)
        {
            #region Defining variables
            RaycastHit hit;
            Vector3 targetPos;
            bool isHit = Physics.Raycast(wheelReferences[i].transform.position, -transform.up, out hit, springHeight);
            Vector3 wheelVel = wheelReferences[i].transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            #endregion

            //Applying all car forces
            if (isHit)
            {
                targetPos = hit.point + wheelReferences[i].transform.up * wheelRadius;

                #region Calculate Forces
                //Suspension
                float offset = Mathf.InverseLerp(springHeight, 0, wheelReferences[i].transform.position.y - hit.point.y);
                float calculatedSpringForce = (springForce * offset) - (wheelVel.y * springDamping);
                Vector3 supspensionForce = wheelReferences[i].transform.up * calculatedSpringForce;

                //Acceleration
                Vector3 accelerationForce = engineForceOutput * wheelReferences[i].transform.forward;

                //Deceleration
                Vector3 decelerationForce = Input.GetAxisRaw("Vertical") == 0 ? -wheelReferences[i].transform.forward * wheelVel.z * decelerationAmount : Vector3.zero;

                //Braking
                Vector3 brakingForce = Input.GetKey(KeyCode.Space) ? -wheelReferences[i].transform.forward * wheelVel.z * DataBetweenScenes.carSelected.brakeForce : Vector3.zero;

                //Grip
                Vector3 xGripForce = -wheelReferences[i].transform.right * wheelVel.x * tyreGrip;
                #endregion

                //Applying force
                rb.AddForceAtPosition(supspensionForce + accelerationForce + xGripForce + brakingForce + decelerationForce, hit.point);
            }

            //Calculating wheel position if not grounded
            else targetPos = wheelReferences[i].transform.position + Vector3.up *(-springHeight + wheelRadius);

            //Setting wheel position
            wheelReferences[i].GetChild(0).position = Vector3.MoveTowards(wheelReferences[i].GetChild(0).position, targetPos, Time.fixedDeltaTime * 3);
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
        float steerAmount = (Input.GetAxisRaw("Horizontal") == 0 && calculatedAngle != 0) ? -Mathf.Sign(calculatedAngle) * Time.deltaTime * turnSensitivity * 2 : Input.GetAxisRaw("Horizontal") * Time.deltaTime * turnSensitivity;
        wheelTransform.transform.Rotate(Vector3.up, steerAmount, Space.Self);
    }

    private float MpsToKph(float mps)
    {
        return mps * 3.6f;
    }
}