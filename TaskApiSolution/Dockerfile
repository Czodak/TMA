FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copy project files first (for Docker layer caching)
COPY ["src/TaskApi.Data/TaskApi.Data.csproj", "src/TaskApi.Data/"]
COPY ["src/TaskApi/TaskApi.csproj", "src/TaskApi/"] 

# 2. Restore dependencies (this will include referenced projects)
RUN dotnet restore "src/TaskApi/TaskApi.csproj"

# 3. Copy remaining source code
COPY . .

# 4. Build and publish
WORKDIR "/src/src/TaskApi"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskApi.dll"]