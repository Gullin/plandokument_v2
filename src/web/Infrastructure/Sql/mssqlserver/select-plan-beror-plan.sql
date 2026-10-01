SELECT CAST(p1.pb AS VARCHAR(20)) AS NYCKEL,
           p1.planfk AS PLANFK,
           LOWER(kod.beskrivning) AS BESKRIVNING,
           CAST(han.pb_hanvisn AS VARCHAR(20)) AS NYCKEL_PAVARKAN,
           han.lmakt AS PAVERKAN,
           han.planfk AS PAV_PLANFK,
           han.hanvstatus AS STATUS_PAVARKAN,
           1 AS REGISTRERAT_BESLUT
    --FROM fir_plan p1,
    FROM off_fir_plan p1,
        (SELECT h.pb pb, h.pb_hanvisn pb_hanvisn, h.hanvtyp hanvtyp, p2.lmakt lmakt, p2.planfk planfk, p2.pstatus hanvstatus
         --FROM fir_plan_hanvisn h
         FROM off_fir_plan_hanvisn h
            --JOIN fir_plan p2
            JOIN off_fir_plan p2
                ON h.pb_hanvisn = p2.pb) han,
            fir_firkoder kod
    WHERE p1.pb = han.pb
    AND han.hanvtyp = kod.kodvarde
    AND kod.kodtyp = 'HANVTYP'
UNION ALL
    SELECT CAST(p1.pb AS VARCHAR(20)) AS NYCKEL,
           p1.planfk AS PLANFK,
           LOWER(kod.beskrivning) AS BESKRIVNING,
           hant.pav_pb AS NYCKEL_PAVARKAN,
           hant.hanvtext AS PAVERKAN,
           hant.planfk AS PAV_PLANFK,
           hant.hanvstatus AS STATUS_PAVARKAN,
           0 AS REGISTRERAT_BESLUT
    --FROM fir_plan p1,
    FROM off_fir_plan p1,
        (SELECT h.pb pb, p3.pb pav_pb, h.hanvtyp hanvtyp, h.hanvtext hanvtext, p2.planfk planfk, p2.pstatus hanvstatus
            --FROM fir_plan_hanvtext h
            FROM off_fir_plan_hanvtext h
            --JOIN fir_plan p2
            JOIN off_fir_plan p2
                ON h.pb = p2.pb
            --LEFT JOIN fir_plan p3
            LEFT JOIN off_fir_plan p3
                ON h.hanvtext = p3.lmakt) hant,
            fir_firkoder kod
    WHERE p1.pb = hant.pb
    AND hant.hanvtyp = kod.kodvarde
    AND kod.kodtyp = 'HANVTYP'