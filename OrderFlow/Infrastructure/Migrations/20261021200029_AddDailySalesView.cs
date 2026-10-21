using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Migrations
{
    /// <summary>
    /// Creates the vw_DailySales view: sales aggregated per day and per product.
    /// It is mapped to the keyless DailySales entity and queried by ReportService with LINQ.
    /// </summary>
    public partial class AddDailySalesView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE VIEW dbo.vw_DailySales AS
                SELECT
                    CAST(o.OrderDate AS date)  AS SaleDate,
                    ol.ProductId               AS ProductId,
                    p.Name                     AS ProductName,
                    COUNT(DISTINCT o.Id)       AS OrderCount,
                    SUM(ol.Quantity)           AS QuantitySold,
                    SUM(ol.LineTotal)          AS Revenue
                FROM dbo.Orders o
                INNER JOIN dbo.OrderLines ol ON ol.OrderId = o.Id
                INNER JOIN dbo.Products p    ON p.Id = ol.ProductId
                GROUP BY CAST(o.OrderDate AS date), ol.ProductId, p.Name;
                """,
                suppressTransaction: true); // CREATE VIEW must be the only statement in its SQL batch
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_DailySales;", suppressTransaction: true);
        }
    }
}
