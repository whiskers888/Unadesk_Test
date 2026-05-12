FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["APIGateway/APIGateway.csproj", "APIGateway/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "APIGateway/APIGateway.csproj"

COPY . .
WORKDIR "/source/APIGateway"
RUN dotnet publish "APIGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "APIGateway.dll"] 