FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GameAPI/GameAPI.csproj", "GameAPI/"]
COPY ["SharedModels/SharedModels.csproj", "SharedModels/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
RUN dotnet restore "GameAPI/GameAPI.csproj"
COPY . .
WORKDIR "/src/GameAPI"
RUN dotnet build "GameAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameAPI.dll"]