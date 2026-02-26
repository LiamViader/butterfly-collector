using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using System.Linq;
using System.Collections;

[System.Serializable]
public class FeaturePoint
{
    public Vector3 position;
    public float confidence;
}

[RequireComponent(typeof(ARPointCloudManager))]
public class FeaturePointsManager : MonoBehaviour
{
    [SerializeField] private GameObject debugSpherePrefab;

    [SerializeField] private GameObject _scanTextObject;

    [SerializeField] private int _pointsHighThreshold = 20;
    [SerializeField] private int _pointsLowThreshold = 5;
    private bool _uiHidden = false;

    // Lista temporal para mantener referencias a las esferas creadas
    private List<GameObject> _debugSpheres = new List<GameObject>();


    [SerializeField, Range(0f, 1f)]
    private float _minConfidence = 0.5f;

    [SerializeField]
    private float _maxDistanceFromObserver;

    private Dictionary<ulong, FeaturePoint> _featurePoints = new Dictionary<ulong, FeaturePoint>();

    private Coroutine _cleanerCoroutine;

    private ARPointCloudManager _pointCloudManager;

    [SerializeField]
    private bool _debugFeaturePoints = false;

    private void Awake()
    {
        _pointCloudManager = GetComponent<ARPointCloudManager>();
    }

    private void OnEnable()
    {
        _pointCloudManager.trackablesChanged.AddListener(OnTrackablesChanged);
        _cleanerCoroutine = StartCoroutine(RemoveDistantPointsCoroutine());
    }

    private void OnDisable()
    {
        _pointCloudManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        if (_cleanerCoroutine != null)
        {
            StopCoroutine(_cleanerCoroutine);
        }
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPointCloud> args)
    {
        if (_debugFeaturePoints)
        {

            foreach (var sphere in _debugSpheres)
            {
                Destroy(sphere);
            }
            _debugSpheres.Clear();
        }

        AddFeaturePoints(args.added);
        UpdateFeaturePoints(args.updated);
        var removedValues = args.removed.Select(kvp => kvp.Value).ToList();
        RemoveFeaturePoints(removedValues);


        if (_debugFeaturePoints)
        {
            foreach (var point in _featurePoints)
            {
                GameObject sphere = Instantiate(
                    debugSpherePrefab,
                    point.Value.position,
                    Quaternion.identity
                );
                _debugSpheres.Add(sphere);
            }
        }

        if (!_uiHidden && _featurePoints.Count >= _pointsHighThreshold)
        {
            _scanTextObject.SetActive(false);
            _uiHidden = true;
        }
        else if (_uiHidden && _featurePoints.Count < _pointsLowThreshold)
        {
            _scanTextObject.SetActive(true);
            _uiHidden = false;
        }
    }

    private void CheckRemoveFarFeaturePoints()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        List<ulong> keysToRemove = new List<ulong>();

