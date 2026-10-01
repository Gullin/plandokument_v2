SELECT	CASE name
			WHEN NULL THEN 'false'
			ELSE 'true'
		END table_exists
FROM	sqlite_master
WHERE	type = 'table'
AND		name = '@table_name'