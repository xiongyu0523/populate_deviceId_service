# Populate deviceId service (deprecated)
 
> This repo is deprecated, See how to [Get Azure Sphere Device ID code snippet](https://github.com/Azure/azure-sphere-samples/tree/main/CodeSnippets/DeviceId) locally.

This sample code demostrate a service to receive [createDeviceIdentity](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-identity-registry#device-and-module-lifecycle-notifications) event from IoT Hub build-in endpoint and populate 'deviceId' field back to the desired properties of device twin. 

As device SDK can't see [device identity properties](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-device-twins#device-twins) so this is a way for device to know its ID (when it is not available from device App) 

## Configure Azure IoT Hub

1. Enable [device lifecycle events](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-d2c#non-telemetry-events) to the build-in endpoint of IoT Hub. Use `opType = 'createDeviceIdentity'` as query string to only route identity create event.
   
   > Please note this will disable the default telemetry route, if you need telemetry data, you should create a custom event hub endpoint. 
2. Copy IoT Hub connection string with iothubowner policy,
3. Copy IoT build-in event hub connection string

## To build and run the sample

### Build and deploy the application

1. Double click consoleApp.sln to open Visual Studio project.
2. In Program.cs file, replace `EventHubconnectionString` and `IoTHubconnectionString`.
3. Press **F5** to build and run the application
4. Once a new device is registed to IoT Hub, you will see below logs. 
   ```
    Listening for messages on: 0
    Listening for messages on: 1
    Listening for messages on: 2
    Listening for messages on: 3
    ...
    deviceId=A5DDSKQ3271KJASD
    Write to Device Twin
   ```
5. On device App, a device twin change callback will be called with deviceId field.
