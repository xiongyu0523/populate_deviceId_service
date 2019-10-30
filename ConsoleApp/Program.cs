using System;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Devices;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
{
    class Program
    {
        private static EventHubClient s_eventHubClient;
        private static RegistryManager s_iothubRegistryManager;

        // Asynchronously create a PartitionReceiver for a partition and then start 
        // reading any messages sent from the simulated client.
        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            // Create the receiver using the default consumer group.
            // For the purposes of this sample, read only messages sent since 
            // the time the receiver is created. Typically, you don't want to skip any messages.
            var eventHubReceiver = s_eventHubClient.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            Console.WriteLine("Create receiver on partition: " + partition);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                Console.WriteLine("Listening for messages on: " + partition);
                // Check for EventData - this methods times out if there is nothing to retrieve.
                var events = await eventHubReceiver.ReceiveAsync(100);

                // If there is data in the batch, process it.
                if (events == null) continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    JObject obj = JObject.Parse(data);
                    string devid = obj["deviceId"].ToString();
                    Console.WriteLine("deviceId={0}", devid);

                    var twin = await s_iothubRegistryManager.GetTwinAsync(devid);
                    string patch = String.Format("{{\"properties\":{{\"desired\":{{\"deviceId\":\"{0}\"}}}}}}", devid);

                    await s_iothubRegistryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
                    Console.WriteLine("Write to Device Twin");
                }
            }
        }

        private static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts - Read device to cloud messages. Ctrl-C to exit.\n");

            string EventHubconnectionString = "<Event Hub connection string>";
            s_eventHubClient = EventHubClient.CreateFromConnectionString(EventHubconnectionString);

            string IoTHubconnectionString = "<IoT Hub Service(IoTOwner) connection string>";
            s_iothubRegistryManager = RegistryManager.CreateFromConnectionString(IoTHubconnectionString);

            // Create a PartitionReciever for each partition on the hub.
            var runtimeInfo = await s_eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }

            // Wait for all the PartitionReceivers to finsih.
            Task.WaitAll(tasks.ToArray());
        }
    }
}
