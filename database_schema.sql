-- =====================================================
-- 🚀 SCHEMA COMPLETO DE BASE DE DATOS - VERSIÓN FINAL
-- Sistema de Alquiler de Vehículos
-- Incluye: Registro Manual + OAuth (Google, Facebook, Twitter)
-- Base de datos: PostgreSQL
-- Fecha: 2025-06-01
-- =====================================================

-- =====================================================
-- 1. TABLA USUARIOS (Base para todos los roles + OAuth)
-- =====================================================

CREATE TABLE "users" (
                         "id" varchar(36) PRIMARY KEY,
                         "name" varchar(100) NOT NULL,
                         "email" varchar(100) UNIQUE NOT NULL,
                         "password" varchar(255), -- ✅ OPCIONAL para OAuth
                         "role" varchar(20) NOT NULL,
                         "status" varchar(20) NOT NULL DEFAULT 'pending',
                         "profile_image" varchar(255),

    -- ✅ CAMPOS OAUTH
                         "oauth_provider" varchar(20), -- 'google', 'facebook', 'twitter'
                         "oauth_id" varchar(100),
                         "is_oauth_user" boolean DEFAULT false,
                         "email_verified" boolean DEFAULT false,

    -- ✅ TIMESTAMPS
                         "registration_date" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                         "last_access" timestamp
);

-- =====================================================
-- 2. TABLA CUENTAS OAUTH (Múltiples proveedores)
-- =====================================================

CREATE TABLE "user_oauth_accounts" (
                                       "id" varchar(36) PRIMARY KEY,
                                       "user_id" varchar(36) NOT NULL,
                                       "provider" varchar(20) NOT NULL, -- 'google', 'facebook', 'twitter'
                                       "provider_id" varchar(100) NOT NULL,
                                       "provider_email" varchar(100),
                                       "provider_name" varchar(100),
                                       "provider_picture" varchar(255),
                                       "access_token" text, -- Encriptado
                                       "refresh_token" text, -- Encriptado
                                       "token_expires_at" timestamp,
                                       "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                       "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                       UNIQUE(provider, provider_id)
);

-- =====================================================
-- 3. TABLA CLIENTES
-- =====================================================

