FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BrightPathLearningCenter.Api/BrightPathLearningCenter.Api.csproj", "src/BrightPathLearningCenter.Api/"]
COPY ["src/BrightPathLearningCenter.Api.Infrastructure/BrightPathLearningCenter.Api.Infrastructure.csproj", "src/BrightPathLearningCenter.Api.Infrastructure/"]
COPY ["src/BrightPathLearningCenter.Api.Domain/BrightPathLearningCenter.Api.Domain.csproj", "src/BrightPathLearningCenter.Api.Domain/"]
RUN dotnet restore "src/BrightPathLearningCenter.Api/BrightPathLearningCenter.Api.csproj"
COPY . .
RUN dotnet build "src/BrightPathLearningCenter.Api/BrightPathLearningCenter.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/BrightPathLearningCenter.Api/BrightPathLearningCenter.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrightPathLearningCenter.Api.dll"]
