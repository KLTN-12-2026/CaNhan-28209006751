USE [Atomos];
GO

MERGE dbo.[Role] AS target
USING (VALUES
    (N'Admin'),
    (N'Student')
) AS source (TenRole)
ON target.TenRole = source.TenRole
WHEN NOT MATCHED THEN
    INSERT (TenRole) VALUES (source.TenRole);
GO
