dotnet ef database drop -c PersistedGrantDbContext

rmdir /S /Q Migrations

dotnet ef migrations add Grants -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
dotnet ef migrations add DataProtectionKeys -c DataProtectionKeysDbContext -o Migrations/DataProtectionKeysDb

dotnet ef migrations script -c PersistedGrantDbContext -o Migrations/PersistedGrantDb.sql
dotnet ef migrations script -c DataProtectionKeysDbContext -o Migrations/DataProtectionKeysDb.sql

dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c DataProtectionKeysDbContext
