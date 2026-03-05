import type { PagedResult, GuildDto, GuildDetailDto, GuildStatisticsDto } from "@/types/api/Guild";
import type { EncounterListItemDto } from "@/types/api/Encounter";

export interface IGuildsApi {
  getGuilds(params: GetGuildsParams): Promise<PagedResult<GuildDto>>;
  getGuildById(id: number): Promise<GuildDetailDto | null>;
  getGuildStatistics(id: number): Promise<GuildStatisticsDto>;
  getGuildUniqueEncounters(guildId: number, raidTypeId?: number | null): Promise<EncounterListItemDto[]>;
}

export interface GetGuildsParams {
  search?: string;
  sortField?: string;
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export interface GetGuildMembersParams {
  search?: string;
  role?: string;
  characterClass?: string;
  spec?: string;
  itemLevelMin?: number;
  itemLevelMax?: number;
  raidTypeId?: number;
  encounterName?: string;
  sortField?: string;
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}
