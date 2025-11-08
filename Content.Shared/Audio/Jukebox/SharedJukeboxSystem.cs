using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization; // Frontier

namespace Content.Shared.Audio.Jukebox;

public abstract class SharedJukeboxSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
}

// Frontier: Shuffle & Repeat
[Serializable, NetSerializable]
public sealed class JukeboxInterfaceState(JukeboxPlaybackMode playbackMode, float volume = 0.5f) : BoundUserInterfaceState
{
    public JukeboxPlaybackMode PlaybackMode { get; set; } = playbackMode;
    public float Volume { get; set; } = volume;
}
// End Frontier: Shuffle & Repeat
