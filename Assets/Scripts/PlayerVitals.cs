using System.Collections.Generic;
using UnityEngine;

// Minimal "Ase" charm system: charms are data, not hardcoded behavior, so
// irekeonibudo's charm enum/set can plug straight into this without a rewrite.
[System.Serializable]
public class Charm
{
    public string charmName;
    public string description;
    public bool isActive;
}

public class PlayerVitals : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;

    public List<Charm> equippedCharms = new List<Charm>();

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0)
            HandleDefeat();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public bool HasCharm(string name)
    {
        return equippedCharms.Exists(c => c.charmName == name && c.isActive);
    }

    private void HandleDefeat()
    {
        Debug.Log("Akara-Ogun has fallen. Returning to the hub village.");
        // Hook your respawn-at-hub / game-over flow here.
    }
}
