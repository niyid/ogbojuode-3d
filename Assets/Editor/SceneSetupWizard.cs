using UnityEngine;
using UnityEditor;

// Menu: Ogboju Ode > Build Entire Universe
//
// Loads real imported assets where available, falling back to colored
// primitives for anything not yet imported — so this keeps working at every
// stage of asset production, not just once everything is in place.
//
// EXPECTED FOLDER LAYOUT (see README for the import commands that create this):
//   Assets/Models/Characters/tribal_warrior_3d_model/   -> Akara-Ogun
//   Assets/Models/Characters/agbako_creature_3d_model/  -> Agbako
//   Assets/Models/Characters/brute_giant_3d_model/      -> Ijamba
//   Assets/Models/Characters/creature_eru_3d_model/     -> Eru
//   Assets/Models/Characters/ostrich_king_3d_model/     -> Ostrich-King
//   Assets/Models/Characters/forest_spirit_3d_model/    -> Ghommid
//   Assets/Models/Characters/musket/                    -> Musket prop
//   Assets/Models/Props/pillar-wood.fbx                 -> Carved pillar
//   Assets/Models/Props/blade.fbx                        -> Machete
//   Assets/Models/Props/wall-wood.fbx                    -> Hut wall
//   Assets/Models/Props/roof-high-point.fbx              -> Hut roof
//   Assets/Models/Props/tree_oak_dark.fbx                -> Mahogany tree
//   Assets/Models/Props/mushroom_redGroup.fbx            -> Spirit fungus
//   Assets/Models/Props/campfire-pit.fbx                 -> Central bonfire
//   Assets/Models/Props/fence-fortified.fbx              -> Defensive spike line
public static class SceneSetupWizard
{
    // --- Asset locations -----------------------------------------------
    private const string CharRoot = "Assets/Models/Characters/";
    private const string PropRoot = "Assets/Models/Props/";

    private static readonly string AkaraOgunFolder = CharRoot + "tribal_warrior_3d_model";
    private static readonly string AgbakoFolder = CharRoot + "agbako_creature_3d_model";
    private static readonly string IjambaFolder = CharRoot + "brute_giant_3d_model";
    private static readonly string EruFolder = CharRoot + "creature_eru_3d_model";
    private static readonly string OstrichKingFolder = CharRoot + "ostrich_king_3d_model";
    private static readonly string GhommidFolder = CharRoot + "forest_spirit_3d_model";
    private static readonly string MusketFolder = CharRoot + "musket";

    private static readonly string PillarPath = PropRoot + "pillar-wood.fbx";
    private static readonly string MachetePath = PropRoot + "blade.fbx";
    private static readonly string HutWallPath = PropRoot + "wall-wood.fbx";
    private static readonly string HutRoofPath = PropRoot + "roof-high-point.fbx";
    private static readonly string TreePath = PropRoot + "tree_oak_dark.fbx";
    private static readonly string FungusPath = PropRoot + "mushroom_redGroup.fbx";
    private static readonly string BonfirePath = PropRoot + "campfire-pit.fbx";
    private static readonly string SpikePath = PropRoot + "fence-fortified.fbx";

