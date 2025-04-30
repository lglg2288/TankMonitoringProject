using Npgsql;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

public class OilMeasurement
{
    public int Id { get; set; }
    public DateTime MeasurementTime { get; set; }
    public string? TankNumber { get; set; }
    public double DensityKgM3 { get; set; }
    public double MassKg { get; set; }
}

internal class Program
{
    const string connectionString = "Host=IP_ВАШЕГО_СЕРВЕРА;Port=ПОРТ_ВАШЕГО_СЕРВЕРА;Username=ИМЯ_ПОЛЬЗОВАТЕЛЯ_БД;Password=ВАШ_ПАРОЛЬ;Database=ИМЯ_ВАШЕЙ_БД;SSL Mode=Require;Trust Server Certificate=true;";
    List<double> sensors = new List<double>();
    private static List<OilMeasurement> measurements;

    private static void Main(string[] args)
    {
        measurements = new List<OilMeasurement>()
        {
            new OilMeasurement() {
                TankNumber = "TANK001",
                MassKg = 10000
            },
            new OilMeasurement() {
                TankNumber = "TANK002",
                MassKg = 15000
            },
            new OilMeasurement() {
                TankNumber = "TANK003",
                MassKg = 5000
            }
        };
        StartEmulation();
        Console.Read();
    }

    private static async void StartEmulation()
    {
        using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                Console.WriteLine("Подключение установлено!\n");

                while (true)
                {
                    foreach (var measurement in measurements)
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO oil_measurements (measurement_time, tank_number, density_kg_m3, mass_kg) " +
                                                "VALUES (@measurement_time, @tank_number, @density_kg_m3, @mass_kg)", conn);

                        cmd.Parameters.AddWithValue("measurement_time", DateTime.Now);
                        cmd.Parameters.AddWithValue("tank_number", measurement.TankNumber);
                        cmd.Parameters.AddWithValue("density_kg_m3", Random.Shared.NextDouble() * (1200 - 780) + 780);
                        cmd.Parameters.AddWithValue("mass_kg", measurement.MassKg);

                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"{DateTime.Now}\t{measurement.TankNumber}\tUpdate");
                    }
                    await Task.Delay(300000);
                }

            }
            catch (Exception e)
            { Console.WriteLine(e.ToString()); }
        }
    }
}