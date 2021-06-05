FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /src
COPY MarvinNG.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:5.0 

RUN apt-get update && apt-get install -y \
  libgdiplus \
  && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /out/ /app

ENTRYPOINT ["dotnet", "/app/MarvinNG.dll"]