    [MenuItem("Ogboju Ode/Build Entire Universe")]
    public static void BuildEverything()
    {
        EnsureTags();

        GameObject root = new GameObject("Igbo_Irunmole_World");

        BuildForest(root.transform);
        BuildHubVillage(root.transform);
        GameObject player = BuildPlayer(root.transform);
        BuildCreatures(root.transform);
        BuildOstrichKing(root.transform);
        BuildGhommids(root.transform);
        BuildCameraAndLighting(player.transform);
        BuildManagers(root.transform, player.transform);

        Debug.Log("World built. Real assets loaded where present in Assets/Models/; " +
                  "colored primitives used as placeholders for anything not yet imported.");
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

    // --- Asset loading helpers ------------------------------------------

    // Tripo export zips contain the model under an internal filename we don't
    // control, so search the whole folder for the first model asset rather
    // than assuming a specific file name.
    private static GameObject LoadFirstModelInFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath)) return null;

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static GameObject LoadPropModel(string exactPath)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(exactPath);
    }

    // Instantiates the real model as a child of parent if found; otherwise
    // creates a colored primitive placeholder with the same name, so the
    // rest of the hierarchy (scripts, tags, colliders on the root) is
    // identical either way.
    private static GameObject InstantiateModelOrPrimitive(
        GameObject modelAsset, string name, Transform parent,
        PrimitiveType fallbackShape, Color fallbackColor, Vector3 fallbackScale)
    {
        if (modelAsset != null)
        {
            GameObject instance = (GameObject)Object.Instantiate(modelAsset, Vector3.zero, Quaternion.identity);
            instance.name = name;
            if (parent != null) instance.transform.SetParent(parent, false);
            return instance;
        }

        GameObject placeholder = GameObject.CreatePrimitive(fallbackShape);
        placeholder.name = name + "_PLACEHOLDER";
        if (parent != null) placeholder.transform.SetParent(parent, false);
        placeholder.transform.localScale = fallbackScale;
        SetColor(placeholder, fallbackColor);
        return placeholder;
    }

    // --- Forest -----------------------------------------------------------
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

        GameObject treeModel = LoadPropModel(TreePath);
        for (int i = 0; i < 20; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-45f, 45f), 0f, Random.Range(10f, 75f));
            GameObject tree = InstantiateModelOrPrimitive(
                treeModel, "Mahogany_Tree_" + i, forest.transform,
                PrimitiveType.Cylinder, new Color(0.14f, 0.09f, 0.05f),
                new Vector3(Random.Range(1.5f, 2.5f), Random.Range(3.5f, 5.5f), Random.Range(1.5f, 2.5f)));
            tree.transform.position = treeModel != null ? pos : pos + new Vector3(0f, 4f, 0f);
        }

        GameObject fungusModel = LoadPropModel(FungusPath);
        for (int i = 0; i < 15; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-40f, 40f), 0.3f, Random.Range(10f, 75f));
            GameObject flora = InstantiateModelOrPrimitive(
                fungusModel, "Spirit_Fungus_" + i, forest.transform,
                PrimitiveType.Sphere, new Color(0.3f, 0.9f, 0.6f), Vector3.one * 0.4f);
            flora.transform.position = pos;
        }
    }

    // --- Hub village --------------------------------------------------------
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

        GameObject wallModel = LoadPropModel(HutWallPath);
        GameObject roofModel = LoadPropModel(HutRoofPath);
        Vector3[] hutPositions =
        {
            new Vector3(-10f, 0f, -5f),
            new Vector3(10f, 0f, -5f),
            new Vector3(-10f, 0f, 8f),
            new Vector3(10f, 0f, 8f),
        };
        foreach (Vector3 pos in hutPositions)
        {
            GameObject hut = new GameObject("Clay_Hut");
            hut.transform.parent = village.transform;
            hut.transform.position = pos;

            GameObject wall = InstantiateModelOrPrimitive(
                wallModel, "Hut_Wall", hut.transform,
                PrimitiveType.Cylinder, new Color(0.6f, 0.3f, 0.15f), new Vector3(2.5f, 1.5f, 2.5f));
            if (wallModel == null) wall.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            GameObject roof = InstantiateModelOrPrimitive(
                roofModel, "Thatched_Roof", hut.transform,
                PrimitiveType.Cube, new Color(0.4f, 0.3f, 0.1f), new Vector3(0.6f, 0.4f, 0.6f));
            roof.transform.localPosition = roofModel != null ? new Vector3(0f, 2.5f, 0f) : new Vector3(0f, 2.8f, 0f);
            if (roofModel == null) roof.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        }

        GameObject pillarModel = LoadPropModel(PillarPath);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * (360f / 6f) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 6f, 0f, Mathf.Sin(angle) * 6f);
            GameObject pillar = InstantiateModelOrPrimitive(
                pillarModel, "Carved_Pillar_" + i, village.transform,
                PrimitiveType.Cylinder, new Color(0.35f, 0.22f, 0.1f), new Vector3(0.4f, 1.5f, 0.4f));
            pillar.transform.position = pos;
        }

        GameObject bonfireModel = LoadPropModel(BonfirePath);
        GameObject bonfire = InstantiateModelOrPrimitive(
            bonfireModel, "Central_Bonfire", village.transform,
            PrimitiveType.Sphere, new Color(1f, 0.45f, 0.1f), Vector3.one * 1.2f);
        bonfire.transform.position = new Vector3(0f, 0.5f, 0f);

        Light fireLight = bonfire.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.55f, 0.2f);
        fireLight.range = 15f;
        fireLight.intensity = 2f;

        GameObject spikeModel = LoadPropModel(SpikePath);
        for (int i = 0; i < 10; i++)
        {
            float x = Mathf.Lerp(-20f, 20f, i / 9f);
            GameObject spike = InstantiateModelOrPrimitive(
                spikeModel, "Defensive_Spike_" + i, village.transform,
                PrimitiveType.Cylinder, new Color(0.3f, 0.2f, 0.1f), new Vector3(0.15f, 0.75f, 0.15f));
            spike.transform.position = new Vector3(x, 0f, 15f);
        }
    }

    // --- Player -------------------------------------------------------------
    private static GameObject BuildPlayer(Transform parent)
    {
        GameObject player = new GameObject("Player_Akara_Ogun");
        player.tag = "Player";
        player.transform.parent = parent;
        player.transform.position = new Vector3(0f, 1f, -10f);
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerVitals>();
        YorubaHunterController hunter = player.AddComponent<YorubaHunterController>();

        GameObject playerModel = LoadFirstModelInFolder(AkaraOgunFolder);
        InstantiateModelOrPrimitive(
            playerModel, "Akara_Ogun_Model", player.transform,
            PrimitiveType.Capsule, new Color(0.1f, 0.2f, 0.4f), Vector3.one);

        GameObject muzzle = new GameObject("Musket_Muzzle");
        muzzle.transform.parent = player.transform;
        muzzle.transform.localPosition = new Vector3(0f, 0.5f, 0.8f);
        hunter.musketMuzzle = muzzle.transform;

        GameObject musketModel = LoadFirstModelInFolder(MusketFolder);
        if (musketModel != null)
        {
            GameObject musket = InstantiateModelOrPrimitive(
                musketModel, "Musket_Prop", muzzle.transform,
                PrimitiveType.Cube, Color.gray, Vector3.one * 0.1f);
            musket.transform.localPosition = Vector3.zero;
        }

        GameObject macheteModel = LoadPropModel(MachetePath);
        if (macheteModel != null)
        {
            GameObject machete = InstantiateModelOrPrimitive(
                macheteModel, "Machete_Prop", player.transform,
                PrimitiveType.Cube, Color.gray, Vector3.one * 0.1f);
            machete.transform.localPosition = new Vector3(0.3f, 0.8f, 0f);
        }

        return player;
    }

    // --- Combat creatures ------------------------------------------------------
    private static void BuildCreatures(Transform parent)
    {
        SpawnCreature(parent, CreatureAI.CreatureType.Eru, EruFolder,
            new Vector3(-8f, 0f, 30f), new Color(0.2f, 0.5f, 0.2f), new Vector3(1.5f, 2.5f, 1.5f));
        SpawnCreature(parent, CreatureAI.CreatureType.Ijamba, IjambaFolder,
            new Vector3(10f, 0f, 50f), new Color(0.4f, 0.1f, 0.1f), new Vector3(3.5f, 4.5f, 3.5f));
        SpawnCreature(parent, CreatureAI.CreatureType.Agbako, AgbakoFolder,
            new Vector3(0f, 0f, 68f), new Color(0.05f, 0.15f, 0.05f), new Vector3(3f, 6f, 3f));
    }

    private static void SpawnCreature(Transform parent, CreatureAI.CreatureType type, string modelFolder,
        Vector3 position, Color fallbackColor, Vector3 fallbackScale)
    {
        GameObject go = new GameObject("Creature_" + type);
        go.tag = "Enemy";
        go.transform.parent = parent;
        go.transform.position = position;

        CreatureAI ai = go.AddComponent<CreatureAI>();
        CreatureAI.ApplyPreset(ai, type);

        GameObject model = LoadFirstModelInFolder(modelFolder);
        InstantiateModelOrPrimitive(
            model, type + "_Model", go.transform,
            PrimitiveType.Cube, fallbackColor, fallbackScale);

        // Placeholder primitives get a Collider for free, but imported FBX
        // models don't — and either way the collider ends up on the child
        // model object, not on `go` where CreatureAI/IDamageable lives.
        // Add an explicit collider on the root so melee/musket hits always
        // have something reliable to register against, regardless of which
        // model (or lack of one) is present.
        AddRootHitCollider(go, fallbackScale);
    }

    // Sized off the same fallbackScale used for the placeholder, so the hit
    // volume roughly matches the creature's silhouette either way.
    private static void AddRootHitCollider(GameObject root, Vector3 approxSize)
    {
        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.height = Mathf.Max(approxSize.y, 1f);
        col.radius = Mathf.Max(approxSize.x, approxSize.z, 0.5f) * 0.5f;
        col.center = new Vector3(0f, col.height * 0.5f, 0f);
    }

    // --- Ostrich-King ------------------------------------------------------------
    private static void BuildOstrichKing(Transform parent)
    {
        GameObject king = new GameObject("Ostrich_King_Oba_Eye");
        king.transform.parent = parent;
        king.transform.position = new Vector3(0f, 0f, 85f);

        king.AddComponent<OstrichKingBoss>();
        RiddleGiver riddle = king.AddComponent<RiddleGiver>();
        riddle.riddleText = "I am king above the neck, beast below it. What manner of thing am I?";
        riddle.correctAnswerHint = "a creature of two natures / the Ostrich-King himself";
        riddle.wisdomReward = 50;

        GameObject model = LoadFirstModelInFolder(OstrichKingFolder);
        if (model != null)
        {
            InstantiateModelOrPrimitive(model, "Ostrich_King_Model", king.transform,
                PrimitiveType.Capsule, new Color(0.6f, 0.55f, 0.3f), Vector3.one);
        }
        else
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "OstrichKing_Body_PLACEHOLDER";
            body.transform.parent = king.transform;
            body.transform.localScale = new Vector3(1.5f, 2.5f, 1.5f);
            SetColor(body, new Color(0.6f, 0.55f, 0.3f));

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "OstrichKing_Head_PLACEHOLDER";
            head.transform.parent = king.transform;
            head.transform.localPosition = new Vector3(0f, 3f, 0f);
            head.transform.localScale = Vector3.one * 1.2f;
            SetColor(head, new Color(0.75f, 0.6f, 0.3f));
        }

        // Same reasoning as the forest creatures: put a reliable collider on
        // the root where OstrichKingBoss's IDamageable actually lives.
        AddRootHitCollider(king, new Vector3(2f, 4f, 2f));
    }

    // --- Ghommids ------------------------------------------------------------
    private static void BuildGhommids(Transform parent)
    {
        Vector3[] positions =
        {
            new Vector3(-15f, 0f, 22f),
            new Vector3(18f, 0f, 40f),
            new Vector3(-5f, 0f, 58f),
        };

        GameObject model = LoadFirstModelInFolder(GhommidFolder);

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject ghommid = new GameObject("Ghommid_Spirit_" + i);
            ghommid.transform.parent = parent;
            ghommid.transform.position = positions[i];

            ghommid.AddComponent<GhommidSpirit>();
            RiddleGiver riddle = ghommid.AddComponent<RiddleGiver>();
            riddle.riddleText = "What grows taller the more you cut it down?";
            riddle.correctAnswerHint = "a shadow (as evening falls) / the forest itself";
            riddle.wisdomReward = 10;

            InstantiateModelOrPrimitive(
                model, "Ghommid_Model", ghommid.transform,
                PrimitiveType.Sphere, new Color(0.4f, 0.7f, 0.3f), Vector3.one * 0.8f);
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
