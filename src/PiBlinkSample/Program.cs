
// Useful links
// Deploying to raspberry pi: https://docs.microsoft.com/en-us/dotnet/iot/deployment
// .Net 6 Program.cs changes: https://andrewlock.net/exploring-dotnet-6-part-2-comparing-webapplicationbuilder-to-the-generic-host/
// Setting up a cert on RaspberryPi (didn't do yet): https://andrewlock.net/creating-and-trusting-a-self-signed-certificate-on-linux-for-use-in-kestrel-and-asp-net-core/


var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .UseUrls("http://*:5000/");


// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
