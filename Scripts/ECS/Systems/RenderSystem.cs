using Godot;
using System;

public static class RenderSystem
{
    public static void SyncNodes(PlayerEntity player, BossEntity boss) {
        if (player.Render.SpriteNode != null) {
            player.Render.SpriteNode.Position = player.Transform.Position;
            player.Render.SpriteNode.Scale = player.Transform.Scale; // NOVO
        }
        if (boss.Render.SpriteNode != null) {
            boss.Render.SpriteNode.Position = boss.Transform.Position;
            boss.Render.SpriteNode.Scale = boss.Transform.Scale; // NOVO: Fica Gigante!
        }
    }
}