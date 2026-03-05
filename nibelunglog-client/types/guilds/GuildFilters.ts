export interface GuildFilters {
  search?: string;
  sortField?: string;
  sortDirection?: "asc" | "desc";
  page: number;
  pageSize: number;
}
