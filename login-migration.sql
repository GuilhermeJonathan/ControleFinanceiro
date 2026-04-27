CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "AcceptedTerms" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TermName" character varying(100) NOT NULL,
    "IpAddress" character varying(50),
    "UserAgent" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_AcceptedTerms" PRIMARY KEY ("Id")
);

CREATE TABLE "CargoAgentClients" (
    "Id" uuid NOT NULL,
    "CargoAgentCompanyId" integer NOT NULL,
    "CargoAgentClientCompanyId" integer NOT NULL,
    "Associated" boolean NOT NULL,
    "UnassociatedReason" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_CargoAgentClients" PRIMARY KEY ("Id")
);

CREATE TABLE "FreightForwarders" (
    "Id" uuid NOT NULL,
    "CompanyName" character varying(200) NOT NULL,
    "Document" character varying(14) NOT NULL,
    "Email" character varying(256),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_FreightForwarders" PRIMARY KEY ("Id")
);

CREATE TABLE "Hierarchies" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Hierarchies" PRIMARY KEY ("Id")
);

CREATE TABLE "Invites" (
    "Id" uuid NOT NULL,
    "Token" character varying(100) NOT NULL,
    "Email" character varying(200),
    "UsedAt" timestamp with time zone,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Invites" PRIMARY KEY ("Id")
);

CREATE TABLE "Modules" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "HiddenMenu" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Modules" PRIMARY KEY ("Id")
);

CREATE TABLE "Profiles" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "UserTypeId" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Profiles" PRIMARY KEY ("Id")
);

CREATE TABLE "CargoAgentPermissions" (
    "CargoAgentId" uuid NOT NULL,
    "Documents" boolean NOT NULL,
    "Tracking" boolean NOT NULL,
    "Booking" boolean NOT NULL,
    "Bl" boolean NOT NULL,
    CONSTRAINT "PK_CargoAgentPermissions" PRIMARY KEY ("CargoAgentId"),
    CONSTRAINT "FK_CargoAgentPermissions_CargoAgentClients_CargoAgentId" FOREIGN KEY ("CargoAgentId") REFERENCES "CargoAgentClients" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FreightForwarderPermissions" (
    "FreightForwarderId" uuid NOT NULL,
    "Documents" boolean NOT NULL,
    "Tracking" boolean NOT NULL,
    "Booking" boolean NOT NULL,
    "Bl" boolean NOT NULL,
    CONSTRAINT "PK_FreightForwarderPermissions" PRIMARY KEY ("FreightForwarderId"),
    CONSTRAINT "FK_FreightForwarderPermissions_FreightForwarders_FreightForwar~" FOREIGN KEY ("FreightForwarderId") REFERENCES "FreightForwarders" ("Id") ON DELETE CASCADE
);

CREATE TABLE "HierarchyCompanies" (
    "HierarchyId" uuid NOT NULL,
    "ClientId" integer NOT NULL,
    CONSTRAINT "PK_HierarchyCompanies" PRIMARY KEY ("HierarchyId", "ClientId"),
    CONSTRAINT "FK_HierarchyCompanies_Hierarchies_HierarchyId" FOREIGN KEY ("HierarchyId") REFERENCES "Hierarchies" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ModuleFunctions" (
    "Id" uuid NOT NULL,
    "ModuleId" uuid NOT NULL,
    "Name" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_ModuleFunctions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ModuleFunctions_Modules_ModuleId" FOREIGN KEY ("ModuleId") REFERENCES "Modules" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Permissions" (
    "ProfileId" uuid NOT NULL,
    "ModuleId" uuid NOT NULL,
    "FunctionId" uuid NOT NULL,
    CONSTRAINT "PK_Permissions" PRIMARY KEY ("ProfileId", "ModuleId", "FunctionId"),
    CONSTRAINT "FK_Permissions_Profiles_ProfileId" FOREIGN KEY ("ProfileId") REFERENCES "Profiles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Email" character varying(256) NOT NULL,
    "Document" character varying(14) NOT NULL,
    "Cellphone" character varying(20),
    "Phone" character varying(20),
    "Occupation" character varying(200),
    "Address" text,
    "AvatarUrl" text,
    "UserTypeId" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "IsBlocked" boolean NOT NULL,
    "PasswordHash" text NOT NULL,
    "ProfileId" uuid,
    "HierarchyId" uuid,
    "FreightForwarderId" uuid,
    "CountryId" integer,
    "Region" character varying(100),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Profiles_ProfileId" FOREIGN KEY ("ProfileId") REFERENCES "Profiles" ("Id") ON DELETE SET NULL
);

CREATE TABLE "UserRestrictions" (
    "UserId" uuid NOT NULL,
    "ModuleId" uuid NOT NULL,
    "CompanyId" integer NOT NULL,
    CONSTRAINT "PK_UserRestrictions" PRIMARY KEY ("UserId", "ModuleId", "CompanyId"),
    CONSTRAINT "FK_UserRestrictions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AcceptedTerms_UserId_TermName" ON "AcceptedTerms" ("UserId", "TermName");

CREATE UNIQUE INDEX "IX_FreightForwarders_Document" ON "FreightForwarders" ("Document");

CREATE UNIQUE INDEX "IX_Invites_Token" ON "Invites" ("Token");

CREATE INDEX "IX_ModuleFunctions_ModuleId" ON "ModuleFunctions" ("ModuleId");

CREATE INDEX "IX_Users_Document" ON "Users" ("Document");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE INDEX "IX_Users_ProfileId" ON "Users" ("ProfileId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260427171839_InitialCreate', '10.0.7');

COMMIT;

