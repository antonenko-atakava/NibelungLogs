import { apiConfig } from "./config";
import { ApiErrorHandler } from "./errorHandler";
import type { IGuildsApi, GetGuildsParams, GetGuildMembersParams } from "@/interfaces/api/IGuildsApi";
import type { PagedResult, GuildDto, GuildDetailDto, GuildMemberDto, GuildStatisticsDto } from "@/types/api/Guild";
import type { EncounterListItemDto } from "@/types/api/Encounter";

class GuildsApi implements IGuildsApi {
  private readonly baseUrl: string;

  constructor() {
    this.baseUrl = apiConfig.baseUrl;
  }

  async getGuilds(params: GetGuildsParams): Promise<PagedResult<GuildDto>> {
    try {
      const searchParams = new URLSearchParams();
      
      if (params.search)
        searchParams.append("search", params.search);
      if (params.sortField)
        searchParams.append("sortField", params.sortField);
      if (params.sortDirection)
        searchParams.append("sortDirection", params.sortDirection);
      if (params.page)
        searchParams.append("page", params.page.toString());
      if (params.pageSize)
        searchParams.append("pageSize", params.pageSize.toString());

      const response = await fetch(`${this.baseUrl}/api/guilds?${searchParams.toString()}`);
      
      return ApiErrorHandler.handleResponse<PagedResult<GuildDto>>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getGuildById(id: number): Promise<GuildDetailDto | null> {
    try {
      const response = await fetch(`${this.baseUrl}/api/guilds/${id}`);
      
      if (response.status === 404)
        return null;
      
      return ApiErrorHandler.handleResponse<GuildDetailDto>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getGuildMembers(guildId: number, params: GetGuildMembersParams): Promise<PagedResult<GuildMemberDto>> {
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
      if (params.raidTypeId !== undefined)
        searchParams.append("raidTypeId", params.raidTypeId.toString());
      if (params.encounterName)
        searchParams.append("encounterName", params.encounterName);
      if (params.sortField)
        searchParams.append("sortField", params.sortField);
      if (params.sortDirection)
        searchParams.append("sortDirection", params.sortDirection);
      if (params.page)
        searchParams.append("page", params.page.toString());
      if (params.pageSize)
        searchParams.append("pageSize", params.pageSize.toString());

      const response = await fetch(`${this.baseUrl}/api/guilds/${guildId}/members?${searchParams.toString()}`);
      
      return ApiErrorHandler.handleResponse<PagedResult<GuildMemberDto>>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getGuildStatistics(id: number): Promise<GuildStatisticsDto> {
    try {
      const response = await fetch(`${this.baseUrl}/api/guilds/${id}/statistics`);
      return ApiErrorHandler.handleResponse<GuildStatisticsDto>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getGuildUniqueEncounters(guildId: number, raidTypeId?: number | null): Promise<EncounterListItemDto[]> {
    try {
      const searchParams = new URLSearchParams();
      if (raidTypeId !== undefined && raidTypeId !== null)
        searchParams.append("raidTypeId", raidTypeId.toString());

      const response = await fetch(`${this.baseUrl}/api/guilds/${guildId}/encounters?${searchParams.toString()}`);
      return ApiErrorHandler.handleResponse<EncounterListItemDto[]>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }
}

export const guildsApi = new GuildsApi();
