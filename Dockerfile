# ─── Stage 1: Build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install Blazor WebAssembly workload (required for WASM publish)
RUN dotnet workload install wasm-tools

# Copy solution and restore dependencies first (better Docker layer caching)
COPY KanbanApp.sln .
COPY src/KanbanApp.Domain/KanbanApp.Domain.csproj              src/KanbanApp.Domain/
COPY src/KanbanApp.Application/KanbanApp.Application.csproj    src/KanbanApp.Application/
COPY src/KanbanApp.Infrastructure/KanbanApp.Infrastructure.csproj src/KanbanApp.Infrastructure/
COPY src/KanbanApp.Web/KanbanApp.Web/KanbanApp.Web.csproj      src/KanbanApp.Web/KanbanApp.Web/
COPY src/KanbanApp.Web/KanbanApp.Web.Client/KanbanApp.Web.Client.csproj src/KanbanApp.Web/KanbanApp.Web.Client/

RUN dotnet restore

# Copy all source and publish
COPY . .
RUN dotnet publish src/KanbanApp.Web/KanbanApp.Web/KanbanApp.Web.csproj \
    -c Release -o /out --no-restore

# ─── Stage 2: Runtime ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Railway sets PORT automatically; ASP.NET Core reads ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "KanbanApp.Web.dll"]
