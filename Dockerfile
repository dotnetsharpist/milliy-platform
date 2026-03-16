# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .

RUN dotnet publish src/MilliyMock.Api/MilliyMock.Api.csproj -c Release -o /publish

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MilliyMock.Api.dll"]