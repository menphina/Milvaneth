FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Milvaneth.Server/Milvaneth.Server.csproj", "Milvaneth.Server/"]
RUN dotnet restore "Milvaneth.Server/Milvaneth.Server.csproj"
COPY . .
WORKDIR "/src/Milvaneth.Server"
RUN dotnet build "Milvaneth.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Milvaneth.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Milvaneth.Server.dll"]