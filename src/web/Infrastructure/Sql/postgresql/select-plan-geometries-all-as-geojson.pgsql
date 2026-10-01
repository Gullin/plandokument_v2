-- All geometries of all plans as GeoJSON, grouped by rk_extid
-- Returns a JSON Feature for each rk_extid, with either a single geometry or a Multi geometry if multiple geometries exist for the same rk_extid
-- If no geometries are found, returns an empty JSON object
-- Note: This query does not take any input parameters and retrieves all geometries from the td_drk.rk_plan_y table
-- Note: Inga övriga egenskaper inkluderas i resultatet så när som för rk_extid som id
WITH
  planer AS (
    SELECT
      ST_Force2D(l.geom) AS geom,
      l.rk_extid
    FROM td_drk.rk_plan_y l
  ),
  grouped AS (
    SELECT
      COUNT(*) AS cnt,
      array_agg(ST_AsGeoJSON(geom, 6, 8)::TEXT) AS geojson_arr,
      rk_extid
    FROM planer
    GROUP BY rk_extid
  )
SELECT
  COALESCE(
      CASE
        WHEN cnt = 1 THEN
          json_build_object(
            'type',       'Feature',
            'id',         rk_extid,
            'geometry',   geojson_arr[1]::json,
            'properties', '{}'::json
          )
        ELSE
          json_build_object(
            'type',       'Feature',
            'id',         rk_extid,
            'geometry', (
              SELECT ST_AsGeoJSON(ST_Multi(ST_Union(polygons.geom)))::json
              FROM (
                SELECT ST_GeomFromGeoJSON(geojson::json) AS geom
                FROM unnest(grouped.geojson_arr) AS geojson
              ) polygons
            ),
            'properties', '{}'::json
          )
      END::text,
    '{}'::json::text    -- Return empty JSON object if no geometries found
  ) AS result
FROM grouped