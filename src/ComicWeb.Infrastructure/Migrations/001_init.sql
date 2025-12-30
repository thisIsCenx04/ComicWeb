-- ============================================================
-- Comic Platform (Comic Pages) - PostgreSQL Schema (Complete)
-- Based on provided API list, adapted from text -> comic pages
-- ============================================================

BEGIN;

-- ---------- Extensions ----------
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "citext";

-- ---------- Helpers ----------
-- updated_at auto-update trigger
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS trigger AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- 1) USERS / AUTH
-- ============================================================

CREATE TABLE IF NOT EXISTS users (
  id                uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  full_name         text NOT NULL,
  email             citext UNIQUE NOT NULL,
  password_hash     text,                              -- null if Google-only
  avatar_url        text,
  role              text NOT NULL DEFAULT 'USER',       -- USER | ADMIN | ...
  status            int  NOT NULL DEFAULT 1,            -- 1 active, 0 banned...
  email_verified    boolean NOT NULL DEFAULT false,
  created_at        timestamptz NOT NULL DEFAULT now(),
  updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_users_email_trgm
  ON users USING gin (email gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_users_fullname_trgm
  ON users USING gin (full_name gin_trgm_ops);

DROP TRIGGER IF EXISTS trg_users_updated_at ON users;
CREATE TRIGGER trg_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Refresh tokens for /auth/refetchToken, logout
CREATE TABLE IF NOT EXISTS auth_refresh_tokens (
  id                uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id           uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash        text NOT NULL,                      -- store hash only
  revoked           boolean NOT NULL DEFAULT false,
  expires_at        timestamptz NOT NULL,
  created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_refresh_user ON auth_refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_expires ON auth_refresh_tokens(expires_at);

-- Password reset codes for /auth/forgot-password, /auth/reset-password
CREATE TABLE IF NOT EXISTS password_reset_codes (
  id                uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id           uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  code_hash         text NOT NULL,
  expires_at        timestamptz NOT NULL,
  used_at           timestamptz,
  created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_prc_user ON password_reset_codes(user_id);
CREATE INDEX IF NOT EXISTS idx_prc_expires ON password_reset_codes(expires_at);

-- Email verification codes for /auth/verify, /auth/resend-confirm
CREATE TABLE IF NOT EXISTS email_verification_codes (
  id                uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id           uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  code_hash         text NOT NULL,
  expires_at        timestamptz NOT NULL,
  verified_at       timestamptz,
  created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_evc_user ON email_verification_codes(user_id);
CREATE INDEX IF NOT EXISTS idx_evc_expires ON email_verification_codes(expires_at);

-- ============================================================
-- 2) CATEGORY / COMIC / CHAPTER (COMIC PAGES)
-- ============================================================

CREATE TABLE IF NOT EXISTS categories (
  id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  name        text NOT NULL,
  slug        text UNIQUE NOT NULL,
  tags        text[],                         -- optional
  created_at  timestamptz NOT NULL DEFAULT now(),
  updated_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_categories_name_trgm
  ON categories USING gin (name gin_trgm_ops);

DROP TRIGGER IF EXISTS trg_categories_updated_at ON categories;
CREATE TRIGGER trg_categories_updated_at
BEFORE UPDATE ON categories
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE IF NOT EXISTS comics (
  id            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  owner_id      uuid REFERENCES users(id) ON DELETE SET NULL, -- author/owner
  category_id   uuid REFERENCES categories(id) ON DELETE SET NULL,
  slug          text UNIQUE NOT NULL,
  title         text NOT NULL,
  description   text,
  thumbnail_url text,
  author        text,
  unit_price    int NOT NULL DEFAULT 0,       -- buy whole comic (optional)
  salary_type   int,                          -- as in your API note
  status        int NOT NULL DEFAULT 1,        -- requesting/approved/rejected...
  admin_note    text,                         -- approve/reject note
  created_at    timestamptz NOT NULL DEFAULT now(),
  updated_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_comics_slug ON comics(slug);
CREATE INDEX IF NOT EXISTS idx_comics_category ON comics(category_id);
CREATE INDEX IF NOT EXISTS idx_comics_status ON comics(status);
CREATE INDEX IF NOT EXISTS idx_comics_title_trgm
  ON comics USING gin (title gin_trgm_ops);

DROP TRIGGER IF EXISTS trg_comics_updated_at ON comics;
CREATE TRIGGER trg_comics_updated_at
BEFORE UPDATE ON comics
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Chapters (content no longer core; pages are core)
CREATE TABLE IF NOT EXISTS chapters (
  id            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  comic_id      uuid NOT NULL REFERENCES comics(id) ON DELETE CASCADE,
  slug          text UNIQUE NOT NULL,
  title         text NOT NULL,
  thumbnail_url text,
  unit_price    int NOT NULL DEFAULT 0,        -- purchase chapter unlocks pages
  page_count    int NOT NULL DEFAULT 0,        -- denormalized
  status        int NOT NULL DEFAULT 1,
  created_at    timestamptz NOT NULL DEFAULT now(),
  updated_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_chapters_comic ON chapters(comic_id);
CREATE INDEX IF NOT EXISTS idx_chapters_slug ON chapters(slug);
CREATE INDEX IF NOT EXISTS idx_chapters_title_trgm
  ON chapters USING gin (title gin_trgm_ops);

DROP TRIGGER IF EXISTS trg_chapters_updated_at ON chapters;
CREATE TRIGGER trg_chapters_updated_at
BEFORE UPDATE ON chapters
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Chapter pages (core for comics)
CREATE TABLE IF NOT EXISTS chapter_pages (
  id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  chapter_id  uuid NOT NULL REFERENCES chapters(id) ON DELETE CASCADE,
  page_order  int NOT NULL,
  image_url   text NOT NULL,                   -- public URL per your requirement
  created_at  timestamptz NOT NULL DEFAULT now(),
  UNIQUE (chapter_id, page_order)
);

CREATE INDEX IF NOT EXISTS idx_pages_chapter_order
  ON chapter_pages(chapter_id, page_order);

-- Optional: keep chapters.page_count in sync
CREATE OR REPLACE FUNCTION sync_chapter_page_count()
RETURNS trigger AS $$
DECLARE
  v_chapter_id uuid;
BEGIN
  v_chapter_id := COALESCE(NEW.chapter_id, OLD.chapter_id);

  UPDATE chapters
  SET page_count = (
    SELECT COUNT(*) FROM chapter_pages WHERE chapter_id = v_chapter_id
  )
  WHERE id = v_chapter_id;

  RETURN NULL;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_pages_sync_count_ins ON chapter_pages;
CREATE TRIGGER trg_pages_sync_count_ins
AFTER INSERT ON chapter_pages
FOR EACH ROW EXECUTE FUNCTION sync_chapter_page_count();

DROP TRIGGER IF EXISTS trg_pages_sync_count_del ON chapter_pages;
CREATE TRIGGER trg_pages_sync_count_del
AFTER DELETE ON chapter_pages
FOR EACH ROW EXECUTE FUNCTION sync_chapter_page_count();

DROP TRIGGER IF EXISTS trg_pages_sync_count_upd ON chapter_pages;
CREATE TRIGGER trg_pages_sync_count_upd
AFTER UPDATE OF chapter_id ON chapter_pages
FOR EACH ROW EXECUTE FUNCTION sync_chapter_page_count();

-- ============================================================
-- 3) NOTIFICATIONS
-- ============================================================

CREATE TABLE IF NOT EXISTS notifications (
  id            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  slug          text UNIQUE NOT NULL,
  title         text NOT NULL,
  description   text,
  thumbnail_url text,
  status        int NOT NULL DEFAULT 1,
  created_at    timestamptz NOT NULL DEFAULT now(),
  updated_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_notifications_title_trgm
  ON notifications USING gin (title gin_trgm_ops);

DROP TRIGGER IF EXISTS trg_notifications_updated_at ON notifications;
CREATE TRIGGER trg_notifications_updated_at
BEFORE UPDATE ON notifications
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- 4) COMMENTS (comic-level, supports replies)
-- ============================================================

CREATE TABLE IF NOT EXISTS comments (
  id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id     uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  comic_id    uuid NOT NULL REFERENCES comics(id) ON DELETE CASCADE,
  parent_id   uuid REFERENCES comments(id) ON DELETE CASCADE,
  content     text NOT NULL,
  created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_comments_comic_created
  ON comments(comic_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_comments_parent
  ON comments(parent_id);

-- ============================================================
-- 5) FAVORITES / FOLLOWS
-- ============================================================

CREATE TABLE IF NOT EXISTS favorites (
  user_id     uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  comic_id    uuid NOT NULL REFERENCES comics(id) ON DELETE CASCADE,
  created_at  timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (user_id, comic_id)
);

CREATE INDEX IF NOT EXISTS idx_favorites_user_created
  ON favorites(user_id, created_at DESC);

CREATE TABLE IF NOT EXISTS follows (
  follower_id  uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  followed_id  uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  created_at   timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (follower_id, followed_id),
  CHECK (follower_id <> followed_id)
);

CREATE INDEX IF NOT EXISTS idx_follows_followed_created
  ON follows(followed_id, created_at DESC);

-- ============================================================
-- 6) MISSIONS
-- ============================================================

CREATE TABLE IF NOT EXISTS missions (
  id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  title       text NOT NULL,
  description text,
  reward      int NOT NULL DEFAULT 0,
  status      int NOT NULL DEFAULT 1,
  created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS user_missions (
  user_id      uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  mission_id   uuid NOT NULL REFERENCES missions(id) ON DELETE CASCADE,
  completed_at timestamptz,
  PRIMARY KEY (user_id, mission_id)
);

CREATE INDEX IF NOT EXISTS idx_user_missions_user ON user_missions(user_id);

-- ============================================================
-- 7) CURRENCY / PAYMENTS
-- ============================================================

-- Currency ledger (POST /currency, GET /currency/history)
CREATE TABLE IF NOT EXISTS currency_ledger (
  id           uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id      uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  entry_type   text NOT NULL,                -- CREDIT | DEBIT
  amount       int NOT NULL,
  description  text,
  created_at   timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_currency_user_created
  ON currency_ledger(user_id, created_at DESC);

-- Transactions (GET /payments/transactions, check/:id, accept-manual/:id)
CREATE TABLE IF NOT EXISTS transactions (
  id            uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id       uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  type          text NOT NULL,               -- CHAPTER | COMIC | TOPUP | WITHDRAW
  comic_id      uuid REFERENCES comics(id) ON DELETE SET NULL,
  chapter_id    uuid REFERENCES chapters(id) ON DELETE SET NULL,
  amount        int NOT NULL,
  currency_type int NOT NULL,               -- currenceType
  status        text NOT NULL,              -- PENDING | SUCCESS | FAILED
  provider      text,                       -- SEPAY | MANUAL | ...
  provider_ref  text,                       -- external reference id
  created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_tx_user_created
  ON transactions(user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_tx_chapter ON transactions(chapter_id);
CREATE INDEX IF NOT EXISTS idx_tx_status ON transactions(status);

-- Purchase cache for fast authorization checks (e.g., chapter pages access)
CREATE TABLE IF NOT EXISTS user_purchases (
  user_id      uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  type         text NOT NULL,               -- CHAPTER | COMIC
  ref_id       uuid NOT NULL,               -- chapter_id/comic_id
  purchased_at timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (user_id, type, ref_id)
);

CREATE INDEX IF NOT EXISTS idx_purchases_ref
  ON user_purchases(type, ref_id);

-- ============================================================
-- 8) WITHDRAW
-- ============================================================

CREATE TABLE IF NOT EXISTS withdraw_requests (
  id                uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id           uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  amount            int NOT NULL,
  bank_name         text NOT NULL,
  bank_account      text NOT NULL,
  bank_account_name text NOT NULL,
  status            text NOT NULL DEFAULT 'PENDING', -- PENDING|APPROVED|REJECTED
  admin_note        text,
  created_at        timestamptz NOT NULL DEFAULT now(),
  updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_withdraw_user_created
  ON withdraw_requests(user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_withdraw_status
  ON withdraw_requests(status);

DROP TRIGGER IF EXISTS trg_withdraw_updated_at ON withdraw_requests;
CREATE TRIGGER trg_withdraw_updated_at
BEFORE UPDATE ON withdraw_requests
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- 9) OUTSTANDINGS (featured placement)
-- ============================================================

CREATE TABLE IF NOT EXISTS outstandings (
  id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  comic_id    uuid NOT NULL REFERENCES comics(id) ON DELETE CASCADE,
  amount      int NOT NULL,
  created_by  uuid REFERENCES users(id) ON DELETE SET NULL,
  created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_outstandings_comic_created
  ON outstandings(comic_id, created_at DESC);

-- ============================================================
-- 10) UPLOADS (optional audit table for /uploads)
-- ============================================================

CREATE TABLE IF NOT EXISTS uploads (
  id          uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id     uuid REFERENCES users(id) ON DELETE SET NULL,
  file_type   text NOT NULL,               -- IMAGE | AUDIO
  url         text NOT NULL,
  created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_uploads_user_created
  ON uploads(user_id, created_at DESC);

COMMIT;

-- ============================================================
-- End of schema
-- ============================================================
