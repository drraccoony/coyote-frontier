using System.Linq;
using System.Numerics;
using Content.Shared.CCVar;
using Content.Shared.Humanoid;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Maths;

namespace Content.Shared.HeightAdjust;

public sealed class HeightAdjustSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HumanoidAppearanceComponent, RequestSizeRecalcEvent>(OnRequestSizeRecalc);
    }

    /// <summary>
    /// Handles requests to recalculate an entity's size by collecting all active modifiers
    /// and applying the final combined scale.
    /// </summary>
    private void OnRequestSizeRecalc(EntityUid target, HumanoidAppearanceComponent component, ref RequestSizeRecalcEvent ev)
    {
        // Collect all size modifiers from various systems
        var getModifiersEvent = new GetSizeModifierEvent(target);
        RaiseLocalEvent(target, ref getModifiersEvent);

        // Calculate final scale by multiplying all modifiers
        float finalScale = 1.0f;
        
        // Sort by priority (lower priority applied first, so higher priority can override)
        var sortedModifiers = getModifiersEvent.Modifiers.OrderBy(m => m.Priority).ToList();
        
        foreach (var modifier in sortedModifiers)
        {
            finalScale *= modifier.Scale;
        }

        // Apply the final scale, bypassing species limits for temporary effects
        SetScale(target, finalScale, bypassLimits: true);
    }


    /// <summary>
    ///     Changes the density of fixtures and zoom of eyes based on a provided float scale
    /// </summary>
    /// <param name="uid">The entity to modify values for</param>
    /// <param name="scale">The scale to multiply values by</param>
    /// <param name="bypassLimits">Whether to bypass species min/max limits (for temporary effects)</param>
    /// <returns>True if all operations succeeded</returns>
    public bool SetScale(EntityUid uid, float scale, bool bypassLimits = false)
    {
        var succeeded = true;
        
        if (_config.GetCVar(CCVars.HeightAdjustModifiesHitbox) && EntityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
            foreach (var fixture in fixtures.Fixtures)
                _physics.SetRadius(uid, fixture.Key, fixture.Value, fixture.Value.Shape, MathF.MinMagnitude(fixture.Value.Shape.Radius * scale, 0.49f));
        else
            succeeded = false;

        if (EntityManager.HasComponent<HumanoidAppearanceComponent>(uid))
        {
            _appearance.SetHeight(uid, scale, bypassLimits: bypassLimits);
            _appearance.SetWidth(uid, scale, bypassLimits: bypassLimits);
        }
        else
            succeeded = false;

        return succeeded;
    }

    /// <summary>
    ///     Changes the density of fixtures and zoom of eyes based on a provided Vector2 scale
    /// </summary>
    /// <param name="uid">The entity to modify values for</param>
    /// <param name="scale">The scale to multiply values by (X = width, Y = height)</param>
    /// <returns>True if all operations succeeded</returns>
    public bool SetScale(EntityUid uid, Vector2 scale)
    {
        var succeeded = true;

        if (_config.GetCVar(CCVars.HeightAdjustModifiesHitbox) && EntityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
        {
            foreach (var (key, fixture) in fixtures.Fixtures)
            {
                if (fixture.Shape is PhysShapeAabb aabb)
                {
                    // For AABB, scale width more aggressively than height
                    // This makes wide entities truly wide and tall entities truly tall
                    var avg = (scale.X * 0.7f + scale.Y * 0.3f); // Favor width scaling
                    _physics.SetRadius(uid, key, fixture, fixture.Shape, MathF.MinMagnitude(fixture.Shape.Radius * avg, 0.49f));
                }
                else
                {
                    // For circular shapes, use average of width and height
                    var avg = (scale.X + scale.Y) / 2;
                    _physics.SetRadius(uid, key, fixture, fixture.Shape, MathF.MinMagnitude(fixture.Shape.Radius * avg, 0.49f));
                }
            }
        }
        else
            succeeded = false;

        if (EntityManager.HasComponent<HumanoidAppearanceComponent>(uid))
            _appearance.SetScale(uid, scale);
        else
            succeeded = false;

        return succeeded;
    }
}
