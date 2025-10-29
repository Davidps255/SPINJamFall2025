using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CountryHouseGenerator : MonoBehaviour
{
    [Header("House Body (meters)")]
    [Min(1f)] public float width = 8f;      // X
    [Min(1f)] public float depth = 6f;      // Z
    [Min(1f)] public float wallHeight = 3f; // Y

    [Header("Roof (meters)")]
    [Min(0.2f)] public float roofHeight = 2f;
    [Range(0f, 1.5f)] public float roofOverhang = 0.35f;

    [Header("Chimney (meters)")]
    public bool addChimney = true;
    [Min(0.2f)] public float chimneyWidth = 0.6f;
    [Min(0.2f)] public float chimneyDepth = 0.6f;
    [Min(0.1f)] public float chimneyHeight = 1.2f;
    [Range(-0.45f, 0.45f)] public float chimneyXOffset = -0.25f; // relative to roof width (fraction of width)
    [Range(-0.45f, 0.45f)] public float chimneyZOffset = 0.15f;  // relative to roof depth

    [Header("Porch (optional)")]
    public bool addPorch = true;
    [Min(0.2f)] public float porchDepth = 1.8f;
    [Min(0.05f)] public float porchHeight = 0.25f;
    [Min(0.08f)] public float porchPostSize = 0.12f;
    [Min(0.3f)] public float porchPostInset = 0.6f; // inset from sides

    [Header("Output")]
    public bool combineIntoSingleMesh = false; // if true, merges into one mesh under "Combined"
    public bool autoRegenerateOnValidate = true;

    const string ROOT_NAME = "Generated_CountryHouse";
    Transform root;

    // ------------------------------------------------------------
    // Unity Hooks
    // ------------------------------------------------------------
    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Generate();
        }
    }

    void OnValidate()
    {
        // Small guard to avoid errors during domain reloads
        width = Mathf.Max(1f, width);
        depth = Mathf.Max(1f, depth);
        wallHeight = Mathf.Max(1f, wallHeight);
        roofHeight = Mathf.Max(0.2f, roofHeight);

        if (autoRegenerateOnValidate && this.enabled)
        {
            Generate();
        }
    }

    [ContextMenu("Generate House")]
    public void Generate()
    {
        // Create/cleanup root
        root = EnsureChild(ROOT_NAME, transform);
        CleanupChildren(root);

        // Build parts
        var parts = new List<GameObject>();

        // House body (a rectangular prism)
        var wallsGO = CreateMeshGO("Walls", MakeBox(width, wallHeight, depth));
        wallsGO.transform.SetParent(root, false);
        wallsGO.transform.localPosition = new Vector3(0f, wallHeight * 0.5f, 0f);
        parts.Add(wallsGO);

        // Roof (gable)
        var roofGO = CreateMeshGO("Roof", MakeGableRoof(width, depth, roofHeight, roofOverhang));
        roofGO.transform.SetParent(root, false);
        roofGO.transform.localPosition = new Vector3(0f, wallHeight, 0f);
        parts.Add(roofGO);

        // Chimney
        if (addChimney)
        {
            float chimX = chimneyXOffset * width;
            float chimZ = chimneyZOffset * depth;
            var chimGO = CreateMeshGO("Chimney", MakeBox(chimneyWidth, chimneyHeight + 0.05f, chimneyDepth));
            chimGO.transform.SetParent(root, false);
            // Place it so its base sits near the top of the roof; raise a bit above ridge
            chimGO.transform.localPosition = new Vector3(chimX, wallHeight + roofHeight * 0.6f, chimZ);
            parts.Add(chimGO);
        }

        // Porch (simple slab + two posts)
        if (addPorch)
        {
            float slabW = width - 0.2f; // small inset for visual interest
            var slab = CreateMeshGO("PorchSlab", MakeBox(slabW, porchHeight, porchDepth));
            slab.transform.SetParent(root, false);
            slab.transform.localPosition = new Vector3(0f, porchHeight * 0.5f, (depth + porchDepth) * 0.5f);

            // Posts
            float halfW = width * 0.5f;
            float postY = wallHeight * 0.8f; // not quite to the roofline; looks rustic
            float zFront = (depth * 0.5f) + porchDepth - (porchPostSize * 0.5f);

            var postL = CreateMeshGO("PorchPost_L", MakeBox(porchPostSize, postY, porchPostSize));
            postL.transform.SetParent(root, false);
            postL.transform.localPosition = new Vector3(-halfW + porchPostInset, postY * 0.5f, zFront);

            var postR = CreateMeshGO("PorchPost_R", MakeBox(porchPostSize, postY, porchPostSize));
            postR.transform.SetParent(root, false);
            postR.transform.localPosition = new Vector3(halfW - porchPostInset, postY * 0.5f, zFront);

            parts.Add(slab);
            parts.Add(postL);
            parts.Add(postR);
        }

        // Optionally merge meshes to a single draw
        if (combineIntoSingleMesh)
        {
            CombineAllUnder(root, "Combined");
        }

        // Place root at ground level (y = 0 is floor)
        root.localPosition = Vector3.zero;
    }

    // ------------------------------------------------------------
    // Mesh Building
    // ------------------------------------------------------------
    static Mesh MakeBox(float w, float h, float d)
    {
        // Axis-aligned box centered at origin
        float hx = w * 0.5f, hy = h * 0.5f, hz = d * 0.5f;

        Vector3[] v =
        {
            // Front (z+)
            new Vector3(-hx,-hy, hz), new Vector3(hx,-hy, hz), new Vector3(hx,hy, hz), new Vector3(-hx,hy, hz),
            // Back  (z-)
            new Vector3(hx,-hy,-hz),  new Vector3(-hx,-hy,-hz), new Vector3(-hx,hy,-hz), new Vector3(hx,hy,-hz),
            // Left  (x-)
            new Vector3(-hx,-hy,-hz), new Vector3(-hx,-hy, hz), new Vector3(-hx,hy, hz), new Vector3(-hx,hy,-hz),
            // Right (x+)
            new Vector3(hx,-hy, hz),  new Vector3(hx,-hy,-hz),  new Vector3(hx,hy,-hz),  new Vector3(hx,hy, hz),
            // Top   (y+)
            new Vector3(-hx,hy, hz),  new Vector3(hx,hy, hz),   new Vector3(hx,hy,-hz),  new Vector3(-hx,hy,-hz),
            // Bottom(y-)
            new Vector3(-hx,-hy,-hz), new Vector3(hx,-hy,-hz),  new Vector3(hx,-hy, hz), new Vector3(-hx,-hy, hz),
        };

        int[] t = FaceTris();

        Vector3[] n =
        {
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.back,    Vector3.back,    Vector3.back,    Vector3.back,
            Vector3.left,    Vector3.left,    Vector3.left,    Vector3.left,
            Vector3.right,   Vector3.right,   Vector3.right,   Vector3.right,
            Vector3.up,      Vector3.up,      Vector3.up,      Vector3.up,
            Vector3.down,    Vector3.down,    Vector3.down,    Vector3.down
        };

        Vector2[] uv = RepeatedQuadUVs(6);

        var m = new Mesh { name = "Box" };
        m.SetVertices(v);
        m.SetNormals(n);
        m.SetUVs(0, uv);
        m.SetTriangles(t, 0);
        m.RecalculateBounds();
        return m;
    }

    static Mesh MakeGableRoof(float houseW, float houseD, float roofH, float overhang)
    {
        // Gable along X (width). Overhang extends on all sides.
        float halfW = houseW * 0.5f;
        float halfD = houseD * 0.5f;
        float extW = halfW + overhang;
        float extD = halfD + overhang;

        // Vertices: two sloped planes meeting at ridge.
        // We build as a closed prism so underside is capped.
        // Left slope quad and right slope quad + gable end caps + front/back eaves
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var uvs   = new List<Vector2>();
        var tris  = new List<int>();

        // Helper to add a quad
        void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int i = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
            Vector3 n = Vector3.Cross(b - a, d - a).normalized;
            norms.Add(n); norms.Add(n); norms.Add(n); norms.Add(n);
            uvs.Add(new Vector2(0,0)); uvs.Add(new Vector2(1,0)); uvs.Add(new Vector2(1,1)); uvs.Add(new Vector2(0,1));
            tris.Add(i+0); tris.Add(i+1); tris.Add(i+2);
            tris.Add(i+0); tris.Add(i+2); tris.Add(i+3);
        }

        float ridgeY = roofH;

        // Right slope (x+)
        AddQuad(
            new Vector3(0f, ridgeY,  extD),
            new Vector3(extW, 0f,     extD),
            new Vector3(extW, 0f,    -extD),
            new Vector3(0f,   ridgeY,-extD)
        );

        // Left slope (x-)
        AddQuad(
            new Vector3(-extW, 0f,     extD),
            new Vector3(0f,    ridgeY,  extD),
            new Vector3(0f,    ridgeY, -extD),
            new Vector3(-extW, 0f,    -extD)
        );

        // Front cap (z+ gable triangle + eave)
        // Triangle (convert to quad with a thin strip underneath to close)
        // Upper triangle face as two tris via a quad degenerate technique (use a small epsilon down)
        float eps = 0.0001f;
        AddQuad(
            new Vector3(-halfW, 0f,  extD),
            new Vector3( halfW, 0f,  extD),
            new Vector3( 0f, ridgeY, extD),
            new Vector3( 0f, ridgeY - eps, extD) // tiny bottom edge to make a closed surface
        );

        // Back cap (z-)
        AddQuad(
            new Vector3( halfW, 0f, -extD),
            new Vector3(-halfW, 0f, -extD),
            new Vector3( 0f, ridgeY, -extD),
            new Vector3( 0f, ridgeY - eps, -extD)
        );

        // Underside (optional small strip along eaves to close volume)
        // Front underside eave
        AddQuad(
            new Vector3(-extW, 0f,  extD),
            new Vector3( extW, 0f,  extD),
            new Vector3( halfW, 0f,  extD),
            new Vector3(-halfW,0f,  extD)
        );

        // Back underside eave
        AddQuad(
            new Vector3( extW, 0f, -extD),
            new Vector3(-extW, 0f, -extD),
            new Vector3(-halfW,0f, -extD),
            new Vector3( halfW,0f, -extD)
        );

        var m = new Mesh { name = "GableRoof" };
        m.SetVertices(verts);
        m.SetNormals(norms);
        m.SetUVs(0, uvs);
        m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        return m;
    }

    static int[] FaceTris()
    {
        // 6 quads -> 12 tris -> 36 indices
        // Each face in order has 4 vertices
        var tris = new List<int>(36);
        for (int face = 0; face < 6; face++)
        {
            int i = face * 4;
            tris.Add(i + 0); tris.Add(i + 1); tris.Add(i + 2);
            tris.Add(i + 0); tris.Add(i + 2); tris.Add(i + 3);
        }
        return tris.ToArray();
    }

    static Vector2[] RepeatedQuadUVs(int quadCount)
    {
        var uv = new Vector2[quadCount * 4];
        for (int i = 0; i < quadCount; i++)
        {
            int j = i * 4;
            uv[j + 0] = new Vector2(0, 0);
            uv[j + 1] = new Vector2(1, 0);
            uv[j + 2] = new Vector2(1, 1);
            uv[j + 3] = new Vector2(0, 1);
        }
        return uv;
    }

    // ------------------------------------------------------------
    // Utilities
    // ------------------------------------------------------------
    static Transform EnsureChild(string name, Transform parent)
    {
        var t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name);
            t = go.transform;
            t.SetParent(parent, false);
        }
        return t;
    }

    static void CleanupChildren(Transform t)
    {
        // Destroys all children (edit & play mode safe)
        var toDestroy = new List<GameObject>();
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            toDestroy.Add(t.GetChild(i).gameObject);
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            foreach (var go in toDestroy)
                UnityEditor.Undo.DestroyObjectImmediate(go);
        }
        else
