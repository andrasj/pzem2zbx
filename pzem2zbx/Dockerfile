FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./pzem2zbx/
WORKDIR /app/pzem2zbx
RUN dotnet restore

# copy and publish app and libraries
WORKDIR /app/
COPY . ./pzem2zbx/
WORKDIR /app/pzem2zbx
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
COPY --from=build /app/pzem2zbx/out ./
ENTRYPOINT ["dotnet", "pzem2zbx.dll"]