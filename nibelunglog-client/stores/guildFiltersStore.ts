import { create } from "zustand";
import type { GuildFilters } from "@/types/guilds/GuildFilters";

interface GuildFiltersStore {
  filters: GuildFilters;
  setFilters: (filters: GuildFilters) => void;
  updateFilter: <K extends keyof GuildFilters>(key: K, value: GuildFilters[K]) => void;
  resetFilters: () => void;
}

const defaultFilters: GuildFilters = {
  page: 1,
  pageSize: 25,
  sortField: "rating",
  sortDirection: "desc",
};

export const useGuildFiltersStore = create<GuildFiltersStore>((set) => ({
  filters: defaultFilters,
  setFilters: (filters) => set({ filters }),
  updateFilter: (key, value) =>
    set((state) => ({
      filters: { ...state.filters, [key]: value },
    })),
  resetFilters: () => set({ filters: defaultFilters }),
}));
