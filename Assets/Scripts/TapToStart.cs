using UnityEngine;
using UnityEngine.SceneManagement;

public class TapToStart : MonoBehaviour
{
    void Update()
    {

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ChangeScene();
        }

    }

    void ChangeScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}