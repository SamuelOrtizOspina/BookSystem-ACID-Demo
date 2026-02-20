FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BookSystem.csproj", "./"]
RUN dotnet restore "BookSystem.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BookSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookSystem.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookSystem.dll"]