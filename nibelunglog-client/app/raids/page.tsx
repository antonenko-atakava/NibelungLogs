"use client";

import { useState, useEffect } from "react";
import { raidsApi } from "@/utils/api/raidsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { RaidFiltersComponent } from "@/components/tables/raids/RaidFilters";
import { RaidsTable } from "@/components/tables/raids/RaidsTable";
import { RaidsPagination } from "@/components/tables/raids/RaidsPagination";
import { ErrorMessage } from "@/components/ui/error-message";
import type { RaidFilters } from "@/types/raids/RaidFilters";
import type { RaidDto } from "@/types/api/Raid";
import type { PagedResult } from "@/types/api/Player";

export default function RaidsPage() {
  const [filters, setFilters] = useState<RaidFilters>({
    page: 1,
    pageSize: 25,
  });
  const [data, setData] = useState<PagedResult<RaidDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchRaids = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await raidsApi.getRaids({
          raidTypeId: filters.raidTypeId,
          raidTypeName: filters.raidTypeName,
          guildName: filters.guildName,
          leaderName: filters.leaderName,
          page: filters.page,
          pageSize: filters.pageSize,
        });
        setData(result);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRaids();
  }, [filters]);

  const handleFiltersChange = (newFilters: RaidFilters) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  return (
    <div className="container mx-auto py-12 px-8 max-w-7xl">
      <div className="mb-12">
        <h1 className="text-4xl md:text-5xl font-bold tracking-tight mb-4 bg-gradient-to-r from-[#C41F3B] via-[#F58CBA] to-[#FF7D0A] bg-clip-text text-transparent">
          Рейды
        </h1>
        <p className="text-muted-foreground text-lg font-light">
          Список рейдов с детальной статистикой
        </p>
      </div>

      <RaidFiltersComponent
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
        <RaidsTable
          raids={data?.items || []}
          isLoading={isLoading}
        />
      </div>

      {data && (
        <RaidsPagination
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
