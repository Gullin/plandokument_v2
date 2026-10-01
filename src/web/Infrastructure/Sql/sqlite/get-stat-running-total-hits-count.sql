SELECT occurred,
  SUM(nbrhits) OVER (ORDER BY occurred)
  AS total_hits
FROM stat_requests