export interface EncounterDto {
  id: number;
  encounterEntry: string;
  encounterName: string | null;
  startTime: string;
  endTime: string;
  success: boolean;
  totalDamage: number;
  totalHealing: number;
  tanks: number;
  healers: number;
  damageDealers: number;
}

