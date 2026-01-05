export interface PlayerDto {
  rank: number;
  id: number;
  characterName: string;
  characterClass: string;
  className?: string;
  specName?: string;
  characterRace: string;
  characterLevel: string;
  totalEncounters: number;
  totalDamage: number;
  totalHealing: number;
  averageDps: number;
  maxDps: number;
  maxHps?: number;
  encounterDate?: string;
  encounterDuration?: number;
  itemLevel?: string;
  parse?: number;
  role?: string;
  encounterId?: number;
}

