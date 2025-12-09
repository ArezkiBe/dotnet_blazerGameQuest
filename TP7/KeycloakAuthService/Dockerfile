
# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ../KeycloakAuthService/KeycloakAuthService.csproj ./KeycloakAuthService/
#COPY ../SharedModels/SharedModels.csproj ./SharedModels/
RUN dotnet restore ./KeycloakAuthService
#RUN dotnet restore ./SharedModels

# Copy the remaining source code
COPY ./KeycloakAuthService/ ./KeycloakAuthService/
#COPY ./SharedModels/ ./SharedModels/
RUN dotnet publish ./KeycloakAuthService -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set environment variables if needed (e.g., ASPNETCORE_ENVIRONMENT)
# ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 5051
EXPOSE 5051

# Define the entry point
ENTRYPOINT ["dotnet", "KeycloakAuthService.dll"]
