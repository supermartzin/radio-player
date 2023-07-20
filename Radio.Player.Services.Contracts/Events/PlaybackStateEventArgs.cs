using Radio.Player.Models;

namespace Radio.Player.Services.Contracts.Events
{
    public class PlaybackStateEventArgs : EventArgs
    {
        public bool IsPlaying { get; }

        public RadioStation RadioStation { get; }

        public PlaybackStateEventArgs(bool isPlaying, RadioStation radioStation)
        {
            IsPlaying = isPlaying;
            RadioStation = radioStation;
        }
    }

    public delegate void PlaybackStateEventHandler(object sender, PlaybackStateEventArgs eventArgs);
}