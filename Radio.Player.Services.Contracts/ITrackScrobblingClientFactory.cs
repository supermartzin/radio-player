namespace Radio.Player.Services.Contracts
{
    public interface ITrackScrobblingClientFactory<out T>
    {
        T CreateTrackScrobblingClient();
    }
}