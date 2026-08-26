using UnityEngine;

// Single source of truth for the hub/expedition-area boundary and the
// rectangular band spirits/creatures are placed within. Previously this was
// three separate magic-number sets: ExpeditionManager's villageBoundaryZ,
// and the minX/maxX/minZ/maxZ literals duplicated across BuildCreatures and
// BuildGhommids (or BuildMotherSpiritGuides) in SceneSetupWizard. Now both
// read from the same struct, so retuning the map only happens in one place.
[System.Serializable]
public struct WorldBounds
{
    [Tooltip("Z position where the hub/village ends and the expedition area begins.")]
    public float villageBoundaryZ;

    [Header("Spawn band (creatures + procedural spirit placement)")]
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    public bool Contains(Vector3 pos) =>
        pos.x >= minX && pos.x <= maxX && pos.z >= minZ && pos.z <= maxZ;

    // ogbojuode-3d's forest: village ends at Z=15, creatures/ghommids
    // scattered Z 18..62, X -18..20 (matches the old hardcoded values).
    public static WorldBounds OgbojuOdeDefaults => new WorldBounds
    {
        villageBoundaryZ = 15f,
        minX = -18f, maxX = 20f, minZ = 18f, maxZ = 62f
    };

    // irekeonibudo-3d's undersea kingdom: dock/reed-barrier at Z=12, same
    // spawn band shape as the sibling project (matches the old hardcoded
    // values there).
    public static WorldBounds IrekeOnibudoDefaults => new WorldBounds
    {
        villageBoundaryZ = 12f,
        minX = -18f, maxX = 20f, minZ = 18f, maxZ = 62f
    };
}
