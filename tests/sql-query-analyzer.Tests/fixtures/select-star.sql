-- Rule: SELECT * (no column projection)
-- The query pulls every column instead of a bounded projection, which
-- defeats covering indexes and ships unused bytes over the wire.
SELECT *
FROM orders o
JOIN customers c ON c.id = o.customer_id
WHERE o.status = 'OPEN';
