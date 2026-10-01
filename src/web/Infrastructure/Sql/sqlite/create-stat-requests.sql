CREATE TABLE IF NOT EXISTS stat_requests
(
occurred		TEXT,		-- SQLite stödjer ej datatyp DATETIME, måste sparas i ex. text och då ISO8601 YYYY-MM-DD HH:MM:SS.SSS
nbrsearched		INTEGER,	-- Antalet samtida sökta planer
nbrhits			INTEGER,	-- Antalet hittade planer av sökta
searchtime		REAL		-- Tid (ms) sökningen tog, från inkommen sökparameter till hittat resultat
)