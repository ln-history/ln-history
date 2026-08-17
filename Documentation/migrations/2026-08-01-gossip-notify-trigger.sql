-- ============================================================================
-- Migration: live-gossip NOTIFY trigger for the ln-history-api WebSocket stream
-- Date:      2026-08-01
-- Role:      admin / migration role (see ln-history-database skill, .env.migration)
-- Depends:   channels.internal_id, channel_updates.internal_id,
--            node_announcements_complete.internal_id  (already present)
-- ============================================================================
--
-- WHAT THIS DOES
--   Emits a lightweight NOTIFY on every newly ingested gossip message so the API's
--   GossipNotificationListener can push it to WebSocket subscribers in near-real-time.
--   The API holds a dedicated connection doing `LISTEN lnhistory_gossip`.
--
--   Payload is compact JSON: {"i":<internal_id>,"t":<bolt7_type>} — never the raw
--   bytes, so it cannot approach the 8000-byte NOTIFY payload limit. The listener
--   fetches raw_gossip by internal_id after receiving the notification.
--
-- SEMANTICS
--   * AFTER INSERT, FOR EACH ROW. pg_notify is transactional: notifications are
--     delivered to listeners only when the inserting transaction COMMITs, and are
--     dropped on rollback. By the time the listener fetches by internal_id, the row
--     is therefore guaranteed visible. No producer/consumer race.
--   * Rows with a NULL internal_id are skipped (nothing to fetch by).
--   * The message type is a constant per table (256 channels / 257 node
--     announcements / 258 channel updates), passed as a trigger argument.
--
-- COST
--   pg_notify is an in-memory enqueue (~microseconds); negligible per-insert
--   overhead even on the hot ingest path. There is nothing to EXPLAIN (it is not a
--   query); to sanity-check, time a batch INSERT before/after enabling.
--
-- ⚠ BULK-IMPORT WARNING
--   These triggers fire per row. A historical/bulk backfill (millions of inserts in
--   one run) would emit a matching flood of notifications. DISABLE the triggers for
--   the duration of any bulk import, then re-enable:
--       ALTER TABLE channels                    DISABLE TRIGGER trg_notify_gossip_channel;
--       ALTER TABLE channel_updates             DISABLE TRIGGER trg_notify_gossip_channel_update;
--       ALTER TABLE node_announcements_complete  DISABLE TRIGGER trg_notify_gossip_node;
--   (re-enable with ENABLE TRIGGER). Clients reconnect with ?since=<internal_id> to
--   backfill the gap they missed while the triggers were off.
--
-- ROLLBACK  (bottom of file)
-- ============================================================================

BEGIN;

CREATE OR REPLACE FUNCTION notify_new_gossip() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.internal_id IS NOT NULL THEN
        PERFORM pg_notify(
            'lnhistory_gossip',
            json_build_object('i', NEW.internal_id, 't', TG_ARGV[0]::int)::text
        );
    END IF;
    RETURN NULL;  -- AFTER trigger: return value is ignored
END;
$$;

DROP TRIGGER IF EXISTS trg_notify_gossip_channel ON channels;
CREATE TRIGGER trg_notify_gossip_channel
    AFTER INSERT ON channels
    FOR EACH ROW EXECUTE FUNCTION notify_new_gossip('256');

DROP TRIGGER IF EXISTS trg_notify_gossip_channel_update ON channel_updates;
CREATE TRIGGER trg_notify_gossip_channel_update
    AFTER INSERT ON channel_updates
    FOR EACH ROW EXECUTE FUNCTION notify_new_gossip('258');

DROP TRIGGER IF EXISTS trg_notify_gossip_node ON node_announcements_complete;
CREATE TRIGGER trg_notify_gossip_node
    AFTER INSERT ON node_announcements_complete
    FOR EACH ROW EXECUTE FUNCTION notify_new_gossip('257');

COMMIT;

-- --- Smoke test (run in one psql session, insert in another) --------------
--   LISTEN lnhistory_gossip;
--   -- then, from the ingest side, insert a row; you should receive:
--   -- Asynchronous notification "lnhistory_gossip" with payload
--   --   "{"i" : 123456, "t" : 258}" received from server process ...

-- ============================================================================
-- ROLLBACK
-- ============================================================================
-- BEGIN;
-- DROP TRIGGER IF EXISTS trg_notify_gossip_channel            ON channels;
-- DROP TRIGGER IF EXISTS trg_notify_gossip_channel_update     ON channel_updates;
-- DROP TRIGGER IF EXISTS trg_notify_gossip_node               ON node_announcements_complete;
-- DROP FUNCTION IF EXISTS notify_new_gossip();
-- COMMIT;
