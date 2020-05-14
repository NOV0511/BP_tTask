USE master
GO

ALTER DATABASE ttask
SET OFFLINE WITH ROLLBACK IMMEDIATE
GO


ALTER DATABASE ttask SET ONLINE;
GO

DROP DATABASE ttask
GO

DROP LOGIN NewTenantUser

DROP LOGIN [default]

DROP LOGIN test
DROP LOGIN basic
DROP LOGIN pro