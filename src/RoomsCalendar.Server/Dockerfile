# Generate css files with TailwindCSS
FROM node:alpine AS prepare
WORKDIR /src
COPY . .
RUN apk update \
    && npx -y @go-task/cli tailwind-gen

# Build and Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --from=prepare /src .
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
USER $APP_UID
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoomsCalendar.Server.dll"]
