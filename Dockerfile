# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FuelManagement/FuelManagement.csproj", "FuelManagement/"]
RUN dotnet restore "FuelManagement/FuelManagement.csproj"

COPY . .
WORKDIR "/src/FuelManagement"
RUN dotnet build "FuelManagement.csproj" -c Release -o /app/build

# ===== Publish Stage =====
FROM build AS publish
RUN dotnet publish "FuelManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Final Runtime Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "FuelManagement.dll"]
