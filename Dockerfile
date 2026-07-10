# ---- Stage 1: Build Frontend ----
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ---- Stage 2: Build Backend ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /app/backend
COPY backend/StockApp.csproj ./
RUN dotnet restore
COPY backend/ ./
RUN dotnet publish -c Release -o /app/publish

# ---- Stage 3: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy backend
COPY --from=backend-build /app/publish .

# Copy frontend build output
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Persistent volume for LiteDB
VOLUME /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV LITEDB_PATH=/app/data/stock.db

EXPOSE 8080

ENTRYPOINT ["dotnet", "StockApp.dll"]
