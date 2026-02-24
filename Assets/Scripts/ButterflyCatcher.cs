using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ButterflyCatcher : MonoBehaviour
{

    [SerializeField] private InputAction drawAction;

    [SerializeField] private InputAction pressAction;

    private bool isDrawing = false;

    [SerializeField] private RectTransform _catcherTransform;

    [SerializeField] private RectTransform _parentTransform;

    private Vector3 _startCatcherPosition;
    private Vector3 _endCatcherPosition;

    [SerializeField] private float catcherDistance;

    [SerializeField] private InventoryManager _inventory;

    [SerializeField] private ButterflySpawner _butterflySpawner;

    private void Awake()
    {
        //set lineRenderer at topleft of the screen
        int screenWidth = Screen.width; 
        int screenHeight = Screen.height;

        _parentTransform.localPosition = new Vector3(-screenWidth/2, -screenHeight/2, 0);
    }

    private void Update()
    {

    }

    void OnEnable()
    {
        drawAction.Enable();
        pressAction.Enable();
        drawAction.performed += OnDrawPerformed;
        pressAction.canceled += OnPressEnded;
    }

    void OnDisable()
    {
        drawAction.Disable();
        pressAction.Disable();
        drawAction.performed -= OnDrawPerformed;
        pressAction.canceled -= OnPressEnded;
    }

    public void EnableActions()
    {
        drawAction.Enable();
        pressAction.Enable();
    }

    public void DisableActions()
    {
        drawAction.Disable();
        pressAction.Disable();
    }


    private void OnDrawPerformed(InputAction.CallbackContext context)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Vector2 inputPosition = context.ReadValue<Vector2>();
        if (!isDrawing)
        {
            _startCatcherPosition = new Vector3(inputPosition.x, inputPosition.y, 0);
            isDrawing = true;
        }
        else
        {
            _endCatcherPosition = new Vector3(inputPosition.x, inputPosition.y, 0);
            Vector3 vStartEnd = _endCatcherPosition - _startCatcherPosition;
            Vector3 calculatedPosition = _startCatcherPosition + (vStartEnd / 2);
            float size = Mathf.Max(Mathf.Abs(vStartEnd.x), Mathf.Abs(vStartEnd.y));
            _catcherTransform.sizeDelta = new Vector2(size,size);
            _catcherTransform.localPosition = calculatedPosition;
        }
        
    }
    private void OnPressEnded(InputAction.CallbackContext context)
    {
        if (context.duration > 0.1f)
        {
            Vector3 vStartEnd = _endCatcherPosition - _startCatcherPosition;
            Vector3 calculatedPosition = _startCatcherPosition + (vStartEnd / 2);
            float size = Mathf.Max(Mathf.Abs(vStartEnd.x), Mathf.Abs(vStartEnd.y));
            TryCatchButterfly(calculatedPosition, size);
        }
        _catcherTransform.sizeDelta = new Vector2(Mathf.Abs(0), Mathf.Abs(0));
        isDrawing = false;
    }


    private void TryCatchButterfly(Vector3 catcherLocalPosition, float catcherRadius)
    {
        if (catcherRadius <= 0)
        {
            Debug.LogWarning("El círculo dibujado no tiene un radio válido.");
            return;
        }

        List<GameObject> butterflies = _butterflySpawner.GetActiveButterflies();
        Vector3 catcherWorldPosition = Camera.main.ScreenToWorldPoint(catcherLocalPosition);
        ButterflyInteractable bestCandidate = null;
        foreach (var butterfly in butterflies)
        {
            Vector3 butterflyWorldPos = butterfly.transform.position;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(butterflyWorldPos);

            // verificar si esta davant de la camera
            if (screenPos.z < 0.1f)
                continue; // està darrere

            Vector2 butterflyScreenPos = new Vector2(screenPos.x, screenPos.y);



            if (IsPointInCircle(butterflyScreenPos, catcherLocalPosition, catcherRadius))
            {
                if (Vector3.Distance(catcherWorldPosition, butterflyWorldPos) < catcherDistance)
                {
                    ButterflyInteractable candidate = butterfly.GetComponentInChildren<ButterflyInteractable>();
                    if (bestCandidate == null) bestCandidate = candidate;
                    else
                    {
                        if (Vector3.Distance(catcherWorldPosition, butterflyWorldPos)< Vector3.Distance(catcherWorldPosition, bestCandidate.transform.position)) bestCandidate = candidate;

                    }
                    
                }
            }

        }
        if (bestCandidate != null) CatchButterfly(bestCandidate);

    }
    private bool IsPointInCircle(Vector2 point, Vector2 circleCenter, float circleRadius)
    {
        float distance = Vector2.Distance(point, circleCenter);
        return distance <= circleRadius*0.85;
    }

    private void CatchButterfly(ButterflyInteractable butterfly)
    {
        ButterflyData catchedButterfly = butterfly.Catch();
        _inventory.AddNewButterfly(catchedButterfly);
    }

}
