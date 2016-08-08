using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.HttpClient;
using PokemonGo.RocketAPI.Login;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        internal static WebProxy Proxy = null;

        public Rpc.Login Login;
        public Rpc.Player Player;
        public Rpc.Download Download;
        public Rpc.Inventory Inventory;
        public Rpc.Map Map;
        public Rpc.Fort Fort;
        public Rpc.Encounter Encounter;
        public Rpc.Misc Misc;

        public IApiFailureStrategy ApiFailure { get; set; }
        public ISettings Settings { get; }
        public string AuthToken { get; set; }

        public double CurrentLatitude { get; internal set; }
        public double CurrentLongitude { get; internal set; }
        public double CurrentAltitude { get; internal set; }

        public AuthType AuthType => Settings.AuthType;

        internal readonly PokemonHttpClient PokemonHttpClient;
        internal string ApiUrl { get; set; }
        internal AuthTicket AuthTicket { get; set; }

        public Client(ISettings settings, IApiFailureStrategy apiFailureStrategy)
        {
            Settings = settings;
            ApiFailure = apiFailureStrategy;

            SetProxy(settings);

            PokemonHttpClient = new PokemonHttpClient();

            Login = new Rpc.Login(this);
            Player = new Rpc.Player(this);
            Download = new Rpc.Download(this);
            Inventory = new Rpc.Inventory(this);
            Map = new Rpc.Map(this);
            Fort = new Rpc.Fort(this);
            Encounter = new Rpc.Encounter(this);
            Misc = new Rpc.Misc(this);

            Player.SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAltitude);
        }

        private void SetProxy(ISettings settings)
        {
            if (settings.UseProxy && !string.IsNullOrWhiteSpace(settings.UseProxyHost) && !string.IsNullOrWhiteSpace(settings.UseProxyPort))
            {
                var proxyHost = settings.UseProxyHost;
                if (!proxyHost.StartsWith("http://") && !proxyHost.StartsWith("https://"))
                {
                    proxyHost = "http://" + proxyHost;
                }
                int proxyPort;
                if (int.TryParse(settings.UseProxyPort, out proxyPort))
                {
                    proxyHost = proxyHost + ":" + proxyPort;
                }
                Proxy = new WebProxy(proxyHost, false);

                if (settings.UseProxyAuthentication && !string.IsNullOrWhiteSpace(settings.UseProxyUsername) &&
                    !string.IsNullOrWhiteSpace(settings.UseProxyPassword))
                {
                    Proxy.Credentials = new NetworkCredential { UserName = settings.UseProxyUsername, Password = settings.UseProxyPassword };
                }
            }
        }
    }
}