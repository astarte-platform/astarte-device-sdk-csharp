using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AstarteDeviceSDKCSharp
{
    public class Device
    {

        string DeviceId;
        string Realm;
        string CredentialSecret;
        string PairingBaseUrl;
        string PersistencyDir;
        bool IgnoreSSLErrors;
        bool IsConnected = false;
        IMqttClient? mqttClient;
        MqttFactory? mqttFactory;
        PairingHandler pairingHandler = new();
        MqttClientOptionsBuilderTlsParameters? tlsOptions;

        /// <summary>
        /// Basic class to define an Astarte Device.
        /// Device represents an Astarte Device.It is the base class used for managing the Device lifecycle and data.
        /// Users should instantiate a Device with the right credentials and connect it to the configured instance to
        /// start working with it.
        /// 
        /// </summary>
        /// <param name="deviceId">The Device ID for this Device. It has to be a valid Astarte Device ID.</param>
        /// <param name="realm">The Realm this Device will be connecting against.</param>
        /// <param name="credentialSecret">The Credentials Secret for this Device. The Device class assumes your Device has already been registered - if that
        ///    is not the case, you can use either `RegisterDeviceWithJwtToken` or `RegisterDeviceWithPrivateKey`.</param>
        /// <param name="pairingBaseURL">The Base URL of Pairing API of the Astarte Instance the Device will connect to.</param>
        /// <param name="persistencyDir">Path to an existing directory which will be used for holding persistency for this device: certificates, caching and more.
        /// It doesn't have to be unique per device, a subdirectory for the given Device ID will be created.</param>
        /// <param name="ignoreSSLErrors">Useful if you're using the Device to connect to a test instance of Astarte with self signed certificates,
        /// it is not recommended to leave this `true` in production.
        /// Defaults to `false`, if `true` the device will ignore SSL errors during connection.</param>
        /// <exception cref="FileNotFoundException"></exception>
        public Device(string deviceId, string realm, string credentialSecret, string pairingBaseURL, string persistencyDir, bool ignoreSSLErrors = false)
        {
            DeviceId = deviceId;
            Realm = realm;
            CredentialSecret = credentialSecret;
            PairingBaseUrl = pairingBaseURL;
            PersistencyDir = persistencyDir;
            IgnoreSSLErrors = ignoreSSLErrors;

            if (!Directory.Exists(persistencyDir))
            {
                throw new FileNotFoundException(persistencyDir + " is not directory");
            }

            if (!Directory.Exists(Path.Join(persistencyDir, deviceId)))
            {
                Directory.CreateDirectory(Path.Join(persistencyDir, deviceId));
            }

            if (!Directory.Exists(Path.Join(persistencyDir, deviceId, "crypto")))
            {
                Directory.CreateDirectory(Path.Join(persistencyDir, deviceId, "crypto"));
            }

            SetupMqtt();

        }

        private void SetupMqtt()
        {
            mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();
            mqttClient.UseDisconnectedHandler( e =>  OnDisconnect());
            mqttClient.UseConnectedHandler(async e => await OnConnect());
            mqttClient.UseApplicationMessageReceivedHandler( e =>  OnMessage(e.ApplicationMessage));

        }

        private void OnDisconnect()
        {
            IsConnected = false;
            Debug.WriteLine("Client disconnected!");
        }
        private async Task OnConnect()
        {
            IsConnected = true;
            await SendIntrospection();
            Debug.WriteLine("Client connected!");
        }
        private void OnMessage(MqttApplicationMessage message)
        {
           
            if (!message.Topic.StartsWith(GetBaseTopic()))
            {
                Debug.WriteLine($"Received unexpected message on topic {message.Topic}, {message.Payload}");
            }
            if (message.Topic == GetBaseTopic())
            {
                Debug.WriteLine($"Received control message");
            }
        }

        private async Task SetupCryptoAsync()
        {
            var cryptoStoreDir = Path.Join(PersistencyDir, DeviceId, "crypto");

            await pairingHandler.ObtainDeviceCertificate(DeviceId, Realm, CredentialSecret, PairingBaseUrl, cryptoStoreDir);

            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            string privateKey = File.ReadAllText(
                    Path.Combine(cryptoStoreDir, "device.key")).ToString()
                    .Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
            ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);

            X509Certificate2 caCert = new(File.ReadAllBytes(cryptoStoreDir + @"\device.crt"));
            X509Certificate2 caCertWithKey = caCert.CopyWithPrivateKey(ecdsa);
            X509Certificate2 clCertPFX = new(caCertWithKey.Export(X509ContentType.Pfx));

            tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                Certificates = new List<X509Certificate>
                    {
                      clCertPFX
                    },
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                AllowUntrustedCertificates = IgnoreSSLErrors

            };
        }
        /// <summary>
        /// Connects the Device asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {

            await SetupCryptoAsync();
            string transportInfo = await pairingHandler.ObtainDeviceTransportInformationAsync(DeviceId, Realm, CredentialSecret, PairingBaseUrl);
            var brokerUrl = "";
            if (transportInfo != null)
            {
                dynamic? jsonInfo = JsonConvert.DeserializeObject<object>(transportInfo);

                if (jsonInfo != null)
                {
                    foreach (var item in jsonInfo.data.protocols.astarte_mqtt_v1)
                    {
                        brokerUrl = item.Value;
                    }
                }
            }


            Uri uri = new(brokerUrl);

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(uri.Host, uri.Port)
                .WithTls(tlsOptions)
                .WithCleanSession()
                .Build();

            try
            {
                if (mqttClient != null)
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        /// <summary>
        /// Disconnects the Device asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            if (IsConnected)
            {
                await mqttClient.DisconnectAsync(CancellationToken.None);
            }
        }
        /// <summary>
        /// Sends an individual message to an interface.
        /// </summary>
        /// <param name="interfaceName">The name of an the Interface to send data to.</param>
        /// <param name="interfacePath">The path on the Interface to send data to.</param>
        /// <param name="payload">The value to be sent. The type should be compatible to the one specified in the interface path.</param>
        /// <param name="timeStamp">If sending a Datastream with explicit_timestamp, you can specify a datetime object which will be registered as the timestamp for the value.</param>
        /// <returns></returns>
        public async Task Send (string interfaceName, string interfacePath, object payload, DateTime? timeStamp)
        {
            ObjectPayload objectPayload = new() { v = payload };
            if (timeStamp.HasValue)
            {
                objectPayload.t = timeStamp.Value;
            }

            await SendGeneric($"{GetBaseTopic()}/{interfaceName}{interfacePath}", objectPayload);
        }

        private async Task SendGeneric (string topic, ObjectPayload objectPayload, int qos = 2)
        {
         
            MemoryStream ms = new();
            using (BsonDataWriter writer = new(ms))
            {
                JsonSerializer serializer = new();
                serializer.Serialize(writer, objectPayload);
            }

            byte[] payload = ms.ToArray();

            try
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                                  .WithTopic(topic)
                                  .WithPayload(payload)
                                  .WithQualityOfServiceLevel(qos)
                                  .Build();
                await mqttClient.PublishAsync(applicationMessage);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        private string GetBaseTopic()
        {
            return $"{Realm}/{DeviceId}";
        }

        private async Task SendIntrospection()
        {
            var introspectionMessage = new MqttApplicationMessageBuilder()
                    .WithTopic($"{Realm}/{DeviceId}")
                    .WithPayload("org.astarte-platform.genericsensors.Values:1:0")
                    .Build();

            await mqttClient.PublishAsync(introspectionMessage);
        }
        /// <summary>
        /// Returns whether the Device is currently connected.
        /// </summary>
        public bool Is_Connected { get => IsConnected; }
        /// <summary>
        /// Returns the Device ID of the Device.
        /// </summary>
        public string Device_Id { get => DeviceId; }

    }
    internal class ObjectPayload
    {
        public object v { get; set; } = new object();
        public DateTime t { get; set; }
    }
}

