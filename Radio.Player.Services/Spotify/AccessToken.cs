using System;
using Newtonsoft.Json;

namespace Rtvs.iRadio.Services
{
    public class AccessToken
    {
        private readonly DateTime _expirationTime;
        
        public string Value { get; }

        public string Type { get; }

        public bool IsValid => _expirationTime > DateTime.Now;

        [JsonConstructor]
        public AccessToken([JsonProperty("access_token")] string value,
                           [JsonProperty("token_type")] string type,
                           [JsonProperty("expires_in")] int expiresIn)
        {
            _expirationTime = DateTime.Now.AddSeconds(expiresIn);
            Value = value;
            Type = type;
        }
    }
}