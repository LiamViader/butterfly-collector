using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

public class ARCameraCapture : MonoBehaviour
{
    [SerializeField] private ARCameraManager cameraManager;



    [SerializeField] private int patchSize = 128;     // dimensions del patch
    [SerializeField] private bool randomPatch = true; // aleatori?

    private Texture2D previousText = null;


    public Texture2D GetRandomPatch()
    {
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
        {
            //convertir imatge en textura
            Texture2D fullTex = ConvertCpuImageToTexture2D(cpuImage);

            previousText = fullTex;

            // agafar un parche de la textura completa
            Texture2D patchTex = ExtractPatch(fullTex, patchSize, randomPatch);

            return patchTex;

        }
        else
        {
            Texture2D patchTex = ExtractPatch(previousText, patchSize, randomPatch);

            return patchTex;
        }
    }



    private Texture2D ConvertCpuImageToTexture2D(XRCpuImage cpuImage)
    {

        var conversionParams = new XRCpuImage.ConversionParams
        {

            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),


            outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),


            outputFormat = TextureFormat.RGB24,


            transformation = XRCpuImage.Transformation.None
        };


        int size = cpuImage.GetConvertedDataSize(conversionParams);


        var buffer = new NativeArray<byte>(size, Allocator.Temp);


        cpuImage.Convert(conversionParams, buffer);


        cpuImage.Dispose();

        Texture2D texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false
        );


        texture.LoadRawTextureData(buffer);
        texture.Apply();


        buffer.Dispose();

        return texture;
    }

    private Texture2D ExtractPatch(Texture2D fullTex, int patchSize, bool randomPatch)
    {

        if (fullTex == null) return null;

        int w = fullTex.width;
        int h = fullTex.height;
        patchSize = Mathf.Min(patchSize, w);
        patchSize = Mathf.Min(patchSize, h);

        int startX, startY;

        if (randomPatch)
        {
            startX = Random.Range(0, w - patchSize);
            startY = Random.Range(0, h - patchSize);
        }
        else
        {
            startX = 0;
            startY = 0;
        }


        Color[] patchPixels = fullTex.GetPixels(startX, startY, patchSize, patchSize);

        Texture2D patchTex = new Texture2D(patchSize, patchSize, TextureFormat.RGB24, false);
        patchTex.SetPixels(patchPixels);
        patchTex.Apply();

        return patchTex;
    }
}
