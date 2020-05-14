/* 
Create script vytvoří databázi, všechny potřebné role, usery a taky základní testovací data.

Po supštění skriptu má aplikace 3 subdomény: test, basic a pro

test.localhost:
	obsahuje 4 usery:
		tenant@test.cz - admin domény, aktivní featury v uživatelském nastavení
		user1@test.cz - manager, aktivní featury v uživatelském nastavení
		user2@test.cz
		user3@test.cz
	obsahuje základní testovací data
	nastnavená na nejvyšší service - "Business", takže tenant nemá žádné omezení

basic.localhost:
	5 users:
		tenant@test.cz
		user1-4@test.cz
	Jako testovací data má pouze jeden prázdný projekt, jedná se o test funčnosti jednotlivých service a omezení

pro.localhost:
	2 users:
		tenant@test.cz
		user1@test.cz
	Neobsahuje testovací data, pouze odebírá službu "PRO", kterou nemá předplacenou, na tento tenant se dá dostat 
		tedy jen adminem domény a to jen do čásiti o provedení platby, aby se aplikace mohla znovu zpřístupnit i pto další uživatele.

do admin prostředí se přihlašuje bez subdomény (localhost)
	uživatelské jméno pro admina je admin@admin.cz


heslo je pro všechny uživatelé stejné: "heslo123"
*/


use master;

CREATE DATABASE ttask;
GO

use ttask;

create login [default] with password = 'default', DEFAULT_DATABASE=ttask, DEFAULT_LANGUAGE=[English], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
create user [default] from login [default];

create login [NewTenantUser] with password = 'NewTenantUser', DEFAULT_DATABASE=ttask, DEFAULT_LANGUAGE=[English], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
create user [NewTenantUser] from login [NewTenantUser];
GO

exec sp_addsrvrolemember [NewTenantUser], 'securityadmin';

CREATE ROLE [tenants] AUTHORIZATION dbo;  

GRANT SELECT, UPDATE, DELETE, INSERT ON SCHEMA::dbo TO [tenants];
DENY SELECT, UPDATE, DELETE, INSERT ON SCHEMA::sys TO [tenants];
DENY SELECT, UPDATE, DELETE, INSERT ON SCHEMA::INFORMATION_SCHEMA TO [tenants];

exec sp_addrolemember 'tenants', [default];

exec sp_addrolemember 'db_accessadmin', [NewTenantUser];
exec sp_addrolemember 'db_owner', [NewTenantUser];
exec sp_addrolemember 'db_securityadmin', [NewTenantUser];
 


CREATE TABLE [GlobalUser] (
	[Id] INTEGER NOT NULL, 
	[Email] VARCHAR(100) NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[UserName] [nvarchar](256) NULL
)


ALTER TABLE GlobalUser ADD constraint globaluser_pk PRIMARY KEY CLUSTERED (Id)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON ) 

CREATE TABLE Payment (
    IdPayment   INTEGER NOT NULL,
    Price       INTEGER NOT NULL,
    Paid        datetime2
)


ALTER TABLE Payment ADD constraint payment_pk PRIMARY KEY CLUSTERED (IdPayment)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON ) 

CREATE TABLE Role (
    Id INTEGER NOT NULL, 
	"Name" VARCHAR(100) NOT NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[NormalizedName] [nvarchar](256) NULL
)


ALTER TABLE Role ADD constraint role_pk PRIMARY KEY CLUSTERED (Id)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON )

CREATE TABLE Service (
    IdService   INTEGER NOT NULL,
    Name        VARCHAR(100) NOT NULL,
    Price       NUMERIC(28) NOT NULL
)


ALTER TABLE Service ADD constraint service_pk PRIMARY KEY CLUSTERED (IdService)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON ) 

CREATE TABLE ServiceOrder (
    OrderDate           DATETIME2 NOT NULL,
    IdTenant     INTEGER NOT NULL,
    IdService   INTEGER NOT NULL,
    IdPayment   INTEGER NOT NULL
)


    
CREATE unique nonclustered index serviceorder__idx ON ServiceOrder ( IdPayment ) 

ALTER TABLE ServiceOrder ADD constraint serviceorder_pk PRIMARY KEY CLUSTERED (IdTenant, OrderDate, IdService)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON ) 



CREATE TABLE Tenant (
    IdTenant   INTEGER NOT NULL,
    Name       VARCHAR(100) NOT NULL,
    Domain     VARCHAR(100) NOT NULL
)


ALTER TABLE Tenant ADD constraint tenant_pk PRIMARY KEY CLUSTERED (IdTenant)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON ) 


ALTER TABLE ServiceOrder
    ADD CONSTRAINT serviceorder_payment_fk FOREIGN KEY ( IdPayment )
        REFERENCES Payment ( IdPayment )
ON DELETE NO ACTION 
    ON UPDATE no action 

ALTER TABLE ServiceOrder
    ADD CONSTRAINT serviceorder_service_fk FOREIGN KEY ( IdService )
        REFERENCES Service ( IdService )
ON DELETE NO ACTION 
    ON UPDATE no action 

