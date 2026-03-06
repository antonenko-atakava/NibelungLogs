export interface SeasonClassStatisticsDto {
  season: number;
  className: string;
  averageDps: number;
  averageHps: number;
  totalEncounters: number;
  totalPlayers: number;
}

export interface SeasonSpecStatisticsDto {
  season: number;
  className: string;
  specName: string;
  averageDps: number;
  averageHps: number;
  totalEncounters: number;
  totalPlayers: number;
}
