using Content.Server.Body.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Log;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed class SizeManipulatorSystem : EntitySystem
{
    [Dependency] private readonly SizeManipulationSystem _sizeManipulation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SizeManipulatorComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<BulletSizeManipulatorComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnAmmoShot(EntityUid uid, SizeManipulatorComponent component, AmmoShotEvent args)
    {
        // Update all fired projectiles with the safety state from the gun
        foreach (var projectile in args.FiredProjectiles)
        {
            if (TryComp<BulletSizeManipulatorComponent>(projectile, out var bullet))
            {
                bullet.SafetyDisabled = component.SafetyDisabled;
                Dirty(projectile, bullet);
            }
        }
    }

    private void OnProjectileHit(EntityUid uid, BulletSizeManipulatorComponent component, ref ProjectileHitEvent args)
    {
        var hitEntity = args.Target;

        if (!Exists(hitEntity))
        {
            Logger.Debug("SizeManipulator: Hit entity doesn't exist");
            return;
        }

        Logger.Debug($"SizeManipulator: Projectile {ToPrettyString(uid)} hit entity {ToPrettyString(hitEntity)}, applying size change mode: {component.Mode}, safety disabled: {component.SafetyDisabled}");

        // Apply size change to the hit entity, passing the safety state
        _sizeManipulation.TryChangeSize(hitEntity, component.Mode, args.Shooter, component.SafetyDisabled);
    }
}
