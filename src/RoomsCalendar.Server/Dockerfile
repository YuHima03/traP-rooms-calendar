# Base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# .NET SDK and Node.js environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs

# Generate css file with Tailwind and build dotnet projects
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RoomsCalendar.Server/RoomsCalendar.Server.csproj", "src/RoomsCalendar.Server/"]
COPY ["src/RoomsCalendar.Client/RoomsCalendar.Client.csproj", "src/RoomsCalendar.Client/"]
RUN dotnet restore "./src/RoomsCalendar.Server/RoomsCalendar.Server.csproj"
COPY . .
WORKDIR "/src/src/RoomsCalendar.Client"
RUN npm i
RUN npx @tailwindcss/cli -i "./Styles/app.css" -o "./wwwroot/css/app.css"
WORKDIR "/src/src/RoomsCalendar.Server"
RUN dotnet build "./RoomsCalendar.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RoomsCalendar.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoomsCalendar.Server.dll"]
