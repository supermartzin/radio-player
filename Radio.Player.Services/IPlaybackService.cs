using System;

using Radio.Player.Models;
using Radio.Player.Services.Events;

namespace Radio.Player.Services
{
    public interface IPlaybackService
    {
        TimeSpan CurrentPosition { get; }

        TimeSpan TotalDuration { get; }

        bool IsPlaying { get; }

        event PlaybackStateEventHandler PlaybackChanged;
        
        void Play(string mediaUrl, RadioStation currentRadioStation);

        void Pause();

        void SeekForward();

        void SeekBackward();

        void SetCurrentPlayingInfo(RadioStation radioStation, Track track = null);
    }
}