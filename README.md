# WebChat
# Как захостить приложение?
    1) Иметь кластер PostgreSQL, создать в ней болванку базы данных для Hangfire и самого приложения (две пустых БД с любым именем)
    2) Иметь MinIO S3, создать в нём bucket и access key
    3) Иметь эл. почту gmail, на которой нужно создать пароль приложения(или, если вы пользуетесь другим smtp сервером, указать соотвестувующие данные в appsettngs.json)
    5) Указать все данные в appsettings.Development.json(если вы хостите из ide или собрали в Debug) или в appsettings.Production.json(если вы собрали приложение в Release)
    6) Запустить приложение, есть заранее зарегистрированный аккаунт админа (логин - admin, пароль - admin)
# Команды для dotnet ef cli:
    dotnet ef migrations script --idempotent --output .\Migrations\Scripts\databaseScript.sql
    dotnet ef migrations add Имя миграции
    dotnet ef migrations remove

# Команды для Docker:
    docker compose up -d (Из папки minioDocker)
