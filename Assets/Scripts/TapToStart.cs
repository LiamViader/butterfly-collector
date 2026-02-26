using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TapToStart : MonoBehaviour
{
    void Update()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            ChangeScene();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ChangeScene();
        }
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}