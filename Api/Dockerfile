
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

COPY API/*.csproj API/
COPY Application/*.csproj Application/
COPY Domain/*.csproj Domain/
COPY Infrastructure/*.csproj Infrastructure/
COPY Shared/*.csproj Shared/
COPY ChatCommunicator.sln .

RUN dotnet restore API/API.csproj


COPY . .

RUN dotnet publish API/API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "API.dll"]