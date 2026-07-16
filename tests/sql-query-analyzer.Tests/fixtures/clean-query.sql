-- Control fixture: a well-formed query that should NOT trip the bad-query rules.
-- Explicit projection, explicit join, sargable predicate, bounded result set.
SELECT o.id, o.total, c.name
FROM orders o
JOIN customers c ON c.id = o.customer_id
WHERE o.status = 'OPEN'
ORDER BY o.created_at DESC
LIMIT 50;
