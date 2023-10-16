using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// L�gger till SignalR-tj�nster till applikationen.
builder.Services.AddSignalR();

var app = builder.Build();


//Lite klumpig l�sning, men ser till att ##### Database ##### printas ut i cmd-rutan, s� att det �r l�ttare att h�lla koll.
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

// Kartl�gger SignalR-hubben till en specifik endpoint.
app.MapHub<DatabaseHub>("/databaseHub");

app.Run();

Console.WriteLine("##### Database #####");

public class DatabaseHub : Hub
{
    // En statisk lista som h�ller alla mottagna temperaturv�rden.
    private static List<double> temperatureData = new List<double>();

    // Metod f�r att lagra mottagna temperaturv�rden.
    public void StoreTemperature(double temperature)
    {
        temperatureData.Add(temperature);
        Console.WriteLine($"Lagrad temperatur: {temperature}�C. Totalt antal avl�sningar: {temperatureData.Count}");
    }

    // Metod f�r att returnera alla lagrade temperaturv�rden.
    public List<double> GetAllTemperatures()
    {
        return temperatureData;
    }
}