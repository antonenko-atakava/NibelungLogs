import { create } from "zustand";
import type { GuildMemberFilters } from "@/types/guilds/GuildMemberFilters";

interface GuildMemberFiltersStore {
  filters: GuildMemberFilters;
  setFilters: (filters: GuildMemberFilters) => void;
  updateFilter: <K extends keyof GuildMemberFilters>(key: K, value: GuildMemberFilters[K]) => void;
  resetFilters: () => void;
}

const defaultFilters: GuildMemberFilters = {
  page: 1,
  pageSize: 25,
};

export const useGuildMemberFiltersStore = create<GuildMemberFiltersStore>((set) => ({
  filters: defaultFilters,
  setFilters: (filters) => set({ filters }),
  updateFilter: (key, value) =>
    set((state) => ({
      filters: { ...state.filters, [key]: value },
    })),
  resetFilters: () => set({ filters: defaultFilters }),
}));
