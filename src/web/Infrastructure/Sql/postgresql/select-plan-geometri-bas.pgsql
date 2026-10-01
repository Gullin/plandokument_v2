SELECT  f.rk_extid AS lmakt,
        STRING_AGG(f.td_id::TEXT,',') AS local_id_list,
        STRING_AGG(rk_gmlid,',') AS rk_gmlid_list,
        CASE i.planavgift
          WHEN 'N' THEN 'Nej'
          WHEN 'J' THEN 'Ja'
        ELSE i.planavgift END AS planavgift,
        i.arkivserie_pb AS akt_pb
FROM        td_drk.rk_plan_y f
  LEFT JOIN td_drk.rk_plan_y_info i
    ON f.rk_extid::TEXT = i.rk_extid::TEXT
WHERE f.rk_dep IN (
   'PLANDP',
   'PLANÄDP',
   'PLANOB',
   'PLANÄOB',
   'PLANBPL',
   'PLANSPL'
)
GROUP BY  f.rk_extid,
          CASE i.planavgift
              WHEN 'N' THEN 'Nej'
              WHEN 'J' THEN 'Ja'
          ELSE i.planavgift END,
          i.arkivserie_pb;