/// <summary>Health, lives and the seconds of invincibility after a hit.</summary>
public struct HealthComponent
{
    public int CurrentHealth;
    public int MaxHealth;
    public int Lives;
    public float InvincibilityTimer;
    public HealthComponent() { CurrentHealth = 1; MaxHealth = 1; Lives = 1; InvincibilityTimer = 0; }
    public HealthComponent(int currentHealth, int maxHealth, int lives, float invincibilityTimer)
    { CurrentHealth = currentHealth; MaxHealth = maxHealth; Lives = lives; InvincibilityTimer = invincibilityTimer; }
}
