using Godot;
using System;

public static class UISystem
{
    public static void UpdateUI(
        PlayerEntity player, 
        BossEntity boss, 
        WindState wind, 
        Label livesLabel, 
        Label powerLabel, 
        Label windLabel, 
        Label bombsLabel,
        Label bossHpLabel)
    {
        // 1. Atualiza Informações do Jogador
        if (player != null && livesLabel != null && powerLabel != null)
        {
            livesLabel.Text = $"Lives of the Hero: {player.Health.Lives}";
            powerLabel.Text = $"Fire Power: Level {player.Weapon.PowerLevel}";
        }

        if (bombsLabel != null) {
            bombsLabel.Text = "Bombs: " + player.Bomb.BombsLeft;
        }

        // 2. Atualiza Informações do Boss
        if (boss != null && bossHpLabel != null)
        {
            // Se a vida for <= 0, mostra 0 para não exibir valores negativos bizarros
            int displayHp = Mathf.Max(0, boss.Health.CurrentHealth);
            bossHpLabel.Text = $"Boss HP: {displayHp}";
        }

        // 3. Atualiza Informações do Vento Global
        if (windLabel != null)
        {
            string strengthText = wind.Strength switch
            {
                0f => "None",
                25f => "Weak",
                50f => "Average",
                75f => "Strong",
                _ => "Unknown"
            };
            
            // Formatando o vetor para texto legível
            string dirText = GetDirectionName(wind.Direction);
            
            windLabel.Text = $"Wind: {dirText} ({strengthText})";
        }
    }

    // Método auxiliar para transformar o Vetor2 em Texto de Bússola
    private static string GetDirectionName(Vector2 dir)
    {
        if (dir == Vector2.Zero) return "Center";
        if (dir.X > 0) return "East";
        if (dir.X < 0) return "West";
        if (dir.Y > 0) return "South";
        if (dir.Y < 0) return "North";
        return "Mixed";
    }
}