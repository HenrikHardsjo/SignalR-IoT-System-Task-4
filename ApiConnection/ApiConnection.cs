using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// L�gger till SignalR-tj�nster till applikationen.
builder.Services.AddSignalR();


//Lite klumpig l�sning, men ser till att ##### ApiConnection ##### printas ut i cmd-rutan, s� att det �r l�ttare att h�lla koll.
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

// Kartl�gger SignalR-hubben till en specifik endpoint.
app.MapHub<ThermometerHub>("/thermometerHub");

app.Run();

Console.WriteLine("##### ApiConnection #####");

public class ThermometerHub : Hub
{
    // Skapar en statisk anslutning till DatabaseServern.
    private static HubConnection databaseServerConnection;

    // Statisk konstruktor som initieras n�r klassen f�rst anv�nds.
    static ThermometerHub()
    {
        databaseServerConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7239/databaseHub") // Anger URL f�r Database's SignalR-hub.
            .Build();

        // Startar anslutningen till Database.
        databaseServerConnection.StartAsync().Wait();
    }

    // Metod som tar emot temperaturen fr�n IoTThermometer och skickar den vidare till Database.
    public async Task SendTemperature(double temperature)
    {
        Console.WriteLine($"Mottagen temperatur: {temperature}�C");

        // Skickar temperaturen till DatabaseServer.
        await databaseServerConnection.SendAsync("StoreTemperature", temperature);
    }

    // Metod som h�mtar temperaturdata fr�n databas.
    public async Task<List<double>> GetTemperaturesFromDatabase()
    {
        return await databaseServerConnection.InvokeAsync<List<double>>("GetAllTemperatures");
    }


}