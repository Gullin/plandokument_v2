SELECT occurred,
  SUM(1) OVER (ORDER BY occurred)
  AS total_searched
FROM stat_requests