using UnityEngine;

public class ButterflyMaterialManager : MonoBehaviour
{
    [SerializeField]
    private Material _baseWingsMaterial;
    [SerializeField]
    private SkinnedMeshRenderer _butterflyMesh;

    private Material _currentMaterial = null;

    private void Awake()
    {
        _currentMaterial = _baseWingsMaterial;
    }

    public void SetMaterialFromTexture(Texture2D texture)
    {
        if (texture == null) return;
        Material newMat = new Material(_baseWingsMaterial);
        newMat.mainTexture = texture;

        Material[] mats = _butterflyMesh.materials;

        mats[0] = newMat;

        _butterflyMesh.materials = mats;

        _currentMaterial = newMat;
    }

    public Material GetCurrentMaterial()
    {
        return _currentMaterial;
    }

    public void SetMaterial(Material material)
    {
        _currentMaterial = material;

        Material[] mats = _butterflyMesh.materials;

        mats[0] = material;
        _butterflyMesh.materials = mats;
    }

}
