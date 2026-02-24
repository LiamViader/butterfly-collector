using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class ButterflySpawner : MonoBehaviour
{
    [Header("Butterfly Settings")]
    public GameObject butterflyPrefab; // Prefab de les papallones
    public int maxButterflies = 10; 

    [Header("AR Settings")]
    public FeaturePointsManager featurePointsManager; // referencia al script que controla els feature points
    private List<GameObject> activeButterflies = new List<GameObject>(); //llista de papallones actives

    private float spawnProbability = 0.8f; //  probabilitatSpawn/segon .  Després es pondera tenint en compte que com més papallones hi hagi en escena, més dificil és que n'apareguin noves

    public ARCameraCapture cameraCapture;
    void Start()
    {
        
    }

    void Update()
    {
        if (ShouldSpawn()) SpawnButterfly();


        activeButterflies.RemoveAll(b => b == null);
    }

    public List<GameObject> GetActiveButterflies()
    {
        return activeButterflies;
    }

    private bool ShouldSpawn()
    {
        if (activeButterflies.Count>=maxButterflies) return false;

        float randomValue = Random.value;
        float normalizedSpawnProbability = Time.deltaTime * GetWeightedSpawnProbability();
        return randomValue < normalizedSpawnProbability;
    }

    private float GetWeightedSpawnProbability()
    {
        if (activeButterflies.Count == 0) return spawnProbability;
        return spawnProbability / (1.5f * activeButterflies.Count);
    }

    void SpawnButterfly()
    {
        Vector3 randomPoint = featurePointsManager.GetRandomFeaturePoint();
        if (randomPoint != Vector3.zero)
        {

            Vector3 spawnPosition = randomPoint;

            GameObject butterfly = Instantiate(butterflyPrefab, spawnPosition, Quaternion.identity);

            float randomSize = Random.Range(0.5f, 1f);
            butterfly.transform.localScale = new Vector3(randomSize, randomSize, randomSize);
            activeButterflies.Add(butterfly);

            Texture2D materialTexture = cameraCapture.GetRandomPatch();
            if ( materialTexture != null) butterfly.GetComponent<ButterflyMaterialManager>()?.SetMaterialFromTexture(materialTexture);
        }

    }
}
