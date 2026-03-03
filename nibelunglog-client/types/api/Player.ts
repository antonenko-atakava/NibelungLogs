export interface PlayerDto {
  rank: number;
  id: number;
  characterName: string;
  characterClass: string;
  className: string | null;
  specName: string | null;
  characterRace: string;
  characterLevel: string;
  totalEncounters: number;
  totalDamage: number;
  totalHealing: number;
  averageDps: number;
  maxDps: number;
  averageHps: number | null;
  maxHps: number | null;
  encounterDate: string | null;
  encounterDuration: number | null;
  itemLevel: string | null;
  parse: number | null;
  role: string | null;
  encounterId: number | null;
}

export interface PlayerDetailDto {
  id: number;
  characterName: string;
  characterClass: string;
  characterRace: string;
  characterLevel: string;
  totalEncounters: number;
  totalDamage: number;
  totalHealing: number;
  averageDps: number;
}

export interface PlayerExtendedDetailDto {
  id: number;
  characterName: string;
  characterClass: string;
  className: string | null;
  characterRace: string;
  characterLevel: string;
  totalEncounters: number;
  successfulEncounters: number;
  failedEncounters: number;
  totalDamage: number;
  totalHealing: number;
  totalAbsorbProvided: number;
  averageDps: number;
  maxDps: number;
  minDps: number;
  averageHps: number | null;
  maxHps: number | null;
  minHps: number | null;
  bestItemLevel: string | null;
  currentItemLevel: string | null;
  firstEncounterDate: string | null;
  lastEncounterDate: string | null;
  specStatistics: PlayerSpecStatisticsDto[];
  roleStatistics: PlayerRoleStatisticsDto[];
}

export interface PlayerSpecStatisticsDto {
  specName: string;
  encountersCount: number;
  averageDps: number;
  maxDps: number;
  averageHps: number | null;
  maxHps: number | null;
}

export interface PlayerRoleStatisticsDto {
  role: string;
  encountersCount: number;
  averageDps: number;
  maxDps: number;
  averageHps: number | null;
  maxHps: number | null;
}

export interface PlayerEncounterDetailDto {
  playerEncounterId: number;
  encounterId: number;
  encounterName: string;
  encounterEntry: string;
  startTime: string;
  endTime: string;
  duration: number;
  success: boolean;
  specName: string;
  role: string;
  damageDone: number;
  healingDone: number;
  absorbProvided: number;
  dps: number;
  hps: number | null;
  itemLevel: string;
  raidId: number;
  raidName: string | null;
  raidTypeName: string | null;
  characterClass: string | null;
  className: string | null;
}

export interface PlayerEncounterTimelineDto {
  encounterId: number;
  encounterName: string;
  startTime: string;
  duration: number;
  success: boolean;
  dps: number;
  hps: number | null;
  damageDone: number;
  healingDone: number;
  specName: string;
  role: string;
  itemLevel: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
