import { apiConfig } from "./config";
import { ApiErrorHandler } from "./errorHandler";
import type { IRaidsApi, GetRaidsParams } from "@/interfaces/api/IRaidsApi";
import type { PagedResult, RaidDto, RaidDetailDto } from "@/types/api/Raid";

class RaidsApi implements IRaidsApi {
  private readonly baseUrl: string;

  constructor() {
    this.baseUrl = apiConfig.baseUrl;
  }

  async getRaids(params: GetRaidsParams): Promise<PagedResult<RaidDto>> {
    try {
      const searchParams = new URLSearchParams();
      
      if (params.raidTypeId)
        searchParams.append("raidTypeId", params.raidTypeId.toString());
      if (params.raidTypeName)
        searchParams.append("raidTypeName", params.raidTypeName);
      if (params.guildName)
        searchParams.append("guildName", params.guildName);
      if (params.leaderName)
        searchParams.append("leaderName", params.leaderName);
      if (params.guild)
        searchParams.append("guild", params.guild);
      if (params.leader)
        searchParams.append("leader", params.leader);
      if (params.page)
        searchParams.append("page", params.page.toString());
      if (params.pageSize)
        searchParams.append("pageSize", params.pageSize.toString());

      const response = await fetch(`${this.baseUrl}/api/raids?${searchParams.toString()}`);
      
      return ApiErrorHandler.handleResponse<PagedResult<RaidDto>>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getRaidById(id: number): Promise<RaidDetailDto | null> {
    const response = await fetch(`${this.baseUrl}/api/raids/${id}`);
    
    if (response.status === 404)
      return null;
    
    return ApiErrorHandler.handleResponse<RaidDetailDto>(response);
  }
}

export const raidsApi = new RaidsApi();
