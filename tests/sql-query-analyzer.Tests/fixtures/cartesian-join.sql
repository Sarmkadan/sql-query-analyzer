-- Rule: implicit (comma) join with no join predicate -> cartesian product
-- Two tables listed in FROM with only a filter, no ON/WHERE key equality,
-- multiplies row counts instead of matching related rows.
SELECT p.name, w.location
FROM products p, warehouses w
WHERE p.active = 1;
