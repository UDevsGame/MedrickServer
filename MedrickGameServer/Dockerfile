FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# کپی فایل csproj مستقیماً (بدون پوشه اضافی)
COPY ["MedrickGameServer.csproj", "./"]
RUN dotnet restore "MedrickGameServer.csproj"

# کپی تمام فایل‌ها
COPY . .

# build کردن پروژه
RUN dotnet build "MedrickGameServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MedrickGameServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MedrickGameServer.dll"]