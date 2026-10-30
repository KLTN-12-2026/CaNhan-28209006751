USE [master];
GO

IF DB_ID(N'Atomos') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [Atomos]');
END;
GO

USE [Atomos];
GO
