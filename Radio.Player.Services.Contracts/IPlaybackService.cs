using Radio.Player.Models;
using Radio.Player.Services.Contracts.Events;

namespace Radio.Player.Services.Contracts;

public interface IPlaybackService
{
    TimeSpan CurrentPosition { get; }

    TimeSpan TotalDuration { get; }

    bool IsPlaying { get; }

    event PlaybackStateEventHandler PlaybackChanged;
        
    void Play(RadioStation currentRadioStation);

    void Pause();

    void SeekForward();

    void SeekBackward();

    void SetCurrentPlayingInfo(RadioStation radioStation, Track? track = default);
}