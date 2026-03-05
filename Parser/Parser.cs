using System.Collections.Concurrent;
using System.Diagnostics;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto;
using Parser.Models;
using Parser.Models.Rpc;

namespace Parser;

public class Parser : BaseParser
{
    private readonly SemaphoreSlim _semaphoreSlim = new(6, 6);
    private readonly IRaidDataService _raidDataService;

    public Parser(IRaidDataService raidDataService)
    {
        _raidDataService = raidDataService;
    }

    public override async Task InvokeAsync(ParserOptions options, CancellationToken cancellationToken = default)
    {
        var globalWatch = Stopwatch.StartNew();

        if (options.RaidTypes.Count == 0)
        {
            Console.WriteLine("ERROR: No RaidTypes provided in options");
            return;
        }

        var totalSavedRaids = 0;
        var totalSavedEncounters = 0;
        var totalSavedPlayerEncounters = 0;

        for (var raidTypeIndex = 0; raidTypeIndex < options.RaidTypes.Count; raidTypeIndex++)
        {
            var raidType = options.RaidTypes[raidTypeIndex];
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"\n=== Processing RaidType {raidTypeIndex + 1}/{options.RaidTypes.Count}: {raidType.Name} ===");

            var raids = await GetRaidsForRaidTypeAsync(options, raidType);

            if (raids.Count == 0)
            {
                Console.WriteLine($"No raids found for {raidType.Name}, skipping...");
                continue;
            }

            Console.WriteLine($"Raids parsed: {raids.Count}");

            var raidEncounters = await GetRaidEncountersAsync(options, raids, 100);

            var encounters = new List<RaidEncounter>();
            foreach (var (_, _encounters) in raidEncounters)
                encounters.AddRange(_encounters);

            Console.WriteLine($"Encounters parsed: {encounters.Count}");

            var encounterDetails = await GetEncounterDetailsAsync(options, encounters, 50);

            var totalPlayerEncounters = encounterDetails.Values.Sum(details => details.Count);
            Console.WriteLine($"Encounter details parsed: {encounterDetails.Count} encounters, {totalPlayerEncounters} player records");

            var batchRaidRecords = raids.Select(ParserMapper.ToRaidRecord).ToList();

            var batchEncounterRecords = new List<EncounterRecord>();
            foreach (var (raid, raidEncounterList) in raidEncounters)
            {
                foreach (var encounter in raidEncounterList)
                {
                    batchEncounterRecords.Add(ParserMapper.ToEncounterRecord(encounter));
                }
            }

            var batchPlayerEncounterRecords = new List<PlayerEncounterRecord>();
            foreach (var (encounter, detailList) in encounterDetails)
            {
                foreach (var detail in detailList)
                {
                    batchPlayerEncounterRecords.Add(ParserMapper.ToPlayerEncounterRecord(detail));
                }
            }

            Console.WriteLine($"Saving to database: {batchRaidRecords.Count} raids, {batchEncounterRecords.Count} encounters, {batchPlayerEncounterRecords.Count} player encounters");

            await _raidDataService.SaveRaidDataAsync(batchRaidRecords, batchEncounterRecords, batchPlayerEncounterRecords, cancellationToken);

            totalSavedRaids += batchRaidRecords.Count;
            totalSavedEncounters += batchEncounterRecords.Count;
            totalSavedPlayerEncounters += batchPlayerEncounterRecords.Count;

            stopwatch.Stop();
            Console.WriteLine($"✓ Спарсили: {raidType.Name} - {batchRaidRecords.Count} рейдов, {batchEncounterRecords.Count} энкаунтеров, {batchPlayerEncounterRecords.Count} записей игроков, Time: {stopwatch.ElapsedMilliseconds}ms");
        }

