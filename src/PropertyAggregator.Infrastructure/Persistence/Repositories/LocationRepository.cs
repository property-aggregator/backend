using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Infrastructure.Persistence.Converters;

namespace PropertyAggregator.Infrastructure.Persistence.Repositories;

internal sealed class LocationRepository(AppDbContext db) : ILocationRepository
{
	private static readonly string Voivodeship = EnumDatabaseValue.For(LocationType.Voivodeship);
	private static readonly string County = EnumDatabaseValue.For(LocationType.County);
	private static readonly string Municipality = EnumDatabaseValue.For(LocationType.Municipality);
	private static readonly string Locality = EnumDatabaseValue.For(LocationType.Locality);
	private static readonly string District = EnumDatabaseValue.For(LocationType.District);

	public Task<bool> IsImportedAsync(string version, CancellationToken cancellationToken) =>
		db.LocationDatasetVersions.AnyAsync(entry => entry.Version == version, cancellationToken);

	public async Task ImportAsync(string version, IReadOnlyList<LocationImportRow> rows, DateTime importedAt,
		CancellationToken cancellationToken)
	{
		await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken);

		await db.Database.ExecuteSqlRawAsync(
			"""
			CREATE TEMP TABLE "locationStaging" (
				"type" varchar(20) NOT NULL,
				"code" varchar(120) NOT NULL,
				"name" varchar(100) NOT NULL,
				"label" varchar(300) NOT NULL,
				"tercCode" varchar(7) NOT NULL,
				"simcCode" varchar(7),
				"isCity" boolean NOT NULL,
				"isHidden" boolean NOT NULL
			) ON COMMIT DROP
			""",
			cancellationToken);

		await CopyToStagingAsync(rows, cancellationToken);

		await db.Database.ExecuteSqlAsync(
			$"""
			INSERT INTO "locations"
				("type", "code", "name", "normalizedName", "label", "tercCode", "simcCode", "isCity", "isHidden",
				 "isActive", "offerCount")
			SELECT "type", "code", "name", lower(unaccent("name")), "label", "tercCode", "simcCode", "isCity", "isHidden",
				true, 0
			FROM "locationStaging"
			ON CONFLICT ("type", "code") DO UPDATE SET
				"name" = excluded."name",
				"normalizedName" = excluded."normalizedName",
				"label" = excluded."label",
				"tercCode" = excluded."tercCode",
				"simcCode" = excluded."simcCode",
				"isCity" = excluded."isCity",
				"isHidden" = excluded."isHidden",
				"isActive" = true
			""",
			cancellationToken);

		// Removed units stay referenced by properties and saved searches, so they are deactivated, not deleted.
		await db.Database.ExecuteSqlAsync(
			$"""
			UPDATE "locations" AS l
			SET "isActive" = false
			WHERE l."type" <> {District}
				AND l."isActive"
				AND NOT EXISTS (SELECT 1 FROM "locationStaging" AS s WHERE s."type" = l."type" AND s."code" = l."code")
			""",
			cancellationToken);

		await db.Database.ExecuteSqlAsync(
			$"""UPDATE "properties" SET "locationMatchedAt" = NULL WHERE "locationMatchedAt" IS NOT NULL""",
			cancellationToken);

		await db.Database.ExecuteSqlAsync(
			$"""INSERT INTO "locationDatasetVersions" ("version", "importedAt") VALUES ({version}, {importedAt})""",
			cancellationToken);

