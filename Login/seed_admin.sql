-- ============================================================
-- Seed: Usuário Admin
-- Senha: Admin@123!
-- Hash BCrypt gerado com custo 11
-- ============================================================

DO $$
DECLARE
    v_user_id    UUID := gen_random_uuid();
    v_profile_id UUID := gen_random_uuid();
BEGIN

    -- 1. Perfil Admin (UserType = 1 = Internal)
    INSERT INTO "Profiles" ("Id", "Name", "UserTypeId", "IsActive", "CreatedAt")
    VALUES (v_profile_id, 'Administrador', 1, true, NOW());

    -- 2. Usuário Admin
    INSERT INTO "Users" (
        "Id",
        "Name",
        "Email",
        "Document",
        "PasswordHash",
        "UserTypeId",
        "IsActive",
        "IsBlocked",
        "ProfileId",
        "HierarchyId",
        "FreightForwarderId",
        "Cellphone",
        "Phone",
        "Occupation",
        "Address",
        "AvatarUrl",
        "CountryId",
        "Region",
        "CreatedAt",
        "UpdatedAt"
    )
    VALUES (
        v_user_id,
        'Administrador',
        'admin@plataforma.com',
        '00000000000',
        '$2a$11$zslyjXp/VpJ1Sqh1f6/Zf.6eUeyOIMyxCXCZ0JFaUR6I5UzqxcKLO', -- Admin@123!
        1,
        true,
        false,
        v_profile_id,
        NULL,
        NULL,
        NULL,
        NULL,
        'Administrador do Sistema',
        NULL,
        NULL,
        NULL,
        NULL,
        NOW(),
        NULL
    );

    RAISE NOTICE 'Usuário admin criado com sucesso! UserId: %, ProfileId: %', v_user_id, v_profile_id;

END $$;
