using UnityEngine;

/// <summary>
/// System to follow target bones respecting the root bone and bone hierarchy constraints.
/// </summary>
public class FollowTransform : MonoBehaviour
{

    /* Properties */

    [SerializeField] 
    private Transform targetBone;

    [SerializeField]
    private Transform rootBone;

    // Offset Value
    [SerializeField]
    private Vector3 targetPositionOffset;

    private Vector3 headPivotOffset;
    private Quaternion headRestRot;
    private Quaternion socketRestRot;
    private bool isInitialized;

    public Transform TargetBone => targetBone;


    private void Awake()
    {
        Initialize();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        if (targetBone == null) return;

        // Auto-detect rootBone if not assigned
        if (rootBone == null)
        {
            Transform current = targetBone.parent;
            while (current != null)
            {
                if (current.name == "def_root" || current.name.EndsWith("_root") || current.name.Contains("Root"))
                {
                    rootBone = current;
                    break;
                }
                current = current.parent;
            }
        }

        headRestRot = targetBone.rotation;
        socketRestRot = transform.rotation;

        if (rootBone != null)
        {
            Vector3 headInRoot = rootBone.InverseTransformPoint(targetBone.position);
            Vector3 socketInRoot = rootBone.InverseTransformPoint(transform.position);
            headPivotOffset = rootBone.TransformVector(headInRoot - socketInRoot);
        }
        else
        {
            headPivotOffset = targetBone.position - transform.position;
        }

        isInitialized = true;
    }


    private void LateUpdate()
    {
        if (!isInitialized || targetBone == null) return;

        Quaternion deltaRot = targetBone.rotation * Quaternion.Inverse(headRestRot);
        Vector3 socketWorldPosition = targetBone.position - (deltaRot * headPivotOffset);
        Quaternion socketWorldRotation = deltaRot * socketRestRot;

        transform.SetPositionAndRotation(socketWorldPosition + deltaRot * targetPositionOffset, socketWorldRotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
