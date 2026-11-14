
# FOODSMART - Backend .NET 8

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FoodSmart.Api.csproj", "./"]
RUN dotnet restore "FoodSmart.Api.csproj"

COPY . .
RUN dotnet publish "FoodSmart.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FoodSmart.Api.dll"]
