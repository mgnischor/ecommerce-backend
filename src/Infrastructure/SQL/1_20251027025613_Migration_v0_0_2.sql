CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE public.users (
    id uuid NOT NULL,
    created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
    "UpdatedBy" uuid NOT NULL,
    "AccessLevel" integer NOT NULL,
    address character varying(500) NOT NULL,
    city character varying(100) NOT NULL,
    country character varying(100) NOT NULL,
    email character varying(255) NOT NULL,
    password_hash character varying(256) NOT NULL,
    username character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL,
    "IsBanned" boolean NOT NULL,
    "IsDebugEnabled" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "IsEmailVerified" boolean NOT NULL,
    "Groups" uuid[] NOT NULL,
    "FavoriteProducts" uuid[] NOT NULL,
    birth_date date NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_users" PRIMARY KEY (id)
);

CREATE INDEX ix_users_created_at ON public.users (created_at);

CREATE UNIQUE INDEX ix_users_email ON public.users (email);

CREATE UNIQUE INDEX ix_users_username ON public.users (username);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027025613_Migration_v0_0_2', '9.0.10');

COMMIT;

