FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY src/*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 7256

ENTRYPOINT ["dotnet", "MangaStore.Inventory.dll"]