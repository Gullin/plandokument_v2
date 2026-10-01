SELECT strftime(@period, occurred) period, COUNT(*) total
FROM stat_requests
GROUP BY period
ORDER BY period;