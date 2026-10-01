-- Tar bort 5-percentilerna (eliminera outlines/spikar) och avrundar decimaler till millisekunderna
SELECT strftime(@period, a.occurred) period, ROUND(AVG(a.searchtime),0) searchtime_ms_snitt
FROM (
SELECT occurred, searchtime, NTILE(20) OVER (ORDER BY searchtime) AS percentil
FROM stat_requests) a
WHERE percentil BETWEEN 2 AND 19
GROUP BY period
ORDER BY period;