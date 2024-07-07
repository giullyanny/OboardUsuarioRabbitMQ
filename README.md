# OboardUsuario
Link youtube: https://youtu.be/ATIiv8vhcF4

##Criar tabela e banco
use [master]
go

CREATE DATABASE onboard_user
GO

use onboard_user
go

CREATE TABLE [Users] (
    [Id] [int] IDENTITY(1, 1) NOT NULL
    , [DataCriacao] DATETIME NOT NULL
    , [Name] [nvarchar](100) NOT NULL
    , [Email] [nvarchar](100) NOT NULL
    , [Phone] [nvarchar](50) NOT NULL
    ) ON [PRIMARY]

