"use client";

import { useState, useEffect } from "react";
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { GuildsTable } from "@/components/tables/guilds/GuildsTable";
import { GuildsPagination } from "@/components/tables/guilds/GuildsPagination";
import { ErrorMessage } from "@/components/ui/error-message";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useGuildFiltersStore } from "@/stores/guildFiltersStore";
import { Search } from "lucide-react";
import type { GuildDto, PagedResult } from "@/types/api/Guild";

export default function GuildsPage() {
  const { filters, updateFilter } = useGuildFiltersStore();
  const [localSearch, setLocalSearch] = useState(filters.search || "");
  const [data, setData] = useState<PagedResult<GuildDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchGuilds = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await guildsApi.getGuilds({
          search: filters.search,
          sortField: filters.sortField,
          sortDirection: filters.sortDirection,
          page: filters.page,
          pageSize: filters.pageSize,
        });
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

    fetchGuilds();
  }, [filters]);

  const handlePageChange = (page: number) => {
    useGuildFiltersStore.getState().updateFilter("page", page);
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
            placeholder="Поиск по названию гильдии..."
            value={localSearch}
            onChange={(e) => setLocalSearch(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter")
                handleSearchSubmit();
            }}
            className="pl-10"
          />
        </div>
      </div>

      {error && (
        <div className="mt-4">
          <ErrorMessage
            message={error}
            onRetry={() => {
              useGuildFiltersStore.getState().setFilters(filters);
            }}
          />
        </div>
      )}

      <div className="mt-8">
        <div className="border border-border/40 rounded-2xl overflow-hidden bg-card shadow-lg">
          <GuildsTable
            guilds={data?.items || []}
            isLoading={isLoading}
            onSortChange={(sortField, sortDirection) => {
              updateFilter("sortField", sortField || undefined);
              updateFilter("sortDirection", sortDirection);
              updateFilter("page", 1);
            }}
          />
          {data && (
            <GuildsPagination
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
