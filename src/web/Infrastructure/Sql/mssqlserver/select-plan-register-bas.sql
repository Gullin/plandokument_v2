SELECT  CAST(fir.plan_id AS VARCHAR(20)) AS plan_id,
        fir.lmakt,
        fir.egn_akt,
        fir.uuid,
        fir.planfk,
        fir.plannamn,
        fir.status,
        fir.status_text,
        CASE WHEN CAST(GETDATE() AS DATETIME) BETWEEN CONVERT(DATETIME, genomf_fromdat, 112) AND CONVERT(DATETIME, genomf_tilldat, 112) THEN 1 ELSE 0 END AS isgenomf,
        CASE
            WHEN fir.beslutsdat IS NOT NULL THEN CONCAT(SUBSTRING(fir.beslutsdat, 1, 4), '-', SUBSTRING(fir.beslutsdat, 5, 2), '-', SUBSTRING(fir.beslutsdat, 7, 2))
            ELSE fir.beslutsdat END AS dat_beslut,
        CASE
            WHEN fir.genomf_fromdat IS NOT NULL THEN CONCAT(SUBSTRING(fir.genomf_fromdat, 1, 4), '-', SUBSTRING(fir.genomf_fromdat, 5, 2), '-', SUBSTRING(fir.genomf_fromdat, 7, 2))
            ELSE fir.genomf_fromdat END AS dat_genomf_f,
        CASE
            WHEN fir.genomf_tilldat IS NOT NULL THEN CONCAT(SUBSTRING(fir.genomf_tilldat, 1, 4), '-', SUBSTRING(fir.genomf_tilldat, 5, 2), '-', SUBSTRING(fir.genomf_tilldat, 7, 2))
            ELSE fir.genomf_tilldat END AS dat_genomf_t,
        CASE
            WHEN fir.lagakraft_dat IS NOT NULL THEN CONCAT(SUBSTRING(fir.lagakraft_dat, 1, 4), '-', SUBSTRING(fir.lagakraft_dat, 5, 2), '-', SUBSTRING(fir.lagakraft_dat, 7, 2))
            ELSE fir.lagakraft_dat END AS dat_lagakraft,
        kom.komkod
FROM(SELECT fr.pb AS plan_id, fr.planfk AS planfk, fr.lmakt AS lmakt, pegn.plannr AS egn_akt, fr.uuid AS uuid, fr.plannamn AS plannamn, fr.pstatus AS status,
            CASE fr.pstatus
                WHEN 'A' THEN 'Avregistrerad'
                WHEN 'B' THEN 'Beslut'
                WHEN 'F' THEN 'Förslag'
                WHEN 'P' THEN 'Preliminär registrering'
                ELSE 'n/a' END AS status_text,
             dat.beslutsdat AS beslutsdat, dat.genomf_fromdat AS genomf_fromdat, dat.genomf_tilldat AS genomf_tilldat, dat.gallertill_dat AS gallertill_dat, dat.lagakraft_dat AS lagakraft_dat, dat.sajdat AS sajdat
      --FROM   fir_plan fr
      FROM   off_fir_plan fr
        LEFT JOIN fir_plan_egnauppg pegn
            ON fr.pb = pegn.pb
        --LEFT JOIN fir_plan_beslut dat
        LEFT JOIN off_fir_plan_beslut dat
            ON fr.pb = dat.pb
      WHERE fr.planfk IN (
               'DP',
               'ÄDP',
               'OB',
               'ÄOB',
               'BPL',
               'SPL'
            )) fir
    LEFT JOIN(SELECT fr.kodvarde AS status, fr.beskrivning AS status_text
               FROM   fir_firkoder fr
               WHERE  fr.kodtyp = 'PSTATUS') fir_kod
        ON fir.status = fir_kod.status
    --LEFT JOIN fir_plan_kommun kom
    LEFT JOIN off_fir_plan_kommun kom
        ON fir.plan_id = kom.pb