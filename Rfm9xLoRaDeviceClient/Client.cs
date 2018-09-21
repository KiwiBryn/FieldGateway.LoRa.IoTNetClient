//---------------------------------------------------------------------------------
// Copyright (c) Sept 2018, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// git remote add origin https://github.com/KiwiBryn/FieldGateway.LoRa.IoTNetClient.git
// git push -u origin master
//---------------------------------------------------------------------------------
namespace devMobile.IoT.IoTNet.FieldGateway
{
	using System;
	using System.Text;
	using System.Threading;
	using Microsoft.SPOT;
	using Microsoft.SPOT.Hardware;
	using devMobile.IoT.NetMF.ISM;
	using IngenuityMicro.Sensors;

	class IoTNetClient
	{
		private readonly Rfm9XDevice rfm9XDevice;
		private readonly TimeSpan dueTime = new TimeSpan(0, 0, 10);
		private readonly TimeSpan periodTime = new TimeSpan(0, 0, 30);
		private readonly MCP9808 mcp9808 = new MCP9808();
		private readonly OutputPort _led = new OutputPort((Cpu.Pin)16 + 8, false);
		private readonly byte[] fieldGatewayAddress = Encoding.UTF8.GetBytes("LoRaIoT1");
		private readonly byte[] deviceAddress = Encoding.UTF8.GetBytes("IoTNet1");

		public IoTNetClient()
		{
			rfm9XDevice = new Rfm9XDevice( SPI.SPI_module.SPI3, (Cpu.Pin)16 + 9, (Cpu.Pin)5, (Cpu.Pin)4);
		}

		public void Run()
		{
			rfm9XDevice.Initialise(frequency: 915000000, paBoost: true, rxPayloadCrcOn: true);
			rfm9XDevice.Receive(deviceAddress);

			rfm9XDevice.OnDataReceived += rfm9XDevice_OnDataReceived;
			rfm9XDevice.OnTransmit += rfm9XDevice_OnTransmit;

			Timer temperatureUpdates = new Timer(TemperatureTimerProc, null, dueTime, periodTime);

			Thread.Sleep(Timeout.Infinite);
		}

		private void TemperatureTimerProc(object state)
		{
			_led.Write(true);

			double temperature = mcp9808.ReadTempInC();

			Debug.Print(DateTime.UtcNow.ToString("hh:mm:ss") + "  T:" + temperature.ToString("F1"));

			rfm9XDevice.Send(fieldGatewayAddress, Encoding.UTF8.GetBytes("t " + temperature.ToString("F1")));

			_led.Write(true);
		}

		void rfm9XDevice_OnTransmit()
		{
			Debug.Print("Transmit-Done");
			_led.Write(false);
		}

		void rfm9XDevice_OnDataReceived(byte[] address, float packetSnr, int packetRssi, int rssi, byte[] data)
		{
			try
			{
				string messageText = new string(UTF8Encoding.UTF8.GetChars(data));
				string addressText = new string(UTF8Encoding.UTF8.GetChars(address));

				Debug.Print(DateTime.UtcNow.ToString("HH:MM:ss") + "-Rfm9X PacketSnr " + packetSnr.ToString("F1") + " Packet RSSI " + packetRssi + "dBm RSSI " + rssi + "dBm = " + data.Length + " byte message " + @"""" + messageText + @"""");
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message);
			}
		}
	}
}