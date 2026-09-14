FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CashFlow.sln ./
COPY CashFlow.API/CashFlow.API.csproj CashFlow.API/
COPY CashFlow.Application/CashFlow.Application.csproj CashFlow.Application/
COPY CashFlow.Domain/CashFlow.Domain.csproj CashFlow.Domain/
COPY CashFlow.Infrastructure/CashFlow.Infrastructure.csproj CashFlow.Infrastructure/
RUN dotnet restore CashFlow.sln

COPY . .
RUN dotnet publish CashFlow.API/CashFlow.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CashFlow.API.dll"]

