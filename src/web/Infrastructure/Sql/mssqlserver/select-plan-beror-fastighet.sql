SELECT CAST(b.pb AS VARCHAR(20)) AS NYCKEL,
       f.fnr AS NYCKEL_FASTIGHET,
       UPPER(CONCAT(f.trakt, ' ', f.fbetnr)) AS FASTIGHET
FROM   fir_fastigh f,
       --fir_plan_planberor b
       off_fir_plan_planberor b
WHERE b.fnr = f.fnr
GROUP BY CAST(b.pb AS VARCHAR(20)), f.fnr, UPPER(CONCAT(f.trakt, ' ', f.fbetnr))