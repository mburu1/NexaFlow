# syntax=docker/dockerfile:1
# Build context is backend/src (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NexaFlow.slnx .
COPY NexaFlow.Domain/NexaFlow.Domain.csproj NexaFlow.Domain/
COPY NexaFlow.Application/NexaFlow.Application.csproj NexaFlow.Application/
COPY NexaFlow.Infrastructure/NexaFlow.Infrastructure.csproj NexaFlow.Infrastructure/
COPY NexaFlow.Messaging/NexaFlow.Messaging.csproj NexaFlow.Messaging/
COPY NexaFlow.Notifications/NexaFlow.Notifications.csproj NexaFlow.Notifications/
COPY NexaFlow.Api/NexaFlow.Api.csproj NexaFlow.Api/

RUN dotnet restore NexaFlow.Api/NexaFlow.Api.csproj

COPY NexaFlow.Domain/ NexaFlow.Domain/
COPY NexaFlow.Application/ NexaFlow.Application/
COPY NexaFlow.Infrastructure/ NexaFlow.Infrastructure/
COPY NexaFlow.Messaging/ NexaFlow.Messaging/
COPY NexaFlow.Notifications/ NexaFlow.Notifications/
COPY NexaFlow.Api/ NexaFlow.Api/

RUN dotnet publish NexaFlow.Api/NexaFlow.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NexaFlow.Api.dll"]
