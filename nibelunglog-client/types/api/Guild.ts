export interface GuildDto {
  id: number;
  guildId: string;
  guildName: string;
  leaderName: string;
  membersCount: number;
  lastUpdated: string;
  createDate: string | null;
  fullRaidsCount: number;
  uniqueRaidLeadersCount: number;
  topDamageDealersCount: number;
  rating: number;
}

export interface GuildDetailDto {
  id: number;
  guildId: string;
  guildName: string;
  leaderName: string;
  membersCount: number;
  lastUpdated: string;
  createDate: string | null;
  fullRaidsCount: number;
  totalRaidsCount: number;
  uniqueRaidLeadersCount: number;
  topDamageDealersCount: number;
  totalEncountersCount: number;
  rating: number;
}

export interface GuildMemberDto {
  playerId: number;
  characterName: string;
  characterClass: string;
  className: string | null;
  specName: string | null;
  role: string | null;
  rank: string;
  joinedDate: string | null;
  totalEncounters: number;
  averageDps: number;
  maxDps: number;
  averageHps: number | null;
  maxHps: number | null;
}

export interface GuildStatisticsDto {
  classes: GuildClassStatisticsDto[];
  specs: GuildSpecStatisticsDto[];
  roles: GuildRoleStatisticsDto[];
}

export interface GuildClassStatisticsDto {
  className: string;
  count: number;
  percentage: number;
}

export interface GuildSpecStatisticsDto {
  specName: string;
  className: string;
  count: number;
  percentage: number;
}

export interface GuildRoleStatisticsDto {
  role: string;
  roleName: string;
  count: number;
  percentage: number;
}

export interface GuildProgressDto {
  startTime: string;
  raidTypeName: string;
  wipes: number;
  completedBosses: number;
  totalBosses: number;
  progressScore: number;
}

export interface GuildRaidStatisticsDto {
  averageWipesPerRaid: number;
  successRate: number;
  averageRaidTimeMinutes: number;
  totalDamage: number;
  totalHealing: number;
  averageGearScore: number;
  maxGearScore: number;
  totalSuccessfulEncounters: number;
  totalFailedEncounters: number;
  averageRaidSize: number;
}

export interface GuildBossStatisticsDto {
  encounterName: string;
  encounterEntry: string;
  totalAttempts: number;
  successfulAttempts: number;
  successRate: number;
  averageKillTimeSeconds: number;
  totalKills: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