        globalWatch.Stop();
        Console.WriteLine($"\n=== Total Summary ===");
        Console.WriteLine($"Total time: {globalWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Total saved: {totalSavedRaids} raids, {totalSavedEncounters} encounters, {totalSavedPlayerEncounters} player encounters");
    }


    public async Task<List<Raid>> GetRaidsForRaidTypeAsync(ParserOptions options, NibelungLog.Domain.Entities.RaidType raidType, uint batchSize = 500)
    {
        try
        {
            Console.WriteLine(
                $"Requesting raids for RaidType: {raidType.Name} (Map={raidType.Map}, Difficulty={raidType.Difficulty}, InstanceType={raidType.InstanceType})");

            var mapIds = new[] { int.Parse(raidType.Map) };
            var firstRequest = CreateRequest("cmdGetPveLadder",
                [CreateRaidRequestData(mapIds, options.RealmId, int.Parse(raidType.Difficulty), raidType.InstanceType, 0, batchSize)]);
            var firstResponse = await SendRequestAsync<RaidResult>(options.ServerId, firstRequest);

            if (firstResponse == null)
            {
                Console.WriteLine($"ERROR: firstResponse is null for RaidType {raidType.Name}");
                return [];
            }

            Console.WriteLine($"Response received: {firstResponse.Count} items");

            var firstResult = GetResponseByRequestTid(firstResponse, firstRequest.TId)?.Result;

            if (firstResult == null)
            {
                Console.WriteLine($"ERROR: firstResult is null for RaidType {raidType.Name}");
                return [];
            }

            Console.WriteLine($"First result for {raidType.Name}: Total={firstResult.Total}, Data.Count={firstResult.Data.Count}");

            var entries = new List<Raid>(firstResult.Data.Count);
            entries.AddRange(firstResult.Data);

            while (entries.Count < firstResult.Total)
            {
                var nextRequest = CreateRequest("cmdGetPveLadder",
                    [CreateRaidRequestData(mapIds, options.RealmId, int.Parse(raidType.Difficulty), raidType.InstanceType, (uint)entries.Count, batchSize)]);
                var nextResponse = await SendRequestAsync<RaidResult>(options.ServerId, nextRequest);

                if (nextResponse == null)
                {
                    Console.WriteLine($"WARNING: nextResponse is null at offset {entries.Count} for RaidType {raidType.Name}");
                    break;
                }

                var nextBatch = GetResponseByRequestTid(nextResponse, nextRequest.TId)?.Result?.Data;

                if (nextBatch == null || nextBatch.Count == 0)
                {
                    Console.WriteLine($"No more batches at offset {entries.Count} for RaidType {raidType.Name}");
                    break;
                }

                entries.AddRange(nextBatch);
                Console.WriteLine($"Loaded {entries.Count} / {firstResult.Total} raids for {raidType.Name}");
            }

            var filteredEntries = entries
                .Where(r => r.Map.ToString() == raidType.Map 
                            && r.Difficulty.ToString() == raidType.Difficulty 
                            && r.InstanceType.ToString() == raidType.InstanceType)
                .ToList();

            Console.WriteLine($"Filtered {filteredEntries.Count} raids for RaidType {raidType.Name} (from {entries.Count} total)");

            return filteredEntries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION in GetRaidsForRaidTypeAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return [];
        }
    }

    private static RequestData CreateRaidRequestData(int[] filterMaps, int filterRealm, int? filterDifficulty, string? filterInstanceType, uint start, uint limit)
    {
        var filters = new List<FilterModel>
        {
            new FilterModel
            {
                Property = "map",
                Value = filterMaps.Select(x => x.ToString()).ToList()
            },
            new FilterModel
            {
                Property = "realm",
                Value = filterRealm
            }
        };

        if (filterDifficulty.HasValue)
        {
            filters.Add(new FilterModel
            {
                Property = "difficulty",
                Value = filterDifficulty.Value
            });
        }

        if (!string.IsNullOrEmpty(filterInstanceType))
        {
            filters.Add(new FilterModel
            {
                Property = "instance_type",
                Value = filterInstanceType
            });
        }

        return new RequestData
        {
            Page = start / limit + 1,
            Start = start,
            Limit = limit,
            Sort =
            [
                new SortModel
                {
                    Property = "total_pve_points_guild",
                    Direction = "DESC"
                },
                new SortModel
                {
                    Property = "total_time",
                    Direction = "ASC"
                }
            ],
            Filter = filters
        };
    }

    public async Task<Dictionary<Raid, List<RaidEncounter>>> GetRaidEncountersAsync(ParserOptions options,
        List<Raid> raids, uint batchSize = 50)
    {
        try
        {
            var batchIndex = 0;
            var batches = Math.Ceiling(raids.Count / (double)batchSize);
            var results = new ConcurrentDictionary<Raid, List<RaidEncounter>>();
            var tasks = new List<Task>();


            for (var i = 0; i < batches; i++)
            {
                var limit = (int)batchSize;
                var offset = i * (int)batchSize;

                tasks.Add(Task.Run(async () =>
                    {
                        await _semaphoreSlim.WaitAsync();

                        try
                        {
                            Console.WriteLine($"Batch {++batchIndex} / {batches}");

                            var raidIds = raids
                                .Select(raid => raid.Id)
                                .Skip(offset)
                                .Take(limit)
                                .ToArray();

                            var requests = raidIds.Select(id => CreateRequest("cmdGetPveLadderDetail",
                            [
                                new RequestData
                                {
                                    Id = id,
                                    Sort =
                                    [
                                        new SortModel
                                        {
                                            Property = null,
                                            Direction = "ASC"
                                        }
                                    ]
                                }
                            ])).ToArray();

                            var responseModel = await SendRequestAsync<RaidDetailsResult>(options.ServerId, requests);

                            if (responseModel == null)
                                throw new Exception("Response is null");

                            for (var index = 0; index < raidIds.Length; index++)
                            {
                                var id = raidIds[index];

                                var raid = raids.First(raid => raid.Id == id);
                                var details = responseModel[index];

                                if (details.Result == null)
                                    throw new Exception("Details is null");

                                if (!results.ContainsKey(raid))
                                    results.TryAdd(raid, details.Result.Data);
                                else
                                    Console.WriteLine($"Raid {id} already exists");
                            }
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                            await Task.Delay(TimeSpan.FromMilliseconds(200));
                        }
                    })
                );
            }

            Task.WaitAll(tasks);

            return results.ToDictionary();
        }
        catch
        {
            return [];
        }
    }


    public async Task<Dictionary<RaidEncounter, List<RaidEncounterDetail>>> GetEncounterDetailsAsync(
        ParserOptions options, List<RaidEncounter> encounters, uint batchSize = 50)
    {
        try
        {
            var batchIndex = 0;
            var batches = Math.Ceiling(encounters.Count / (double)batchSize);
            var results = new ConcurrentDictionary<RaidEncounter, List<RaidEncounterDetail>>();
            var tasks = new List<Task>();
            var errors = new ConcurrentBag<string>();

            Console.WriteLine($"Starting GetEncounterDetailsAsync: {encounters.Count} encounters, {batches} batches");

            for (var i = 0; i < batches; i++)
            {
                var limit = (int)batchSize;
                var offset = i * (int)batchSize;
                var batchNum = i + 1;

                tasks.Add(Task.Run(async () =>
                    {
                        await _semaphoreSlim.WaitAsync();

                        try
                        {
                            Console.WriteLine($"Batch {++batchIndex} / {batches}");

                            var encounterDetails = encounters
                                .Select(encounter => (encounter.LogInstanceId, encounter.StartTime))
                                .Skip(offset)
                                .Take(limit)
                                .ToArray();

                            if (encounterDetails.Length == 0)
                            {
                                Console.WriteLine($"Batch {batchNum}: No encounters to process");
                                return;
                            }

                            var requests = encounterDetails.Select(tuple => CreateRequest("cmdGetPveLadderEncounters",
                            [
                                new RequestData
                                {
                                    Id = tuple.LogInstanceId,
                                    Time = tuple.StartTime,
                                    Sort =
                                    [
                                        new SortModel
                                        {
                                            Property = null,
                                            Direction = "ASC"
                                        }
                                    ]
                                }
                            ])).ToArray();

                            var responseModel =
                                await SendRequestAsync<RaidEncounterDetailResult>(options.ServerId, requests);

                            if (responseModel == null)
                            {
                                var errorMsg = $"Batch {batchNum}: Response is null";
                                Console.WriteLine($"ERROR: {errorMsg}");
                                errors.Add(errorMsg);
                                return;
                            }

                            if (responseModel.Count != encounterDetails.Length)
                            {
                                var errorMsg =
                                    $"Batch {batchNum}: Response count mismatch. Expected {encounterDetails.Length}, got {responseModel.Count}";
                                Console.WriteLine($"ERROR: {errorMsg}");
                                errors.Add(errorMsg);
                            }

                            for (var index = 0; index < encounterDetails.Length; index++)
                            {
                                var (id, startTime) = encounterDetails[index];

                                var encounter = encounters.FirstOrDefault(e =>
                                    e.LogInstanceId == id && e.StartTime == startTime);

                                if (encounter == null)
                                {
                                    var errorMsg =
                                        $"Batch {batchNum}, index {index}: Encounter not found for LogInstanceId={id}, StartTime={startTime}";
                                    Console.WriteLine($"ERROR: {errorMsg}");
                                    errors.Add(errorMsg);
                                    continue;
                                }

                                if (index >= responseModel.Count)
                                {
                                    var errorMsg =
                                        $"Batch {batchNum}, index {index}: Response index out of range. Response count: {responseModel.Count}";
                                    Console.WriteLine($"ERROR: {errorMsg}");
                                    errors.Add(errorMsg);
                                    continue;
                                }

                                var details = responseModel[index];

                                if (details.Result == null)
                                {
                                    var errorMsg =
                                        $"Batch {batchNum}, index {index}: Details.Result is null for LogInstanceId={id}";
                                    Console.WriteLine($"ERROR: {errorMsg}");
                                    errors.Add(errorMsg);
                                    continue;
                                }

                                if (details.Result.Data == null || details.Result.Data.Count == 0)
                                {
                                    Console.WriteLine(
                                        $"WARNING: Batch {batchNum}, index {index}: No player data for LogInstanceId={id}, StartTime={startTime}");
                                    continue;
                                }

                                if (!results.ContainsKey(encounter))
                                {
                                    results.TryAdd(encounter, details.Result.Data);
                                    Console.WriteLine(
                                        $"Batch {batchNum}, index {index}: Added {details.Result.Data.Count} player records for LogInstanceId={id}");
                                }
                                else
                                {
                                    Console.WriteLine(
                                        $"WARNING: Encounter LogInstanceId={id}, StartTime={startTime} already exists");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = $"Batch {batchNum}: Exception - {ex.Message}";
                            Console.WriteLine($"ERROR: {errorMsg}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");
                            errors.Add(errorMsg);
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    })
                );
            }

            Task.WaitAll(tasks);

            Console.WriteLine(
                $"GetEncounterDetailsAsync completed. Results: {results.Count} encounters, Total errors: {errors.Count}");

            if (errors.Count > 0)
            {
                Console.WriteLine($"First 10 errors:");
                foreach (var error in errors.Take(10))
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            return results.ToDictionary();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION in GetEncounterDetailsAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return [];
        }
    }

    public async Task<List<DungeonMapModelResult>?> GetDungeonMapsAsync(int serverId)
    {
        try
        {
            var request = CreateRequest("cmdGetDungeonMaps", [new RequestData()]);
            var responseModel = await SendRequestAsync<List<DungeonMapModelResult>>(serverId, request);
            return responseModel![0].Result;
        }
        catch
        {
            return null;
        }
    }
}