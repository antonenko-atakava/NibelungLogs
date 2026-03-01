import { PagedResult } from '@/types/api/PagedResult';
import { RaidDto } from '@/types/api/RaidDto';
import { RaidDetailDto } from '@/types/api/RaidDetailDto';
import { PlayerDto } from '@/types/api/PlayerDto';
import { PlayerEncounterDto } from '@/types/api/PlayerEncounterDto';
import { RaidTypeDto } from '@/types/api/RaidTypeDto';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5097';

async function fetchApi<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;
  
  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
      cache: 'no-store',
    });

    if (!response.ok)
      throw new Error(`API error: ${response.status} ${response.statusText}`);

    return response.json();
  } catch (error) {
    if (error instanceof Error)
      throw new Error(`Failed to fetch from ${url}: ${error.message}`);
    
    throw new Error(`Failed to fetch from ${url}: Unknown error`);
  }
}

export const api = {
  async getRaids(params?: {
    raidTypeId?: number;
    raidTypeName?: string;
    guildName?: string;
    leaderName?: string;
    page?: number;
    pageSize?: number;
  }): Promise<PagedResult<RaidDto>> {
    const searchParams = new URLSearchParams();

    if (params?.raidTypeId !== undefined) searchParams.append('raidTypeId', params.raidTypeId.toString());
    if (params?.raidTypeName) searchParams.append('raidTypeName', params.raidTypeName);

    if (params?.guildName) searchParams.append('guildName', params.guildName);
    if (params?.leaderName) searchParams.append('leaderName', params.leaderName);

    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());

    return fetchApi(`/api/raids?${searchParams.toString()}`);
  },

  async getRaid(id: number): Promise<RaidDetailDto> {
    return fetchApi(`/api/raids/${id}`);
  },

  async getPlayers(params?: { search?: string; role?: string; page?: number; pageSize?: number }): Promise<PagedResult<PlayerDto>> {
    const searchParams = new URLSearchParams();
    if (params?.search)
      searchParams.append('search', params.search);
    if (params?.role)
      searchParams.append('role', params.role);
    if (params?.page)
      searchParams.append('page', params.page.toString());
    if (params?.pageSize)
      searchParams.append('pageSize', params.pageSize.toString());

    return fetchApi(`/api/players?${searchParams.toString()}`);
  },

  async getPlayersByClass(params: { characterClass: string; spec?: string; role?: string; search?: string; encounterEntry?: string; encounterName?: string; page?: number; pageSize?: number }): Promise<PagedResult<PlayerDto>> {
    const searchParams = new URLSearchParams();
    searchParams.append('characterClass', params.characterClass);
    if (params.spec)
      searchParams.append('spec', params.spec);
    if (params.role)
      searchParams.append('role', params.role);
    if (params.search)
      searchParams.append('search', params.search);
    if (params.encounterEntry)
      searchParams.append('encounterEntry', params.encounterEntry);
    if (params.encounterName)
      searchParams.append('encounterName', params.encounterName);
    if (params.page)
      searchParams.append('page', params.page.toString());
    if (params.pageSize)
      searchParams.append('pageSize', params.pageSize.toString());

    return fetchApi(`/api/players/by-class?${searchParams.toString()}`);
  },

  async getRaidTypes(): Promise<RaidTypeDto[]> {
    return fetchApi('/api/raidtypes');
  },

  async getEncounterPlayers(encounterId: number): Promise<PlayerEncounterDto[]> {
    return fetchApi(`/api/encounters/${encounterId}/players`);
  },

  async getEncountersList(): Promise<Array<{ encounterEntry: string; encounterName: string | null }>> {
    return fetchApi('/api/encounters/list');
  },

  async getEncountersGroupedByRaid(): Promise<Array<{
    raidTypeName: string;
    encounters: Array<{ encounterEntry: string; encounterName: string | null }>;
  }>> {
    return fetchApi('/api/encounters/grouped-by-raid');
  },

  async getPlayersByEncounter(params: { encounterName?: string; encounterEntry?: string; search?: string; characterClass?: string; role?: string; page?: number; pageSize?: number }): Promise<PagedResult<PlayerDto>> {
    const searchParams = new URLSearchParams();
    if (params.encounterName)
      searchParams.append('encounterName', params.encounterName);
    if (params.encounterEntry)
      searchParams.append('encounterEntry', params.encounterEntry);
    if (params.search)
      searchParams.append('search', params.search);
    if (params.characterClass)
      searchParams.append('characterClass', params.characterClass);
    if (params.role)
      searchParams.append('role', params.role);
    if (params.page)
      searchParams.append('page', params.page.toString());
    if (params.pageSize)
      searchParams.append('pageSize', params.pageSize.toString());

    return fetchApi(`/api/players/by-encounter?${searchParams.toString()}`);
  },

  async getRaidByEncounterId(encounterId: number): Promise<RaidDetailDto> {
    return fetchApi(`/api/encounters/${encounterId}/raid`);
  },
};

