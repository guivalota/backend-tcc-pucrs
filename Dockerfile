# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "backend-tcc-pucrs.csproj"
RUN dotnet publish "backend-tcc-pucrs.csproj" -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5063
ENTRYPOINT ["dotnet", "backend-tcc-pucrs.dll"]
