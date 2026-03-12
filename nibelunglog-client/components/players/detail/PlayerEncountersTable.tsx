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
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { RoleBadge } from "@/components/wow/RoleBadge";
import { SpecBadge } from "@/components/wow/SpecBadge";
import { getSpecIcon } from "@/utils/wow/specIcons";
import { raidTypesApi } from "@/utils/api/raidTypesApi";
import { getEncounterName } from "@/utils/wow/encounterMappings";
import Image from "next/image";
import Link from "next/link";
import { CheckCircle2, XCircle, Clock, Filter } from "lucide-react";
import type { PlayerEncounterDetailDto, PagedResult } from "@/types/api/Player";
import type { RaidTypeDto } from "@/types/api/RaidType";
import type { EncounterListItemDto } from "@/types/api/Encounter";

interface PlayerEncountersTableProps {
  playerId: number;
}

export function PlayerEncountersTable({ playerId }: PlayerEncountersTableProps) {
  const [filters, setFilters] = useState({
    encounterEntry: "",
    specName: "",
    role: "",
    success: undefined as boolean | undefined,
    raidTypeId: null as number | null,
    startDate: null as string | null,
    endDate: null as string | null,
    page: 1,
    pageSize: 25,
  });
  const [data, setData] = useState<PagedResult<PlayerEncounterDetailDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [raidTypes, setRaidTypes] = useState<RaidTypeDto[]>([]);
  const [uniqueEncounters, setUniqueEncounters] = useState<EncounterListItemDto[]>([]);
  const [uniqueSpecs, setUniqueSpecs] = useState<string[]>([]);

  const [localSpec, setLocalSpec] = useState<string | null>(null);
  const [localRole, setLocalRole] = useState<string | null>(null);
  const [localSuccess, setLocalSuccess] = useState<boolean | undefined>(undefined);
  const [localRaidTypeId, setLocalRaidTypeId] = useState<number | null>(null);
  const [localEncounterEntry, setLocalEncounterEntry] = useState<string | null>(null);
  const [localStartDate, setLocalStartDate] = useState<string>("");
  const [localEndDate, setLocalEndDate] = useState<string>("");

  useEffect(() => {
    const fetchRaidTypes = async () => {
      try {
        const types = await raidTypesApi.getRaidTypes();
        setRaidTypes(types);
      } catch (err) {
        console.error("Failed to load raid types:", err);
      }
    };

    fetchRaidTypes();
  }, []);

  useEffect(() => {
    const fetchUniqueEncounters = async () => {
      try {
        const encounters = await playersApi.getPlayerUniqueEncounters(playerId, localRaidTypeId);
        setUniqueEncounters(encounters);
        if (localEncounterEntry && !encounters.some(e => e.encounterEntry === localEncounterEntry)) {
          setLocalEncounterEntry(null);
        }
      } catch (err) {
        console.error("Failed to load unique encounters:", err);
      }
    };

    fetchUniqueEncounters();
  }, [playerId, localRaidTypeId]);

  useEffect(() => {
    const fetchEncounters = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const startDateParam = filters.startDate ? `${filters.startDate}T00:00:00.000Z` : null;
        const endDateParam = filters.endDate ? `${filters.endDate}T23:59:59.999Z` : null;

        const result = await playersApi.getPlayerEncounters({
          playerId,
          encounterEntry: filters.encounterEntry || undefined,
          specName: filters.specName || undefined,
          role: filters.role || undefined,
          success: filters.success,
          raidTypeId: filters.raidTypeId,
          startDate: startDateParam,
          endDate: endDateParam,
          page: filters.page,
          pageSize: filters.pageSize,
        });
        setData(result);
        
        const specs = new Set<string>();
        result.items.forEach(item => {
          if (item.specName)
            specs.add(item.specName);
        });
        setUniqueSpecs(Array.from(specs).sort());
        
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

  const handleApplyFilters = () => {
    setFilters((prev) => ({
      ...prev,
      specName: localSpec || "",
      role: localRole || "",
      success: localSuccess,
      raidTypeId: localRaidTypeId,
      encounterEntry: localEncounterEntry || "",
      startDate: localStartDate || null,
      endDate: localEndDate || null,
      page: 1,
    }));
    setIsFiltersOpen(false);
  };

  const handleClearFilters = () => {
    setLocalSpec(null);
    setLocalRole(null);
    setLocalSuccess(undefined);
    setLocalRaidTypeId(null);
    setLocalEncounterName(null);
    setLocalStartDate("");
    setLocalEndDate("");
    setFilters((prev) => ({
      ...prev,
      specName: "",
      role: "",
      success: undefined,
      raidTypeId: null,
      encounterEntry: "",
      startDate: null,
      endDate: null,
      page: 1,
    }));
  };

  useEffect(() => {
    if (isFiltersOpen) {
      setLocalSpec(filters.specName || null);
      setLocalRole(filters.role || null);
      setLocalSuccess(filters.success);
      setLocalRaidTypeId(filters.raidTypeId);
      setLocalEncounterEntry(filters.encounterEntry || null);
      setLocalStartDate(filters.startDate || "");
      setLocalEndDate(filters.endDate || "");
    }
  }, [isFiltersOpen, filters]);

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  return (
    <>
      <Card className="border-border/40 bg-card shadow-lg">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg font-semibold">История энкаунтеров</CardTitle>
            <Button
              variant="outline"
              onClick={() => setIsFiltersOpen(true)}
              className="gap-2 h-11"
            >
              <Filter className="h-4 w-4" />
              Фильтр
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">

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
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Дата</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Энкаунтер</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Специализация</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Роль</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">DPS</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">HPS</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Длительность</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Результат</TableHead>
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
                          <TableCell className="text-center text-sm">
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
                              {getEncounterName(encounter.encounterEntry)}
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
                              <SpecBadge
                                specName={encounter.specName}
                                className={encounter.className || encounter.characterClass}
                                variant="text"
                              />
                            </div>
                          </TableCell>
                          <TableCell className="text-center">
                            <div className="flex justify-center">
                              <RoleBadge role={encounter.role} />
                            </div>
                          </TableCell>
                          <TableCell className="text-center font-medium text-sm">
                            {formatNumber(encounter.dps)}
                          </TableCell>
                          <TableCell className="text-center text-sm text-muted-foreground">
                            {encounter.hps ? formatNumber(encounter.hps) : "-"}
                          </TableCell>
                          <TableCell className="text-center text-sm text-muted-foreground">
                            <div className="flex items-center justify-center gap-1">
                              <Clock className="size-3" />
                              {formatDuration(encounter.duration)}
                            </div>
                          </TableCell>
                          <TableCell className="text-center">
                            <div className="flex justify-center">
                              {encounter.success ? (
                                <CheckCircle2 className="size-5 text-green-500" />
                              ) : (
                                <XCircle className="size-5 text-red-500" />
                              )}
                            </div>
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

    <Dialog open={isFiltersOpen} onOpenChange={setIsFiltersOpen}>
      <DialogContent className="sm:max-w-[600px] overflow-visible">
        <DialogHeader className="pb-2">
          <DialogTitle>Фильтры</DialogTitle>
        </DialogHeader>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Специализация</label>
            <Select value={localSpec || "all"} onValueChange={(value) => setLocalSpec(value === "all" ? null : value)}>
              <SelectTrigger className="h-10 w-full">
                <SelectValue placeholder="Все специализации" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все специализации</SelectItem>
                {uniqueSpecs.map((spec) => (
                  <SelectItem key={spec} value={spec}>
                    {spec}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Роль</label>
            <Select value={localRole || "all"} onValueChange={(value) => setLocalRole(value === "all" ? null : value)}>
              <SelectTrigger className="h-10 w-full">
                <SelectValue placeholder="Все роли" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все роли</SelectItem>
                <SelectItem value="0">ДД</SelectItem>
                <SelectItem value="1">Танк</SelectItem>
                <SelectItem value="2">Хил</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Результат</label>
            <Select value={localSuccess === undefined ? "all" : localSuccess ? "success" : "failed"} onValueChange={(value) => {
              if (value === "all")
                setLocalSuccess(undefined);
              else
                setLocalSuccess(value === "success");
            }}>
              <SelectTrigger className="h-10 w-full">
                <SelectValue placeholder="Все" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все</SelectItem>
                <SelectItem value="success">Успешные</SelectItem>
                <SelectItem value="failed">Провалы</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Рейд</label>
            <Select value={localRaidTypeId?.toString() || "all"} onValueChange={(value) => setLocalRaidTypeId(value === "all" ? null : parseInt(value, 10))}>
              <SelectTrigger className="h-10 w-full">
                <SelectValue placeholder="Все рейды" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все рейды</SelectItem>
                {raidTypes.map((raidType) => (
                  <SelectItem key={raidType.id} value={raidType.id.toString()}>
                    {raidType.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Энкаунтер</label>
            <Select value={localEncounterEntry || "all"} onValueChange={(value) => setLocalEncounterEntry(value === "all" ? null : value)}>
              <SelectTrigger className="h-10 w-full">
                <SelectValue placeholder="Все энкаунтеры" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все энкаунтеры</SelectItem>
                {uniqueEncounters.map((encounter) => (
                  <SelectItem key={encounter.encounterEntry} value={encounter.encounterEntry}>
                    {getEncounterName(encounter.encounterEntry)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Дата от</label>
            <Input
              type="date"
              value={localStartDate}
              onChange={(e) => setLocalStartDate(e.target.value)}
              className="h-10 bg-input/60"
            />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Дата до</label>
            <Input
              type="date"
              value={localEndDate}
              onChange={(e) => setLocalEndDate(e.target.value)}
              className="h-10 bg-input/60"
            />
          </div>
        </div>
        <DialogFooter className="pt-3 mt-3 border-t border-border/40 gap-2 justify-between">
          <Button variant="destructive" onClick={handleClearFilters} className="w-full sm:w-auto">
            Сбросить
          </Button>
          <Button variant="outline" onClick={handleApplyFilters} className="w-full sm:w-auto bg-white text-black hover:bg-white/90 hover:text-black border-border/60">
            Применить
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    </>
  );
}
