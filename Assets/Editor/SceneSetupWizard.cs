using UnityEngine;
using UnityEditor;

// Menu: Ogboju Ode > Build Entire Universe
// The real version of the "all-in-one master script" idea: one menu click
// assembles a forest, a safe hub village, all three creatures, the player,
// camera and lighting — using only real Unity API calls, so it actually
// compiles and runs. Swap primitives for Meshy/Tripo assets later; the
// GameObject names below are stable anchors you can re-parent real meshes
// onto without touching this script.
public static class SceneSetupWizard
{
    [MenuItem("Ogboju Ode/Build Entire Universe")]
    public static void BuildEverything()
    {
        EnsureTags();

        GameObject root = new GameObject("Igbo_Irunmole_World");

        BuildForest(root.transform);
        BuildHubVillage(root.transform);
        GameObject player = BuildPlayer(root.transform);
        BuildCreatures(root.transform, player.transform);
        BuildOstrichKing(root.transform);
        BuildGhommids(root.transform);
        BuildCameraAndLighting(player.transform);
        BuildManagers(root.transform, player.transform);

        Debug.Log("World built: forest, hub village, three creatures (Agbako, Ijamba, Eru), Ostrich-King, ghommid spirits, player, camera, lighting, expedition/wisdom tracking.");
    }

    private static void EnsureTags()
    {
        AddTagIfMissing("Player");
        AddTagIfMissing("Enemy");
    }

    private static void AddTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    // --- Forest: the dangerous expanse outside the village, per the source
    // material's "ancient, dense rainforest with reality-warping terrain" ---
    private static void BuildForest(Transform parent)
    {
        GameObject forest = new GameObject("Forest_Igbo_Irunmole");
        forest.transform.parent = parent;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Forest_Floor";
        floor.transform.parent = forest.transform;
        floor.transform.position = new Vector3(0f, 0f, 40f);
        floor.transform.localScale = new Vector3(12f, 1f, 8f);
        SetColor(floor, new Color(0.22f, 0.11f, 0.07f));

        for (int i = 0; i < 20; i++)
        {
            GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tree.name = "Mahogany_Tree_" + i;
            tree.transform.parent = forest.transform;
            tree.transform.position = new Vector3(Random.Range(-45f, 45f), 4f, Random.Range(10f, 75f));
            tree.transform.localScale = new Vector3(Random.Range(1.5f, 2.5f), Random.Range(3.5f, 5.5f), Random.Range(1.5f, 2.5f));
            SetColor(tree, new Color(0.14f, 0.09f, 0.05f));
        }

        // Glowing spiritual flora — small markers scattered on the forest floor.
        for (int i = 0; i < 15; i++)
        {
            GameObject flora = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flora.name = "Spirit_Fungus_" + i;
            flora.transform.parent = forest.transform;
            flora.transform.position = new Vector3(Random.Range(-40f, 40f), 0.3f, Random.Range(10f, 75f));
            flora.transform.localScale = Vector3.one * 0.4f;
            SetColor(flora, new Color(0.3f, 0.9f, 0.6f));
        }
    }

    // --- Hub village: the safe base the book's expeditions launch from and
    // return to. Clay huts, carved pillars, bonfire, defensive spikes. ---
    private static void BuildHubVillage(Transform parent)
    {
        GameObject village = new GameObject("Hub_Village");
        village.transform.parent = parent;

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Village_Ground";
        ground.transform.parent = village.transform;
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        SetColor(ground, new Color(0.55f, 0.35f, 0.2f));

        Vector3[] hutPositions =
        {
            new Vector3(-10f, 1.5f, -5f),
            new Vector3(10f, 1.5f, -5f),
            new Vector3(-10f, 1.5f, 8f),
            new Vector3(10f, 1.5f, 8f),
        };
        foreach (Vector3 pos in hutPositions)
        {
            GameObject hut = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hut.name = "Clay_Hut";
            hut.transform.parent = village.transform;
            hut.transform.position = pos;
            hut.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);
            SetColor(hut, new Color(0.6f, 0.3f, 0.15f));

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Thatched_Roof";
            roof.transform.parent = hut.transform;
            roof.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            roof.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            roof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            SetColor(roof, new Color(0.4f, 0.3f, 0.1f));
        }

        // Carved wooden pillars (Arugba) ringing the central fire.
        for (int i = 0; i < 6; i++)
        {
            float angle = i * (360f / 6f) * Mathf.Deg2Rad;
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Carved_Pillar_" + i;
            pillar.transform.parent = village.transform;
            pillar.transform.position = new Vector3(Mathf.Cos(angle) * 6f, 1.5f, Mathf.Sin(angle) * 6f);
            pillar.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
            SetColor(pillar, new Color(0.35f, 0.22f, 0.1f));
        }

        GameObject bonfire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bonfire.name = "Central_Bonfire";
        bonfire.transform.parent = village.transform;
        bonfire.transform.position = new Vector3(0f, 0.5f, 0f);
        bonfire.transform.localScale = Vector3.one * 1.2f;
        SetColor(bonfire, new Color(1f, 0.45f, 0.1f));

