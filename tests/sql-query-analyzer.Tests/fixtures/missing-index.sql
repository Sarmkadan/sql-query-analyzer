-- Rule: function-on-column + leading-wildcard LIKE (non-sargable predicate)
-- Wrapping the indexed column in UPPER() and using a leading-wildcard LIKE
-- both prevent an index seek, forcing a scan on a column that "should" be indexed.
SELECT id, email, last_name
FROM users
WHERE UPPER(last_name) = 'SMITH'
  AND email LIKE '%@example.com';
