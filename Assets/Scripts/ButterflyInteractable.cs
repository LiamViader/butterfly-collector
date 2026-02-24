using UnityEngine;

public struct ButterflyData
{
    public float Size { get; }
    public Material Mat { get; }

    public ButterflyData(float size, Material material)
    {
        Size = size;
        Mat = material;
    }
}


public class ButterflyInteractable : MonoBehaviour
{
    [SerializeField] private GameObject butterflyRoot;

    [SerializeField] private ButterflyMaterialManager _materialManager;



    public ButterflyData Catch()
    {
        GameObject.Destroy(butterflyRoot);
        return new ButterflyData(butterflyRoot.transform.localScale.x, _materialManager.GetCurrentMaterial());
    }
}
