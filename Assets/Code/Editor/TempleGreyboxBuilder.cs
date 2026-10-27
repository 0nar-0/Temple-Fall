// Temple Fall – greybox level builder (editor only, static boxes, nothing dynamic).
// Menu: Temple Fall > Build Greybox Level
// Rebuilds "TempleLevel_Greybox" in LevelTest_Scene every time you run it.
// The previous test objects are moved (not deleted) into a disabled "_OldLevelTest" object.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TempleGreyboxBuilder
{
    const string ScenePath = "Assets/Level/Scenes/LevelTest_Scene.unity";
    const string RootName = "TempleLevel_Greybox";
    const string OldName = "_OldLevelTest";

    static Material groundMat, wallMat, obstacleMat, fogMat;
    static GameObject pillarModel, doorModel;

    // Level metrics (player: walk 6, sprint 10, jump height 2, crouch height 1)
    const float FloorThick = 1f;
    const float Abyss = -40f;   // bottom of supports, hidden in fog

    [MenuItem("Temple Fall/Build Greybox Level")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ScenePath);
        }

        groundMat   = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Ground.mat");
        wallMat     = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Walls.mat");
        obstacleMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Obstacles.mat");
        fogMat      = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/fakefog.mat");
        pillarModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/pillar.fbx");
        doorModel   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/Ancient-door.fbx");

        StashOldObjects(scene);

        var old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject(RootName);

        BuildEntrance(Group(root, "00_Entrance"));
        BuildBridge(Group(root, "01_Bridge"));
        BuildSlide1(Group(root, "02_Slide"));
        BuildTower(Group(root, "03_Tower"));
        BuildPassage(Group(root, "04_NarrowPassage"));
        BuildSlide2(Group(root, "05_Slide"));
        BuildFinale(Group(root, "06_Finale"));

        // Abyss fog under the whole level (top at y -20)
        Box(Group(root, "Fog"), "Fog_Abyss", Min(-150, -60, -40), Max(150, -20, 380), fogMat, false);

        PlacePlayerAndCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Temple Fall greybox built: Entrance > Bridge > Slide > Tower > Narrow Passage > Slide > Finale");
    }

    // ---------------------------------------------------------------- sections

    // Start chamber: z -8..12, floor y 0. Sealed door behind the player.
    static void BuildEntrance(Transform g)
    {
        Box(g, "Entrance_Floor_01", Min(-5, -FloorThick, -8), Max(5, 0, 12), groundMat);
        Box(g, "Entrance_Base", Min(-6, Abyss, -9), Max(6, -FloorThick, 12), wallMat);
        Box(g, "Entrance_Wall_L", Min(-6, 0, -9), Max(-5, 9, 12), wallMat);
        Box(g, "Entrance_Wall_R", Min(5, 0, -9), Max(6, 9, 12), wallMat);
        Box(g, "Entrance_Wall_Back", Min(-6, 0, -9), Max(6, 9, -8), wallMat);
        Box(g, "Entrance_Ceiling", Min(-6, 8, -9), Max(6, 9, 12), wallMat);
        WallWithHole(g, "Entrance_Facade", 11, 12, -6, 6, 0, 9, -2.5f, 2.5f, 0, 5);
        Model(g, doorModel, "Entrance_SealedDoor", new Vector3(0, 0, -7.6f), Quaternion.Euler(-90, 0, -90), Vector3.one * 50, 6f, false);
        Box(g, "Entrance_Rubble_01", Min(3, 0, 6), Max(4.5f, 0.7f, 7.5f), obstacleMat, true, 12);
        Box(g, "Entrance_Rubble_02", Min(-4.6f, 0, 1), Max(-3.4f, 0.5f, 2.4f), obstacleMat, true, -20);
    }

    // Bridge: z 12..92, width 5, y 0. Two already-collapsed gaps. Ruined colonnade on both sides.
    static void BuildBridge(Transform g)
    {
        float[][] runs = { new[] { 12f, 44f }, new[] { 46.5f, 68f }, new[] { 70.5f, 92f } };
        int n = 1;
        foreach (var r in runs)
            for (float z = r[0]; z < r[1] - 0.01f; z += 4f)
            {
                float z1 = Mathf.Min(z + 4f, r[1]);
                Box(g, $"Bridge_Floor_{n++:00}", Min(-2.5f, -FloorThick, z), Max(2.5f, 0, z1), groundMat);
            }

        // supports down into the abyss
        int s = 1;
        for (float z = 18; z < 92; z += 16)
            Box(g, $"Bridge_Support_{s++:00}", Min(-1.5f, Abyss, z - 1.5f), Max(1.5f, -FloorThick, z + 1.5f), wallMat);

        // pillars on side bases + broken ceiling beams on top
        int p = 1, b = 1;
        for (float z = 18; z < 92; z += 12)
        {
            float top = 10f;
            foreach (float x in new[] { -5f, 5f })
            {
                Box(g, $"Bridge_PillarBase_{p:00}", Min(x - 1.2f, Abyss, z - 1.2f), Max(x + 1.2f, 0, z + 1.2f), wallMat);
                top = Pillar(g, $"Bridge_Pillar_{p:00}", new Vector3(x, 0, z), 10f);
                p++;
            }
            if (((int)(z - 18) / 12) % 2 == 0)
                Box(g, $"Bridge_CeilingBeam_{b++:00}", Min(-6.2f, top, z - 0.8f), Max(6.2f, top + 1.2f, z + 0.8f), wallMat);
        }

        // low rubble to hop over
        Box(g, "Bridge_Rubble_01", Min(-2.5f, 0, 30), Max(1f, 0.7f, 31.2f), obstacleMat, true, 8);
        Box(g, "Bridge_Rubble_02", Min(-1f, 0, 80), Max(2.5f, 0.7f, 81.2f), obstacleMat, true, -6);
        // a pillar that already fell into the gap area (static)
        Box(g, "Bridge_FallenPillar_01", Min(-0.8f, -1.6f, 66.5f), Max(0.8f, 0f, 74f), obstacleMat, true, 0, 0, 25);
    }

    // Slide 1: z 92..132, y 0 -> -16, inside a tunnel. Enters the tower at the bottom.
    static void BuildSlide1(Transform g)
    {
        Slope(g, "Slide1_Floor", 0, 92, 0, 132, -16, 5f, groundMat);
        Slope(g, "Slide1_Ceiling", 0, 92, 6, 132, -10, 7f, wallMat);
        Box(g, "Slide1_Wall_L", Min(-3.5f, -17, 92), Max(-2.5f, 7, 131), wallMat);
        Box(g, "Slide1_Wall_R", Min(2.5f, -17, 92), Max(3.5f, 7, 131), wallMat);
        Box(g, "Slide1_Base", Min(-3.5f, Abyss, 92), Max(3.5f, -17, 131), wallMat);
        WallWithHole(g, "Slide1_Facade", 91, 92, -9, 9, -2, 16, -2.5f, 2.5f, -2, 6);
    }

    // Tower: room x -8..8, z 132..152, floor y -16. Broken climb around the walls up to y 12.
    static void BuildTower(Transform g)
    {
        const float floorY = -16, topY = 12, ceilY = 24;
        Box(g, "Tower_Floor", Min(-8, Abyss, 132), Max(8, floorY, 152), groundMat);
        WallWithHole(g, "Tower_Wall_South", 131, 132, -9, 9, Abyss, ceilY + 1, -2.5f, 2.5f, floorY, floorY + 5);
        WallWithHole(g, "Tower_Wall_North", 152, 153, -9, 9, Abyss, ceilY + 1, -2f, 2f, topY, topY + 4);
        Box(g, "Tower_Wall_W", Min(-9, Abyss, 132), Max(-8, ceilY + 1, 152), wallMat);
        Box(g, "Tower_Wall_E", Min(8, Abyss, 132), Max(9, ceilY + 1, 152), wallMat);
        Box(g, "Tower_Ceiling", Min(-9, ceilY, 131), Max(9, ceilY + 1, 153), wallMat);

        // Platforms on a rectangular path (inset from walls), rising evenly to the exit ledge.
        const int count = 18;
        float rise = (topY - floorY) / (count + 1);      // ~1.47 per jump
        float spacing = 73.5f / count;                    // lands the last step at the north-centre ledge
        for (int i = 0; i < count; i++)
        {
            Vector2 p = PerimeterPoint(i * spacing);
            float top = floorY + rise * (i + 1);
            Box(g, $"Tower_Platform_{i + 1:00}", Min(p.x - 1.5f, top - 0.6f, p.y - 1.5f), Max(p.x + 1.5f, top, p.y + 1.5f), groundMat);
        }
        Box(g, "Tower_ExitLedge", Min(-2.5f, topY - 0.8f, 148.5f), Max(2.5f, topY, 152), groundMat);

        // the staircase that collapsed in front of the player
        Box(g, "Tower_Rubble_01", Min(-3, floorY, 140), Max(1, floorY + 1.2f, 143), obstacleMat, true, 15, 0, 8);
        Box(g, "Tower_Rubble_02", Min(0, floorY, 144), Max(3.5f, floorY + 0.9f, 146), obstacleMat, true, -25);
        Box(g, "Tower_Rubble_03", Min(-1.5f, floorY, 137.5f), Max(0.5f, floorY + 0.6f, 139), obstacleMat, true, 40);
    }

    // perimeter of rectangle x[-5.5,5.5] z[135.5,149], starting at south-centre going east
    static Vector2 PerimeterPoint(float d)
    {
        const float x0 = -5.5f, x1 = 5.5f, z0 = 135.5f, z1 = 149f;
        float w = x1 - x0, h = z1 - z0, per = 2 * (w + h);
        d = Mathf.Repeat(d, per);
        if (d < x1) return new Vector2(d, z0);                       // south, centre -> east
        d -= x1;
        if (d < h) return new Vector2(x1, z0 + d);                   // east, going north
        d -= h;
        if (d < w) return new Vector2(x1 - d, z1);                   // north, going west
        d -= w;
        if (d < h) return new Vector2(x0, z1 - d);                   // west, going south
        d -= h;
        return new Vector2(x0 + d, z0);                              // south, west -> centre
    }

    // Narrow passage: z 153..232, width 4, floor y 12, ceiling y 16. Static obstacles + empty trap slots.
    static void BuildPassage(Transform g)
    {
        const float y = 12;
        float[][] runs = { new[] { 152f, 212f }, new[] { 214.5f, 232f } };
        int n = 1;
        foreach (var r in runs)
            for (float z = r[0]; z < r[1] - 0.01f; z += 4f)
                Box(g, $"Passage_Floor_{n++:00}", Min(-2, y - FloorThick, z), Max(2, y, Mathf.Min(z + 4f, r[1])), groundMat);

        Box(g, "Passage_Wall_L", Min(-3, y - 1, 153), Max(-2, y + 5, 232), wallMat);
        Box(g, "Passage_Wall_R", Min(2, y - 1, 153), Max(3, y + 5, 232), wallMat);
        Box(g, "Passage_Ceiling", Min(-3, y + 4, 153), Max(3, y + 5, 232), wallMat);
        Box(g, "Passage_TempleBody", Min(-6, Abyss, 153), Max(6, y - 1, 211.5f), wallMat);
        Box(g, "Passage_TempleBody_02", Min(-6, Abyss, 215), Max(6, y - 1, 232), wallMat);
        // spike pit (bottom at y 6)
        Box(g, "Passage_Pit_Bottom", Min(-2, 5, 211.5f), Max(2, 6, 215), obstacleMat);
        Box(g, "Passage_Pit_Base", Min(-6, Abyss, 211.5f), Max(6, 5, 215), wallMat);
        Box(g, "Passage_Pit_Wall_L", Min(-6, 5, 211.5f), Max(-2, y - 1, 215), wallMat);
        Box(g, "Passage_Pit_Wall_R", Min(2, 5, 211.5f), Max(6, y - 1, 215), wallMat);

        Box(g, "Passage_Log_01", Min(-2, y, 164.6f), Max(2, y + 0.8f, 165.4f), obstacleMat);   // jump over
        Box(g, "Passage_LowBeam_01", Min(-2, y + 1.4f, 178), Max(2, y + 4, 181), obstacleMat); // crouch under
        Box(g, "Passage_Step_01", Min(-2, y, 190), Max(2, y + 0.6f, 200), groundMat);           // bumps from the sketch
        Box(g, "Passage_Step_02", Min(-2, y + 0.6f, 194), Max(2, y + 1.2f, 197), groundMat);
        Box(g, "Passage_Log_02", Min(-2, y, 224.6f), Max(2, y + 0.8f, 225.4f), obstacleMat);

        Marker(g, "TrapSlot_SwingingAxe_01", new Vector3(0, y + 3, 171));
        Marker(g, "TrapSlot_SpikeBall_01", new Vector3(0, y + 1, 186));
        Marker(g, "TrapSlot_SpikePit_01", new Vector3(0, 6, 213));
        Marker(g, "TrapSlot_Arrows_01", new Vector3(-2, y + 1.5f, 220));
        Marker(g, "TrapSlot_SpikedLog_01", new Vector3(0, y + 0.5f, 229));
    }

    // Slide 2: z 232..262, y 12 -> 0, open air, leaving the temple.
    static void BuildSlide2(Transform g)
    {
        WallWithHole(g, "Slide2_TempleExit", 232, 233, -8, 8, Abyss, 22, -2f, 2f, 12, 16);
        Slope(g, "Slide2_Floor", 0, 233, 12, 262, 0, 6f, groundMat);
        Slope(g, "Slide2_Curb_L", -3.25f, 233, 12.5f, 262, 0.5f, 0.5f, obstacleMat);
        Slope(g, "Slide2_Curb_R", 3.25f, 233, 12.5f, 262, 0.5f, 0.5f, obstacleMat);
        Box(g, "Slide2_Support_01", Min(-1.5f, Abyss, 246), Max(1.5f, 5, 249), wallMat);
    }

    // Finale: z 262..322 wide run, collapsed colonnade, exit gate, outside ledge.
    static void BuildFinale(Transform g)
    {
        int n = 1;
        for (float z = 262; z < 322; z += 4f)
            Box(g, $"Finale_Floor_{n++:00}", Min(-4.5f, -FloorThick, z), Max(4.5f, 0, z + 4f), groundMat);
        Box(g, "Finale_Base", Min(-3, Abyss, 262), Max(3, -FloorThick, 322), wallMat);

        int p = 1;
        for (float z = 268; z < 320; z += 10)
            foreach (float x in new[] { -6.2f, 6.2f })
            {
                Box(g, $"Finale_PillarBase_{p:00}", Min(x - 1.2f, Abyss, z - 1.2f), Max(x + 1.2f, 0, z + 1.2f), wallMat);
                if (!(z == 288 && x > 0)) Pillar(g, $"Finale_Pillar_{p:00}", new Vector3(x, 0, z), 12f);
                p++;
            }
        Box(g, "Finale_FallenPillar_01", Min(-4.5f, 0, 288.2f), Max(5.5f, 0.9f, 289.4f), obstacleMat, true, 0, 4, 0); // jump over
        Box(g, "Finale_Rubble_01", Min(-4.5f, 0, 300), Max(-2, 0.8f, 302), obstacleMat, true, 18);
        Box(g, "Finale_Rubble_02", Min(2, 0, 308), Max(4.5f, 0.6f, 309.5f), obstacleMat, true, -12);

        WallWithHole(g, "Finale_ExitGate", 322, 323.5f, -12, 12, Abyss, 16, -3f, 3f, 0, 7);
        Box(g, "Exit_Outside_Floor", Min(-6, -FloorThick, 323.5f), Max(6, 0, 345), groundMat);
        Box(g, "Exit_Outside_Base", Min(-6, Abyss, 323.5f), Max(6, -FloorThick, 345), wallMat);
        Marker(g, "Exit_Goal", new Vector3(0, 1, 330));
    }

    // ---------------------------------------------------------------- helpers

    static Transform Group(GameObject root, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        return go.transform;
    }

    static Vector3 Min(float x, float y, float z) => new Vector3(x, y, z);
    static Vector3 Max(float x, float y, float z) => new Vector3(x, y, z);

    static GameObject Box(Transform parent, string name, Vector3 min, Vector3 max, Material mat,
                          bool collider = true, float yaw = 0, float roll = 0, float pitch = 0)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = (min + max) * 0.5f;
        go.transform.localScale = max - min;
        go.transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic = true;
        return go;
    }

    // Slab whose TOP surface runs from (z0,y0) to (z1,y1), centred on x.
    static void Slope(Transform parent, string name, float x, float z0, float y0, float z1, float y1, float width, Material mat)
    {
        float dz = z1 - z0, dy = y0 - y1;
        float len = Mathf.Sqrt(dz * dz + dy * dy);
        float ang = Mathf.Atan2(dy, dz) * Mathf.Rad2Deg;
        var rot = Quaternion.Euler(ang, 0, 0);
        Vector3 topMid = new Vector3(x, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f);
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.rotation = rot;
        go.transform.localScale = new Vector3(width, FloorThick, len + 0.6f);
        go.transform.position = topMid - rot * Vector3.up * (FloorThick * 0.5f);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        go.isStatic = true;
    }

    // Wall slab (z range) with a rectangular opening.
    static void WallWithHole(Transform g, string name, float z0, float z1, float x0, float x1, float y0, float y1,
                             float hx0, float hx1, float hy0, float hy1)
    {
        Box(g, name + "_L", Min(x0, y0, z0), Max(hx0, y1, z1), wallMat);
        Box(g, name + "_R", Min(hx1, y0, z0), Max(x1, y1, z1), wallMat);
        Box(g, name + "_Top", Min(hx0, hy1, z0), Max(hx1, y1, z1), wallMat);
        if (hy0 > y0 + 0.01f) Box(g, name + "_Bottom", Min(hx0, y0, z0), Max(hx1, hy0, z1), wallMat);
    }

    // Square stone pillar made from a cube (Walls material). Returns its height.
    static float Pillar(Transform g, string name, Vector3 basePos, float height)
    {
        Box(g, name, basePos + new Vector3(-0.9f, 0, -0.9f), basePos + new Vector3(0.9f, height, 0.9f), wallMat);
        Box(g, name + "_Cap", basePos + new Vector3(-1.2f, height - 0.6f, -1.2f), basePos + new Vector3(1.2f, height, 1.2f), wallMat);
        return height;
    }

    // height <= 0 keeps the given scale. Returns the final height (or -1 if it failed).
    static float Model(Transform g, GameObject src, string name, Vector3 basePos, Quaternion rot, Vector3 scale,
                       float height, bool stretchOnly)
    {
        if (src == null) return -1;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
        go.name = name;
        go.transform.SetParent(g, false);
        go.transform.rotation = rot;
        go.transform.localScale = scale;
        go.transform.position = Vector3.zero;
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) { Object.DestroyImmediate(go); return -1; }
        Bounds b = Bounds(rends);
        if (height > 0 && b.size.y > 0.001f)
        {
            float f = height / b.size.y;
            go.transform.localScale = stretchOnly ? new Vector3(scale.x, scale.y, scale.z * f) : scale * f;
            b = Bounds(rends);
        }
        go.transform.position += basePos - new Vector3(b.center.x, b.min.y, b.center.z);
        foreach (var t in go.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
        return b.size.y;
    }

    static Bounds Bounds(Renderer[] rends)
    {
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        return b;
    }

    static void Marker(Transform g, string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(g, false);
        go.transform.position = pos;
    }

    // Moves the old test geometry into a disabled holder (once). Camera, light and player stay.
    static void StashOldObjects(UnityEngine.SceneManagement.Scene scene)
    {
        if (GameObject.Find(OldName) != null) return;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == OldName && !go.activeSelf) return;

        var holder = new GameObject(OldName);
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go == holder || go.name == RootName || go.name == "Player" ||
                go.GetComponent<Camera>() != null || go.GetComponent<Light>() != null) continue;
            go.transform.SetParent(holder.transform, true);
        }
        holder.SetActive(false);
    }

    static void PlacePlayerAndCamera()
    {
        var player = GameObject.Find("Player");
        var cam = Camera.main;
        Vector3 offset = new Vector3(0.57f, -0.12f, -4.55f);   // keeps the old camera offset
        if (player != null)
        {
            player.transform.SetPositionAndRotation(new Vector3(0, 1.05f, 0), Quaternion.identity);
            if (cam != null) cam.transform.position = player.transform.position + offset;
        }
    }
}
