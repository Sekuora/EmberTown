using UnityEngine;

public class MeshTargetSocket : MonoBehaviour
{
    // Properties
    [SerializeField]
    private Socket[] sockets;

    [SerializeField]
    private int selectedHairFrontIndex = 0;

    [SerializeField]
    private int selectedHairBackIndex = 0;

    [SerializeField]
    private GameObject rightHandMesh;

    private GameObject equippedRightHandMesh;

    private EntityDataBridge data;

    private SkinnedMeshRenderer sourceMesh;

    [Header("Head Mesh Fit")]
    [SerializeField, Min(0.01f)]
    private float minimumHeadScale = 0.5f;

    [SerializeField, Min(0.01f)]
    private float maximumHeadScale = 2f;

    [SerializeField, Range(0f, 2f)]
    private float headFitStrength = 1f;

    [SerializeField, Tooltip("Keep the hairstyle intact while following the head volume. Disable for local surface deformation.")]
    private bool preserveHeadMeshShape = true;

    [SerializeField]
    private Vector3 headMeshSize = Vector3.one;

    [SerializeField, Tooltip("Offset in meters along the head bone's axes.")]
    private Vector3 headMeshOffset;

    // Public Properties
    public Socket[] Sockets { get => sockets; private set => sockets = value; }

    public GameObject EquippedRightHandMesh { get => equippedRightHandMesh; private set => equippedRightHandMesh = value; }

    private void Awake()
    {
        data = GetComponent<EntityDataBridge>();

        if (sockets == null || sockets.Length == 0)
        {
            sockets = GetComponentsInChildren<Socket>();
        }

        AttachMeshes();
    }

    /// <summary>
    /// Spawns and attaches configured meshes to their corresponding sockets.
    /// </summary>
    public void AttachMeshes()
    {
        if (data == null || sockets == null) return;

        RefreshHeadMeshes();

        foreach (Socket socket in sockets)
        {
            if (socket == null) continue;

            string socketLabel = string.IsNullOrEmpty(socket.label) ? socket.name : socket.label;
            if (socketLabel.Contains("Head"))
            {
                continue;
            }
            else if (socket.label == "RightHandSocket_01")
            {
                AttachMeshToRightHandSocket(socket);
            }
            else if (socket.label == "RootSocket_01" || socket.label.Contains("Root"))
            {
                AttachMeshToRootSocket(socket);
            }
        }
    }

    private void AttachHairToHeadSocket(Socket socket)
    {
        // Replace only meshes managed by the surface attachment system.
        for (int i = socket.transform.childCount - 1; i >= 0; i--)
        {
            GameObject previous = socket.transform.GetChild(i).gameObject;
            if (!previous.GetComponent<SurfaceMeshAttachment>()) { continue; }
            previous.SetActive(false);
            if (Application.isPlaying) Destroy(previous);
            else DestroyImmediate(previous);
        }

        // Front hair (bangs/fringe)
        if (data.HairStylesFront != null && data.HairStylesFront.Length > 0)
        {
            int frontIndex = Mathf.Clamp(selectedHairFrontIndex, 0, data.HairStylesFront.Length - 1);
            GameObject hairFront = data.HairStylesFront[frontIndex];
            if (hairFront != null)
            {
                AttachHeadMesh(hairFront, socket);
            }
        }

        // Back hair (long hair / ponytail)
        if (data.HairStylesBack != null && data.HairStylesBack.Length > 0)
        {
            int backIndex = Mathf.Clamp(selectedHairBackIndex, 0, data.HairStylesBack.Length - 1);
            GameObject hairBack = data.HairStylesBack[backIndex];
            if (hairBack != null)
            {
                AttachHeadMesh(hairBack, socket);
            }
        }
    }

    private void AttachMeshToRootSocket(Socket socket)
    {
        // Extension point for root-bound wearable sockets or accessories
    }

    private void AttachHeadMesh(GameObject prefab, Socket socket)
    {
        GameObject mesh = Instantiate(prefab, socket.transform, false);
        BlendShapeGroups groups = GetComponent<BlendShapeGroups>();
        if (groups) sourceMesh = groups.SourceMesh;
        if (!sourceMesh)
        {
            foreach (SkinnedMeshRenderer skin in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!skin.sharedMesh || skin.GetComponentInParent<Item>() || skin.transform.IsChildOf(socket.transform)) { continue; }
                if (!sourceMesh || skin.sharedMesh.vertexCount > sourceMesh.sharedMesh.vertexCount) sourceMesh = skin;
            }
        }
        FollowTransform follow = socket.GetComponent<FollowTransform>();
        if (!sourceMesh || !follow || !follow.TargetBone) { return; }
        SurfaceMeshAttachment attachment = mesh.GetComponent<SurfaceMeshAttachment>();
        if (!attachment) attachment = mesh.AddComponent<SurfaceMeshAttachment>();
        attachment.Initialize(sourceMesh, follow.TargetBone);
        attachment.SetFitSettings(minimumHeadScale, maximumHeadScale, headFitStrength, headMeshSize, headMeshOffset, preserveHeadMeshShape);
    }

    [ContextMenu("Refresh Head Meshes")]
    public void RefreshHeadMeshes()
    {
        if (!data) data = GetComponent<EntityDataBridge>();
        if (!data) { return; }
        foreach (Socket socket in GetComponentsInChildren<Socket>(true))
        {
            if (!socket || socket.GetComponentInParent<MeshTargetSocket>() != this) { continue; }
            string socketLabel = string.IsNullOrEmpty(socket.label) ? socket.name : socket.label;
            if (socketLabel.Contains("Head")) AttachHairToHeadSocket(socket);
        }
    }

    [ContextMenu("Apply Head Fit Settings")]
    public void ApplyHeadFitSettings()
    {
        minimumHeadScale = Mathf.Max(0.01f, minimumHeadScale);
        maximumHeadScale = Mathf.Max(minimumHeadScale, maximumHeadScale);
        foreach (SurfaceMeshAttachment attachment in GetComponentsInChildren<SurfaceMeshAttachment>(true))
        {
            if (attachment.GetComponentInParent<MeshTargetSocket>() != this) { continue; }
            attachment.SetFitSettings(minimumHeadScale, maximumHeadScale, headFitStrength, headMeshSize, headMeshOffset, preserveHeadMeshShape);
            attachment.FitSurface();
        }
    }

    private void AttachMeshToRightHandSocket(Socket socket)
    {
        if (rightHandMesh == null || equippedRightHandMesh != null) return;

        equippedRightHandMesh = Instantiate(rightHandMesh, socket.transform, false);
    }

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
