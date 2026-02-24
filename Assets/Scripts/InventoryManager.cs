using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;


public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private Transform _inventoryScrollViewContent;

    [SerializeField]
    private Transform _inventoryRoot;


    [SerializeField]
    private GameObject _butterflyItemPrefab;

    [SerializeField]
    private ButterflyCatcher _butterflyCatcher;

    [SerializeField]
    private Material _baseWingsMaterial;

    private List<ButterflyData> _collectedButterflies = new List<ButterflyData>();

    

    private void Awake()
    {
        LoadAllTextures();
    }

    private void LoadAllTextures()
    {
        string path = Application.persistentDataPath + "/textures/";

        if (System.IO.Directory.Exists(path))
        {
            string[] textureFiles = System.IO.Directory.GetFiles(path, "*.png");

            foreach (string filePath in textureFiles)
            {
                Texture2D texture = LoadTexture(filePath);

                if (texture != null)
                {
                    Material newMat = new Material(_baseWingsMaterial);
                    newMat.mainTexture = texture;
                    AddButterfly(new ButterflyData(1f, newMat));
                }

            }
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            byte[] textureBytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(textureBytes);

            return texture;
        }


        return null;
    }

    public void AddNewButterfly(ButterflyData butterfly)
    {
        AddButterfly(butterfly );
        string fileName = SaveTextureFromMaterial(butterfly.Mat);
    }

    private void AddButterfly(ButterflyData butterfly)
    {
        _collectedButterflies.Add(butterfly);
        GameObject butterflyObject = GameObject.Instantiate(_butterflyItemPrefab, _inventoryScrollViewContent);
        ButterflyMaterialManager butterflyMaterialManager = butterflyObject.GetComponent<ButterflyMaterialManager>();
        butterflyMaterialManager.SetMaterial(butterfly.Mat);
    }

    private string SaveTextureFromMaterial(Material material)
    {
        Texture2D texture = GetTextureFromMaterial(material);

        if (texture != null)
        {
            string fileName = System.Guid.NewGuid().ToString() + ".png";

            SaveTexture(texture, fileName);

            return fileName;
        }

        return null;
    }

    private Texture2D GetTextureFromMaterial(Material material)
    {
        if (material != null && material.mainTexture is Texture2D texture)
        {
            return texture;
        }

        return null;
    }

    private void SaveTexture(Texture2D texture, string fileName)
    {
        string path = Application.persistentDataPath + "/textures/";

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        path = System.IO.Path.Combine(path, fileName);

        byte[] textureBytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, textureBytes);
    }

    public void OpenInventory()
    {
        _inventoryRoot.gameObject.SetActive(true);
        _butterflyCatcher.DisableActions();
    }

    public void CloseInventory()
    {
        _inventoryRoot.gameObject.SetActive(false);
        _butterflyCatcher.EnableActions();

    }

}
