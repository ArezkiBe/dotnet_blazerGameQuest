# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ApiGateway/ApiGateway.csproj ApiGateway/
RUN dotnet restore ApiGateway/ApiGateway.csproj

# Copy the entire source code and build the app
COPY ApiGateway/ ApiGateway/
WORKDIR /src/ApiGateway
RUN dotnet build ApiGateway.csproj -c Release -o /app/build

# Publish the app
RUN dotnet publish ApiGateway.csproj -c Release -o /app/publish

# Use the official ASP.NET Core runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Start the application
ENTRYPOINT ["dotnet", "ApiGateway.dll"]