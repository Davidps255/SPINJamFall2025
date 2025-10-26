using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CornPlant : MonoBehaviour
{
    [Header("Shape")]
    [Min(0.1f)] public float stalkHeight = 1.6f;
    [Min(0.02f)] public float stalkRadius = 0.06f;
    [Range(0, 1)] public float cobHeightRatio = 0.55f;
    [Min(0.02f)] public float cobLength = 0.25f;
    [Min(0.02f)] public float cobRadius = 0.07f;

    [Header("Leaves")]
    [Range(2, 10)] public int leafCount = 4;
    [Min(0.05f)] public float leafLength = 0.45f;
    [Min(0.02f)] public float leafWidth = 0.15f;
    [Range(0f, 60f)] public float leafUpTilt = 20f;

    [Header("Variation")]
    [Range(0f, 0.2f)] public float heightJitter = 0.08f;
    [Range(0f, 12f)] public float leanDegrees = 2f;
    [Range(0f, 20f)] public float randomYaw = 12f;
    public int seed = 0;

    [Header("Colors")]
    public Color stalkColor = new Color(0.16f, 0.35f, 0.12f);
    public Color leafColor  = new Color(0.18f, 0.5f, 0.16f);
    public Color cobColor   = new Color(0.9f, 0.8f, 0.2f);

    [Header("Render")]
    public bool castShadows = true;
    public bool receiveShadows = true;

    static readonly string ROOT_NAME = "_CornRoot";

    Material stalkMat, leafMat, cobMat;
    System.Random rng;
    float rnd01() => (float)rng.NextDouble();

    [ContextMenu("Rebuild Corn")]
    public void RebuildCorn()
    {
        // Clear first, then build fresh (manual trigger only)
        ClearExistingRootImmediate();
        ActuallyBuildCorn();
    }

    void OnEnable()
    {
        // Build once if dropped fresh and nothing exists
        if (transform.Find(ROOT_NAME) == null)
        {
            RebuildCorn();
        }
    }

    void OnValidate()
    {
        // Clamp/validate values ONLY — no rebuild here
        stalkHeight = Mathf.Max(0.1f, stalkHeight);
        stalkRadius = Mathf.Max(0.02f, stalkRadius);
        cobLength   = Mathf.Max(0.02f, cobLength);
        cobRadius   = Mathf.Max(0.02f, cobRadius);
        leafLength  = Mathf.Max(0.05f, leafLength);
        leafWidth   = Mathf.Max(0.02f, leafWidth);
        leafCount   = Mathf.Clamp(leafCount, 2, 10);
        leafUpTilt  = Mathf.Clamp(leafUpTilt, 0f, 60f);
        heightJitter = Mathf.Clamp(heightJitter, 0f, 0.2f);
        leanDegrees  = Mathf.Clamp(leanDegrees, 0f, 12f);
        randomYaw    = Mathf.Clamp(randomYaw, 0f, 20f);
        cobHeightRatio = Mathf.Clamp01(cobHeightRatio);
    }

    void ActuallyBuildCorn()
    {
        rng = seed == 0 ? new System.Random(transform.GetInstanceID()) : new System.Random(seed);

        EnsureMaterials();

        // Randomize slight height, lean, yaw
        float hJ    = Mathf.Lerp(-heightJitter, heightJitter, rnd01());
        float height = Mathf.Max(0.2f, stalkHeight + hJ);
        float yaw    = Mathf.Lerp(-randomYaw, randomYaw, rnd01());
        float lean   = Mathf.Lerp(-leanDegrees, leanDegrees, rnd01());

        var root = new GameObject(ROOT_NAME);
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(lean, 0f, 0f);

        // Stalk (cylinder)
        var stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        SetupPiece(stalk, root.transform, stalkMat);
        stalk.transform.localScale = new Vector3(stalkRadius * 2f, height * 0.5f, stalkRadius * 2f);
        stalk.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);

        // Cob (capsule)
        float cobY = Mathf.Clamp01(cobHeightRatio) * height;
        var cob = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        SetupPiece(cob, root.transform, cobMat);
        cob.transform.localScale = new Vector3(cobRadius * 2f, cobLength * 0.5f, cobRadius * 2f);
        cob.transform.localPosition = new Vector3(0.0f, cobY, 0.0f);
        cob.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Leaves (quads)
        for (int i = 0; i < leafCount; i++)
        {
            float t = (i + 1f) / (leafCount + 1f);
            float y = Mathf.Lerp(height * 0.25f, height * 0.9f, t);
            float around = (360f / leafCount) * i + rnd01() * 25f;

            var leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
            SetupPiece(leaf, root.transform, leafMat);
            leaf.transform.localScale = new Vector3(leafWidth, leafLength, 1f);
            leaf.transform.localPosition = new Vector3(0f, y, 0f);

            var rot = Quaternion.AngleAxis(around, Vector3.up) * Quaternion.AngleAxis(90f - leafUpTilt, Vector3.right);
            leaf.transform.localRotation = rot;

            float offset = stalkRadius + (leafWidth * 0.2f);
            leaf.transform.localPosition += leaf.transform.right * offset * 0.5f;

            DestroyColliderIfPresent(leaf);
        }

        // Colliders: remove detail colliders unless you need them
        DestroyColliderIfPresent(stalk);
        DestroyColliderIfPresent(cob);

        // Selection collider
        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0, height * 0.5f, 0);
        box.size = new Vector3(Mathf.Max(0.2f, cobRadius * 2.2f), height, Mathf.Max(0.2f, cobRadius * 2.2f));

        // Shadow settings
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            r.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = receiveShadows;
        }
    }

    void EnsureMaterials()
    {
        // Use shared Standard materials so these batch nicely
        if (stalkMat == null) stalkMat = MakeMat(stalkColor);
        if (leafMat  == null) leafMat  = MakeMat(leafColor);
        if (cobMat   == null) cobMat   = MakeMat(cobColor);

        // Keep colors in sync if edited
        stalkMat.color = stalkColor;
        leafMat.color  = leafColor;
        cobMat.color   = cobColor;
    }

    void ClearExistingRootImmediate()
    {
        var existing = transform.Find(ROOT_NAME);
        if (existing != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(existing.gameObject);
            else Destroy(existing.gameObject);
#else
            DestroyImmediate(existing.gameObject);
#endif
        }
    }

    static void DestroyColliderIfPresent(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(col);
            else Destroy(col);
#else
            DestroyImmediate(col);
#endif
        }
    }

    static Material MakeMat(Color c)
    {
        Shader shader = null;

        // Detect pipeline
    #if UNITY_EDITOR
        var rpAsset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        bool usingURP = rpAsset && rpAsset.GetType().Name.Contains("Universal");
        bool usingHDRP = rpAsset && rpAsset.GetType().Name.Contains("HD");
    #else
        bool usingURP = false;
        bool usingHDRP = false;
    #endif

        if (usingURP)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        else if (usingHDRP)
            shader = Shader.Find("HDRP/Lit");
        else
            shader = Shader.Find("Standard");

        if (!shader)
        {
            Debug.LogWarning("CornPlant: Could not find a Lit/Standard shader! Using default.");
            shader = Shader.Find("Diffuse"); // fallback
        }

        var m = new Material(shader);
        m.color = c;
        m.enableInstancing = true;
        return m;
    }

    static void SetupPiece(GameObject go, Transform parent, Material mat)
    {
        go.transform.SetParent(parent, false);
        var mr = go.GetComponent<MeshRenderer>();
        if (mr) mr.sharedMaterial = mat;
    }
}
