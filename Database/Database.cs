using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Lägger till SignalR-tjänster till applikationen.
builder.Services.AddSignalR();

var app = builder.Build();


//Lite klumpig lösning, men ser till att ##### Database ##### printas ut i cmd-rutan, så att det är lättare att hålla koll.
bool hasPrintedStartupMessage = false;

app.Use(async (context, next) =>
{
    if (!hasPrintedStartupMessage)
    {
        Console.WriteLine("##### Database #####");
        hasPrintedStartupMessage = true;
    }

    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Kartlägger SignalR-hubben till en specifik endpoint.
app.MapHub<DatabaseHub>("/databaseHub");

app.Run();

Console.WriteLine("##### Database #####");

public class DatabaseHub : Hub
{
    // En statisk lista som håller alla mottagna temperaturvärden.
    private static List<double> temperatureData = new List<double>();

    // Metod för att lagra mottagna temperaturvärden.
    public void StoreTemperature(double temperature)
    {
        temperatureData.Add(temperature);
        Console.WriteLine($"Lagrad temperatur: {temperature}°C. Totalt antal avläsningar: {temperatureData.Count}");
    }

    // Metod för att returnera alla lagrade temperaturvärden.
    public List<double> GetAllTemperatures()
    {
        return temperatureData;
    }
}