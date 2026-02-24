using UnityEngine;

public class ButterflyMovement : MonoBehaviour
{
    private float speed = 0.2f;

    [SerializeField] private float averageSpeed = 0.2f;

    [SerializeField] private float arriveThreshold = 0.05f;

    [SerializeField] private bool rotateToFaceTarget = true;

    private Vector3 targetPosition = Vector3.zero;
    private FeaturePointsManager featurePointsManager;

    private bool _isResting = false;
    private float _restingTime = 0f;
    private float _restingElapsed = 0f;
    private float _restingProbability = 0.25f;

    private float _timeAlive = 0f;
    private float _maxTimeAlive = 150f;
    private float _lifeTime = 20f;


    [SerializeField]
    private Animator _animator;

    private void Start()
    {
        // Busca en la escena una instancia de FeaturePointsManager
        featurePointsManager = Object.FindFirstObjectByType<FeaturePointsManager>();

        // Selecciona el primer destino
        InitializeFly();

        _lifeTime = Random.Range(20f,_maxTimeAlive);
    }

    private void Update()
    {

        _timeAlive += Time.deltaTime;
        TryKill();

        _animator.SetBool("Flying", !_isResting);
        if (_isResting)
        {
            Rest();
        }
        else
        {
            Fly();
            
        }
        
    }

    private void TryKill()
    {
        if (_timeAlive > _lifeTime || (targetPosition != Vector3.zero && Vector3.Distance(targetPosition, transform.position) > 5f))
        {

            Camera mainCamera = Camera.main;

            float margin = 600f;

            Vector3 screenPosition = mainCamera.WorldToViewportPoint(transform.position);

            bool isVisible = screenPosition.z > 0;

            if (!isVisible)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Rest()
    {
        _restingElapsed += Time.deltaTime;
        if (_restingElapsed > _restingTime)
        {
            InitializeFly();
        }
    }

    private void Fly()
    {
        // si el destí és vector.zero es que no ha pogut escollir per tant torna a intentar
        if (targetPosition == Vector3.zero)
        {
            PickNewTarget();
        }
        else
        {
            if (rotateToFaceTarget)
            {
                
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        lookRotation,
                        Time.deltaTime * 1.5f 
                    );
                }
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance < arriveThreshold)
            { // si arriba al destí
                float randomValue = Random.value;
                if (randomValue < _restingProbability) InitializeRest();
                else PickNewTarget();
            }
        }
    }

    private void InitializeFly()
    {
        _isResting = false;
        PickNewTarget();
        speed = Random.Range(averageSpeed - (averageSpeed * 2 / 3), averageSpeed + (averageSpeed * 2 / 3));
    }

    private void InitializeRest()
    {
        _restingTime = Random.Range(1f, 30f);
        _restingElapsed = 0f;
        _isResting = true;
    }

    private void PickNewTarget()
    {
        if (featurePointsManager != null)
        {
            Vector3 newPos = featurePointsManager.GetRandomFeaturePointDistantEnought(transform.position, 0.1f);
            if (newPos == Vector3.zero)
            {
                // No hay puntos válidos; la mariposa se queda quieta
                targetPosition = Vector3.zero;
            }
            else
            {
                if (Vector3.Distance(newPos, transform.position) > 0.1f) targetPosition = newPos;
            }
        }
    }
}