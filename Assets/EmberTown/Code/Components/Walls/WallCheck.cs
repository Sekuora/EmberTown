using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(EntityDataBridge))]
public class WallCheck : MonoBehaviour
{
    private CharacterController controller;

    private EntityDataBridge data;

    [SerializeField]
    private LayerMask heavyWallLayerMask;

    [SerializeField]
    private LayerMask slideWallLayerMask;

    [Header("Detection Distances")]
    [SerializeField]
    private float heavyWallDistance = 1.5f;

    [SerializeField]
    private float slideWallDistance = 0.35f;

    public float HeavyWallDistance => heavyWallDistance;
    public float SlideWallDistance => slideWallDistance;

    Vector3 origin;

    Vector3 direction;

    // Flags
    bool hitWall;

    // Debug
    private float debugDistance;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        data = GetComponent<EntityDataBridge>();
    }

    private void Start()
    {
        if (heavyWallLayerMask == 0)
        {
            heavyWallLayerMask = LayerMask.GetMask("HeavyWall");
        }

        if (slideWallLayerMask == 0)
        {
            slideWallLayerMask = LayerMask.GetMask("SlideWall");
        }
    }

    public bool CheckHeavyWall(Vector3 moveDirection, out RaycastHit hitInfo, float maxDistance = -1f)
    {
        float dist = maxDistance > 0f ? maxDistance : (heavyWallDistance > 0f ? heavyWallDistance : data.WallCollisionDistance);
        return CastWall(moveDirection, heavyWallLayerMask, dist, out hitInfo);
    }

    public bool CheckSlideWall(Vector3 moveDirection, out RaycastHit hitInfo, float maxDistance = -1f)
    {
        float dist = maxDistance > 0f ? maxDistance : slideWallDistance;
        return CastWall(moveDirection, slideWallLayerMask, dist, out hitInfo);
    }

    private bool CastWall(Vector3 castDirection, LayerMask layerMask, float castDistance, out RaycastHit hitInfo)
    {
        direction = castDirection;

        if (!controller || !data)
        {
            controller = GetComponent<CharacterController>();
            data = GetComponent<EntityDataBridge>();
        }

        origin = transform.TransformPoint(controller.center);

        Vector3 dir = castDirection.sqrMagnitude > 0.001f ? castDirection.normalized : transform.forward;

        hitWall = Physics.SphereCast(origin, controller.radius, dir, out hitInfo, castDistance, layerMask);

        if (hitWall)
        {
            debugDistance = hitInfo.distance;
        }

        return hitWall;
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        // Set color based on whether we are detecting a wall
        Gizmos.color = hitWall ? Color.red : Color.green;

        float sphereRadius = controller.radius;

        // 1. Draw the starting sphere (The Character)
        Gizmos.DrawWireSphere(origin, sphereRadius);

        if (direction.sqrMagnitude > 0.01f)
        {
            // 2. Calculate where the sphere stopped
            Vector3 endCenter = origin + (direction.normalized * debugDistance);

            // 3. Draw the ending sphere
            Gizmos.DrawWireSphere(endCenter, sphereRadius);

            // 4. Draw lines connecting them to show the "tunnel"
            Gizmos.DrawLine(origin + Vector3.up * sphereRadius, endCenter + Vector3.up * sphereRadius);
            Gizmos.DrawLine(origin - Vector3.up * sphereRadius, endCenter - Vector3.up * sphereRadius);
            Gizmos.DrawLine(origin + Vector3.right * sphereRadius, endCenter + Vector3.right * sphereRadius);
            Gizmos.DrawLine(origin - Vector3.right * sphereRadius, endCenter - Vector3.right * sphereRadius);
        }
    }
}