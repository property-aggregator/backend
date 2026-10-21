using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Infrastructure.Persistence.Converters;

namespace PropertyAggregator.Infrastructure.Persistence;

internal sealed class PriceStatisticsWriter(AppDbContext db) : IPriceStatisticsWriter
{
	public Task<int> InsertAsync(int minSampleSize, DateOnly calculatedOn, CancellationToken cancellationToken)
	{
		decimal[] areaBounds = [.. PriceStatistic.AreaRangeBounds.Select(range => range.UpperBound)];
		string[] rangeNames =
		[
			.. PriceStatistic.AreaRangeBounds.Select(range => EnumDatabaseValue.For(range.Range)),
			EnumDatabaseValue.For(AreaM2Range.Over100)
		];
		string allRange = EnumDatabaseValue.For(AreaM2Range.All);

		return db.Database.ExecuteSqlAsync(
			$"""
			WITH all_offers AS (
				SELECT
					lower(btrim("city")) AS "city",
					lower(NULLIF(btrim("district"), '')) AS "district",
					"propertyType",
					"transactionType",
					"marketType",
					"pricePerM2",
					({rangeNames})[1 + (SELECT count(*) FROM unnest({areaBounds}) AS "bound" WHERE "bound" <= "areaM2")] AS "areaRange"
				FROM "properties"
				WHERE "archivedAt" IS NULL
					AND "pricePerM2" IS NOT NULL
					AND NULLIF(btrim("city"), '') IS NOT NULL
			),
			bounds AS (
				SELECT
					"city",
					"propertyType",
					"transactionType",
					"marketType",
					count(*) AS "total",
					percentile_cont(0.25) WITHIN GROUP (ORDER BY "pricePerM2") AS "q1",
					percentile_cont(0.75) WITHIN GROUP (ORDER BY "pricePerM2") AS "q3"
				FROM all_offers
				GROUP BY "city", "propertyType", "transactionType", "marketType"
			),
			offers AS (
				SELECT all_offers.*
				FROM all_offers
				JOIN bounds USING ("city", "propertyType", "transactionType", "marketType")
				WHERE bounds."total" < 8
					OR all_offers."pricePerM2" BETWEEN bounds."q1" - 3 * (bounds."q3" - bounds."q1")
						AND bounds."q3" + 3 * (bounds."q3" - bounds."q1")
			)
			INSERT INTO "priceStatistics"
				("city", "district", "propertyType", "transactionType", "marketType", "areaM2Range",
				 "avgPricePerM2", "medianPricePerM2", "sampleSize", "calculatedOn")
			SELECT
				"city",
				"district",
				"propertyType",
				"transactionType",
				"marketType",
				COALESCE("areaRange", {allRange}),
				round(avg("pricePerM2"), 2),
				round((percentile_cont(0.5) WITHIN GROUP (ORDER BY "pricePerM2"))::numeric, 2),
				count(*),
				{calculatedOn}
			FROM offers
			GROUP BY "city", "propertyType", "transactionType", "marketType",
				GROUPING SETS (("district", "areaRange"), ("district"), ("areaRange"), ())
			HAVING count(*) >= {minSampleSize}
				AND ("district" IS NOT NULL OR GROUPING("district") = 1)
			ON CONFLICT ("city", "district", "propertyType", "transactionType", "marketType", "areaM2Range", "calculatedOn")
			DO NOTHING
			""",
			cancellationToken);
	}
}
