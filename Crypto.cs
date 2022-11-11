using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AstarteDeviceSDKCSharp
{
    public class Crypto
    {

        public string GenerateCsr(string realm, string deviceId, string cryptoStoreDir)
        {

            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            string fileName = "device.key";

            if (!File.Exists(Path.Combine(cryptoStoreDir, fileName)))
            {
                string privateKey = new(PemEncoding.Write("PRIVATE KEY", ecdsa.ExportECPrivateKey()).ToArray());
                File.WriteAllText(Path.Combine(cryptoStoreDir, fileName), privateKey);
            }
            else
            {
                string privateKey = File.ReadAllText(
                Path.Combine(cryptoStoreDir, fileName)).ToString()
               .Replace("-----BEGIN PRIVATE KEY-----", "")
               .Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
                ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
            }

            var cert = new CertificateRequest($"O=Devices,CN={realm}/{deviceId}", ecdsa, HashAlgorithmName.SHA256);

            byte[] pkcs10 = cert.CreateSigningRequest();
            StringBuilder builder = new();

            builder.AppendLine("-----BEGIN CERTIFICATE REQUEST-----");

            string base64 = Convert.ToBase64String(pkcs10);

            int offset = 0;
            const int LineLength = 64;

            while (offset < base64.Length)
            {
                int lineEnd = Math.Min(offset + LineLength, base64.Length);
                builder.AppendLine(base64.Substring(offset, lineEnd - offset));
                offset = lineEnd;
            }

            builder.AppendLine("-----END CERTIFICATE REQUEST-----");
            return builder.ToString();
        }
        public void ImportDeviceCertificate(string clientCrt, string cryptoStoreDir)
        {
            var cert = new X509Certificate2(Convert.FromBase64String(clientCrt
                        .Replace("-----BEGIN CERTIFICATE-----", "")
                        .Replace("-----END CERTIFICATE-----", "")
                        .Replace("\n", "")));

            File.WriteAllText(Path.Combine(cryptoStoreDir, "device.crt"), 
                new(PemEncoding.Write("CERTIFICATE", cert.GetRawCertData()).ToArray()));
        }
        //public async Task SetupMqttAsync(string realm, string deviceId, string cryptoStoreDir)
        //{

        //    var mqttFactory = new MqttFactory();
        //    ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        //    string privateKey = File.ReadAllText(
        //            Path.Combine(cryptoStoreDir, "device.key")).ToString()
        //            .Replace("-----BEGIN PRIVATE KEY-----", "")
        //            .Replace("-----END PRIVATE KEY-----", "").Replace("\n", "");
        //    ecdsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);

        //    X509Certificate2 caCert = new X509Certificate2(File.ReadAllBytes(cryptoStoreDir + @"\device.crt"));
        //    X509Certificate2 caCertWithKey = caCert.CopyWithPrivateKey(ecdsa);
        //    X509Certificate2 clCertPFX = new X509Certificate2(caCertWithKey.Export(X509ContentType.Pfx));

        //    using (var mqttClient = mqttFactory.CreateMqttClient())
        //    {

        //        var tlsOptions = new MqttClientOptionsBuilderTlsParameters
        //        {
        //            UseTls = true,
        //            Certificates = new List<X509Certificate>
        //            {
        //              clCertPFX
        //            },
        //            IgnoreCertificateChainErrors = true,
        //            IgnoreCertificateRevocationErrors = true,
        //            SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
        //            AllowUntrustedCertificates = true,

        //        };

        //        var mqttClientOptions = new MqttClientOptionsBuilder()
        //            .WithClientId(Guid.NewGuid().ToString())
        //            .WithTcpServer("localhost", 8883)
        //            .WithTls(tlsOptions)
        //            .WithCleanSession()
        //            .Build();

        //        try
        //        {
        //            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);


        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }

        //        var introspectionMessage = new MqttApplicationMessageBuilder()
        //         .WithTopic($"{realm}/{deviceId}")
        //         .WithPayload("org.astarte-platform.genericsensors.Values:1:0")
        //         .Build();

        //        await mqttClient.PublishAsync(introspectionMessage);

        //        Event e = new Event
        //        {
        //            v = 19.5
        //        };


        //        MemoryStream ms = new MemoryStream();
        //        using (BsonDataWriter writer = new BsonDataWriter(ms))
        //        {
        //            JsonSerializer serializer = new JsonSerializer();
        //            serializer.Serialize(writer, e);
        //        }

        //        var a = ms.ToArray();

        //        try
        //        {
        //            var applicationMessage = new MqttApplicationMessageBuilder()
        //                              .WithTopic($"{realm}/{deviceId}/org.astarte-platform.genericsensors.Values/test/value")
        //                              .WithPayload(a)
        //                              .Build();
        //            await mqttClient.PublishAsync(applicationMessage);

        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message.ToString());
        //        }

        //        mqttClient.SubscribeAsync(
        //           new MqttTopicFilterBuilder()
        //             .WithTopic($"{realm}/{deviceId}/#")
        //             .WithExactlyOnceQoS()
        //             .Build())
        //             .GetAwaiter().GetResult();

        //        await mqttClient.DisconnectAsync();

        //        Console.WriteLine("MQTT application message is published.");
        //    }
        //}
        //public class Certificate
        //{
        //    public Data data { get; set; }
        //}
        public class Data
        {
            public string client_crt { get; set; } = string.Empty;

        }
   
    }
}



