FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

COPY ["src/RescueLink.API/RescueLink.API.csproj", "src/RescueLink.API/"]
COPY ["src/RescueLink.Application/RescueLink.Application.csproj", "src/RescueLink.Application/"]
COPY ["src/RescueLink.Domain/RescueLink.Domain.csproj", "src/RescueLink.Domain/"]
COPY ["src/RescueLink.Infrastructure/RescueLink.Infrastructure.csproj", "src/RescueLink.Infrastructure/"]
COPY ["src/RescueLink.Persistence/RescueLink.Persistence.csproj", "src/RescueLink.Persistence/"]

RUN dotnet restore "src/RescueLink.API/RescueLink.API.csproj"

COPY . .

RUN dotnet publish \
    "src/RescueLink.API/RescueLink.API.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app

COPY --from=build /app/publish .

USER root

RUN mkdir -p /app/wwwroot/uploads/pet-reports \
    && chown -R $APP_UID:$APP_UID /app/wwwroot

USER $APP_UID

ENTRYPOINT ["dotnet", "RescueLink.API.dll"]