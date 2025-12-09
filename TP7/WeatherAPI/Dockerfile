# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY WeatherAPI/WeatherAPI.csproj WeatherAPI/
RUN dotnet restore WeatherAPI/WeatherAPI.csproj

# Copy the entire source code and build the app
COPY WeatherAPI/ WeatherAPI/
WORKDIR /src/WeatherAPI
RUN dotnet build WeatherAPI.csproj -c Release -o /app/build

# Publish the app
RUN dotnet publish WeatherAPI.csproj -c Release -o /app/publish

# Use the official ASP.NET Core runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

# Start the application
ENTRYPOINT ["dotnet", "WeatherAPI.dll"]