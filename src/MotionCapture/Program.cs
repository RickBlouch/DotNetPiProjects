using System.Runtime.InteropServices;
using MotionCapture.Application;
using MotionCapture.Application.Device;

// This version of the project uses the PIR sensor to detect motion and then enables the video camera.  Once motion has been detected, there is an 8 second
// delay on turning off the camera.  If motion is detected during this window, the window resets and the camera continues to record.

// Useful links
// Deploying to raspberry pi: https://docs.microsoft.com/en-us/dotnet/iot/deployment
// .Net 6 Program.cs changes: https://andrewlock.net/exploring-dotnet-6-part-2-comparing-webapplicationbuilder-to-the-generic-host/
// Setting up a cert on RaspberryPi (didn't do yet): https://andrewlock.net/creating-and-trusting-a-self-signed-certificate-on-linux-for-use-in-kestrel-and-asp-net-core/


// Deploy steps:
// In Visual Studio:
//  - Right click project, publish
//  - In command prompt, run: scp -r C:\[PathToCode]\linux-arm\* pi@[PiIpAddress]:/home/pi/WebApps/MotionCapture
//
// On RaspberryPi
//  - In /home/pi/WebApps/Motioncapture run chmod +x MotionCapture
//  - ./MotionCapture

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .UseUrls("http://*:5000/");


// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddHostedService<MotionCaptureService>();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) // Local debug environment - fake services that interact with the RaspberryPi hardware
{
    builder.Services.AddSingleton<FakeDeviceManager>();
    builder.Services.AddSingleton<IHostedService>(x => x.GetRequiredService<FakeDeviceManager>());
    builder.Services.AddSingleton<IDeviceManager>(x => x.GetRequiredService<FakeDeviceManager>());
}
else
{
    builder.Services.AddSingleton<RaspberryPiDeviceManager>();
    builder.Services.AddSingleton<IHostedService>(x => x.GetRequiredService<RaspberryPiDeviceManager>());
    builder.Services.AddSingleton<IDeviceManager>(x => x.GetRequiredService<RaspberryPiDeviceManager>());
}


var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
