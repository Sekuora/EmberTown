using Unity.VisualScripting;
using UnityEngine;


public class FreelookCamera : MonoBehaviour
{
    [Header("Free Look Camera Settings")]
    [SerializeField]
    private Quaternion initialRotation;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private Vector3 targetOffset;
    [SerializeField]
    private float distance = 5f;


    [SerializeField]
    private float sensitivity = 0.2f;

    [SerializeField]
    private Vector2 clampMinMax = new Vector2(-35f, 85f);



    private float yaw;
    private float pitch;

    public float Sensitivity { get => sensitivity; set => sensitivity = value; }
    public Vector2 ClampMinMax { get => clampMinMax; set => clampMinMax = value; }
    public float Yaw { get => yaw; set => yaw = value; }
    public float Pitch { get => pitch; set => pitch = value; }
    public float Distance { get => distance; set => distance = value; }
    public Vector3 TargetOffset { get => targetOffset; set => targetOffset = value; }
    public Transform Target { get => target; set => target = value; }


    private void OnEnable()
    {


    }

    private void Start()
    {
        InitializeCameraSettings();
    }


    private void InitializeCameraSettings()
    {
        Vector3 rotationEuler = initialRotation.eulerAngles;

        float initialPitch = rotationEuler.x;
        if (initialPitch > 180f)
        {
            initialPitch -= 360f;
        }

        pitch = initialPitch;
        yaw = rotationEuler.y;
    }


    public void CameraUpdate(Vector2 lookInput)
    {
        if (Target == null) return;

        Yaw += lookInput.x * Sensitivity;
        Pitch += -lookInput.y * Sensitivity;


        Pitch = Mathf.Clamp(Pitch, ClampMinMax.x, ClampMinMax.y);


        transform.eulerAngles = new Vector3(Pitch, Yaw, 0f);

        Vector3 focusPoint = Target.position + TargetOffset;

        transform.position = focusPoint - transform.forward * Distance;

    }
}