        Light fireLight = bonfire.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.55f, 0.2f);
        fireLight.range = 15f;
        fireLight.intensity = 2f;

        // Defensive perimeter spikes marking the village boundary, facing the forest.
        for (int i = 0; i < 10; i++)
        {
            GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = "Defensive_Spike_" + i;
            spike.transform.parent = village.transform;
            float x = Mathf.Lerp(-20f, 20f, i / 9f);
            spike.transform.position = new Vector3(x, 0.75f, 15f);
            spike.transform.localScale = new Vector3(0.15f, 0.75f, 0.15f);
            SetColor(spike, new Color(0.3f, 0.2f, 0.1f));
        }
    }

    private static GameObject BuildPlayer(Transform parent)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player_Akara_Ogun";
        player.tag = "Player";
        player.transform.parent = parent;
        player.transform.position = new Vector3(0f, 1f, -10f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerVitals>();
        YorubaHunterController hunter = player.AddComponent<YorubaHunterController>();
        SetColor(player, new Color(0.1f, 0.2f, 0.4f));

        GameObject muzzle = new GameObject("Musket_Muzzle");
        muzzle.transform.parent = player.transform;
        muzzle.transform.localPosition = new Vector3(0f, 0.5f, 0.8f);
        hunter.musketMuzzle = muzzle.transform;

        return player;
    }

    // Places one of each creature type in the forest, at increasing distance
    // from the village so Eru (fastest, weakest) is the closer encounter and
    // Agbako/Ijamba guard the depths — mirrors the book's escalating danger.
    private static void BuildCreatures(Transform parent, Transform playerTransform)
    {
        SpawnCreature(parent, CreatureAI.CreatureType.Eru, new Vector3(-8f, 2f, 30f), new Color(0.2f, 0.5f, 0.2f), new Vector3(1.5f, 2.5f, 1.5f));
        SpawnCreature(parent, CreatureAI.CreatureType.Ijamba, new Vector3(10f, 3f, 50f), new Color(0.4f, 0.1f, 0.1f), new Vector3(3.5f, 4.5f, 3.5f));
        SpawnCreature(parent, CreatureAI.CreatureType.Agbako, new Vector3(0f, 3.5f, 68f), new Color(0.05f, 0.15f, 0.05f), new Vector3(3f, 6f, 3f));
    }

    private static void SpawnCreature(Transform parent, CreatureAI.CreatureType type, Vector3 position, Color color, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Creature_" + type;
        go.tag = "Enemy";
        go.transform.parent = parent;
        go.transform.position = position;
        go.transform.localScale = scale;
        CreatureAI ai = go.AddComponent<CreatureAI>();
        CreatureAI.ApplyPreset(ai, type);
        SetColor(go, color);
    }

    // The Ostrich-King (Oba Eye) sits deepest in the forest, past all three
    // creatures — the book's climactic riddle encounter, not a rush target.
    private static void BuildOstrichKing(Transform parent)
    {
        GameObject king = new GameObject("Ostrich_King_Oba_Eye");
        king.transform.parent = parent;
        king.transform.position = new Vector3(0f, 2f, 85f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "OstrichKing_Body";
        body.transform.parent = king.transform;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1.5f, 2.5f, 1.5f);
        SetColor(body, new Color(0.6f, 0.55f, 0.3f));

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "OstrichKing_Head_Crowned";
        head.transform.parent = king.transform;
        head.transform.localPosition = new Vector3(0f, 3f, 0f);
        head.transform.localScale = Vector3.one * 1.2f;
        SetColor(head, new Color(0.75f, 0.6f, 0.3f));

        king.AddComponent<RiddleGiver>();
        OstrichKingBoss boss = king.AddComponent<OstrichKingBoss>();

        RiddleGiver riddle = king.GetComponent<RiddleGiver>();
        riddle.riddleText = "I am king above the neck, beast below it. What manner of thing am I?";
        riddle.correctAnswerHint = "a creature of two natures / the Ostrich-King himself";
        riddle.wisdomReward = 50;
    }

    // Ghommids (Elere): small, mischievous, non-hostile — scattered through
    // the forest rather than guarding the path like the three creatures.
    private static void BuildGhommids(Transform parent)
    {
        Vector3[] positions =
        {
            new Vector3(-15f, 0.5f, 22f),
            new Vector3(18f, 0.5f, 40f),
            new Vector3(-5f, 0.5f, 58f),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject ghommid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ghommid.name = "Ghommid_Spirit_" + i;
            ghommid.transform.parent = parent;
            ghommid.transform.position = positions[i];
            ghommid.transform.localScale = Vector3.one * 0.8f;
            SetColor(ghommid, new Color(0.4f, 0.7f, 0.3f));

            ghommid.AddComponent<RiddleGiver>();
            ghommid.AddComponent<GhommidSpirit>();

            RiddleGiver riddle = ghommid.GetComponent<RiddleGiver>();
            riddle.riddleText = "What grows taller the more you cut it down?";
            riddle.correctAnswerHint = "a shadow (as evening falls) / the forest itself";
            riddle.wisdomReward = 10;
        }
    }

    private static void BuildManagers(Transform parent, Transform playerTransform)
    {
        GameObject managers = new GameObject("Managers");
        managers.transform.parent = parent;

        managers.AddComponent<WisdomTracker>();

        ExpeditionManager expedition = managers.AddComponent<ExpeditionManager>();
        expedition.player = playerTransform;
    }

    private static void BuildCameraAndLighting(Transform playerTransform)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.SetParent(playerTransform);
            cam.transform.localPosition = new Vector3(0f, 6f, -8f);
            cam.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
        }

        GameObject moonlight = new GameObject("Moonlight");
        Light light = moonlight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.45f, 0.5f, 0.65f);
        light.intensity = 0.6f;
        moonlight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void SetColor(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.color = color;
        r.sharedMaterial = mat;
    }
}
