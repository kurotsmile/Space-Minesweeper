using UnityEngine;
using UnityEngine.Rendering;

public class SkyboxBackdrop : MonoBehaviour
{
    private const string URP_UNLIT_SHADER_NAME = "Universal Render Pipeline/Unlit";
    private const string FALLBACK_UNLIT_SHADER_NAME = "Unlit/Texture";
    private const float DEFAULT_DISTANCE = 250f;

    private readonly Material[] _faceMaterials = new Material[6];
    private Transform _root;
    private Vector3 _rotation;

    public Vector3 Rotation
    {
        get { return _rotation; }
        set { _rotation = value; }
    }

    private void Awake()
    {
        EnsureBackdrop();
    }

    private void LateUpdate()
    {
        if (_root == null || _rotation == Vector3.zero) return;

        _root.Rotate(_rotation * Time.deltaTime, Space.Self);
    }

    public void ApplySkybox(Material skyboxMaterial)
    {
        if (skyboxMaterial == null) return;

        EnsureBackdrop();

        ApplyFaceTexture(0, skyboxMaterial, "_FrontTex");
        ApplyFaceTexture(1, skyboxMaterial, "_BackTex");
        ApplyFaceTexture(2, skyboxMaterial, "_LeftTex");
        ApplyFaceTexture(3, skyboxMaterial, "_RightTex");
        ApplyFaceTexture(4, skyboxMaterial, "_UpTex");
        ApplyFaceTexture(5, skyboxMaterial, "_DownTex");
    }

    private void EnsureBackdrop()
    {
        if (_root != null) return;

        GameObject rootObject = new GameObject("Skybox Backdrop");
        _root = rootObject.transform;
        _root.SetParent(transform, false);
        _root.localPosition = Vector3.zero;
        _root.localRotation = Quaternion.identity;
        _root.localScale = Vector3.one;

        CreateFace(0, "Front", new Vector3(0f, 0f, DEFAULT_DISTANCE), new Vector3(0f, 180f, 0f));
        CreateFace(1, "Back", new Vector3(0f, 0f, -DEFAULT_DISTANCE), Vector3.zero);
        CreateFace(2, "Left", new Vector3(-DEFAULT_DISTANCE, 0f, 0f), new Vector3(0f, -90f, 0f));
        CreateFace(3, "Right", new Vector3(DEFAULT_DISTANCE, 0f, 0f), new Vector3(0f, 90f, 0f));
        CreateFace(4, "Up", new Vector3(0f, DEFAULT_DISTANCE, 0f), new Vector3(90f, 180f, 0f));
        CreateFace(5, "Down", new Vector3(0f, -DEFAULT_DISTANCE, 0f), new Vector3(-90f, 180f, 0f));
    }

    private void CreateFace(int index, string faceName, Vector3 localPosition, Vector3 localEulerAngles)
    {
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = faceName;
        face.transform.SetParent(_root, false);
        face.transform.localPosition = localPosition;
        face.transform.localRotation = Quaternion.Euler(localEulerAngles);
        face.transform.localScale = new Vector3(DEFAULT_DISTANCE * 2f, DEFAULT_DISTANCE * 2f, 1f);

        Collider faceCollider = face.GetComponent<Collider>();
        if (faceCollider != null)
            Destroy(faceCollider);

        MeshRenderer renderer = face.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

        Material faceMaterial = new Material(GetUnlitShader());
        if (faceMaterial.HasProperty("_BaseColor"))
            faceMaterial.SetColor("_BaseColor", Color.white);
        if (faceMaterial.HasProperty("_Color"))
            faceMaterial.SetColor("_Color", Color.white);
        if (faceMaterial.HasProperty("_Cull"))
            faceMaterial.SetFloat("_Cull", 0f);

        renderer.sharedMaterial = faceMaterial;
        _faceMaterials[index] = faceMaterial;
    }

    private void ApplyFaceTexture(int index, Material skyboxMaterial, string textureProperty)
    {
        if (!skyboxMaterial.HasProperty(textureProperty)) return;

        Texture texture = skyboxMaterial.GetTexture(textureProperty);
        if (texture == null) return;

        Material faceMaterial = _faceMaterials[index];
        if (faceMaterial == null) return;

        if (faceMaterial.HasProperty("_BaseMap"))
            faceMaterial.SetTexture("_BaseMap", texture);
        if (faceMaterial.HasProperty("_MainTex"))
            faceMaterial.SetTexture("_MainTex", texture);
        faceMaterial.mainTexture = texture;
    }

    private Shader GetUnlitShader()
    {
        Shader shader = Shader.Find(URP_UNLIT_SHADER_NAME);
        if (shader != null) return shader;

        shader = Shader.Find(FALLBACK_UNLIT_SHADER_NAME);
        if (shader != null) return shader;

        return Shader.Find("Standard");
    }
}
