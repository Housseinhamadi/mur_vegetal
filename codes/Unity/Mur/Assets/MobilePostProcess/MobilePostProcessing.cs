using UnityEngine;


public class MobilePostProcessing : MonoBehaviour
{
    public bool Blur = true;
    [Range(0, 5)]
    public float BlurAmount = 2f;
    public Texture2D BlurMask;

    public bool Bloom = true;
    [Range(0, 5)]
    public float BloomAmount = 3f;
    [Range(0, 1)]
    public float BloomThreshold = 0.07f;

    public bool LUT = true;
    [Range(0, 1)]
    public float LutAmount = 1.0f;
    public Texture2D SourceLut = null;

    static readonly int lutTextureString = Shader.PropertyToID("_LutTex");
    static readonly int lutAmountString = Shader.PropertyToID("_LutAmount");
    static readonly int maskTextureString = Shader.PropertyToID("_MaskTex");
    static readonly int blAmountString = Shader.PropertyToID("_BloomAmount");
    static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");
    static readonly int blThresholdString = Shader.PropertyToID("_BloomThreshold");
    static readonly int blurTexString = Shader.PropertyToID("_BlurTex");

    static readonly int scrWidth = Screen.width / 4;
    static readonly int scrHeight = Screen.height / 4;

    public Material material;

    private Texture2D prevSorceLut;
    private Texture2D converted2DLut = null;

    private int finalPass, firstPass;

    public void Start()
    {
        if (BlurMask.Equals(null))
        {
            Shader.SetGlobalTexture(maskTextureString, Texture2D.whiteTexture);
        }
        else
            Shader.SetGlobalTexture(maskTextureString, BlurMask);
    }

    private void Update()
    {
        if (SourceLut != prevSorceLut)
        {
            prevSorceLut = SourceLut;
            Convert(SourceLut);
        }

    }

    private void OnDestroy()
    {
        if (converted2DLut != null)
        {
            DestroyImmediate(converted2DLut);
        }
        converted2DLut = null;
    }

    public void Convert(Texture2D temp2DTex)
    {
        if (temp2DTex)
        {
            Color[] color = temp2DTex.GetPixels();
            Color[] newCol = new Color[65536];

            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    for (int x = 0; x < 16; x++)
                        for (int y = 0; y < 16; y++)
                        {
                            float bChannel = (i + j * 16.0f) / 16;
                            int bchIndex0 = Mathf.FloorToInt(bChannel);
                            int bchIndex1 = Mathf.Min(bchIndex0 + 1, 15);
                            float lerpFactor = bChannel - bchIndex0;
                            int index = x + (15 - y) * 256;
                            Color col1 = color[index + bchIndex0 * 16];
                            Color col2 = color[index + bchIndex1 * 16];

                            newCol[x + i * 16 + y * 256 + j * 4096] =
                                Color.Lerp(col1, col2, lerpFactor);
                        }

            if (converted2DLut)
                DestroyImmediate(converted2DLut);
            converted2DLut = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            converted2DLut.SetPixels(newCol);
            converted2DLut.wrapMode = TextureWrapMode.Clamp;
            converted2DLut.Apply();
        }
        else
        {
            Debug.LogError("Couldn't color correct with 2D LUT texture. Image Effect will be disabled.");
        }
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Blur)
        {
            firstPass = 0;
            finalPass = 3;

            if (Bloom)
            {
                firstPass = 1;
                finalPass = 4;
            }

            material.SetFloat(blAmountString, BloomAmount);
            material.SetFloat(blThresholdString, BloomThreshold);
            material.SetFloat(blurAmountString, BlurAmount);

            RenderTexture buffer = RenderTexture.GetTemporary(scrWidth, scrHeight, 0, source.format);
            Graphics.Blit(source, buffer, material, firstPass);

            RenderTexture temp = RenderTexture.GetTemporary(scrWidth / 2, scrHeight / 2, 0, source.format);
            Graphics.Blit(buffer, temp, material, 0);
            RenderTexture.ReleaseTemporary(buffer);

            RenderTexture temp2 = RenderTexture.GetTemporary(scrWidth, scrHeight, 0, source.format);
            Graphics.Blit(temp, temp2, material, 0);
            RenderTexture.ReleaseTemporary(temp);

            material.SetTexture(blurTexString, temp2);
            if (LUT)
            {
                finalPass = Bloom ? 6 : 5;
                material.SetTexture(lutTextureString, converted2DLut);
                material.SetFloat(lutAmountString, LutAmount);
            }
            Graphics.Blit(source, destination, material, finalPass);
            RenderTexture.ReleaseTemporary(temp2);
            return;
        }
        else if (LUT)
        {
            material.SetTexture(lutTextureString, converted2DLut);
            material.SetFloat(lutAmountString, LutAmount);
            Graphics.Blit(source, destination, material, 2);
            return;
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
