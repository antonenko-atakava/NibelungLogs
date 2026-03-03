export interface ApiError {
  message: string;
  status?: number;
  details?: unknown;
}

export class ApiErrorHandler {
  static async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      let errorMessage = `Ошибка: ${response.status} ${response.statusText}`;
      let errorDetails: unknown = null;

      try {
        const contentType = response.headers.get("content-type");
        if (contentType?.includes("application/json")) {
          const errorData = await response.json();
          
          if (errorData.errors && Array.isArray(errorData.errors)) {
            errorMessage = errorData.errors
              .map((err: { propertyName?: string; errorMessage?: string }) => 
                err.errorMessage || err.propertyName || "Ошибка валидации")
              .join(", ");
            errorDetails = errorData.errors;
          } else if (errorData.message) {
            errorMessage = errorData.message;
            errorDetails = errorData;
          } else if (typeof errorData === "string") {
            errorMessage = errorData;
          }
        } else {
          const text = await response.text();
          if (text)
            errorMessage = text;
        }
      } catch {
        errorMessage = `Ошибка: ${response.status} ${response.statusText}`;
      }

      const error: ApiError = {
        message: errorMessage,
        status: response.status,
        details: errorDetails,
      };

      throw error;
    }

    try {
      return await response.json();
    } catch {
      throw {
        message: "Ошибка парсинга ответа сервера",
        status: response.status,
      } as ApiError;
    }
  }

  static getErrorMessage(error: unknown): string {
    if (error instanceof Error)
      return error.message;
    
    if (typeof error === "object" && error !== null && "message" in error)
      return String((error as ApiError).message);
    
    return "Произошла неизвестная ошибка";
  }
}
