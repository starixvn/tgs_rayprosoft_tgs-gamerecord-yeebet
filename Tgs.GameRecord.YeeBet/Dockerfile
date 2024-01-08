#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.YeeBet/Tgs.GameRecord.YeeBet.csproj", "Tgs.GameRecord.YeeBet/"]
COPY ["Tgs.GameRecord.YeeBet.Common/Tgs.GameRecord.YeeBet.Common.csproj", "Tgs.GameRecord.YeeBet.Common/"]
COPY ["Tgs.GameRecord.YeeBet.Core/Tgs.GameRecord.YeeBet.Core.csproj", "Tgs.GameRecord.YeeBet.Core/"]
RUN dotnet restore "./Tgs.GameRecord.YeeBet/./Tgs.GameRecord.YeeBet.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.YeeBet"
RUN dotnet build "./Tgs.GameRecord.YeeBet.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.YeeBet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.YeeBet.dll"]