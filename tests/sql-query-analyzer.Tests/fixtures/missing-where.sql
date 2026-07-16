-- Rule: missing WHERE / no LIMIT (unbounded result set)
-- A SELECT with neither a WHERE predicate nor a LIMIT streams the whole table.
SELECT id, name, created_at
FROM audit_log;
