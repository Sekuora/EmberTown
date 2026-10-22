using System.Collections.Generic;
using UnityEngine;

public class CinematicSubsystem : MonoBehaviour
{
    private static CinematicSubsystem instance;

    [SerializeField]
    private List<ETCamera> cameras;


    private void Awake()
    {
        cameras = new List<ETCamera>(GetComponentsInChildren<ETCamera>());

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

    }

    public ETCamera FindCamera(string inLabel)
    {
        foreach (ETCamera camera in cameras)
        {
            if (camera.Label == inLabel)
            {
                return camera;
            }
        }

        return default;
    }

    public static CinematicSubsystem Call()
    {
        return instance;
    }



    public List<ETCamera> Cameras { get => cameras; set => cameras = value; }

}
