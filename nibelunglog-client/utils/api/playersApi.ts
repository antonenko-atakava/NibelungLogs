import { apiConfig } from "./config";
import { ApiErrorHandler } from "./errorHandler";
import type { IPlayersApi, GetPlayersParams, GetPlayersByClassParams, GetPlayersByEncounterParams, GetPlayerEncountersParams } from "@/interfaces/api/IPlayersApi";
import type { PagedResult, PlayerDto, PlayerDetailDto, PlayerExtendedDetailDto, PlayerEncounterDetailDto, PlayerEncounterTimelineDto, PlayerSpecComparisonDto } from "@/types/api/Player";

class PlayersApi implements IPlayersApi {
  private readonly baseUrl: string;

  constructor() {
    this.baseUrl = apiConfig.baseUrl;
  }

  async getPlayers(params: GetPlayersParams): Promise<PagedResult<PlayerDto>> {
    try {
      const searchParams = new URLSearchParams();
      
      if (params.search)
        searchParams.append("search", params.search);
      if (params.role)
        searchParams.append("role", params.role);
      if (params.characterClass)
        searchParams.append("characterClass", params.characterClass);
      if (params.spec)
        searchParams.append("spec", params.spec);
      if (params.itemLevelMin !== undefined)
        searchParams.append("itemLevelMin", params.itemLevelMin.toString());
      if (params.itemLevelMax !== undefined)
        searchParams.append("itemLevelMax", params.itemLevelMax.toString());
      if (params.race)
        searchParams.append("race", params.race);
      if (params.faction)
        searchParams.append("faction", params.faction);
      if (params.sortField)
        searchParams.append("sortField", params.sortField);
      if (params.sortDirection)
        searchParams.append("sortDirection", params.sortDirection);
      if (params.page)
        searchParams.append("page", params.page.toString());
      if (params.pageSize)
        searchParams.append("pageSize", params.pageSize.toString());

      const response = await fetch(`${this.baseUrl}/api/players?${searchParams.toString()}`);
      
      return ApiErrorHandler.handleResponse<PagedResult<PlayerDto>>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getPlayerById(id: number): Promise<PlayerDetailDto | null> {
    const response = await fetch(`${this.baseUrl}/api/players/${id}`);
    
    if (response.status === 404)
      return null;
    
    return ApiErrorHandler.handleResponse<PlayerDetailDto>(response);
  }

  async getPlayersByClass(params: GetPlayersByClassParams): Promise<PagedResult<PlayerDto>> {
    const searchParams = new URLSearchParams();
    
    searchParams.append("characterClass", params.characterClass);
    if (params.spec)
      searchParams.append("spec", params.spec);
    if (params.encounterEntry)
      searchParams.append("encounterEntry", params.encounterEntry);
    if (params.encounterName)
      searchParams.append("encounterName", params.encounterName);
    if (params.role)
      searchParams.append("role", params.role);
    if (params.search)
      searchParams.append("search", params.search);
    if (params.page)
      searchParams.append("page", params.page.toString());
    if (params.pageSize)
      searchParams.append("pageSize", params.pageSize.toString());

    const response = await fetch(`${this.baseUrl}/api/players/by-class?${searchParams.toString()}`);
    
    return ApiErrorHandler.handleResponse<PagedResult<PlayerDto>>(response);
  }

  async getPlayersByEncounter(params: GetPlayersByEncounterParams): Promise<PagedResult<PlayerDto>> {
    const searchParams = new URLSearchParams();
    
    if (params.encounterName)
      searchParams.append("encounterName", params.encounterName);
    if (params.encounterEntry)
      searchParams.append("encounterEntry", params.encounterEntry);
    if (params.search)
      searchParams.append("search", params.search);
    if (params.characterClass)
      searchParams.append("characterClass", params.characterClass);
    if (params.role)
      searchParams.append("role", params.role);
    if (params.page)
      searchParams.append("page", params.page.toString());
    if (params.pageSize)
      searchParams.append("pageSize", params.pageSize.toString());

    const response = await fetch(`${this.baseUrl}/api/players/by-encounter?${searchParams.toString()}`);
    
    return ApiErrorHandler.handleResponse<PagedResult<PlayerDto>>(response);
  }

  async getPlayerExtended(id: number): Promise<PlayerExtendedDetailDto | null> {
    try {
      const response = await fetch(`${this.baseUrl}/api/players/${id}/extended`);
      
      if (response.status === 404)
        return null;
      
      return ApiErrorHandler.handleResponse<PlayerExtendedDetailDto>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getPlayerEncounters(params: GetPlayerEncountersParams): Promise<PagedResult<PlayerEncounterDetailDto>> {
    try {
      const searchParams = new URLSearchParams();
      
      searchParams.append("page", (params.page || 1).toString());
      searchParams.append("pageSize", (params.pageSize || 25).toString());
      
      if (params.encounterName)
        searchParams.append("encounterName", params.encounterName);
      if (params.specName)
        searchParams.append("specName", params.specName);
      if (params.role)
        searchParams.append("role", params.role);
      if (params.success !== undefined)
        searchParams.append("success", params.success.toString());

      const response = await fetch(`${this.baseUrl}/api/players/${params.playerId}/encounters?${searchParams.toString()}`);
      
      return ApiErrorHandler.handleResponse<PagedResult<PlayerEncounterDetailDto>>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getPlayerEncounterTimeline(playerId: number, encounterEntry: string): Promise<PlayerEncounterTimelineDto[]> {
    try {
      const encodedEntry = encodeURIComponent(encounterEntry);
      const response = await fetch(`${this.baseUrl}/api/players/${playerId}/encounters/${encodedEntry}/timeline`);
      
      return ApiErrorHandler.handleResponse<PlayerEncounterTimelineDto[]>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getPlayerUniqueEncounters(playerId: number): Promise<EncounterListItemDto[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/players/${playerId}/encounters/unique`);
      
      return ApiErrorHandler.handleResponse<EncounterListItemDto[]>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getPlayerSpecComparison(playerId: number, specName: string, useAverageDps: boolean, topCount: number = 20, raidTypeId?: number | null): Promise<PlayerSpecComparisonDto | null> {
    try {
      const searchParams = new URLSearchParams();
      searchParams.append("specName", specName);
      searchParams.append("useAverageDps", useAverageDps.toString());
      searchParams.append("topCount", topCount.toString());
      if (raidTypeId !== undefined && raidTypeId !== null)
        searchParams.append("raidTypeId", raidTypeId.toString());

      const response = await fetch(`${this.baseUrl}/api/players/${playerId}/spec-comparison?${searchParams.toString()}`);
      
      if (response.status === 404)
        return null;
      
      return ApiErrorHandler.handleResponse<PlayerSpecComparisonDto>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }
}

export const playersApi = new PlayersApi();
