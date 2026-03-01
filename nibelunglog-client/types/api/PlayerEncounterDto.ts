export interface PlayerEncounterDto {
  id: number;
  playerName: string;
  characterClass: string;
  characterSpec: string;
  role: string;
  damageDone: number;
  healingDone: number;
  absorbProvided: number;
  dps: number;
  maxAverageGearScore: string;
  maxGearScore: string;
}

