import { EncounterDto } from './EncounterDto';

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