ALTER TABLE ServiceOrder
    ADD CONSTRAINT serviceorder_tenant_fk FOREIGN KEY ( IdTenant )
        REFERENCES Tenant ( IdTenant )
ON DELETE NO ACTION 
    ON UPDATE no action




CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[RoleId] INTEGER NOT NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[UserId] INTEGER NOT NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] INTEGER NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] INTEGER NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] INTEGER NOT NULL,
	[RoleId] INTEGER NOT NULL,
		CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
	(
		[UserId] ASC,
		[RoleId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY];

ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[GlobalUser] ([Id]);

ALTER TABLE[dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Role] ([Id]);

CREATE INDEX idx_tenant_domain
ON dbo.Tenant (Domain);

Insert into Service (IdService, Name, Price) values
		(1, 'Basic', 0),
		(2, 'Pro', 249),
		(3, 'Business', 999);


Insert into Role (Id, Name, NormalizedName) values
		(1, 'DomainAdmin', 'DOMAINADMIN'),
		(2, 'NormalUser', 'NORMALUSER'),
		(3, 'Manager', 'MANAGER'),
		(4, 'Developer', 'DEVELOPER'),
		(5, 'ProjectUser', 'PROJECTUSER'),
		(6, 'ProjectLeader', 'PROJECTLEADER'),
		(7, 'ProjectRequest', 'PROJECTREQUEST');

INSERT [dbo].[GlobalUser] ([Id], [Email], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (1, N'admin@admin.cz', 0, N'e3b33164-a016-4c08-a07d-b294a52a9479', 0, 1, NULL, N'ADMIN@ADMIN.CZ', N'ADMIN@ADMIN.CZ', N'AQAAAAEAACcQAAAAEPaAyTVj8wkRetknfveb8yCpgwIWHl8xd1w4+e5lP6RCsOIKRDtci0fSFFJ1HNSxKw==', NULL, 0, N'6VDE4NTIW23HKRPCNZFUNEL2GZESQSVB', 0, N'admin@admin.cz')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 4)
GO


create or alter procedure PNewTenant( 
	@p_name varchar(100), 
	@p_password varchar(100), 
	@p_domain varchar(100))
as
begin
	BEGIN TRANSACTION;  
	declare @sqlUser nvarchar(max)
	set @sqlUser = 'use ttask;' +
        'create login ' + @p_domain + 
            ' with password = ''' + @p_password + ''', DEFAULT_DATABASE=ttask, DEFAULT_LANGUAGE=[English], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF; ' +
        'create user ' + @p_domain + ' from login ' + @p_domain + ';'
	exec (@sqlUser)

	
	declare @sqlSchema nvarchar(max)
	set @sqlSchema = 'create schema ' + @p_domain + ' authorization ' + @p_domain + ';'
	exec (@sqlSchema)


	declare @sqlDef nvarchar(max)
	set @sqlDef = 'ALTER USER ' + @p_domain + ' WITH DEFAULT_SCHEMA = ' + @p_domain + ';'
	exec (@sqlDef)


	EXEC sp_addrolemember 'tenants', @p_domain;  


	declare @sqlCreate nvarchar(max)
	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.Project (
								IdProject INTEGER NOT NULL, 
								"Name" VARCHAR(100) NOT NULL, 
								"Description" VARCHAR(300));' +
							+ 'ALTER TABLE ' + @p_domain + '.Project ADD constraint project_pk PRIMARY KEY CLUSTERED (IdProject)
								 WITH (ALLOW_PAGE_LOCKS = ON , ALLOW_ROW_LOCKS = ON ); '
		
	exec (@sqlCreate)			 
	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.TaskUserComment (
								IdComment INTEGER NOT NULL, 
								Created datetime2 NOT NULL,
								"Text" VARCHAR(1000) NOT NULL, 
								IdUser INTEGER NOT NULL, 
								IdTask INTEGER NOT NULL);'
							+ 'ALTER TABLE ' + @p_domain + '.TaskUserComment ADD constraint comment_pk PRIMARY KEY CLUSTERED (IdComment)
								 WITH (ALLOW_PAGE_LOCKS = ON , ALLOW_ROW_LOCKS = ON ); '

	exec (@sqlCreate)
	
	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.UserSettings (
								Notifications CHAR(1) NOT NULL,
								Coloring CHAR(1) NOT NULL, 
								CustomizeView CHAR(1) NOT NULL, 
								IdUser INTEGER NOT NULL);' +
							+ 'ALTER TABLE ' + @p_domain + '.UserSettings ADD constraint usersettings_pk PRIMARY KEY CLUSTERED (IdUser)
									WITH (ALLOW_PAGE_LOCKS = ON ,ALLOW_ROW_LOCKS = ON );' 

	exec (@sqlCreate)

	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.UserNotification (
								[IdNotification] INTEGER NOT NULL,
								[Text] VARCHAR(1000) NOT NULL, 
								[Read] CHAR(1) NOT NULL, 
								[Created] DATETIME2 NOT NULL, 
								[IdUser] INTEGER NOT NULL);' +
							+ 'ALTER TABLE ' + @p_domain + '.UserNotification ADD constraint [usernotification_pk] PRIMARY KEY CLUSTERED ([IdNotification])
									WITH (ALLOW_PAGE_LOCKS = ON ,ALLOW_ROW_LOCKS = ON );' 

	exec (@sqlCreate)

	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.Task (
								IdTask INTEGER NOT NULL, 
								"Name" VARCHAR(100) NOT NULL, 
								"Description" VARCHAR(1000), 
								Priority VARCHAR(10) NOT NULL,
								Created datetime2 NOT NULL, 
								Completed datetime2, 
								Deadline datetime2 NOT NULL,
								IdUser INTEGER NOT NULL, 
								IdProject INTEGER NOT NULL);' +
							+ 'ALTER TABLE ' + @p_domain + '.Task ADD constraint task_pk PRIMARY KEY CLUSTERED (IdTask)
								 WITH (ALLOW_PAGE_LOCKS = ON , ALLOW_ROW_LOCKS = ON ); '

	exec (@sqlCreate)
	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '."User" (
								Id INTEGER NOT NULL, 
								Email VARCHAR(100) NOT NULL,
								FirstName VARCHAR(100) NOT NULL, 
								Surname VARCHAR(100) NOT NULL,
								Photopath VARCHAR(240), 
								IdTenant INTEGER NOT NULL,
								[AccessFailedCount] [int] NOT NULL,
								[ConcurrencyStamp] [nvarchar](max) NULL,
								[EmailConfirmed] [bit] NOT NULL,
								[LockoutEnabled] [bit] NOT NULL,
								[LockoutEnd] [datetimeoffset](7) NULL,
								[NormalizedEmail] [nvarchar](256) NULL,
								[NormalizedUserName] [nvarchar](256) NULL,
								[PasswordHash] [nvarchar](max) NULL,
								[PhoneNumber] [nvarchar](max) NULL,
								[PhoneNumberConfirmed] [bit] NOT NULL,
								[SecurityStamp] [nvarchar](max) NULL,
								[TwoFactorEnabled] [bit] NOT NULL,
								[UserName] [nvarchar](256) NULL);' +
							+ 'ALTER TABLE ' + @p_domain + '."User" ADD constraint user_pk PRIMARY KEY CLUSTERED (Id)
								 WITH (ALLOW_PAGE_LOCKS = ON , ALLOW_ROW_LOCKS = ON ); '

	exec (@sqlCreate)
	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.UserProject (
								Active CHAR(1) NOT NULL, 
								IdUser INTEGER NOT NULL, 
								IdRole INTEGER NOT NULL, 
								IdProject INTEGER NOT NULL);' +
							+ 'ALTER TABLE ' + @p_domain + '.UserProject ADD constraint userproject_pk PRIMARY KEY CLUSTERED (IdUser, IdProject)
								 WITH (ALLOW_PAGE_LOCKS = ON , ALLOW_ROW_LOCKS = ON ); '
	
	exec (@sqlCreate)	
	set @sqlCreate = 'CREATE TABLE ' + @p_domain + '.UserTask (
								Completed datetime2, 
								IdUser INTEGER NOT NULL, 
								IdTask INTEGER NOT NULL);' +
							+ 'ALTER TABLE ' + @p_domain + '.UserTask ADD constraint usertask_pk PRIMARY KEY CLUSTERED (IdUser, IdTask)
								 WITH (ALLOW_PAGE_LOCKS = ON , ALLOW_ROW_LOCKS = ON ); '

	exec (@sqlCreate)
	set @sqlCreate = 'ALTER TABLE ' + @p_domain + '.TaskUserComment ADD CONSTRAINT comment_task_fk FOREIGN KEY ( IdTask ) ' +
									+ 'REFERENCES ' + @p_domain + '.Task ( IdTask ) ' +
							+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate = 'ALTER TABLE ' + @p_domain + '.TaskUserComment ADD CONSTRAINT comment_user_fk FOREIGN KEY ( IdUser ) ' +
								+ 'REFERENCES ' + @p_domain + '."User" ( Id ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate =  'ALTER TABLE ' + @p_domain + '.UserSettings ADD CONSTRAINT settings_user_fk FOREIGN KEY ( IdUser ) ' +
								+ 'REFERENCES ' + @p_domain + '."User" ( Id ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate =  'ALTER TABLE ' + @p_domain + '.Task ADD CONSTRAINT task_project_fk FOREIGN KEY ( IdProject ) ' +
									+ 'REFERENCES ' + @p_domain + '.Project ( IdProject ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate =  'ALTER TABLE ' + @p_domain +'.UserNotification ADD CONSTRAINT user_usernotification_fk FOREIGN KEY ( IdUser ) 
								REFERENCES ' + @p_domain + '."User" ( Id )
								ON DELETE NO ACTION
								 ON UPDATE no action;'
	exec (@sqlCreate)
	set @sqlCreate = 'ALTER TABLE ' + @p_domain + '.Task ADD CONSTRAINT task_user_fk FOREIGN KEY ( IdUser ) ' +
								+ 'REFERENCES ' + @p_domain + '."User" ( Id ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate =  'ALTER TABLE ' + @p_domain + '."User" ADD CONSTRAINT user_tenant_fk FOREIGN KEY ( IdTenant ) ' +
									+ 'REFERENCES Tenant ( IdTenant ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate = 'ALTER TABLE ' + @p_domain + '.UserProject ADD CONSTRAINT userproject_project_fk FOREIGN KEY ( IdProject ) ' +
									+ 'REFERENCES ' + @p_domain + '.Project ( IdProject ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate = 'ALTER TABLE ' + @p_domain + '.UserProject ADD CONSTRAINT userproject_role_fk FOREIGN KEY ( IdRole ) ' +
									+ 'REFERENCES dbo."Role" ( Id ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;'

	exec (@sqlCreate)
	set @sqlCreate = 'ALTER TABLE ' + @p_domain + '.UserProject ADD CONSTRAINT userproject_user_fk FOREIGN KEY ( IdUser ) ' +
								+ 'REFERENCES ' + @p_domain + '."User" ( Id ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate =  'ALTER TABLE ' + @p_domain + '.UserTask ADD CONSTRAINT usertask_task_fk FOREIGN KEY ( IdTask ) ' +
								+ 'REFERENCES ' + @p_domain + '.Task ( IdTask ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;' 

	exec (@sqlCreate)
	set @sqlCreate =  'ALTER TABLE ' + @p_domain + '.UserTask ADD CONSTRAINT usertask_user_fk FOREIGN KEY ( IdUser ) ' +
								+ 'REFERENCES ' + @p_domain + '."User" ( Id ) ' +
								+ 'ON DELETE NO ACTION' +
								+ ' ON UPDATE no action;'
	exec (@sqlCreate)

		set @sqlCreate =  'CREATE TABLE ' + @p_domain + '.[AspNetUserRoles](
						[UserId] INTEGER NOT NULL,
						[RoleId] INTEGER NOT NULL,
						 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
						(
							[UserId] ASC,
							[RoleId] ASC
						)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
						) ON [PRIMARY];

						ALTER TABLE ' + @p_domain + '.[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
						REFERENCES ' + @p_domain + '.[User] ([Id])
							ON DELETE NO ACTION
							ON UPDATE no action;

						ALTER TABLE ' + @p_domain + '.[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
						REFERENCES [dbo].[Role] ([Id])
							ON DELETE NO ACTION
							ON UPDATE no action;'
	exec (@sqlCreate)

	set @sqlCreate = 'CREATE INDEX idx_task_completed
				ON ' + @p_domain + '.Task (Completed);

				CREATE INDEX idx_task_projekt
				ON ' + @p_domain + '.Task (IdProject);

				CREATE INDEX idx_usertask_completed
				ON ' + @p_domain + '.UserTask (Completed);

				CREATE INDEX idx_comment_task
				ON ' + @p_domain + '.TaskUserComment (IdTask);

				CREATE INDEX idx_user_username
				ON ' + @p_domain + '."User" (UserName);'
	exec (@sqlCreate)

	declare @v_tenantID int = (select COALESCE(max(IdTenant), 0) from Tenant) + 1
	declare @sqlInsert nvarchar(max)
	insert into Tenant( IdTenant, Name, Domain)
		values (@v_tenantID, @p_name, @p_domain);


	declare @v_paymentID int = (select COALESCE(max(IdPayment), 0) from Payment) + 1

	insert into Payment (IdPayment, Price, Paid)
		values (@v_paymentID, 0, SYSDATETIME());

	insert into ServiceOrder (OrderDate, IdTenant, IdService, IdPayment)
		values (CONVERT (date, SYSDATETIME()), @v_tenantID, 1, @v_paymentID);
	COMMIT;

end

GO


exec PNewTenant 'Test data', 'B6945EAE04D2A4775CBD09270CB654E66DADCE477AC0B6AE92510A0BD4DA9862', 'test';
GO
exec PNewTenant 'Basic test', '7BEA1D2CC3A79656065A3F2225CE69B4E9418EE73BC622890E380D6AFDBAFFEF', 'basic';
GO
exec PNewTenant 'PRO test', '879EF319065CC7919DA1CBDACB3766C09DE36E9E66BE79BBBB2AB6896D8C55D9', 'pro';
GO

INSERT [dbo].[Payment] ([IdPayment], [Price], [Paid]) VALUES (4, 999, SYSDATETIME())
GO
INSERT [dbo].[Payment] ([IdPayment], [Price], [Paid]) VALUES (5, 249, CAST(N'2020-03-01T21:32:30.4123167' AS DateTime2))
GO
INSERT [dbo].[ServiceOrder] ([OrderDate], [IdTenant], [IdService], [IdPayment]) VALUES (SYSDATETIME(), 1, 3, 4)
GO
INSERT [dbo].[ServiceOrder] ([OrderDate], [IdTenant], [IdService], [IdPayment]) VALUES (SYSDATETIME(), 3, 2, 5)
GO



INSERT [pro].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (1, N'tenant@test.cz', N'Tenant', N'Admin', NULL, 3, 0, N'7d68c892-21ad-440c-bb1f-28e72eb97ed0', 0, 1, NULL, N'TENANT@TEST.CZ', N'TENANT@TEST.CZ', N'AQAAAAEAACcQAAAAEFTJU+0oOuu2PxjbYWM+amCRoXWKkv0k/EIf/a6nmSe1nzLYW3B26qp8Qo++K+VNFw==', N'613 612 611', 0, N'PLX7UM7WY3GVAB7M4MFUPIOLYD7G2ICQ', 0, N'tenant@test.cz')
GO
INSERT [pro].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (2, N'user1@test.cz', N'User', N'One', NULL, 3, 0, N'b492ab30-bfce-4f83-8f2d-14a478cc30b9', 0, 1, NULL, N'USER1@TEST.CZ', N'USER1@TEST.CZ', N'AQAAAAEAACcQAAAAEPaqQtYWiwb6L1ErmuaiJ0Q9+Xnm3qa90cM8dC9KoTf6/LiJxluTzEdIOJx4MHur5w==', N'613 612 611', 0, N'XCQFBXKRPY7YAIJE64KR4FNLWRPWJMUQ', 0, N'user1@test.cz')
GO
INSERT [pro].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 1)
GO
INSERT [pro].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 2)
GO
INSERT [pro].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (1, N'User Tenant Admin has signed up in your application.', N'0', CAST(N'2020-04-01T21:11:03.1972726' AS DateTime2), 1)
GO
INSERT [pro].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (2, N'User User One has signed up in your application.', N'0', CAST(N'2020-04-01T21:11:35.9845643' AS DateTime2), 1)
GO
INSERT [pro].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 1)
GO
INSERT [pro].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 2)
GO
INSERT [pro].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 3)
GO
INSERT [pro].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (2, 2)
GO
INSERT [test].[Project] ([IdProject], [Name], [Description]) VALUES (1, N'Test projekt 1', 'This is description of project 1.')
GO
INSERT [test].[Project] ([IdProject], [Name], [Description]) VALUES (2, N'Test projekt 2', 'This is description of project 2.')
GO
INSERT [test].[Project] ([IdProject], [Name], [Description]) VALUES (3, N'Test projekt 3', null)
GO
INSERT [test].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (1, N'tenant@test.cz', N'Tenant', N'Admin', NULL, 1, 0, N'3902cd28-b96a-48d4-92f7-e4431c80780e', 0, 1, NULL, N'TENANT@TEST.CZ', N'TENANT@TEST.CZ', N'AQAAAAEAACcQAAAAECTkZwM0Vfmj0Zf2sCbBj87cYmmrRDDZmBtFY21iHYHaxi6898cWuWY/LT4ow25RzQ==', N'613 612 611', 0, N'HTWKGMIAMOOZUHZI5UOUDIEIY5F5PCDO', 0, N'tenant@test.cz')
GO
INSERT [test].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (2, N'user1@test.cz', N'User', N'One', NULL, 1, 0, N'7ec46ba4-5290-42f0-a429-ed92d09419b4', 0, 1, NULL, N'USER1@TEST.CZ', N'USER1@TEST.CZ', N'AQAAAAEAACcQAAAAEGDS7bQ4Yk5BARJZXYvvAlt82DKP3hX/2IvXDetP9Yu61YOgwQ9Nu8JFupH3Vw4FXA==', N'613 612 611', 0, N'F33IPIHFWRIDR6BZCVPNQUMA35K7GJIZ', 0, N'user1@test.cz')
GO
INSERT [test].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (3, N'user2@test.cz', N'User', N'Two', NULL, 1, 0, N'ed1ec6f5-24d9-4f1b-b9c7-9ded93438332', 0, 1, NULL, N'USER2@TEST.CZ', N'USER2@TEST.CZ', N'AQAAAAEAACcQAAAAEKhgK5Ttkh4M0i3pq18wd58nyiveEZ6BQHncdJ+5ib0zgkrq+kZHNpe53FNMHtV3+A==', N'613 612 611', 0, N'LHHL5CDXMK5X5OWGBWKRVQD5CLQ7YSWP', 0, N'user2@test.cz')
GO
INSERT [test].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (4, N'user3@test.cz', N'User', N'Three', NULL, 1, 0, N'ec120dea-8548-4256-a417-4a16a2fe19ae', 0, 1, NULL, N'USER3@TEST.CZ', N'USER3@TEST.CZ', N'AQAAAAEAACcQAAAAEKOkX808tuENQ103xvYpgWPwBwGyMGEp3rXBQ2hTE+rgTDeCltS9hU+ipSfMHbxobA==', N'613 612 611', 0, N'4ABSTKBD7W7ASPASHBVRJOIDH4RZI32E', 0, N'user3@test.cz')
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (1, N'Task high priority', N'Test task s high prioritou', N'high', CAST(N'2020-04-01T20:47:29.8763614' AS DateTime2), NULL, CAST(N'2020-05-08T20:20:00.0000000' AS DateTime2), 1, 1)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (2, N'Low proirity', N'Completed task with low priority.', N'low', CAST(N'2020-04-01T20:49:34.8068067' AS DateTime2), CAST(N'2020-05-15T20:51:01.8920135' AS DateTime2), CAST(N'2020-05-30T11:11:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (3, N'Critical task', N'Test critical task', N'critical', CAST(N'2020-04-01T20:50:51.0717073' AS DateTime2), NULL, CAST(N'2020-05-30T21:00:00.0000000' AS DateTime2), 1, 2)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (4, N'Test task 4', N'desc', N'low', CAST(N'2020-04-01T20:53:17.6721932' AS DateTime2), NULL, CAST(N'2020-10-15T15:10:00.0000000' AS DateTime2), 1, 2)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (5, N'Medium task', N'medium task assigned only to one person', N'medium', CAST(N'2020-04-01T20:55:46.7657685' AS DateTime2), NULL, CAST(N'2020-05-20T20:20:00.0000000' AS DateTime2), 1, 1)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (6, N'Critical P1', N'crtical task in project 1', N'critical', CAST(N'2020-04-01T20:56:54.3224945' AS DateTime2), NULL, CAST(N'2020-06-20T12:12:00.0000000' AS DateTime2), 1, 1)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (7, N'completed task ', N'completed', N'medium', CAST(N'2020-04-01T20:57:40.6478530' AS DateTime2), CAST(N'2020-04-01T20:58:11.5170613' AS DateTime2), CAST(N'2020-05-14T12:12:00.0000000' AS DateTime2), 1, 1)
GO
INSERT [test].[Task] ([IdTask], [Name], [Description], [Priority], [Created], [Completed], [Deadline], [IdUser], [IdProject]) VALUES (8, N'Low in P1', N'low priority task in project 1', N'low', CAST(N'2020-04-01T21:00:06.3550730' AS DateTime2), NULL, CAST(N'2020-04-08T12:12:00.0000000' AS DateTime2), 1, 1)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 1, 6, 1)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 1, 6, 2)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 1, 6, 3)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 2, 6, 1)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 2, 5, 2)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 2, 7, 3)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 3, 5, 1)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 3, 5, 2)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 4, 5, 1)
GO
INSERT [test].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 4, 7, 2)
GO
INSERT [test].[TaskUserComment] ([IdComment], [Created], [Text], [IdUser], [IdTask]) VALUES (1, CAST(N'2020-04-01T21:00:52.2605725' AS DateTime2), N'Test comment from tenant admin
', 1, 6)
GO
INSERT [test].[TaskUserComment] ([IdComment], [Created], [Text], [IdUser], [IdTask]) VALUES (2, CAST(N'2020-04-01T21:01:32.3567725' AS DateTime2), N'Reply user 1', 2, 6)
GO
INSERT [test].[TaskUserComment] ([IdComment], [Created], [Text], [IdUser], [IdTask]) VALUES (3, CAST(N'2020-04-01T21:01:38.2753977' AS DateTime2), N'Test', 2, 6)
GO
INSERT [test].[TaskUserComment] ([IdComment], [Created], [Text], [IdUser], [IdTask]) VALUES (4, CAST(N'2020-04-01T21:01:43.6499697' AS DateTime2), N'test', 2, 6)
GO
INSERT [test].[TaskUserComment] ([IdComment], [Created], [Text], [IdUser], [IdTask]) VALUES (5, CAST(N'2020-04-01T21:03:21.0395407' AS DateTime2), N'Reply', 1, 6)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 1, 1)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (CAST(N'2020-04-01T20:51:01.8799452' AS DateTime2), 1, 2)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 1, 4)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 1, 8)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 2, 1)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (CAST(N'2020-04-01T20:54:25.0804228' AS DateTime2), 2, 3)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 2, 5)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (CAST(N'2020-04-01T20:57:54.6343569' AS DateTime2), 2, 7)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 2, 8)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 3, 1)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 3, 3)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 3, 4)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 3, 6)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (CAST(N'2020-04-01T20:58:11.5142875' AS DateTime2), 3, 7)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 3, 8)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 4, 1)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 4, 6)
GO
INSERT [test].[UserTask] ([Completed], [IdUser], [IdTask]) VALUES (NULL, 4, 8)
GO
INSERT [test].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'1', N'1', N'1', 1)
GO
INSERT [test].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'1', N'1', N'1', 2)
GO
INSERT [test].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 3)
GO
INSERT [test].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 4)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (1, N'User Tenant Admin has signed up in your application.', N'1', CAST(N'2020-04-01T20:38:42.1237609' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (2, N'User User One has signed up in your application.', N'0', CAST(N'2020-04-01T20:39:12.7378961' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (3, N'User User Two has signed up in your application.', N'0', CAST(N'2020-04-01T20:39:40.5671855' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (4, N'User User Three has signed up in your application.', N'0', CAST(N'2020-04-01T20:40:44.3944354' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (5, N'You have been promoted to manager! You can create your own projects now.', N'0', CAST(N'2020-04-01T20:42:25.7294519' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (6, N'You have been invited to project Test projekt 1', N'0', CAST(N'2020-04-01T20:43:30.4349860' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (7, N'You have been invited to project Test projekt 1', N'0', CAST(N'2020-04-01T20:43:33.5649717' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (8, N'You have been invited to project Test projekt 1', N'0', CAST(N'2020-04-01T20:43:36.5353964' AS DateTime2), 4)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (9, N'You have been invited to project Test projekt 2', N'0', CAST(N'2020-04-01T20:43:47.8034531' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (10, N'You have been invited to project Test projekt 2', N'0', CAST(N'2020-04-01T20:43:51.5168026' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (11, N'You have been invited to project Test projekt 2', N'0', CAST(N'2020-04-01T20:44:06.6203764' AS DateTime2), 4)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (12, N'You have been invited to project Test projekt 3', N'0', CAST(N'2020-04-01T20:44:20.5616625' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (13, N'You have been promoted in project Test projekt 1', N'0', CAST(N'2020-04-01T20:46:11.2498160' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (14, N'Task Task high priority has been assigned to you.', N'0', CAST(N'2020-04-01T20:47:29.9062032' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (15, N'Task Task high priority has been assigned to you.', N'0', CAST(N'2020-04-01T20:47:29.9091840' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (16, N'Task Task high priority has been assigned to you.', N'0', CAST(N'2020-04-01T20:47:29.9119177' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (17, N'Task Task high priority has been assigned to you.', N'0', CAST(N'2020-04-01T20:47:29.9156856' AS DateTime2), 4)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (18, N'Task Low proirity has been assigned to you.', N'0', CAST(N'2020-04-01T20:49:34.8129109' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (19, N'Task Critical task has been assigned to you.', N'0', CAST(N'2020-04-01T20:50:51.0764844' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (20, N'Task Critical task has been assigned to you.', N'0', CAST(N'2020-04-01T20:50:51.0794796' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (21, N'User Tenant Admin has completed your task Low proirity.', N'0', CAST(N'2020-04-01T20:51:01.8996178' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (22, N'Task Test task 4 has been assigned to you.', N'0', CAST(N'2020-04-01T20:53:17.6768108' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (23, N'Task Test task 4 has been assigned to you.', N'0', CAST(N'2020-04-01T20:53:17.6800656' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (24, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:54:10.5618788' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (25, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:54:10.5640880' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (26, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:54:10.5656783' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (27, N'User User One has completed your task Critical task.', N'0', CAST(N'2020-04-01T20:54:25.0858474' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (28, N'Task Medium task has been assigned to you.', N'0', CAST(N'2020-04-01T20:55:46.7709266' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (29, N'Task Critical P1 has been assigned to you.', N'0', CAST(N'2020-04-01T20:56:54.3270824' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (30, N'Task Critical P1 has been assigned to you.', N'0', CAST(N'2020-04-01T20:56:54.3300054' AS DateTime2), 4)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (31, N'Task completed task  has been assigned to you.', N'0', CAST(N'2020-04-01T20:57:40.6519563' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (32, N'Task completed task  has been assigned to you.', N'0', CAST(N'2020-04-01T20:57:40.6556142' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (33, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:57:49.0007553' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (34, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:57:49.0034176' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (35, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:57:49.0053643' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (36, N'User User One has completed your task completed task .', N'0', CAST(N'2020-04-01T20:57:54.6379932' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (37, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:58:05.3397505' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (38, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:58:05.3413778' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (39, N'Deadline on task Critical task is in less than one hour!', N'0', CAST(N'2020-04-01T20:58:05.3428827' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (40, N'User User Two has completed your task completed task .', N'0', CAST(N'2020-04-01T20:58:11.5197126' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (41, N'Task Low in P1 has been assigned to you.', N'0', CAST(N'2020-04-01T21:00:06.3590336' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (42, N'Task Low in P1 has been assigned to you.', N'0', CAST(N'2020-04-01T21:00:06.3619019' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (43, N'Task Low in P1 has been assigned to you.', N'0', CAST(N'2020-04-01T21:00:06.3643788' AS DateTime2), 3)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (44, N'Task Low in P1 has been assigned to you.', N'0', CAST(N'2020-04-01T21:00:06.3666396' AS DateTime2), 4)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (45, N'Your task Critical P1 has new comment', N'0', CAST(N'2020-04-01T21:00:52.2778392' AS DateTime2), 1)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (46, N'Your task Critical P1 has new comment', N'0', CAST(N'2020-04-01T21:01:32.3595542' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (47, N'Your task Critical P1 has new comment', N'0', CAST(N'2020-04-01T21:01:38.2779972' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (48, N'Your task Critical P1 has new comment', N'0', CAST(N'2020-04-01T21:01:43.6527762' AS DateTime2), 2)
GO
INSERT [test].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (49, N'Your task Critical P1 has new comment', N'0', CAST(N'2020-04-01T21:03:21.0422967' AS DateTime2), 1)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 1)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 2)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 3)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (2, 2)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (3, 2)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (3, 3)
GO
INSERT [test].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (4, 2)
GO
INSERT [basic].[Project] ([IdProject], [Name]) VALUES (1, N'Projekt')
GO
INSERT [basic].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (1, N'tenant@test.cz', N'Tenant', N'Admin', NULL, 2, 0, N'9f5725b5-f51e-4cff-bc44-8db6669f8a47', 0, 1, NULL, N'TENANT@TEST.CZ', N'TENANT@TEST.CZ', N'AQAAAAEAACcQAAAAEB8KNkTVH5rkx+ojPPf5z0lz9xrfWbUMIrWX/e2TVTBvGBeMJFBqVDV0mDZ+i8AxAg==', N'613 612 611', 0, N'AFUROJI65RY2RA6NRDX7QYZ5VMWKFRJL', 0, N'tenant@test.cz')
GO
INSERT [basic].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (2, N'user1@test.cz', N'User', N'One', NULL, 2, 0, N'7c0b6a7a-02cc-423c-a6ba-65d151482063', 0, 1, NULL, N'USER1@TEST.CZ', N'USER1@TEST.CZ', N'AQAAAAEAACcQAAAAEAjydqLAs5m+49KIYAJiDhuk3BnA9ujPwDR68kpK227QNHdU8GPAE6OJFlW7FW3bOw==', N'613 612 611', 0, N'ESTA4RQUMKTDRJRXLTONRJAU7WSEIESI', 0, N'user1@test.cz')
GO
INSERT [basic].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (3, N'user2@test.cz', N'User', N'Two', NULL, 2, 0, N'bfc4f966-1568-49f2-8f34-687d8b16826d', 0, 1, NULL, N'USER2@TEST.CZ', N'USER2@TEST.CZ', N'AQAAAAEAACcQAAAAEKcpGWJwxX11Apbq6QCVJmw+9uEMoZbeMAtp8VPku7ew2jaQJGG0Zj56cA0ImL2T6A==', N'613 612 611 ', 0, N'4QMDVMUCQPSNJK6FMCCUTEXWB4U34KAM', 0, N'user2@test.cz')
GO
INSERT [basic].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (4, N'user3@test.cz', N'User', N'Three', NULL, 2, 0, N'ef922dbb-a5fb-4b3b-b1b3-cb638b8457bb', 0, 1, NULL, N'USER3@TEST.CZ', N'USER3@TEST.CZ', N'AQAAAAEAACcQAAAAEErV5kD8omcT1eg+tjVvSYiFnOFyAwv/D39vFCRc6NjQLlMcAv1uFfq9cRIUiQRfeg==', N'613 612 611', 0, N'PLPBPDRTMCN7QFTJV2BDLTINLNQAZO6M', 0, N'user3@test.cz')
GO
INSERT [basic].[User] ([Id], [Email], [FirstName], [Surname], [Photopath], [IdTenant], [AccessFailedCount], [ConcurrencyStamp], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName]) VALUES (5, N'user4@test.cz', N'User', N'Four', NULL, 2, 0, N'1fab4279-8cd6-4823-89dd-40773990da81', 0, 1, NULL, N'USER4@TEST.CZ', N'USER4@TEST.CZ', N'AQAAAAEAACcQAAAAEB6weJ210XXjk8gIq/d8rlJ/io77uNHoHwyGtePDu3Ufdda2mnncGgIfXfwpVVs69A==', N'613 612 611', 0, N'MHPHVQPNQGIBJXGRK7GUCV4WWP4EZMKZ', 0, N'user4@test.cz')
GO
INSERT [basic].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 1, 6, 1)
GO
INSERT [basic].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 2, 7, 1)
GO
INSERT [basic].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 3, 7, 1)
GO
INSERT [basic].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 4, 7, 1)
GO
INSERT [basic].[UserProject] ([Active], [IdUser], [IdRole], [IdProject]) VALUES (N'1', 5, 7, 1)
GO
INSERT [basic].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 1)
GO
INSERT [basic].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 2)
GO
INSERT [basic].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 3)
GO
INSERT [basic].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 4)
GO
INSERT [basic].[UserSettings] ([Notifications], [Coloring], [CustomizeView], [IdUser]) VALUES (N'0', N'0', N'0', 5)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (1, N'User Tenant Admin has signed up in your application.', N'0', CAST(N'2020-04-01T21:12:52.5336246' AS DateTime2), 1)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (2, N'User User One has signed up in your application.', N'0', CAST(N'2020-04-01T21:13:48.5421985' AS DateTime2), 1)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (3, N'User User Two has signed up in your application.', N'0', CAST(N'2020-04-01T21:14:10.1831142' AS DateTime2), 1)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (4, N'User User Three has signed up in your application.', N'0', CAST(N'2020-04-01T21:14:32.6219063' AS DateTime2), 1)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (5, N'User User Four has signed up in your application.', N'0', CAST(N'2020-04-01T21:14:52.0017338' AS DateTime2), 1)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (6, N'You have been invited to project Projekt', N'0', CAST(N'2020-04-01T21:15:25.3031853' AS DateTime2), 2)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (7, N'You have been invited to project Projekt', N'0', CAST(N'2020-04-01T21:15:27.6966326' AS DateTime2), 3)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (8, N'You have been invited to project Projekt', N'0', CAST(N'2020-04-01T21:15:30.0096010' AS DateTime2), 4)
GO
INSERT [basic].[UserNotification] ([IdNotification], [Text], [Read], [Created], [IdUser]) VALUES (9, N'You have been invited to project Projekt', N'0', CAST(N'2020-04-01T21:15:32.4961922' AS DateTime2), 5)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 1)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 2)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 3)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (2, 2)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (3, 2)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (4, 2)
GO
INSERT [basic].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (5, 2)
GO



