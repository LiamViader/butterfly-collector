using UnityEngine;
using TMPro;
using System.Collections;

public class ScanTextController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scanText;

    [Header("Settings")]
    [SerializeField] private string baseMessage = "Move your phone slowly to scan the area";
    [SerializeField] private float animationSpeed = 0.5f;

    private Coroutine animationCoroutine;

    void OnEnable()
    {
        StartAnimation();
    }

    void OnDisable()
    {
        StopAnimation();
    }

    void StartAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateDots());
    }

    void StopAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
    }

    IEnumerator AnimateDots()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4; 

            string dots = new string('.', dotCount);
            scanText.text = baseMessage + dots;

            yield return new WaitForSeconds(animationSpeed);
        }
    }

}