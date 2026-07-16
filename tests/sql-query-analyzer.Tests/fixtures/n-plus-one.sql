-- Rule: N+1 access pattern
-- The same table is hit repeatedly by primary key in a loop instead of a
-- single set-based fetch. Each line represents one round-trip the ORM emits
-- per parent row. DetectNPlusOnePattern flags the repeated single-row access.
SELECT id, total FROM order_items WHERE order_id = 1;
SELECT id, total FROM order_items WHERE order_id = 2;
SELECT id, total FROM order_items WHERE order_id = 3;
SELECT id, total FROM order_items WHERE order_id = 4;
SELECT id, total FROM order_items WHERE order_id = 5;
SELECT id, total FROM order_items WHERE order_id = 6;
