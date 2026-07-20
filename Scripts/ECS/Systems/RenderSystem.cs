using Godot;
using System;

public static class RenderSystem
{
    public static void SyncNodes(PlayerEntity player, BossEntity boss)
    {
        if (player.Render.SpriteNode != null)
            player.Render.SpriteNode.Position = player.Transform.Position;

        if (boss.Render.SpriteNode != null)
            boss.Render.SpriteNode.Position = boss.Transform.Position;
    }
}