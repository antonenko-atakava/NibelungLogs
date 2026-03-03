export interface RaidDto {
  id: number;
  raidId: string;
  raidTypeName: string;
  guildName: string;
  leaderName: string;
  startTime: string;
  totalTime: number;
  totalDamage: number;
  totalHealing: number;
  wipes: number;
  completedBosses: number;
  totalBosses: number;
}

export interface RaidDetailDto {
  id: number;
  raidId: string;
  raidTypeName: string;
  guildName: string;
  leaderName: string;
  startTime: string;
  totalTime: number;
  totalDamage: number;
  totalHealing: number;
  wipes: number;
  completedBosses: number;
  totalBosses: number;
  encounters: EncounterDto[];
}

export interface EncounterDto {
  id: number;
  encounterEntry: string;
  encounterName: string;
  startTime: string;
  endTime: string;
  success: boolean;
  totalDamage: number;
  totalHealing: number;
  averageDps: number;
  attempts: number;
  wipes: number;
  tanks: number;
  healers: number;
  damageDealers: number;
}
