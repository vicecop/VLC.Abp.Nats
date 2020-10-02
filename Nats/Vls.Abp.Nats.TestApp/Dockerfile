#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Vls.Abp.Nats.TestApp/Vls.Abp.Nats.TestApp.csproj", "Vls.Abp.Nats.TestApp/"]
RUN dotnet restore "Vls.Abp.Nats.TestApp/Vls.Abp.Nats.TestApp.csproj"
COPY . .
WORKDIR "/src/Vls.Abp.Nats.TestApp"
RUN dotnet build "Vls.Abp.Nats.TestApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vls.Abp.Nats.TestApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vls.Abp.Nats.TestApp.dll"]