		await transaction.CommitAsync(cancellationToken);
	}

	// Ambiguous names (several villages called the same in one voivodeship) leave the SIMC code empty and keep
	// only the TERC levels the candidates share, so the property can still be found by county or voivodeship.
	public Task<int> MatchPropertiesAsync(DateTime matchedAt, CancellationToken cancellationToken) =>
		db.Database.ExecuteSqlAsync(
			$"""
			WITH pending AS (
				SELECT
					p."id",
					lower(unaccent(btrim(p."city"))) AS "cityKey",
					NULLIF(lower(unaccent(btrim(p."district"))), '') AS "districtKey",
					v."code" AS "voivodeshipCode"
				FROM "properties" AS p
				LEFT JOIN "locations" AS v ON v."type" = {Voivodeship}
					AND v."normalizedName" = regexp_replace(lower(unaccent(btrim(p."province"))), '^wojewodztwo\s+', '')
				WHERE p."locationMatchedAt" IS NULL
			),
			resolved AS (
				SELECT
					pending."id",
					CASE
						WHEN count(*) = 1 THEN min(l."code")
						WHEN count(*) FILTER (WHERE l."isCity") = 1 THEN min(l."code") FILTER (WHERE l."isCity")
					END AS "simcCode",
					CASE
						WHEN count(*) = 1 THEN min(l."tercCode")
						WHEN count(*) FILTER (WHERE l."isCity") = 1 THEN min(l."tercCode") FILTER (WHERE l."isCity")
						WHEN count(DISTINCT l."tercCode") = 1 THEN min(l."tercCode")
						WHEN count(DISTINCT left(l."tercCode", 4)) = 1 THEN min(left(l."tercCode", 4))
						WHEN count(DISTINCT left(l."tercCode", 2)) = 1 THEN min(left(l."tercCode", 2))
					END AS "tercCode"
				FROM pending
				JOIN "locations" AS l ON l."type" = {Locality}
					AND l."isActive"
					AND l."normalizedName" = pending."cityKey"
					AND (pending."voivodeshipCode" IS NULL OR left(l."tercCode", 2) = pending."voivodeshipCode")
				GROUP BY pending."id"
			)
			UPDATE "properties" AS p
			SET
				"simcCode" = resolved."simcCode",
				"voivodeshipCode" = COALESCE(left(resolved."tercCode", 2), pending."voivodeshipCode"),
				"countyCode" = CASE WHEN length(resolved."tercCode") >= 4 THEN left(resolved."tercCode", 4) END,
				"municipalityCode" = CASE WHEN length(resolved."tercCode") = 7 THEN resolved."tercCode" END,
				"districtCode" = resolved."simcCode" || ':' || pending."districtKey",
				"locationMatchedAt" = {matchedAt}
			FROM pending
			LEFT JOIN resolved USING ("id")
			WHERE p."id" = pending."id"
			""",
			cancellationToken);

	// Archived offers are included, so a district survives until its offers come back.
	public Task RefreshDistrictsAsync(CancellationToken cancellationToken) =>
		db.Database.ExecuteSqlAsync(
			$"""
			INSERT INTO "locations"
				("type", "code", "name", "normalizedName", "label", "tercCode", "simcCode", "isCity", "isHidden",
				 "isActive", "offerCount")
			SELECT {District}, d."districtCode", d."name", lower(unaccent(d."name")), d."name" || ', ' || l."label",
				l."tercCode", l."code", false, false, true, 0
			FROM (
				SELECT "districtCode", mode() WITHIN GROUP (ORDER BY btrim("district")) AS "name"
				FROM "properties"
				WHERE "districtCode" IS NOT NULL
				GROUP BY "districtCode"
			) AS d
			JOIN "locations" AS l ON l."type" = {Locality} AND l."code" = split_part(d."districtCode", ':', 1)
			ON CONFLICT ("type", "code") DO UPDATE SET
				"name" = excluded."name",
				"normalizedName" = excluded."normalizedName",
				"label" = excluded."label",
				"tercCode" = excluded."tercCode"
			WHERE "locations"."name" <> excluded."name" OR "locations"."label" <> excluded."label"
			""",
			cancellationToken);

	public Task RefreshOfferCountsAsync(CancellationToken cancellationToken) =>
		db.Database.ExecuteSqlAsync(
			$"""
			WITH active AS (
				SELECT "voivodeshipCode", "countyCode", "municipalityCode", "simcCode", "districtCode"
				FROM "properties"
				WHERE "archivedAt" IS NULL AND "locationMatchedAt" IS NOT NULL
			),
			counts AS (
				SELECT {Voivodeship} AS "type", "voivodeshipCode" AS "code", count(*) AS "total"
				FROM active WHERE "voivodeshipCode" IS NOT NULL GROUP BY 2
				UNION ALL
				SELECT {County}, "countyCode", count(*)
				FROM active WHERE "countyCode" IS NOT NULL GROUP BY 2
				UNION ALL
				SELECT {Municipality}, "municipalityCode", count(*)
				FROM active WHERE "municipalityCode" IS NOT NULL GROUP BY 2
				UNION ALL
				SELECT {Locality}, "simcCode", count(*)
				FROM active WHERE "simcCode" IS NOT NULL GROUP BY 2
				UNION ALL
				SELECT {District}, "districtCode", count(*)
				FROM active WHERE "districtCode" IS NOT NULL GROUP BY 2
			)
			UPDATE "locations" AS l
			SET "offerCount" = COALESCE(c."total", 0)
			FROM "locations" AS current
			LEFT JOIN counts AS c ON c."type" = current."type" AND c."code" = current."code"
			WHERE l."id" = current."id" AND l."offerCount" <> COALESCE(c."total", 0)
			""",
			cancellationToken);

	private async Task CopyToStagingAsync(IReadOnlyList<LocationImportRow> rows, CancellationToken cancellationToken)
	{
		NpgsqlConnection connection = (NpgsqlConnection)db.Database.GetDbConnection();

		await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(
			"""
			COPY "locationStaging" ("type", "code", "name", "label", "tercCode", "simcCode", "isCity", "isHidden")
			FROM STDIN (FORMAT BINARY)
			""",
			cancellationToken);

		foreach (LocationImportRow row in rows)
		{
			await importer.StartRowAsync(cancellationToken);
			await importer.WriteAsync(EnumDatabaseValue.For(row.Type), NpgsqlDbType.Varchar, cancellationToken);
			await importer.WriteAsync(row.Code, NpgsqlDbType.Varchar, cancellationToken);
			await importer.WriteAsync(row.Name, NpgsqlDbType.Varchar, cancellationToken);
			await importer.WriteAsync(row.Label, NpgsqlDbType.Varchar, cancellationToken);
			await importer.WriteAsync(row.TercCode, NpgsqlDbType.Varchar, cancellationToken);

			if (row.SimcCode is null)
				await importer.WriteNullAsync(cancellationToken);
			else
				await importer.WriteAsync(row.SimcCode, NpgsqlDbType.Varchar, cancellationToken);

			await importer.WriteAsync(row.IsCity, NpgsqlDbType.Boolean, cancellationToken);
			await importer.WriteAsync(row.IsHidden, NpgsqlDbType.Boolean, cancellationToken);
		}

		await importer.CompleteAsync(cancellationToken);
	}
}
