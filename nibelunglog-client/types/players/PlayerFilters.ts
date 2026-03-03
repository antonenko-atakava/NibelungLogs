export interface PlayerFilters {
  search?: string;
  role?: string;
  characterClass?: string;
  spec?: string;
  itemLevelMin?: number;
  itemLevelMax?: number;
  race?: string;
  faction?: string;
  sortField?: string;
  sortDirection?: "asc" | "desc";
  page: number;
  pageSize: number;
}