        foreach (var point in _featurePoints)
        {
            float distanceToCamera = Vector3.Distance(point.Value.position, cameraPos);
            if (distanceToCamera > _maxDistanceFromObserver)
            {
                keysToRemove.Add(point.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _featurePoints.Remove(key);
        }
    }

    private void AddFeaturePoints(Unity.XR.CoreUtils.Collections.ReadOnlyList<ARPointCloud> pointClouds)
    {
        Vector3 cameraPos = Camera.main.transform.position;
        foreach (var pointCloud in pointClouds)
        {
            var positions = pointCloud.positions;
            var confidences = pointCloud.confidenceValues;
            var ids = pointCloud.identifiers;

            if (!positions.HasValue || !confidences.HasValue || !ids.HasValue)
                continue;

            var positionsSlice = positions.Value;
            var confidencesArray = confidences.Value;
            var idsSlice = ids.Value;

            Transform cloudTransform = pointCloud.transform;

            int count = Mathf.Min(Mathf.Min(positionsSlice.Length, confidencesArray.Length), idsSlice.Length);

            for (int i = 0; i < count; i++)
            {
                float confidence = confidencesArray[i];
                if (confidence < _minConfidence)
                    continue;

                Vector3 localPos = positionsSlice[i];
                Vector3 worldPos = cloudTransform.TransformPoint(localPos);

                float distanceToCamera = Vector3.Distance(worldPos, cameraPos);

                if (distanceToCamera > _maxDistanceFromObserver)
                    continue;

                var newPoint = new FeaturePoint
                {
                    position = worldPos,
                    confidence = confidence
                };

                _featurePoints.Add(idsSlice[i], newPoint);

            }
        }
    }

    private void UpdateFeaturePoints(Unity.XR.CoreUtils.Collections.ReadOnlyList<ARPointCloud> pointClouds)
    {
        Vector3 cameraPos = Camera.main.transform.position;
        foreach (var pointCloud in pointClouds)
        {
            var positions = pointCloud.positions;
            var confidences = pointCloud.confidenceValues;
            var ids = pointCloud.identifiers;

            if (!positions.HasValue || !confidences.HasValue || !ids.HasValue)
                continue;

            var positionsSlice = positions.Value;
            var confidencesArray = confidences.Value;
            var idsSlice = ids.Value;

            Transform cloudTransform = pointCloud.transform;

            int count = Mathf.Min(Mathf.Min(positionsSlice.Length, confidencesArray.Length), idsSlice.Length);

            for (int i = 0; i < count; i++)
            {
                ulong id = idsSlice[i];
                float confidence = confidencesArray[i];
                if (confidence < _minConfidence)
                {
                    if (_featurePoints.ContainsKey(id)) _featurePoints.Remove(id);
                }
                else
                {
                    Vector3 localPos = positionsSlice[i];
                    Vector3 worldPos = cloudTransform.TransformPoint(localPos);

                    float distanceToCamera = Vector3.Distance(worldPos, cameraPos);

                    if (distanceToCamera > _maxDistanceFromObserver)
                    {
                        if (_featurePoints.ContainsKey(id))
                        {
                            _featurePoints.Remove(id);
                        }
                        continue;
                    }
                    else
                    {
                        if (_featurePoints.ContainsKey(id))
                        {
                            _featurePoints[id].position = worldPos;
                            _featurePoints[id].confidence = confidence;
                        }
                        else
                        {
                            var newPoint = new FeaturePoint
                            {
                                position = worldPos,
                                confidence = confidence
                            };

                            _featurePoints.Add(idsSlice[i], newPoint);
                        }
                    }
                }                

            }
        }
    }

    private void RemoveFeaturePoints(List<ARPointCloud> pointClouds)
    {
        foreach (var pointCloud in pointClouds)
        {
            var positions = pointCloud.positions;
            var confidences = pointCloud.confidenceValues;
            var ids = pointCloud.identifiers;

            if (!positions.HasValue || !confidences.HasValue || !ids.HasValue)
                continue;

            var positionsSlice = positions.Value;
            var confidencesArray = confidences.Value;
            var idsSlice = ids.Value;

            Transform cloudTransform = pointCloud.transform;

            int count = Mathf.Min(Mathf.Min(positionsSlice.Length, confidencesArray.Length), idsSlice.Length);

            for (int i = 0; i < count; i++)
            {
                ulong id = idsSlice[i];
                float confidence = confidencesArray[i];
                
                if (_featurePoints.ContainsKey(id))
                {
                    _featurePoints.Remove(id);
                }       
            }
        }
    }


    public Vector3 GetRandomFeaturePointDistantEnought(Vector3 origin, float minDistance)
    {

        List<Vector3> validPoints = new List<Vector3>();

        foreach (var kvp in _featurePoints)
        {
            Vector3 pt = kvp.Value.position;
            if (Vector3.Distance(pt, origin) >= minDistance)
            {
                validPoints.Add(pt);
            }
        }

        if (validPoints.Count == 0)
            return Vector3.zero;

        int randomIndex = Random.Range(0, validPoints.Count);
        return validPoints[randomIndex];
    }

    public Vector3 GetRandomFeaturePoint()
    {
        if (_featurePoints.Count == 0)
            return Vector3.zero; 

        var keys = new List<ulong>(_featurePoints.Keys);

        ulong randomKey = keys[Random.Range(0, keys.Count)];

        return _featurePoints[randomKey].position;
    }


    private IEnumerator RemoveDistantPointsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            CheckRemoveFarFeaturePoints();
        }
    }
}