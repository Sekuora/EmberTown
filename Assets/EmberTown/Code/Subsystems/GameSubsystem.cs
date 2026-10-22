using UnityEngine;

public class GameSubsystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (DialogueSubsystem.Call().DSPlayer.State == DialoguePlayer.States.Active)
        {
            // Unlock Cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Lock Cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }
}
