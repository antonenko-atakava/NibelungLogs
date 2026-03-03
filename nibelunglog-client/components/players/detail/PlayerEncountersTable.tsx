"use client";

import { useState, useEffect } from "react";
import { playersApi } from "@/utils/api/playersApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { RoleBadge } from "@/components/wow/RoleBadge";
import { getSpecIcon } from "@/utils/wow/specIcons";
import Image from "next/image";
import Link from "next/link";
import { CheckCircle2, XCircle, Clock } from "lucide-react";
import type { PlayerEncounterDetailDto, PagedResult } from "@/types/api/Player";

interface PlayerEncountersTableProps {
  playerId: number;
}

export function PlayerEncountersTable({ playerId }: PlayerEncountersTableProps) {
  const [filters, setFilters] = useState({
    encounterName: "",
    specName: "",
    role: "",
    success: undefined as boolean | undefined,
    page: 1,
    pageSize: 25,
  });
  const [data, setData] = useState<PagedResult<PlayerEncounterDetailDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchEncounters = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await playersApi.getPlayerEncounters({
          playerId,
          encounterName: filters.encounterName || undefined,
          specName: filters.specName || undefined,
          role: filters.role || undefined,
          success: filters.success,
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

    fetchEncounters();
  }, [playerId, filters]);

  const formatNumber = (value: number): string => {
    return Math.round(value).toLocaleString("ru-RU");
  };

  const formatDuration = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

  const handleFilterChange = (key: string, value: string | boolean | undefined) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
      page: 1,
    }));
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  return (
    <Card className="border-border/40 bg-card shadow-lg">
      <CardHeader>
        <CardTitle className="text-lg font-semibold">История энкаунтеров</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-3">
            <Input
              placeholder="Поиск по названию энкаунтера..."
              value={filters.encounterName}
              onChange={(e) => handleFilterChange("encounterName", e.target.value)}
              className="flex-1"
            />
            <Select
              value={filters.role || "all"}
              onValueChange={(value) => handleFilterChange("role", value === "all" ? undefined : value)}
            >
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Роль" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все роли</SelectItem>
                <SelectItem value="0">ДД</SelectItem>
                <SelectItem value="1">Танк</SelectItem>
                <SelectItem value="2">Хил</SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={filters.success === undefined ? "all" : filters.success ? "success" : "failed"}
              onValueChange={(value) => {
                if (value === "all")
                  handleFilterChange("success", undefined);
                else
                  handleFilterChange("success", value === "success");
              }}
            >
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Результат" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все</SelectItem>
                <SelectItem value="success">Успешные</SelectItem>
                <SelectItem value="failed">Провалы</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {isLoading ? (
            <div className="flex items-center justify-center p-8">
              <div className="text-muted-foreground">Загрузка...</div>
            </div>
          ) : error ? (
            <div className="text-destructive text-center p-4">{error}</div>
          ) : !data || data.items.length === 0 ? (
            <div className="text-center p-8 text-muted-foreground">
              Энкаунтеры не найдены
            </div>
          ) : (
            <>
              <div className="border border-border/40 rounded-2xl overflow-hidden">
                <Table>
                  <TableHeader>
                    <TableRow className="bg-secondary/30 border-b border-border/30">
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Дата</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Энкаунтер</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Специализация</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Роль</TableHead>
                      <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">DPS</TableHead>
                      <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">HPS</TableHead>
                      <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Длительность</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Результат</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {data.items.map((encounter) => {
                      const specIcon = getSpecIcon(encounter.className || encounter.characterClass || null, encounter.specName);
                      return (
                        <TableRow
                          key={encounter.playerEncounterId}
                          className="hover:bg-secondary/20 transition-colors border-b border-border/20"
                        >
                          <TableCell className="text-sm">
                            {new Date(encounter.startTime).toLocaleDateString("ru-RU", {
                              day: "2-digit",
                              month: "2-digit",
                              year: "numeric",
                            })}
                          </TableCell>
                          <TableCell>
                            <Link
                              href={`/encounters/${encounter.encounterId}`}
                              className="font-medium hover:text-primary transition-colors"
                            >
                              {encounter.encounterName}
                            </Link>
                          </TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              {specIcon && (
                                <Image
                                  src={specIcon}
                                  alt={encounter.specName}
                                  width={20}
                                  height={20}
                                  className="rounded-full"
                                />
                              )}
                              <span className="text-sm">{encounter.specName}</span>
                            </div>
                          </TableCell>
                          <TableCell>
                            <RoleBadge role={encounter.role} />
                          </TableCell>
                          <TableCell className="text-right font-medium text-sm">
                            {formatNumber(encounter.dps)}
                          </TableCell>
                          <TableCell className="text-right text-sm text-muted-foreground">
                            {encounter.hps ? formatNumber(encounter.hps) : "-"}
                          </TableCell>
                          <TableCell className="text-right text-sm text-muted-foreground">
                            <div className="flex items-center justify-end gap-1">
                              <Clock className="size-3" />
                              {formatDuration(encounter.duration)}
                            </div>
                          </TableCell>
                          <TableCell>
                            {encounter.success ? (
                              <CheckCircle2 className="size-5 text-green-500" />
                            ) : (
                              <XCircle className="size-5 text-red-500" />
                            )}
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </div>

              {data.totalPages > 1 && (
                <div className="flex items-center justify-between">
                  <div className="text-sm text-muted-foreground">
                    Показано {((data.page - 1) * data.pageSize) + 1}-{Math.min(data.page * data.pageSize, data.totalCount)} из {data.totalCount}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(data.page - 1)}
                      disabled={data.page === 1}
                    >
                      Назад
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(data.page + 1)}
                      disabled={data.page >= data.totalPages}
                    >
                      Вперед
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
