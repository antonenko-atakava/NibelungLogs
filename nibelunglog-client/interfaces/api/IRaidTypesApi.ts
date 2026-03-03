import type { RaidTypeDto } from "@/types/api/RaidType";

export interface IRaidTypesApi {
  getRaidTypes(): Promise<RaidTypeDto[]>;
}
