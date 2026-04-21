# See https://aka.ms/customizecontainer to learn how to customize your debug container

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia todos os .csproj primeiro (para cache de restore)
COPY ["src/FixTrading.OrderAccumulator.Worker/FixTrading.OrderAccumulator.Worker.csproj", "src/FixTrading.OrderAccumulator.Worker/"]
COPY ["src/FixTrading.OrderProcessing.Infrastructure.Fix/FixTrading.OrderProcessing.Infrastructure.Fix.csproj", "src/FixTrading.OrderProcessing.Infrastructure.Fix/"]
COPY ["src/FixTrading.OrderProcessing.Application/FixTrading.OrderProcessing.Application.csproj", "src/FixTrading.OrderProcessing.Application/"]
COPY ["src/FixTrading.OrderProcessing.Domain/FixTrading.OrderProcessing.Domain.csproj", "src/FixTrading.OrderProcessing.Domain/"]

# Restore
RUN dotnet restore "src/FixTrading.OrderAccumulator.Worker/FixTrading.OrderAccumulator.Worker.csproj"

# Copia todo o código fonte
COPY . .

# Build
WORKDIR "/src/src/FixTrading.OrderAccumulator.Worker"
RUN dotnet build "FixTrading.OrderAccumulator.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FixTrading.OrderAccumulator.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FixTrading.OrderAccumulator.Worker.dll"]