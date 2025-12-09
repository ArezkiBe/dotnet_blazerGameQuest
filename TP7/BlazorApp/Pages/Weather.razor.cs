using Microsoft.AspNetCore.Components;

namespace BlazorApp.Pages
{
    public partial class Weather : ComponentBase
    {
        [Inject]
        public AuthenticatedHttpClient AuthHttp { get; set; } = default!;

        protected WeatherForecast[]? forecasts;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                forecasts = await AuthHttp.GetFromJsonAsync<WeatherForecast[]>("api/weather/weatherforecast");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch weather data: {ex.Message}");
                forecasts = null;
            }
        }

        public class WeatherForecast
        {
            public DateOnly Date { get; set; }
            public int TemperatureC { get; set; }
            public string? Summary { get; set; }
            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}
