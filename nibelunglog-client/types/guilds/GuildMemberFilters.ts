export interface GuildMemberFilters {
  search?: string;
  role?: string;
  characterClass?: string;
  spec?: string;
  itemLevelMin?: number;
  itemLevelMax?: number;
  sortField?: string;
  sortDirection?: "asc" | "desc";
  page: number;
  pageSize: number;
}
