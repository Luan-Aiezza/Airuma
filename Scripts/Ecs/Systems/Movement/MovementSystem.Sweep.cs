using System;
using Godot;

/// <summary>Movement of the bosses: the sweep of the great jet.</summary>
public static partial class MovementSystem
{
    /// <summary>The sweep: the height it sweeps at, how fast it travels, and the gaps the jet leaves at the end of each pass (at the right edge in the first, at the left in the second).</summary>
    public const float SweepY = 195, SweepTravelSpeed = 800, SweepChargeSeconds = 1.0f, SweepCharge2Seconds = 0.8f, SweepPassSpeed = 190,
                       SweepRestSeconds = 4.0f, SweepFirstGap = 130, SweepSecondGap = 80;

    private static void SweepStep(BossEntity boss, Rect2 bounds, float delta)
    {
        float r = boss.Hitbox.Radius, left = bounds.Position.X + r + 4, right = bounds.End.X - r - 4;
        float middle = (bounds.Position.X + bounds.End.X) / 2;
        var p = boss.Transform.Position;
        boss.Sweep.Clock += delta;
        // Where each pass ends: the jet stops short of the far edge, leaving a gap for the boat.
        float firstEnd = Math.Clamp(bounds.End.X - SweepFirstGap * boss.Sweep.GapScale - SweepBrain.BeamHalfWidth, left, right);
        float secondEnd = Math.Clamp(bounds.Position.X + SweepSecondGap * boss.Sweep.GapScale + SweepBrain.BeamHalfWidth, left, right);

        switch (boss.Sweep.State)
        {
            case SweepState.ToLeft:
                boss.Transform.Position = p.MoveToward(new Vector2(left, SweepY), SweepTravelSpeed * delta);
                if (boss.Transform.Position.DistanceTo(new Vector2(left, SweepY)) < 0.5f) Enter(boss, SweepState.Charge);
                break;
            case SweepState.ToRight:
                boss.Transform.Position = p.MoveToward(new Vector2(right, SweepY), SweepTravelSpeed * delta);
                if (boss.Transform.Position.DistanceTo(new Vector2(right, SweepY)) < 0.5f) Enter(boss, SweepState.Charge2);
                break;
            case SweepState.Charge:
            case SweepState.Charge2:
                if (boss.Sweep.Clock >= (boss.Sweep.State == SweepState.Charge ? SweepChargeSeconds : SweepCharge2Seconds))
                    Enter(boss, boss.Sweep.State == SweepState.Charge ? SweepState.Pass : SweepState.Pass2);
                break;
            case SweepState.Pass:
            {
                float x = Math.Min(firstEnd, p.X + SweepPassSpeed * boss.Sweep.SpeedScale * delta);
                boss.Transform.Position = new Vector2(x, SweepY);
                if (x >= firstEnd) Enter(boss, SweepState.ToRight);
                break;
            }
            case SweepState.Pass2:
            {
                float x = Math.Max(secondEnd, p.X - SweepPassSpeed * boss.Sweep.SpeedScale * delta);
                boss.Transform.Position = new Vector2(x, SweepY);
                if (x <= secondEnd) Enter(boss, SweepState.Return);
                break;
            }
            case SweepState.Return:
                boss.Transform.Position = p.MoveToward(new Vector2(middle, SweepY), SweepTravelSpeed * 0.5f * delta);
                if (boss.Transform.Position.DistanceTo(new Vector2(middle, SweepY)) < 0.5f) Enter(boss, SweepState.Rest);
                break;
            case SweepState.Rest:
                // A few seconds, in the middle, for the boat to hit it; and then again.
                if (boss.Sweep.Clock >= SweepRestSeconds) Enter(boss, SweepState.ToLeft);
                break;
        }
        boss.Sweep.BeamWarn = boss.Sweep.State == SweepState.Charge || boss.Sweep.State == SweepState.Charge2;
        boss.Sweep.BeamOn = boss.Sweep.State == SweepState.Pass || boss.Sweep.State == SweepState.Pass2;
    }

    private static void Enter(BossEntity boss, SweepState state) { boss.Sweep.State = state; boss.Sweep.Clock = 0; }
}