#endif
        {
            foreach (var go in toDestroy)
                Destroy(go);
        }
    }

    static GameObject CreateMeshGO(string name, Mesh mesh)
    {
        var go = new GameObject(name);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        // Untextured default material; Unity will assign a default if null.
        mr.sharedMaterial = new Material(Shader.Find("Standard"));
        return go;
    }

    static void CombineAllUnder(Transform parent, string combinedName)
    {
        var mfs = parent.GetComponentsInChildren<MeshFilter>();
        var combine = new List<CombineInstance>();

        foreach (var mf in mfs)
        {
            if (mf.transform == parent) continue;
            if (mf.sharedMesh == null) continue;
            var ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            combine.Add(ci);
        }

        // Cleanup children
        CleanupChildren(parent);

        // Create combined
        var combined = new GameObject(combinedName);
        combined.transform.SetParent(parent, false);

        var mfCombined = combined.AddComponent<MeshFilter>();
        var mrCombined = combined.AddComponent<MeshRenderer>();

        var mesh = new Mesh { name = "CombinedHouse" };
        mesh.CombineMeshes(combine.ToArray(), true, true, false);
        mfCombined.sharedMesh = mesh;
        mrCombined.sharedMaterial = new Material(Shader.Find("Standard"));
    }

#if UNITY_EDITOR
    // Add a menu item to create a preconfigured generator in the scene
    [UnityEditor.MenuItem("GameObject/3D Object/Procedural/Country House", false, 10)]
    static void CreateMenu()
    {
        var host = new GameObject("CountryHouse");
        var gen = host.AddComponent<CountryHouseGenerator>();
        gen.Generate();
        UnityEditor.Selection.activeObject = host;
        UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();
    }
#endif

    // Optional: draw ground & outline for quick preview in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 0, 0.25f);
        Gizmos.DrawCube(transform.position + new Vector3(0, -0.001f, 0), new Vector3(width + 2f, 0.002f, depth + 2f));

        Gizmos.color = Color.gray;
        Vector3 p = transform.position;
        Gizmos.DrawWireCube(p + new Vector3(0, wallHeight * 0.5f, 0), new Vector3(width, wallHeight, depth));
        Gizmos.DrawLine(p + new Vector3(-width * 0.5f - roofOverhang, wallHeight + roofHeight, -depth * 0.5f - roofOverhang),
                        p + new Vector3( width * 0.5f + roofOverhang, wallHeight + roofHeight, -depth * 0.5f - roofOverhang));
        Gizmos.DrawLine(p + new Vector3(-width * 0.5f - roofOverhang, wallHeight + roofHeight,  depth * 0.5f + roofOverhang),
                        p + new Vector3( width * 0.5f + roofOverhang, wallHeight + roofHeight,  depth * 0.5f + roofOverhang));
    }
}
