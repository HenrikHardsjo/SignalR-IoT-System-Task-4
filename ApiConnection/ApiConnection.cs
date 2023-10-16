using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Lägger till SignalR-tjänster till applikationen.
builder.Services.AddSignalR();


//Lite klumpig lösning, men ser till att ##### ApiConnection ##### printas ut i cmd-rutan, så att det är lättare att hålla koll.
var app = builder.Build();
bool hasPrintedStartupMessage = false;

app.Use(async (context, next) =>
{
    if (!hasPrintedStartupMessage)
    {
        Console.WriteLine("##### ApiConnection #####");
        hasPrintedStartupMessage = true;
    }

    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Kartlägger SignalR-hubben till en specifik endpoint.
app.MapHub<ThermometerHub>("/thermometerHub");

app.Run();

Console.WriteLine("##### ApiConnection #####");

public class ThermometerHub : Hub
{
    // Skapar en statisk anslutning till DatabaseServern.
    private static HubConnection databaseServerConnection;

    // Statisk konstruktor som initieras när klassen först används.
    static ThermometerHub()
    {
        databaseServerConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7239/databaseHub") // Anger URL för Database's SignalR-hub.
            .Build();

        // Startar anslutningen till Database.
        databaseServerConnection.StartAsync().Wait();
    }

    // Metod som tar emot temperaturen från IoTThermometer och skickar den vidare till Database.
    public async Task SendTemperature(double temperature)
    {
        Console.WriteLine($"Mottagen temperatur: {temperature}°C");

        // Skickar temperaturen till DatabaseServer.
        await databaseServerConnection.SendAsync("StoreTemperature", temperature);
    }

    // Metod som hämtar temperaturdata från databas.
    public async Task<List<double>> GetTemperaturesFromDatabase()
    {
        return await databaseServerConnection.InvokeAsync<List<double>>("GetAllTemperatures");
    }


}