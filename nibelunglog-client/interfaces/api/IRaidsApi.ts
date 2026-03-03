import type { PagedResult, RaidDto, RaidDetailDto } from "@/types/api/Raid";

export interface IRaidsApi {
  getRaids(params: GetRaidsParams): Promise<PagedResult<RaidDto>>;
  getRaidById(id: number): Promise<RaidDetailDto | null>;
}

export interface GetRaidsParams {
  raidTypeId?: number;
  raidTypeName?: string;
  guildName?: string;
  leaderName?: string;
  guild?: string;
  leader?: string;
  page?: number;
  pageSize?: number;
}
