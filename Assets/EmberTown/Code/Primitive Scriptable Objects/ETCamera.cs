using UnityEngine;

public class ETCamera : MonoBehaviour
{

    private string label;

    public string Label { get => label; set => label = value; }

    private void Awake()
    {

        label = gameObject.name;
    }


}
