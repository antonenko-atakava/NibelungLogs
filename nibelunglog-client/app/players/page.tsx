"use client";

import { useState, useEffect } from "react";
import { playersApi } from "@/utils/api/playersApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { PlayersTable } from "@/components/tables/players/PlayersTable";
import { PlayersPagination } from "@/components/tables/players/PlayersPagination";
import { PlayerFiltersModal } from "@/components/tables/players/PlayerFiltersModal";
import { ErrorMessage } from "@/components/ui/error-message";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { usePlayerFiltersStore } from "@/stores/playerFiltersStore";
import { getClassId, getSpecId } from "@/utils/wow/classMappings";
import { Filter, Search } from "lucide-react";
import type { PlayerDto, PagedResult } from "@/types/api/Player";

export default function PlayersPage() {
  const { filters, updateFilter } = usePlayerFiltersStore();
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [localSearch, setLocalSearch] = useState(filters.search || "");
  const [data, setData] = useState<PagedResult<PlayerDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPlayers = async () => {
      setIsLoading(true);
      setError(null);

      try {
        let result;
        if (filters.characterClass) {
          const classId = getClassId(filters.characterClass);
          if (!classId) {
            setError("Неверный класс");
            setData(null);
            setIsLoading(false);
            return;
          }
          const specId = filters.spec ? getSpecId(filters.characterClass, filters.spec) : undefined;
          result = await playersApi.getPlayersByClass({
            characterClass: classId,
            spec: specId || undefined,
            search: filters.search,
            role: filters.role,
            page: filters.page,
            pageSize: filters.pageSize,
          });
        } else {
          result = await playersApi.getPlayers({
            search: filters.search,
            role: filters.role,
            characterClass: filters.characterClass,
            spec: filters.spec,
            itemLevelMin: filters.itemLevelMin,
            itemLevelMax: filters.itemLevelMax,
            race: filters.race,
            faction: filters.faction,
            sortField: filters.sortField,
            sortDirection: filters.sortDirection,
            page: filters.page,
            pageSize: filters.pageSize,
          });
        }
        setData(result);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setData(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchPlayers();
  }, [filters]);

  const handlePageChange = (page: number) => {
    usePlayerFiltersStore.getState().updateFilter("page", page);
  };

  const handleSearchSubmit = () => {
    updateFilter("search", localSearch.trim() || undefined);
    updateFilter("page", 1);
  };

  return (
    <div className="container mx-auto py-12 px-8 max-w-7xl">
      <div className="mb-6 flex gap-3">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Поиск по имени игрока..."
            value={localSearch}
            onChange={(e) => setLocalSearch(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter")
                handleSearchSubmit();
            }}
            className="pl-10"
          />
        </div>
        <Button
          variant="outline"
          onClick={() => setIsFiltersOpen(true)}
          className="gap-2"
        >
          <Filter className="h-4 w-4" />
          Фильтр
        </Button>
      </div>

      <PlayerFiltersModal
        open={isFiltersOpen}
        onOpenChange={setIsFiltersOpen}
      />

      {error && (
        <div className="mt-4">
          <ErrorMessage
            message={error}
            onRetry={() => {
              usePlayerFiltersStore.getState().setFilters(filters);
            }}
          />
        </div>
      )}

      <div className="mt-8">
        <div className="border border-border/40 rounded-2xl overflow-hidden bg-card shadow-lg">
          <PlayersTable
            players={data?.items || []}
            isLoading={isLoading}
            onSortChange={(sortField, sortDirection) => {
              updateFilter("sortField", sortField || undefined);
              updateFilter("sortDirection", sortDirection);
              updateFilter("page", 1);
            }}
          />
          {data && (
            <PlayersPagination
              currentPage={data.page}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              pageSize={data.pageSize}
              onPageChange={handlePageChange}
            />
          )}
        </div>
      </div>
    </div>
  );
}
