# WebChat
# Команды для dotnet ef cli:
    dotnet ef migrations script --idempotent --output .\Migrations\Scripts\databaseScript.sql
    dotnet ef migrations add Имя миграции
    dotnet ef migrations remove

# Команды для Docker:
    docker compose up -d (Из папки minioDocker)