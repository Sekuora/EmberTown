using UnityEngine; 

public class Socket : MonoBehaviour
{
    public string label;

    private void OnEnable()
    {
        label = gameObject.name;
    }
    

}

