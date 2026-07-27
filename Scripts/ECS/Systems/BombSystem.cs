using Godot;
using System;
public static class BombSystem {
    public static void ProcessBombs(PlayerEntity player, BulletData[] bulletPool, float delta) {
        // 1. ATIVAÇÃO DA BOMBA
        // Se apertou X, a bomba não está ativa no momento, e ainda tem bombas na mochila
        if (player.Input.IsBombing && !player.Bomb.IsActive && player.Bomb.BombsLeft > 0) {
            player.Bomb.IsActive = true;
            player.Bomb.BombsLeft--;
            player.Bomb.Timer = player.Bomb.Duration;
            GD.Print("BOMBA ATIVADA! Restam: " + player.Bomb.BombsLeft);
        }

        // 2. EFEITO DA BOMBA (Aura de Luz em Ação)
        if (player.Bomb.IsActive) {
            player.Bomb.Timer -= delta;

            // Limpa qualquer bala inimiga que encostar na aura de luz do jogador!
            for (int i = 0; i < bulletPool.Length; i++) {
                // Faction 1 = Balas do Chefe
                if (bulletPool[i].IsActive && bulletPool[i].Faction == 1) { 
                    float distanceToBullet = player.Transform.Position.DistanceTo(bulletPool[i].Position);
                    
                    if (distanceToBullet <= player.Bomb.Radius) {
                        bulletPool[i].IsActive = false; // Neutraliza a bala instantaneamente
                    }
                }
            }

            // Desativa a bomba quando o tempo (5 segundos) acabar
            if (player.Bomb.Timer <= 0) {
                player.Bomb.IsActive = false;
            }
        }
    }
}