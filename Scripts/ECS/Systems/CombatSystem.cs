using Godot;
using System;


public static class CombatSystem
{
    public static void ProcessCollisions(BulletData[] pool, PlayerEntity player, BossEntity boss)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].IsActive) continue;

            if (pool[i].Faction == 1 && player.Health.InvincibilityTimer <= 0)
            {
                if (pool[i].Position.DistanceTo(player.Transform.Position) < (pool[i].HitboxRadius + player.Hitbox.Radius))
                {
                    player.Health.CurrentHealth -= pool[i].Damage;
                    pool[i].IsActive = false;
                }
            }
            else if (pool[i].Faction == 0 && boss.Health.CurrentHealth > 0)
            {
                if (pool[i].Position.DistanceTo(boss.Transform.Position) < (pool[i].HitboxRadius + boss.Hitbox.Radius))
                {
                    boss.Health.CurrentHealth -= pool[i].Damage;
                    pool[i].IsActive = false;
                }
            }
        }
    }

    public static void ProcessHealth(PlayerEntity player, BossEntity boss, float delta)
    {
        // Jogador
        if (player.Health.InvincibilityTimer > 0)
        {
            player.Health.InvincibilityTimer -= delta;
            if (player.Render.SpriteNode != null)
                player.Render.SpriteNode.Visible = (Mathf.FloorToInt(player.Health.InvincibilityTimer * 10) % 2 == 0);
        }
        else if (player.Render.SpriteNode != null)
        {
            player.Render.SpriteNode.Visible = true;
        }

        if (player.Health.CurrentHealth <= 0 && player.Health.Lives > 0)
        {
            player.Health.Lives--;
            if (player.Health.Lives > 0)
            {
                player.Health.CurrentHealth = player.Health.MaxHealth;
                player.Health.InvincibilityTimer = 3.0f;
                player.Transform.Position = new Vector2(300, 700);
                GD.Print("Jogador atingido! Vidas restantes: " + player.Health.Lives);
            }
            else
            {
                GD.Print("GAME OVER!");
                if (player.Render.SpriteNode != null) player.Render.SpriteNode.Visible = false;
            }
        }

        // Chefe
        if (boss.Health.CurrentHealth <= 0 && boss.Health.Lives > 0)
        {
            boss.Health.Lives--;
            GD.Print("Chefe perdeu uma fase! HP zerado.");
            boss.Health.CurrentHealth = 500; 
        }
    }
}