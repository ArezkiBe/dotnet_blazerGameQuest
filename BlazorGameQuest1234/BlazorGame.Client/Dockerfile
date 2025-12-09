FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BlazorGame.Client/BlazorGame.Client.csproj", "BlazorGame.Client/"]
COPY ["SharedModels/SharedModels.csproj", "SharedModels/"]
RUN dotnet restore "BlazorGame.Client/BlazorGame.Client.csproj"
COPY . .
RUN dotnet publish "BlazorGame.Client/BlazorGame.Client.csproj" -c Release -o /app/publish

# Utiliser nginx pour servir les fichiers statiques Blazor WebAssembly
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY BlazorGame.Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]