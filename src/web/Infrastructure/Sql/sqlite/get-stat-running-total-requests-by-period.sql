SELECT a.period period, SUM(a.total) OVER (ORDER BY a.period) AS total_running
FROM
	(SELECT strftime(@period, occurred) period, COUNT(*) total
	FROM stat_requests
	GROUP BY period) a;