CREATE TABLE "clients" (
                           "id" varchar(36) PRIMARY KEY,
                           "user_id" varchar(36) NOT NULL,
                           "dni" varchar(20) NOT NULL,
                           "phone" varchar(20) NOT NULL,
                           "address" text,
                           "city" varchar(100),
                           "postal_code" varchar(10),
                           "country" varchar(50) DEFAULT 'Perú',
                           "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                           "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 4. TABLA ADMINISTRADORES
-- =====================================================

CREATE TABLE "admins" (
                          "id" varchar(36) PRIMARY KEY,
                          "user_id" varchar(36) NOT NULL,
                          "department" varchar(50),
                          "position" varchar(50),
                          "access_level" varchar(20) DEFAULT 'basic',
                          "last_activity" timestamp,
                          "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                          "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 5. TABLA PROVEEDORES (Personas y Empresas + OAuth)
-- =====================================================

CREATE TABLE "providers" (
                             "id" varchar(36) PRIMARY KEY,
                             "user_id" varchar(36) NOT NULL,

    -- ✅ CAMPOS PARA PERSONAS (Nuevo flujo)
                             "dni" varchar(20),
                             "driver_license" varchar(50),

    -- ✅ CAMPOS PARA EMPRESAS (Opcional/Futuro)
                             "business_name" varchar(100),
                             "tax_id" varchar(20),

    -- ✅ CAMPOS COMUNES
                             "phone" varchar(20) NOT NULL,
                             "address" text,
                             "city" varchar(100),
                             "postal_code" varchar(10),
                             "country" varchar(50) DEFAULT 'Perú',

    -- ✅ CAMPOS FINANCIEROS
                             "balance" decimal(10,2) DEFAULT 0,
                             "total_earnings" decimal(10,2) DEFAULT 0,
                             "pending_payments" decimal(10,2) DEFAULT 0,
                             "status" varchar(20) DEFAULT 'pending',
                             "bank_account" varchar(50),

    -- ✅ TIMESTAMPS
                             "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                             "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 6. TABLA VEHÍCULOS
-- =====================================================

CREATE TABLE "vehicles" (
                            "id" varchar(36) PRIMARY KEY,
                            "provider_id" varchar(36),
                            "is_company_owned" boolean DEFAULT false,
                            "brand" varchar(50) NOT NULL,
                            "model" varchar(50) NOT NULL,
                            "year" int NOT NULL,
                            "type" varchar(30) NOT NULL,
                            "description" text,
                            "daily_rate" decimal(10,2) NOT NULL,
                            "location" varchar(100) NOT NULL,
                            "status" varchar(20) DEFAULT 'pending_approval',
                            "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                            "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 7. TABLA IMÁGENES DE VEHÍCULOS
-- =====================================================

CREATE TABLE "vehicle_images" (
                                  "id" varchar(36) PRIMARY KEY,
                                  "vehicle_id" varchar(36) NOT NULL,
                                  "image_url" varchar(255) NOT NULL,
                                  "is_primary" boolean DEFAULT false,
                                  "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 8. TABLA CARACTERÍSTICAS DE VEHÍCULOS
-- =====================================================

CREATE TABLE "vehicle_features" (
                                    "id" varchar(36) PRIMARY KEY,
                                    "vehicle_id" varchar(36) NOT NULL,
                                    "feature" varchar(50) NOT NULL
);

-- =====================================================
-- 9. TABLA ESPECIFICACIONES DE VEHÍCULOS
-- =====================================================

CREATE TABLE "vehicle_specifications" (
                                          "vehicle_id" varchar(36) PRIMARY KEY,
                                          "doors" int NOT NULL DEFAULT 4,
                                          "seats" int NOT NULL DEFAULT 5,
                                          "transmission" varchar(20) DEFAULT 'manual',
                                          "fuel_type" varchar(20) DEFAULT 'gasoline',
                                          "air_conditioner" boolean DEFAULT true
);

-- =====================================================
-- 10. TABLA RESERVACIONES
-- =====================================================

CREATE TABLE "reservations" (
                                "id" varchar(36) PRIMARY KEY,
                                "client_id" varchar(36) NOT NULL,
                                "provider_id" varchar(36),
                                "vehicle_id" varchar(36) NOT NULL,
                                "start_date" timestamp NOT NULL,
                                "end_date" timestamp NOT NULL,
                                "status" varchar(20) DEFAULT 'pending',
                                "payment_status" varchar(20) DEFAULT 'pending',
                                "payment_method" varchar(30),
                                "total_amount" decimal(10,2) NOT NULL,
                                "vehicle_price" decimal(10,2) NOT NULL,
                                "location" varchar(100),
                                "notes" text,
                                "cancellation_reason" text,
                                "cancellation_date" timestamp,
                                "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 11. TABLA EXTRAS DE RESERVACIONES
-- =====================================================

CREATE TABLE "reservation_extras" (
                                      "id" varchar(36) PRIMARY KEY,
                                      "reservation_id" varchar(36) NOT NULL,
                                      "name" varchar(100) NOT NULL,
                                      "price" decimal(10,2) NOT NULL
);

-- =====================================================
-- 12. TABLA MÉTODOS DE PAGO
-- =====================================================

CREATE TABLE "payment_methods" (
                                   "id" varchar(36) PRIMARY KEY,
                                   "user_id" varchar(36) NOT NULL,
                                   "type" varchar(20) NOT NULL,
                                   "is_default" boolean DEFAULT false,
                                   "card_holder" varchar(255),
                                   "card_number_last4" varchar(4),
                                   "card_expiry" varchar(10),
                                   "card_type" varchar(20),
                                   "paypal_email" varchar(100),
                                   "bank_name" varchar(100),
                                   "account_number_last4" varchar(4),
                                   "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                   "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 13. TABLA PAGOS
-- =====================================================

CREATE TABLE "payments" (
                            "id" varchar(36) PRIMARY KEY,
                            "reservation_id" varchar(36) NOT NULL,
                            "payment_method_id" varchar(36),
                            "amount" decimal(10,2) NOT NULL,
                            "status" varchar(20) DEFAULT 'pending',
                            "transaction_id" varchar(100),
                            "payment_date" timestamp,
                            "notes" text,
                            "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                            "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 14. TABLA COMISIONES
-- =====================================================

CREATE TABLE "commissions" (
                               "id" varchar(36) PRIMARY KEY,
                               "reservation_id" varchar(36) NOT NULL,
                               "amount" decimal(10,2) NOT NULL,
                               "percentage" decimal(5,2) NOT NULL,
                               "status" varchar(20) DEFAULT 'pending',
                               "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 15. TABLA APROBACIONES
-- =====================================================

CREATE TABLE "approvals" (
                             "id" varchar(36) PRIMARY KEY,
                             "type" varchar(20) NOT NULL,
                             "entity_id" varchar(36) NOT NULL,
                             "status" varchar(20) DEFAULT 'pending',
                             "admin_id" varchar(36),
                             "notes" text,
                             "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                             "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                             CHECK (type IN ('provider', 'vehicle'))
);

-- =====================================================
-- 16. TABLA RESEÑAS
-- =====================================================

CREATE TABLE "reviews" (
                           "id" varchar(36) PRIMARY KEY,
                           "reservation_id" varchar(36) NOT NULL,
                           "client_id" varchar(36) NOT NULL,
                           "vehicle_id" varchar(36) NOT NULL,
                           "provider_id" varchar(36),
                           "rating" int NOT NULL CHECK (rating BETWEEN 1 AND 5),
                           "comment" text,
                           "is_published" boolean DEFAULT true,
                           "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                           "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 17. TABLA PAGOS A PROVEEDORES
-- =====================================================

CREATE TABLE "provider_payments" (
                                     "id" varchar(36) PRIMARY KEY,
                                     "provider_id" varchar(36) NOT NULL,
                                     "amount" decimal(10,2) NOT NULL,
                                     "status" varchar(20) DEFAULT 'pending',
                                     "payment_date" timestamp,
                                     "payment_method" varchar(30),
                                     "reference_number" varchar(100),
                                     "notes" text,
                                     "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                     "updated_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 18. TABLA LOG DE SESIONES (Seguridad)
-- =====================================================

CREATE TABLE "user_sessions" (
                                 "id" varchar(36) PRIMARY KEY,
                                 "user_id" varchar(36) NOT NULL,
                                 "session_token" varchar(255) NOT NULL,
                                 "ip_address" inet,
                                 "user_agent" text,
                                 "login_method" varchar(20) NOT NULL, -- 'manual', 'google', 'facebook', 'twitter'
                                 "expires_at" timestamp NOT NULL,
                                 "is_active" boolean DEFAULT true,
                                 "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                                 "ended_at" timestamp
);

-- =====================================================
-- 19. TABLA VERIFICACIONES DE EMAIL (Para registro manual)
-- =====================================================

CREATE TABLE "email_verifications" (
                                       "id" varchar(36) PRIMARY KEY,
                                       "user_id" varchar(36) NOT NULL,
                                       "verification_token" varchar(255) NOT NULL,
                                       "email" varchar(100) NOT NULL,
                                       "expires_at" timestamp NOT NULL,
                                       "verified_at" timestamp,
                                       "attempts" int DEFAULT 0,
                                       "created_at" timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

-- =====================================================
-- 📝 COMENTARIOS DESCRIPTIVOS COMPLETOS
-- =====================================================

-- ✅ USUARIOS + OAUTH
COMMENT ON COLUMN "users"."password" IS 'Hash bcrypt para registro manual, NULL para OAuth';
COMMENT ON COLUMN "users"."role" IS 'client, provider, admin';
COMMENT ON COLUMN "users"."status" IS 'active, pending, suspended';
COMMENT ON COLUMN "users"."oauth_provider" IS 'Proveedor OAuth principal: google, facebook, twitter, NULL para manual';
COMMENT ON COLUMN "users"."oauth_id" IS 'ID único del proveedor OAuth';
COMMENT ON COLUMN "users"."is_oauth_user" IS 'true = registrado via OAuth, false = registro manual';
COMMENT ON COLUMN "users"."email_verified" IS 'true = verificado (OAuth automático, manual requiere verificación)';

-- ✅ OAUTH ACCOUNTS
COMMENT ON TABLE "user_oauth_accounts" IS 'Cuentas OAuth vinculadas - permite múltiples proveedores por usuario';
COMMENT ON COLUMN "user_oauth_accounts"."access_token" IS 'Token de acceso OAuth (encriptado)';
COMMENT ON COLUMN "user_oauth_accounts"."refresh_token" IS 'Token de refresco OAuth (encriptado)';

-- ✅ CLIENTES
COMMENT ON COLUMN "clients"."dni" IS 'DNI del cliente (8 dígitos) - Requerido';
COMMENT ON COLUMN "clients"."phone" IS 'Teléfono móvil (9 dígitos, inicia con 9) - Requerido';

-- ✅ ADMINISTRADORES
COMMENT ON COLUMN "admins"."access_level" IS 'basic, intermediate, full';

-- ✅ PROVEEDORES (ACTUALIZADO CON OAUTH)
COMMENT ON COLUMN "providers"."dni" IS 'DNI del proveedor persona (8 dígitos) - Requerido para personas';
COMMENT ON COLUMN "providers"."driver_license" IS 'Número de licencia de conducir - Requerido para personas';
COMMENT ON COLUMN "providers"."business_name" IS 'Nombre de empresa - Opcional (solo para empresas futuras)';
COMMENT ON COLUMN "providers"."tax_id" IS 'RUC/Tax ID - Opcional (solo para empresas futuras)';
COMMENT ON COLUMN "providers"."phone" IS 'Teléfono de contacto - Requerido';
COMMENT ON COLUMN "providers"."status" IS 'pending, active, suspended';

-- ✅ VEHÍCULOS
COMMENT ON COLUMN "vehicles"."type" IS 'sedan, suv, pickup, hatchback, coupe, convertible';
COMMENT ON COLUMN "vehicles"."status" IS 'pending_approval, approved, rejected, available, rented, maintenance, inactive';

-- ✅ ESPECIFICACIONES
COMMENT ON COLUMN "vehicle_specifications"."transmission" IS 'manual, automatic';
COMMENT ON COLUMN "vehicle_specifications"."fuel_type" IS 'gasoline, diesel, electric, hybrid';

-- ✅ RESERVACIONES
COMMENT ON COLUMN "reservations"."status" IS 'pending, confirmed, in_progress, completed, cancelled';
COMMENT ON COLUMN "reservations"."payment_status" IS 'pending, paid, refunded';
COMMENT ON COLUMN "reservations"."provider_id" IS 'NULL para reservas de vehículos propios de la empresa';

-- ✅ MÉTODOS DE PAGO
COMMENT ON COLUMN "payment_methods"."type" IS 'credit_card, paypal, bank_account';
COMMENT ON COLUMN "payment_methods"."card_holder" IS 'Nombre encriptado del titular';

-- ✅ PAGOS
COMMENT ON COLUMN "payments"."status" IS 'pending, completed, failed, refunded';

-- ✅ COMISIONES
COMMENT ON COLUMN "commissions"."status" IS 'pending, paid';

-- ✅ APROBACIONES
COMMENT ON COLUMN "approvals"."type" IS 'provider, vehicle';
COMMENT ON COLUMN "approvals"."entity_id" IS 'ID del proveedor o vehículo que requiere aprobación';
COMMENT ON COLUMN "approvals"."status" IS 'pending, approved, rejected';

-- ✅ SEGURIDAD
COMMENT ON COLUMN "user_sessions"."login_method" IS 'manual, google, facebook, twitter';
COMMENT ON TABLE "email_verifications" IS 'Verificaciones de email para registro manual';

-- ✅ COMENTARIOS DE TABLAS
COMMENT ON TABLE "users" IS 'Usuarios base - soporta registro manual y OAuth';
COMMENT ON TABLE "providers" IS 'Proveedores: personas (DNI + licencia) y empresas futuras';
COMMENT ON TABLE "reviews" IS 'Reseñas de usuarios sobre vehículos y proveedores';
COMMENT ON TABLE "provider_payments" IS 'Pagos realizados a los proveedores';
COMMENT ON TABLE "user_sessions" IS 'Control de sesiones activas por seguridad';

-- =====================================================
-- 🔗 LLAVES FORÁNEAS (FOREIGN KEYS)
-- =====================================================

-- ✅ RELACIONES DE USUARIOS
ALTER TABLE "clients" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;
ALTER TABLE "admins" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;
ALTER TABLE "providers" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

-- ✅ RELACIONES OAUTH
ALTER TABLE "user_oauth_accounts" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

-- ✅ RELACIONES DE VEHÍCULOS
ALTER TABLE "vehicles" ADD FOREIGN KEY ("provider_id") REFERENCES "providers" ("id") ON DELETE SET NULL;
ALTER TABLE "vehicle_images" ADD FOREIGN KEY ("vehicle_id") REFERENCES "vehicles" ("id") ON DELETE CASCADE;
ALTER TABLE "vehicle_features" ADD FOREIGN KEY ("vehicle_id") REFERENCES "vehicles" ("id") ON DELETE CASCADE;
ALTER TABLE "vehicle_specifications" ADD FOREIGN KEY ("vehicle_id") REFERENCES "vehicles" ("id") ON DELETE CASCADE;

-- ✅ RELACIONES DE RESERVACIONES
ALTER TABLE "reservations" ADD FOREIGN KEY ("client_id") REFERENCES "clients" ("id") ON DELETE RESTRICT;
ALTER TABLE "reservations" ADD FOREIGN KEY ("provider_id") REFERENCES "providers" ("id") ON DELETE SET NULL;
ALTER TABLE "reservations" ADD FOREIGN KEY ("vehicle_id") REFERENCES "vehicles" ("id") ON DELETE RESTRICT;
ALTER TABLE "reservation_extras" ADD FOREIGN KEY ("reservation_id") REFERENCES "reservations" ("id") ON DELETE CASCADE;

-- ✅ RELACIONES DE PAGOS
ALTER TABLE "payment_methods" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;
ALTER TABLE "payments" ADD FOREIGN KEY ("reservation_id") REFERENCES "reservations" ("id") ON DELETE RESTRICT;
ALTER TABLE "payments" ADD FOREIGN KEY ("payment_method_id") REFERENCES "payment_methods" ("id") ON DELETE SET NULL;

-- ✅ RELACIONES DE COMISIONES Y APROBACIONES
ALTER TABLE "commissions" ADD FOREIGN KEY ("reservation_id") REFERENCES "reservations" ("id") ON DELETE RESTRICT;
ALTER TABLE "approvals" ADD FOREIGN KEY ("admin_id") REFERENCES "users" ("id") ON DELETE SET NULL;

-- ✅ RELACIONES DE RESEÑAS
ALTER TABLE "reviews" ADD FOREIGN KEY ("reservation_id") REFERENCES "reservations" ("id") ON DELETE RESTRICT;
ALTER TABLE "reviews" ADD FOREIGN KEY ("client_id") REFERENCES "clients" ("id") ON DELETE RESTRICT;
ALTER TABLE "reviews" ADD FOREIGN KEY ("vehicle_id") REFERENCES "vehicles" ("id") ON DELETE RESTRICT;
ALTER TABLE "reviews" ADD FOREIGN KEY ("provider_id") REFERENCES "providers" ("id") ON DELETE SET NULL;

-- ✅ RELACIONES DE PAGOS A PROVEEDORES
ALTER TABLE "provider_payments" ADD FOREIGN KEY ("provider_id") REFERENCES "providers" ("id") ON DELETE RESTRICT;

-- ✅ RELACIONES DE SEGURIDAD
ALTER TABLE "user_sessions" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;
ALTER TABLE "email_verifications" ADD FOREIGN KEY ("user_id") REFERENCES "users" ("id") ON DELETE CASCADE;

-- =====================================================
-- 🛡️ RESTRICCIONES DE VALIDACIÓN (CONSTRAINTS)
-- =====================================================

-- ✅ VALIDACIONES PARA USUARIOS + OAUTH
-- Validar que tenga password O sea usuario OAuth
ALTER TABLE "users"
    ADD CONSTRAINT chk_users_auth_method
        CHECK (
            (is_oauth_user = false AND password IS NOT NULL) OR
            (is_oauth_user = true AND oauth_provider IS NOT NULL AND oauth_id IS NOT NULL)
            );

-- Validar proveedores OAuth válidos
ALTER TABLE "users"
    ADD CONSTRAINT chk_users_oauth_provider
        CHECK (oauth_provider IS NULL OR oauth_provider IN ('google', 'facebook', 'twitter'));

-- ✅ VALIDACIONES PARA CLIENTES
ALTER TABLE "clients"
    ADD CONSTRAINT chk_clients_dni_format
        CHECK (dni ~ '^[0-9]{8}$');

ALTER TABLE "clients"
    ADD CONSTRAINT chk_clients_phone_format
        CHECK (phone ~ '^9[0-9]{8}$');

-- ✅ VALIDACIONES PARA PROVEEDORES
-- Validar formato de DNI para personas
ALTER TABLE "providers"
    ADD CONSTRAINT chk_providers_dni_format
        CHECK (dni IS NULL OR dni ~ '^[0-9]{8}$');

-- Validar formato de teléfono
ALTER TABLE "providers"
    ADD CONSTRAINT chk_providers_phone_format
        CHECK (phone ~ '^9[0-9]{8}$');

-- Validar longitud mínima de licencia
ALTER TABLE "providers"
    ADD CONSTRAINT chk_providers_driver_license_length
        CHECK (driver_license IS NULL OR length(driver_license) >= 8);

-- Validar que tenga DNI O business_name (persona o empresa)
ALTER TABLE "providers"
    ADD CONSTRAINT chk_providers_person_or_business
        CHECK (dni IS NOT NULL OR business_name IS NOT NULL);

-- Validar que si es persona, tenga DNI y licencia
ALTER TABLE "providers"
    ADD CONSTRAINT chk_providers_person_complete
        CHECK (
            (dni IS NOT NULL AND driver_license IS NOT NULL) OR
            (dni IS NULL AND business_name IS NOT NULL)
            );

-- ✅ VALIDACIONES OAUTH
ALTER TABLE "user_oauth_accounts"
    ADD CONSTRAINT chk_oauth_provider_valid
        CHECK (provider IN ('google', 'facebook', 'twitter'));

-- ✅ OTRAS VALIDACIONES
ALTER TABLE "vehicles"
    ADD CONSTRAINT chk_vehicles_year_range
        CHECK (year BETWEEN 1990 AND EXTRACT(YEAR FROM CURRENT_DATE) + 1);

ALTER TABLE "vehicles"
    ADD CONSTRAINT chk_vehicles_daily_rate_positive
        CHECK (daily_rate > 0);

-- Validar método de login
ALTER TABLE "user_sessions"
    ADD CONSTRAINT chk_sessions_login_method
        CHECK (login_method IN ('manual', 'google', 'facebook', 'twitter'));

-- =====================================================
-- 📊 ÍNDICES PARA OPTIMIZACIÓN
-- =====================================================

-- ✅ ÍNDICES BÁSICOS EXISTENTES
CREATE INDEX idx_users_email ON "users" ("email");
CREATE INDEX idx_reservations_dates ON "reservations" ("start_date", "end_date");
CREATE INDEX idx_reservations_status ON "reservations" ("status");
CREATE INDEX idx_vehicles_status ON "vehicles" ("status");
CREATE INDEX idx_providers_status ON "providers" ("status");

-- ✅ ÍNDICES OAUTH
CREATE INDEX idx_users_oauth_provider ON "users" ("oauth_provider", "oauth_id");
CREATE INDEX idx_users_oauth_status ON "users" ("is_oauth_user", "status");
CREATE INDEX idx_oauth_accounts_user ON "user_oauth_accounts" ("user_id");
CREATE INDEX idx_oauth_accounts_provider ON "user_oauth_accounts" ("provider", "provider_id");
CREATE INDEX idx_oauth_accounts_email ON "user_oauth_accounts" ("provider_email");

-- ✅ ÍNDICES PARA PROVEEDORES PERSONA
CREATE INDEX idx_providers_dni ON "providers" ("dni");
CREATE INDEX idx_providers_driver_license ON "providers" ("driver_license");
CREATE INDEX idx_providers_phone ON "providers" ("phone");

-- ✅ ÍNDICES COMPUESTOS OPTIMIZADOS
CREATE INDEX idx_providers_active_person ON "providers" ("status", "dni") WHERE dni IS NOT NULL;
CREATE INDEX idx_providers_active_business ON "providers" ("status", "business_name") WHERE business_name IS NOT NULL;
CREATE INDEX idx_clients_dni ON "clients" ("dni");
CREATE INDEX idx_clients_phone ON "clients" ("phone");

-- ✅ ÍNDICES PARA BÚSQUEDAS FRECUENTES
CREATE INDEX idx_vehicles_location_status ON "vehicles" ("location", "status");
CREATE INDEX idx_reservations_client_status ON "reservations" ("client_id", "status");
CREATE INDEX idx_payments_status_date ON "payments" ("status", "payment_date");

-- ✅ ÍNDICES DE SEGURIDAD
CREATE INDEX idx_sessions_user_active ON "user_sessions" ("user_id", "is_active");
CREATE INDEX idx_sessions_token ON "user_sessions" ("session_token");
CREATE INDEX idx_email_verifications_token ON "email_verifications" ("verification_token");
CREATE INDEX idx_email_verifications_user ON "email_verifications" ("user_id", "verified_at");

-- =====================================================
-- 📋 VISTAS ÚTILES
-- =====================================================

-- ✅ Vista para proveedores persona (nuevo flujo + OAuth)
CREATE VIEW "provider_persons" AS
SELECT
    p.id,
    p.user_id,
    u.name as "full_name",
    u.email,
    u.status as "user_status",
    u.is_oauth_user,
    u.oauth_provider,
    u.profile_image,
    p.dni,
    p.driver_license,
    p.phone,
    p.address,
    p.city,
    p.postal_code,
    p.country,
    p.balance,
    p.total_earnings,
    p.pending_payments,
    p.status as "provider_status",
    p.bank_account,
    p.created_at,
    p.updated_at
FROM "providers" p
         JOIN "users" u ON p.user_id = u.id
WHERE p.dni IS NOT NULL;

-- ✅ Vista para proveedores empresa (compatibilidad futura)
CREATE VIEW "provider_businesses" AS
SELECT
    p.id,
    p.user_id,
    u.name as "contact_name",
    u.email,
    u.status as "user_status",
    u.is_oauth_user,
    u.oauth_provider,
    u.profile_image,
    p.business_name,
    p.tax_id,
    p.phone,
    p.address,
    p.city,
    p.postal_code,
    p.country,
    p.balance,
    p.total_earnings,
    p.pending_payments,
    p.status as "provider_status",
    p.bank_account,
    p.created_at,
    p.updated_at
FROM "providers" p
         JOIN "users" u ON p.user_id = u.id
WHERE p.business_name IS NOT NULL;

-- ✅ Vista completa de clientes (incluyendo OAuth)
CREATE VIEW "client_details" AS
SELECT
    c.id,
    c.user_id,
    u.name as "full_name",
    u.email,
    u.status as "user_status",
    u.is_oauth_user,
    u.oauth_provider,
    u.profile_image,
    c.dni,
    c.phone,
    c.address,
    c.city,
    c.postal_code,
    c.country,
    c.created_at,
    c.updated_at
FROM "clients" c
         JOIN "users" u ON c.user_id = u.id;

-- ✅ Vista de usuarios con información OAuth
CREATE VIEW "user_auth_details" AS
SELECT
    u.id,
    u.name,
    u.email,
    u.role,
    u.status,
    u.is_oauth_user,
    u.oauth_provider,
    u.email_verified,
    u.profile_image,
    u.registration_date,
    u.last_access,
    -- Información OAuth si existe
    oa.provider as "linked_provider",
    oa.provider_email as "oauth_email",
    oa.provider_picture as "oauth_picture",
    oa.token_expires_at as "oauth_expires"
FROM "users" u
         LEFT JOIN "user_oauth_accounts" oa ON u.id = oa.user_id AND u.oauth_provider = oa.provider;

-- ✅ Vista de sesiones activas
CREATE VIEW "active_sessions" AS
SELECT
    s.id as "session_id",
    s.user_id,
    u.name,
    u.email,
    u.role,
    s.login_method,
    s.ip_address,
    s.created_at as "login_at",
    s.expires_at,
    CASE
        WHEN s.expires_at > CURRENT_TIMESTAMP THEN 'active'
        ELSE 'expired'
        END as "session_status"
FROM "user_sessions" s
         JOIN "users" u ON s.user_id = u.id
WHERE s.is_active = true
ORDER BY s.created_at DESC;

-- =====================================================
-- 🔧 FUNCIONES Y TRIGGERS
-- =====================================================

-- ✅ Función para actualizar timestamp automáticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$ language 'plpgsql';

-- ✅ Triggers para updated_at automático
CREATE TRIGGER update_clients_updated_at BEFORE UPDATE ON "clients" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_providers_updated_at BEFORE UPDATE ON "providers" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_admins_updated_at BEFORE UPDATE ON "admins" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_reservations_updated_at BEFORE UPDATE ON "reservations" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_vehicles_updated_at BEFORE UPDATE ON "vehicles" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_payment_methods_updated_at BEFORE UPDATE ON "payment_methods" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_payments_updated_at BEFORE UPDATE ON "payments" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_provider_payments_updated_at BEFORE UPDATE ON "provider_payments" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_reviews_updated_at BEFORE UPDATE ON "reviews" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_approvals_updated_at BEFORE UPDATE ON "approvals" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_oauth_accounts_updated_at BEFORE UPDATE ON "user_oauth_accounts" FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ✅ Función para validar datos de proveedor (actualizada para OAuth)
CREATE OR REPLACE FUNCTION validate_provider_registration()
RETURNS TRIGGER AS $$
BEGIN
    -- Si es persona, validar que tenga DNI y licencia
    IF NEW.dni IS NOT NULL THEN
        IF NEW.driver_license IS NULL OR length(NEW.driver_license) < 8 THEN
            RAISE EXCEPTION 'Los proveedores persona deben tener licencia de conducir válida (mínimo 8 caracteres)';
END IF;
        
        -- Validar formato de DNI
        IF NOT (NEW.dni ~ '^[0-9]{8}$') THEN
            RAISE EXCEPTION 'El DNI debe tener exactamente 8 dígitos numéricos';
END IF;
END IF;
    
    -- Si es empresa, validar que tenga business_name y tax_id
    IF NEW.business_name IS NOT NULL THEN
        IF NEW.tax_id IS NULL THEN
            RAISE EXCEPTION 'Las empresas proveedoras deben tener Tax ID/RUC';
END IF;
END IF;
    
    -- Validar formato de teléfono
    IF NOT (NEW.phone ~ '^9[0-9]{8}$') THEN
        RAISE EXCEPTION 'El teléfono debe tener 9 dígitos y empezar con 9';
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ✅ Trigger para validación de proveedores
CREATE TRIGGER trigger_validate_provider_registration
    BEFORE INSERT OR UPDATE ON "providers"
                         FOR EACH ROW
                         EXECUTE FUNCTION validate_provider_registration();

-- ✅ Función para validar datos de cliente (actualizada)
CREATE OR REPLACE FUNCTION validate_client_registration()
RETURNS TRIGGER AS $$
BEGIN
    -- Validar formato de DNI
    IF NOT (NEW.dni ~ '^[0-9]{8}$') THEN
        RAISE EXCEPTION 'El DNI debe tener exactamente 8 dígitos numéricos';
END IF;
    
    -- Validar formato de teléfono
    IF NOT (NEW.phone ~ '^9[0-9]{8}$') THEN
        RAISE EXCEPTION 'El teléfono debe tener 9 dígitos y empezar con 9';
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ✅ Trigger para validación de clientes
CREATE TRIGGER trigger_validate_client_registration
    BEFORE INSERT OR UPDATE ON "clients"
                         FOR EACH ROW
                         EXECUTE FUNCTION validate_client_registration();

-- ✅ Función para limpiar sesiones expiradas
CREATE OR REPLACE FUNCTION cleanup_expired_sessions()
RETURNS void AS $$
BEGIN
UPDATE "user_sessions"
SET is_active = false, ended_at = CURRENT_TIMESTAMP
WHERE expires_at < CURRENT_TIMESTAMP AND is_active = true;

DELETE FROM "email_verifications"
WHERE expires_at < CURRENT_TIMESTAMP AND verified_at IS NULL;
END;
$$ LANGUAGE plpgsql;

-- ✅ Función para verificar unicidad de email OAuth
CREATE OR REPLACE FUNCTION validate_oauth_email_unique()
RETURNS TRIGGER AS $$
BEGIN
    -- Verificar que el email OAuth no exista en users
    IF EXISTS (SELECT 1 FROM "users" WHERE email = NEW.provider_email AND id != NEW.user_id) THEN
        RAISE EXCEPTION 'El email del proveedor OAuth ya está registrado con otra cuenta';
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ✅ Trigger para validación de OAuth
CREATE TRIGGER trigger_validate_oauth_email
    BEFORE INSERT OR UPDATE ON "user_oauth_accounts"
                         FOR EACH ROW
                         EXECUTE FUNCTION validate_oauth_email_unique();

-- =====================================================
-- 📊 PROCEDIMIENTOS ALMACENADOS ÚTILES
-- =====================================================

-- ✅ Procedimiento para obtener estadísticas de registro
CREATE OR REPLACE FUNCTION get_registration_stats()
RETURNS TABLE(
    total_users bigint,
    manual_users bigint,
    oauth_users bigint,
    google_users bigint,
    facebook_users bigint,
    twitter_users bigint,
    verified_users bigint,
    pending_users bigint
) AS $$
BEGIN
RETURN QUERY
SELECT
    COUNT(*) as total_users,
    COUNT(*) FILTER (WHERE is_oauth_user = false) as manual_users,
    COUNT(*) FILTER (WHERE is_oauth_user = true) as oauth_users,
    COUNT(*) FILTER (WHERE oauth_provider = 'google') as google_users,
    COUNT(*) FILTER (WHERE oauth_provider = 'facebook') as facebook_users,
    COUNT(*) FILTER (WHERE oauth_provider = 'twitter') as twitter_users,
    COUNT(*) FILTER (WHERE email_verified = true) as verified_users,
    COUNT(*) FILTER (WHERE status = 'pending') as pending_users
FROM "users";
END;
$$ LANGUAGE plpgsql;

-- ✅ Procedimiento para obtener sesiones activas por usuario
CREATE OR REPLACE FUNCTION get_user_active_sessions(user_uuid varchar(36))
RETURNS TABLE(
    session_id varchar(36),
    login_method varchar(20),
    ip_address inet,
    user_agent text,
    created_at timestamp,
    expires_at timestamp
) AS $$
BEGIN
RETURN QUERY
SELECT
    s.id,
    s.login_method,
    s.ip_address,
    s.user_agent,
    s.created_at,
    s.expires_at
FROM "user_sessions" s
WHERE s.user_id = user_uuid
  AND s.is_active = true
  AND s.expires_at > CURRENT_TIMESTAMP
ORDER BY s.created_at DESC;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- ✅ SCHEMA COMPLETADO CON OAUTH
-- =====================================================

-- Mostrar resumen del schema final
SELECT
    'SCHEMA COMPLETADO CON OAUTH' as status,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public') as total_tables,
    (SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = 'public') as total_constraints,
    (SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public') as total_indexes,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public') as total_views,
    (SELECT COUNT(*) FROM information_schema.triggers WHERE trigger_schema = 'public') as total_triggers,
    (SELECT COUNT(*) FROM information_schema.routines WHERE routine_schema = 'public') as total_functions;

-- =====================================================
-- 🎯 DATOS DE EJEMPLO PARA TESTING (OPCIONAL)
-- =====================================================

-- Descomenta para insertar datos de prueba
/*
-- ✅ Usuario administrador
INSERT INTO "users" (id, name, email, password, role, status, email_verified) 
VALUES ('admin-001', 'Administrador Sistema', 'admin@carrent.com', '$2b$12$ejemplo_hash', 'admin', 'active', true);

-- ✅ Usuario cliente manual
INSERT INTO "users" (id, name, email, password, role, status, is_oauth_user, email_verified) 
VALUES ('client-001', 'Juan Pérez García', 'juan@email.com', '$2b$12$ejemplo_hash', 'client', 'active', false, true);

-- ✅ Usuario cliente OAuth (Google)
INSERT INTO "users" (id, name, email, role, status, oauth_provider, oauth_id, is_oauth_user, email_verified, profile_image) 
VALUES ('client-002', 'María López Silva', 'maria@gmail.com', 'client', 'active', 'google', 'google_123456789', true, true, 'https://lh3.googleusercontent.com/ejemplo');

-- ✅ Usuario proveedor OAuth (Facebook)
INSERT INTO "users" (id, name, email, role, status, oauth_provider, oauth_id, is_oauth_user, email_verified, profile_image) 
VALUES ('provider-001', 'Carlos Rodríguez', 'carlos@facebook.com', 'provider', 'pending', 'facebook', 'facebook_987654321', true, true, 'https://graph.facebook.com/ejemplo/picture');

-- ✅ Cliente manual
INSERT INTO "clients" (id, user_id, dni, phone, city, country) 
VALUES ('client-data-001', 'client-001', '12345678', '987654321', 'Lima', 'Perú');

-- ✅ Cliente OAuth
INSERT INTO "clients" (id, user_id, dni, phone, city, country) 
VALUES ('client-data-002', 'client-002', '87654321', '987123456', 'Arequipa', 'Perú');

-- ✅ Proveedor OAuth
INSERT INTO "providers" (id, user_id, dni, driver_license, phone, city, country) 
VALUES ('provider-data-001', 'provider-001', '11223344', 'LIC987654321', '987987987', 'Cusco', 'Perú');

-- ✅ Cuenta OAuth para cliente Google
INSERT INTO "user_oauth_accounts" (id, user_id, provider, provider_id, provider_email, provider_name, provider_picture, token_expires_at) 
VALUES ('oauth-001', 'client-002', 'google', 'google_123456789', 'maria@gmail.com', 'María López Silva', 'https://lh3.googleusercontent.com/ejemplo', CURRENT_TIMESTAMP + INTERVAL '1 hour');

-- ✅ Cuenta OAuth para proveedor Facebook
INSERT INTO "user_oauth_accounts" (id, user_id, provider, provider_id, provider_email, provider_name, provider_picture, token_expires_at) 
VALUES ('oauth-002', 'provider-001', 'facebook', 'facebook_987654321', 'carlos@facebook.com', 'Carlos Rodríguez', 'https://graph.facebook.com/ejemplo/picture', CURRENT_TIMESTAMP + INTERVAL '2 hours');

-- ✅ Sesión activa OAuth
INSERT INTO "user_sessions" (id, user_id, session_token, ip_address, user_agent, login_method, expires_at) 
VALUES ('session-001', 'client-002', 'token_ejemplo_google', '192.168.1.100', 'Mozilla/5.0...', 'google', CURRENT_TIMESTAMP + INTERVAL '24 hours');
*/

-- =====================================================
-- 🎉 SCHEMA FINAL COMPLETADO
-- =====================================================

-- Ejecutar limpieza inicial
SELECT cleanup_expired_sessions();

-- Mostrar estadísticas iniciales
SELECT * FROM get_registration_stats();

COMMIT;