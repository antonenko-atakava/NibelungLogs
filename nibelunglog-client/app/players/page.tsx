"use client";

import { useState, useEffect } from "react";
import { playersApi } from "@/utils/api/playersApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { PlayerFiltersComponent } from "@/components/tables/players/PlayerFilters";
import { PlayersTable } from "@/components/tables/players/PlayersTable";
import { PlayersPagination } from "@/components/tables/players/PlayersPagination";
import { ErrorMessage } from "@/components/ui/error-message";
import { getClassId, getSpecId } from "@/utils/wow/classMappings";
import type { PlayerFilters } from "@/types/players/PlayerFilters";
import type { PlayerDto, PagedResult } from "@/types/api/Player";

export default function PlayersPage() {
  const [filters, setFilters] = useState<PlayerFilters>({
    page: 1,
    pageSize: 25,
  });
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

  const handleFiltersChange = (newFilters: PlayerFilters) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  return (
    <div className="container mx-auto py-12 px-8 max-w-7xl">
      <div className="mb-12">
        <h1 className="text-4xl md:text-5xl font-bold tracking-tight mb-4 bg-gradient-to-r from-[#69CCF0] via-[#64d2ff] via-[#ABD473] to-[#FFF569] bg-clip-text text-transparent">
          Игроки
        </h1>
        <p className="text-muted-foreground text-lg font-light">
          Список игроков с детальной статистикой по рейдам
        </p>
      </div>

      <PlayerFiltersComponent
        filters={filters}
        onFiltersChange={handleFiltersChange}
      />

      {error && (
        <div className="mt-4">
          <ErrorMessage
            message={error}
            onRetry={() => {
              setFilters((prev) => ({ ...prev }));
            }}
          />
        </div>
      )}

      <div className="mt-8">
        <PlayersTable
          players={data?.items || []}
          isLoading={isLoading}
        />
      </div>

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
  );
}
