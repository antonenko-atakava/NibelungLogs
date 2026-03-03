import { create } from "zustand";
import type { PlayerFilters } from "@/types/players/PlayerFilters";

interface PlayerFiltersStore {
  filters: PlayerFilters;
  setFilters: (filters: PlayerFilters) => void;
  updateFilter: <K extends keyof PlayerFilters>(key: K, value: PlayerFilters[K]) => void;
  resetFilters: () => void;
}

const defaultFilters: PlayerFilters = {
  page: 1,
  pageSize: 25,
};

export const usePlayerFiltersStore = create<PlayerFiltersStore>((set) => ({
  filters: defaultFilters,
  setFilters: (filters) => set({ filters }),
  updateFilter: (key, value) =>
    set((state) => ({
      filters: { ...state.filters, [key]: value },
    })),
  resetFilters: () => set({ filters: defaultFilters }),
}));
