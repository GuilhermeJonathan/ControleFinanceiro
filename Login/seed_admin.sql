-- ============================================================
-- Seed: Usuário Admin
-- Senha: Admin@123!
-- Hash BCrypt gerado com custo 11
-- ============================================================

DECLARE @UserId   UNIQUEIDENTIFIER = NEWID();
DECLARE @ProfileId UNIQUEIDENTIFIER = NEWID();

-- 1. Perfil Admin (UserType = 1 = Internal)
INSERT INTO Profiles (Id, Name, UserTypeId, IsActive, CreatedAt)
VALUES (@ProfileId, 'Administrador', 1, 1, GETUTCDATE());

-- 2. Usuário Admin
INSERT INTO Users (
    Id,
    Name,
    Email,
    Document,
    PasswordHash,
    UserTypeId,
    IsActive,
    IsBlocked,
    ProfileId,
    HierarchyId,
    FreightForwarderId,
    Cellphone,
    Phone,
    Occupation,
    Address,
    AvatarUrl,
    CountryId,
    Region,
    CreatedAt,
    UpdatedAt
)
VALUES (
    @UserId,
    'Administrador',
    'admin@plataforma.com',
    '00000000000',                                               -- CPF placeholder
    '$2a$11$zslyjXp/VpJ1Sqh1f6/Zf.6eUeyOIMyxCXCZ0JFaUR6I5UzqxcKLO', -- Admin@123!
    1,                                                           -- UserType: Internal
    1,                                                           -- IsActive = true
    0,                                                           -- IsBlocked = false
    @ProfileId,
    NULL,
    NULL,
    NULL,
    NULL,
    'Administrador do Sistema',
    NULL,
    NULL,
    NULL,
    NULL,
    GETUTCDATE(),
    NULL
);

SELECT 'Usuário admin criado com sucesso!' AS Mensagem,
       @UserId   AS UserId,
       @ProfileId AS ProfileId;
