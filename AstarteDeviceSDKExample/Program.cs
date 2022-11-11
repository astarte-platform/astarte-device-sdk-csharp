using AstarteDeviceSDKCSharp;


var myDevice = new Device("OboEfmB1SZK49dRwFR7NjQ", "secobh", "000000000000000000000000000", "http://localhost:4003", @"C:\Users\Documents\GitHub\CryptoStore",true);

await myDevice.Connect();

int i = 0;
while (i<5)
{
   await myDevice.Send("org.astarte-platform.genericsensors.Values", "/test/value", Random.Shared.NextDouble(),DateTime.Now.ToUniversalTime());
   await Task.Delay(TimeSpan.FromSeconds(1));
   i++;
}

await myDevice.Disconnect();
