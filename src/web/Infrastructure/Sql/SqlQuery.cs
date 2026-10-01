namespace Plandokument.Infrastructure.Sql;

public enum SqlQuery
{
    SelectPlanGeometriBas,
    SelectPlanRegisterBas,
    SelectPlanBerorFastighet,
    SelectPlanBerorPlan,
    SelectPlanGeometriAll,
    SelectPlanGeometriBySearch,
    CreateStatRequestsTable,
    InsertStatRequests
}

public static class SqlQueryExtensions
{
    public static string ToFileName(this SqlQuery query) => query switch
    {
        SqlQuery.SelectPlanGeometriBas => "select-plan-geometri-bas.pgsql",
        SqlQuery.SelectPlanRegisterBas => "select-plan-register-bas.sql",
        SqlQuery.SelectPlanBerorFastighet => "select-plan-beror-fastighet.sql",
        SqlQuery.SelectPlanBerorPlan => "select-plan-beror-plan.sql",
        SqlQuery.SelectPlanGeometriAll => "select-plan-geometries-all-as-geojson.pgsql",
        SqlQuery.SelectPlanGeometriBySearch => "select-plan-geometries-bysearch-as-geojson.pgsql",
        SqlQuery.CreateStatRequestsTable => "create-stat-requests.sql",
        SqlQuery.InsertStatRequests => "insert-stat-requests.sql",
        _ => throw new ArgumentOutOfRangeException(nameof(query), query, null)
    };
}
