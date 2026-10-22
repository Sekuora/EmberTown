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

        foreach (Socket socket in sockets)
        {
            if (socket == null) continue;

            if (socket.label == "HeadSocket_01" || socket.label.Contains("Head"))
            {
                AttachHairToHeadSocket(socket);
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
        // Front hair (bangs/fringe)
        if (data.HairStylesFront != null && data.HairStylesFront.Length > 0)
        {
            int frontIndex = Mathf.Clamp(selectedHairFrontIndex, 0, data.HairStylesFront.Length - 1);
            GameObject hairFront = data.HairStylesFront[frontIndex];
            if (hairFront != null)
            {
                Instantiate(hairFront, socket.transform);
            }
        }

        // Back hair (long hair / ponytail)
        if (data.HairStylesBack != null && data.HairStylesBack.Length > 0)
        {
            int backIndex = Mathf.Clamp(selectedHairBackIndex, 0, data.HairStylesBack.Length - 1);
            GameObject hairBack = data.HairStylesBack[backIndex];
            if (hairBack != null)
            {
                Instantiate(hairBack, socket.transform);
            }
        }
    }

    private void AttachMeshToRootSocket(Socket socket)
    {
        // Extension point for root-bound wearable sockets or accessories
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
