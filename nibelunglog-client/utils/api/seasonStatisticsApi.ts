import { apiConfig } from "./config";
import { ApiErrorHandler } from "./errorHandler";
import type { SeasonClassStatisticsDto, SeasonSpecStatisticsDto } from "@/types/api/SeasonStatistics";

class SeasonStatisticsApi {
  private readonly baseUrl: string;

  constructor() {
    this.baseUrl = apiConfig.baseUrl;
  }

  async getSeasonClassStatistics(): Promise<SeasonClassStatisticsDto[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/seasonstatistics/classes`);
      return ApiErrorHandler.handleResponse<SeasonClassStatisticsDto[]>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }

  async getSeasonSpecStatistics(): Promise<SeasonSpecStatisticsDto[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/seasonstatistics/specs`);
      return ApiErrorHandler.handleResponse<SeasonSpecStatisticsDto[]>(response);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch"))
        throw { message: "Ошибка подключения к серверу. Проверьте, что API сервер запущен." };
      throw error;
    }
  }
}

export const seasonStatisticsApi = new SeasonStatisticsApi();
