SELECT occurred,
  SUM(nbrsearched) OVER (ORDER BY occurred)
  AS total_searched
FROM stat_requests