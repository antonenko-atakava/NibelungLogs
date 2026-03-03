import type { PagedResult, PlayerDto, PlayerDetailDto } from "@/types/api/Player";

import type { PlayerExtendedDetailDto, PlayerEncounterDetailDto, PlayerEncounterTimelineDto } from "@/types/api/Player";
import type { EncounterListItemDto } from "@/types/api/Encounter";

export interface IPlayersApi {
  getPlayers(params: GetPlayersParams): Promise<PagedResult<PlayerDto>>;
  getPlayerById(id: number): Promise<PlayerDetailDto | null>;
  getPlayersByClass(params: GetPlayersByClassParams): Promise<PagedResult<PlayerDto>>;
  getPlayersByEncounter(params: GetPlayersByEncounterParams): Promise<PagedResult<PlayerDto>>;
  getPlayerExtended(id: number): Promise<PlayerExtendedDetailDto | null>;
  getPlayerEncounters(params: GetPlayerEncountersParams): Promise<PagedResult<PlayerEncounterDetailDto>>;
  getPlayerEncounterTimeline(playerId: number, encounterEntry: string): Promise<PlayerEncounterTimelineDto[]>;
  getPlayerUniqueEncounters(playerId: number): Promise<EncounterListItemDto[]>;
}

export interface GetPlayersParams {
  search?: string;
  role?: string;
  page?: number;
  pageSize?: number;
}

export interface GetPlayersByClassParams {
  characterClass: string;
  spec?: string;
  encounterEntry?: string;
  encounterName?: string;
  role?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface GetPlayersByEncounterParams {
  encounterName?: string;
  encounterEntry?: string;
  search?: string;
  characterClass?: string;
  role?: string;
  page?: number;
  pageSize?: number;
}

export interface GetPlayerEncountersParams {
  playerId: number;
  encounterName?: string;
  specName?: string;
  role?: string;
  success?: boolean;
  page?: number;
  pageSize?: number;
}
