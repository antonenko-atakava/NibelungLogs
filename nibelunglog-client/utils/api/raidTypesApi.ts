import { apiConfig } from "./config";
import { ApiErrorHandler } from "./errorHandler";
import type { IRaidTypesApi } from "@/interfaces/api/IRaidTypesApi";
import type { RaidTypeDto } from "@/types/api/RaidType";

class RaidTypesApi implements IRaidTypesApi {
  private readonly baseUrl: string;

  constructor() {
    this.baseUrl = apiConfig.baseUrl;
  }

  async getRaidTypes(): Promise<RaidTypeDto[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/raidtypes`);
      
      return ApiErrorHandler.handleResponse<RaidTypeDto[]>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }
}

export const raidTypesApi = new RaidTypesApi();
