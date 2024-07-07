# OboardUsuario
Link  youtube apresentando projeto: https://youtu.be/ATIiv8vhcF4

## Criar Banco de Dados e Tabela

```sql
-- Criar Banco de Dados e Tabela

USE [master];
GO

CREATE DATABASE onboard_user;
GO

USE onboard_user;
GO

CREATE TABLE [Users] (
    [Id] INT IDENTITY(1, 1) NOT NULL,
    [DataCriacao] DATETIME NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [Phone] NVARCHAR(50) NOT NULL
) ON [PRIMARY];
GO
