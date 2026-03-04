"use client";

import { useState, useEffect, useMemo } from "react";
import { playersApi } from "@/utils/api/playersApi";
import { raidTypesApi } from "@/utils/api/raidTypesApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Filter } from "lucide-react";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from "chart.js";
import { Line } from "react-chartjs-2";
import { TrendingUp } from "lucide-react";
import type { PlayerEncounterDetailDto, PlayerSpecStatisticsDto } from "@/types/api/Player";
import type { RaidTypeDto } from "@/types/api/RaidType";
import type { EncounterListItemDto } from "@/types/api/Encounter";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

interface PlayerEncounterChartProps {
  playerId: number;
  specStatistics?: PlayerSpecStatisticsDto[];
}

export function PlayerEncounterChart({ playerId, specStatistics = [] }: PlayerEncounterChartProps) {
  const [encounters, setEncounters] = useState<PlayerEncounterDetailDto[]>([]);
  const [comparisonEncounters, setComparisonEncounters] = useState<PlayerEncounterDetailDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingComparison, setIsLoadingComparison] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [raidTypes, setRaidTypes] = useState<RaidTypeDto[]>([]);
  const [uniqueEncounters, setUniqueEncounters] = useState<EncounterListItemDto[]>([]);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  
  const [selectedSpec, setSelectedSpec] = useState<string | null>(null);
  const [selectedRaidTypeId, setSelectedRaidTypeId] = useState<number | null>(null);
  const [selectedEncounterName, setSelectedEncounterName] = useState<string | null>(null);
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");
  const [comparisonPlayerId, setComparisonPlayerId] = useState<number | null>(null);
  const [comparisonPlayerName, setComparisonPlayerName] = useState<string>("");

  const [localSpec, setLocalSpec] = useState<string | null>(null);
  const [localRaidTypeId, setLocalRaidTypeId] = useState<number | null>(null);
  const [localEncounterName, setLocalEncounterName] = useState<string | null>(null);
  const [localStartDate, setLocalStartDate] = useState<string>("");
  const [localEndDate, setLocalEndDate] = useState<string>("");
  const [localComparisonPlayerName, setLocalComparisonPlayerName] = useState<string>("");
  const [playerSearchResults, setPlayerSearchResults] = useState<Array<{ id: number; name: string }>>([]);
  const [isSearchingPlayers, setIsSearchingPlayers] = useState(false);
  const [viewMode, setViewMode] = useState<"date" | "encounter">("encounter");

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
  }, [playerId]);

  useEffect(() => {
    const fetchUniqueEncounters = async () => {
      try {
        const encounters = await playersApi.getPlayerUniqueEncounters(playerId, localRaidTypeId);
        setUniqueEncounters(encounters);
        if (localEncounterName && !encounters.some(e => e.encounterName === localEncounterName)) {
          setLocalEncounterName(null);
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
        const allEncounters: PlayerEncounterDetailDto[] = [];
        let page = 1;
        const pageSize = 100;
        let hasMore = true;

        while (hasMore) {
          const startDateParam = startDate ? `${startDate}T00:00:00.000Z` : null;
          const endDateParam = endDate ? `${endDate}T23:59:59.999Z` : null;

          const result = await playersApi.getPlayerEncounters({
            playerId,
            specName: selectedSpec || undefined,
            raidTypeId: selectedRaidTypeId,
            encounterName: selectedEncounterName || undefined,
            startDate: startDateParam,
            endDate: endDateParam,
            page,
            pageSize,
          });

          allEncounters.push(...result.items);

          if (result.items.length < pageSize || page * pageSize >= result.totalCount) {
            hasMore = false;
          } else {
            page++;
          }
        }

        const sortedEncounters = allEncounters.sort((a, b) => 
          new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
        );

        setEncounters(sortedEncounters);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setEncounters([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchEncounters();
  }, [playerId, selectedSpec, selectedRaidTypeId, selectedEncounterName, startDate, endDate]);

  const handleSearchPlayers = async (searchTerm: string) => {
    if (searchTerm.length < 2) {
      setPlayerSearchResults([]);
      return;
    }

    setIsSearchingPlayers(true);
    try {
      const result = await playersApi.getPlayers({
        search: searchTerm,
        page: 1,
        pageSize: 10,
      });
      setPlayerSearchResults(result.items.map(p => ({ id: p.id, name: p.characterName })));
    } catch (err) {
      console.error("Failed to search players:", err);
      setPlayerSearchResults([]);
    } finally {
      setIsSearchingPlayers(false);
    }
  };

  const handleApplyFilters = async () => {
    setSelectedSpec(localSpec);
    setSelectedRaidTypeId(localRaidTypeId);
    setSelectedEncounterName(localEncounterName);
    setStartDate(localStartDate);
    setEndDate(localEndDate);
    
    if (localComparisonPlayerName) {
      let selectedPlayer = playerSearchResults.find(p => p.name === localComparisonPlayerName);
      
      if (!selectedPlayer) {
        try {
          const searchResult = await playersApi.getPlayers({
            search: localComparisonPlayerName,
            page: 1,
            pageSize: 1,
          });
          
          if (searchResult.items.length > 0 && searchResult.items[0].characterName === localComparisonPlayerName) {
            selectedPlayer = { id: searchResult.items[0].id, name: searchResult.items[0].characterName };
          }
        } catch (err) {
          console.error("Failed to find player:", err);
        }
      }
      
      if (selectedPlayer) {
        setComparisonPlayerId(selectedPlayer.id);
        setComparisonPlayerName(selectedPlayer.name);
      } else {
        setComparisonPlayerId(null);
        setComparisonPlayerName("");
        setComparisonEncounters([]);
      }
    } else {
      setComparisonPlayerId(null);
      setComparisonPlayerName("");
      setComparisonEncounters([]);
    }
    
    setIsFiltersOpen(false);
  };

  const handleClearFilters = () => {
    setLocalSpec(null);
    setLocalRaidTypeId(null);
    setLocalEncounterName(null);
    setLocalStartDate("");
    setLocalEndDate("");
    setLocalComparisonPlayerName("");
    setSelectedSpec(null);
    setSelectedRaidTypeId(null);
    setSelectedEncounterName(null);
    setStartDate("");
    setEndDate("");
    setComparisonPlayerId(null);
    setComparisonPlayerName("");
    setComparisonEncounters([]);
  };

  useEffect(() => {
    if (!comparisonPlayerId)
      return;

    const fetchComparisonEncounters = async () => {
      setIsLoadingComparison(true);

      try {
        const allEncounters: PlayerEncounterDetailDto[] = [];
        let page = 1;
        const pageSize = 100;
        let hasMore = true;

        while (hasMore) {
          const startDateParam = startDate ? `${startDate}T00:00:00.000Z` : null;
          const endDateParam = endDate ? `${endDate}T23:59:59.999Z` : null;

          const result = await playersApi.getPlayerEncounters({
            playerId: comparisonPlayerId,
            specName: selectedSpec || undefined,
            raidTypeId: selectedRaidTypeId,
            encounterName: selectedEncounterName || undefined,
            startDate: startDateParam,
            endDate: endDateParam,
            page,
            pageSize,
          });

          allEncounters.push(...result.items);

          if (result.items.length < pageSize || page * pageSize >= result.totalCount) {
            hasMore = false;
          } else {
            page++;
          }
        }

        const sortedEncounters = allEncounters.sort((a, b) => 
          new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
        );

        setComparisonEncounters(sortedEncounters);
      } catch (err) {
        console.error("Failed to load comparison encounters:", err);
        setComparisonEncounters([]);
      } finally {
        setIsLoadingComparison(false);
      }
    };

    fetchComparisonEncounters();
  }, [comparisonPlayerId, selectedSpec, selectedRaidTypeId, selectedEncounterName, startDate, endDate]);

  useEffect(() => {
    if (isFiltersOpen) {
      setLocalSpec(selectedSpec);
      setLocalRaidTypeId(selectedRaidTypeId);
      setLocalEncounterName(selectedEncounterName);
      setLocalStartDate(startDate);
      setLocalEndDate(endDate);
      setLocalComparisonPlayerName(comparisonPlayerName);
    }
  }, [isFiltersOpen, selectedSpec, selectedRaidTypeId, selectedEncounterName, startDate, endDate, comparisonPlayerName]);

  useEffect(() => {
    if (localRaidTypeId && localEncounterName) {
      const isEncounterInRaid = uniqueEncounters.some(e => e.encounterName === localEncounterName);
      if (!isEncounterInRaid) {
        setLocalEncounterName(null);
      }
    }
  }, [uniqueEncounters, localRaidTypeId, localEncounterName]);

  const formatNumber = (value: number): string => {
    return Math.round(value).toLocaleString("ru-RU");
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
    });
  };

  const selectedSpecData = specStatistics.find(s => s.specName === selectedSpec);
  const isHealer = selectedSpecData 
    ? (selectedSpecData.averageHps && selectedSpecData.averageHps > 0 || selectedSpecData.maxHps && selectedSpecData.maxHps > 0)
    : encounters.some(e => e.role === "2" && e.hps !== null && e.hps > 0);

  const chartData = useMemo(() => {
    if (encounters.length === 0 && comparisonEncounters.length === 0)
      return null;

    if (viewMode === "encounter") {
      const allTimeKeys = new Set<string>();
      encounters.forEach(e => allTimeKeys.add(e.startTime));
      if (comparisonPlayerId) {
        comparisonEncounters.forEach(e => allTimeKeys.add(e.startTime));
      }

      const sortedTimeKeys = Array.from(allTimeKeys).sort((a, b) => 
        new Date(a).getTime() - new Date(b).getTime()
      );

      const encountersByTime = new Map<string, PlayerEncounterDetailDto>();
      encounters.forEach(e => {
        encountersByTime.set(e.startTime, e);
      });

      const comparisonEncountersByTime = new Map<string, PlayerEncounterDetailDto>();
      comparisonEncounters.forEach(e => {
        comparisonEncountersByTime.set(e.startTime, e);
      });

      const labels = sortedTimeKeys.map(timeKey => {
        const encounter = encountersByTime.get(timeKey) || comparisonEncountersByTime.get(timeKey);
        if (!encounter)
          return "";
        const name = encounter.encounterName;
        return name.length > 25 ? name.substring(0, 25) + "..." : name;
      });

      const dpsData = sortedTimeKeys.map(timeKey => {
        const encounter = encountersByTime.get(timeKey);
        return encounter ? Math.round(encounter.dps) : null;
      });

      const hpsData = isHealer ? sortedTimeKeys.map(timeKey => {
        const encounter = encountersByTime.get(timeKey);
        return encounter && encounter.hps ? Math.round(encounter.hps) : null;
      }) : null;

      const comparisonDpsData = comparisonPlayerId ? sortedTimeKeys.map(timeKey => {
        const encounter = comparisonEncountersByTime.get(timeKey);
        return encounter ? Math.round(encounter.dps) : null;
      }) : null;

      const comparisonHpsData = isHealer && comparisonPlayerId ? sortedTimeKeys.map(timeKey => {
        const encounter = comparisonEncountersByTime.get(timeKey);
        return encounter && encounter.hps ? Math.round(encounter.hps) : null;
      }) : null;

      return {
        labels,
        datasets: [
          ...(isHealer && hpsData ? [{
            label: "HPS",
            data: hpsData,
            borderColor: "rgba(34, 197, 94, 1)",
            backgroundColor: "rgba(34, 197, 94, 0.05)",
            fill: true,
            tension: 0.5,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: "rgba(34, 197, 94, 1)",
            pointBorderColor: "rgba(255, 255, 255, 1)",
            pointBorderWidth: 2,
            borderWidth: 3,
            spanGaps: false,
          }] : []),
          {
            label: "DPS",
            data: dpsData,
            borderColor: "rgba(100, 210, 255, 1)",
            backgroundColor: "rgba(100, 210, 255, 0.05)",
            fill: true,
            tension: 0.5,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: "rgba(100, 210, 255, 1)",
            pointBorderColor: "rgba(255, 255, 255, 1)",
            pointBorderWidth: 2,
            borderWidth: 3,
            spanGaps: false,
          },
          ...(comparisonPlayerId ? [
            ...(isHealer && comparisonHpsData ? [{
              label: `HPS (${comparisonPlayerName})`,
              data: comparisonHpsData,
              borderColor: "rgba(251, 191, 36, 1)",
              backgroundColor: "rgba(251, 191, 36, 0.05)",
              fill: false,
              tension: 0.5,
              pointRadius: 4,
              pointHoverRadius: 6,
              pointBackgroundColor: "rgba(251, 191, 36, 1)",
              pointBorderColor: "rgba(255, 255, 255, 1)",
              pointBorderWidth: 2,
              borderWidth: 3,
              borderDash: [8, 4],
              spanGaps: false,
            }] : []),
            {
              label: `DPS (${comparisonPlayerName})`,
              data: comparisonDpsData || [],
              borderColor: "rgba(251, 191, 36, 1)",
              backgroundColor: "rgba(251, 191, 36, 0.05)",
              fill: false,
              tension: 0.5,
              pointRadius: 4,
              pointHoverRadius: 6,
              pointBackgroundColor: "rgba(251, 191, 36, 1)",
              pointBorderColor: "rgba(255, 255, 255, 1)",
              pointBorderWidth: 2,
              borderWidth: 3,
              borderDash: [8, 4],
              spanGaps: false,
            },
          ] : []),
        ],
      };
    }

    const allDates = new Set<string>();
    encounters.forEach(e => allDates.add(formatDate(e.startTime)));
    comparisonEncounters.forEach(e => allDates.add(formatDate(e.startTime)));
    const sortedDates = Array.from(allDates).sort((a, b) => {
      const dateA = new Date(a.split(".").reverse().join("-"));
      const dateB = new Date(b.split(".").reverse().join("-"));
      return dateA.getTime() - dateB.getTime();
    });

    const encountersByDate = new Map<string, PlayerEncounterDetailDto>();
    encounters.forEach(e => {
      const dateKey = formatDate(e.startTime);
      if (!encountersByDate.has(dateKey) || new Date(e.startTime) > new Date(encountersByDate.get(dateKey)!.startTime)) {
        encountersByDate.set(dateKey, e);
      }
    });

    const comparisonEncountersByDate = new Map<string, PlayerEncounterDetailDto>();
    comparisonEncounters.forEach(e => {
      const dateKey = formatDate(e.startTime);
      if (!comparisonEncountersByDate.has(dateKey) || new Date(e.startTime) > new Date(comparisonEncountersByDate.get(dateKey)!.startTime)) {
        comparisonEncountersByDate.set(dateKey, e);
      }
    });

    const dpsData = sortedDates.map(date => {
      const encounter = encountersByDate.get(date);
      return encounter ? Math.round(encounter.dps) : null;
    });

    const hpsData = isHealer ? sortedDates.map(date => {
      const encounter = encountersByDate.get(date);
      return encounter && encounter.hps ? Math.round(encounter.hps) : null;
    }) : null;

    const comparisonDpsData = comparisonPlayerId ? sortedDates.map(date => {
      const encounter = comparisonEncountersByDate.get(date);
      return encounter ? Math.round(encounter.dps) : null;
    }) : null;

    const comparisonHpsData = isHealer && comparisonPlayerId ? sortedDates.map(date => {
      const encounter = comparisonEncountersByDate.get(date);
      return encounter && encounter.hps ? Math.round(encounter.hps) : null;
    }) : null;

    return {
      labels: sortedDates,
      datasets: [
        ...(isHealer && hpsData ? [{
          label: "HPS",
          data: hpsData,
          borderColor: "rgba(34, 197, 94, 1)",
          backgroundColor: "rgba(34, 197, 94, 0.05)",
          fill: true,
          tension: 0.5,
          pointRadius: 4,
          pointHoverRadius: 6,
          pointBackgroundColor: "rgba(34, 197, 94, 1)",
          pointBorderColor: "rgba(255, 255, 255, 1)",
          pointBorderWidth: 2,
          borderWidth: 3,
          spanGaps: false,
        }] : []),
        {
          label: "DPS",
          data: dpsData,
          borderColor: "rgba(100, 210, 255, 1)",
          backgroundColor: "rgba(100, 210, 255, 0.05)",
          fill: true,
          tension: 0.5,
          pointRadius: 4,
          pointHoverRadius: 6,
          pointBackgroundColor: "rgba(100, 210, 255, 1)",
          pointBorderColor: "rgba(255, 255, 255, 1)",
          pointBorderWidth: 2,
          borderWidth: 3,
          spanGaps: false,
        },
        ...(comparisonPlayerId ? [
          ...(isHealer && comparisonHpsData ? [{
            label: `HPS (${comparisonPlayerName})`,
            data: comparisonHpsData,
            borderColor: "rgba(251, 191, 36, 1)",
            backgroundColor: "rgba(251, 191, 36, 0.05)",
            fill: false,
            tension: 0.5,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: "rgba(251, 191, 36, 1)",
            pointBorderColor: "rgba(255, 255, 255, 1)",
            pointBorderWidth: 2,
            borderWidth: 3,
            borderDash: [8, 4],
            spanGaps: false,
          }] : []),
          {
            label: `DPS (${comparisonPlayerName})`,
            data: comparisonDpsData || [],
            borderColor: "rgba(251, 191, 36, 1)",
            backgroundColor: "rgba(251, 191, 36, 0.05)",
            fill: false,
            tension: 0.5,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: "rgba(251, 191, 36, 1)",
            pointBorderColor: "rgba(255, 255, 255, 1)",
            pointBorderWidth: 2,
            borderWidth: 3,
            borderDash: [8, 4],
            spanGaps: false,
          },
        ] : []),
      ],
    };
  }, [encounters, comparisonEncounters, isHealer, comparisonPlayerId, comparisonPlayerName, viewMode]);

  const chartOptions = useMemo(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: "top" as const,
        labels: {
          color: "rgba(255, 255, 255, 0.9)",
          usePointStyle: true,
          padding: 15,
          font: {
            size: 12,
            weight: "500",
          },
        },
      },
      tooltip: {
        backgroundColor: "rgba(0, 0, 0, 0.8)",
        titleColor: "rgba(255, 255, 255, 1)",
        bodyColor: "rgba(255, 255, 255, 0.9)",
        borderColor: "rgba(255, 255, 255, 0.2)",
        borderWidth: 1,
        callbacks: {
          label: function(context: any) {
            return `${context.dataset.label}: ${formatNumber(context.parsed.y)}`;
          },
        },
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          color: "rgba(255, 255, 255, 0.7)",
          font: {
            size: 11,
          },
          callback: function(value: any) {
            return formatNumber(value);
          },
        },
        grid: {
          color: "rgba(255, 255, 255, 0.1)",
          drawBorder: false,
        },
      },
      x: {
        ticks: {
          display: viewMode !== "encounter",
          color: "rgba(255, 255, 255, 0.7)",
          maxRotation: viewMode === "encounter" ? 90 : 45,
          minRotation: viewMode === "encounter" ? 90 : 45,
          font: {
            size: viewMode === "encounter" ? 9 : 11,
          },
          maxTicksLimit: viewMode === "encounter" ? 20 : undefined,
          autoSkip: viewMode === "encounter",
          autoSkipPadding: viewMode === "encounter" ? 2 : 0,
        },
        grid: {
          color: "rgba(255, 255, 255, 0.1)",
          drawBorder: false,
        },
      },
    },
  }), [viewMode]);

  return (
    <>
    <Card className="border-border/40 bg-card shadow-lg">
      <CardHeader>
        <div className="flex items-center justify-between flex-wrap gap-3">
          <CardTitle className="text-lg font-semibold flex items-center gap-2">
            <TrendingUp className="size-5" />
            Динамика производительности
          </CardTitle>
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-1 bg-input/40 rounded-lg p-1 border border-border/40">
              <button
                onClick={() => setViewMode("encounter")}
                className={`px-3 py-1.5 text-sm font-medium rounded-md transition-colors ${
                  viewMode === "encounter"
                    ? "bg-card text-foreground shadow-sm"
                    : "text-muted-foreground hover:text-foreground"
                }`}
              >
                По энкаунтерам
              </button>
              <button
                onClick={() => setViewMode("date")}
                className={`px-3 py-1.5 text-sm font-medium rounded-md transition-colors ${
                  viewMode === "date"
                    ? "bg-card text-foreground shadow-sm"
                    : "text-muted-foreground hover:text-foreground"
                }`}
              >
                По датам
              </button>
            </div>
            <Button
              variant="outline"
              onClick={() => setIsFiltersOpen(true)}
              className="gap-2 h-11"
            >
              <Filter className="h-4 w-4" />
              Фильтр
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="flex items-center justify-center h-80">
            <div className="text-muted-foreground">Загрузка...</div>
          </div>
        ) : error ? (
          <div className="flex items-center justify-center h-80">
            <div className="text-destructive text-sm">{error}</div>
          </div>
        ) : !chartData ? (
          <div className="flex items-center justify-center h-80">
            <div className="text-muted-foreground">Нет данных для отображения</div>
          </div>
        ) : (
          <div className="h-80">
            <Line data={chartData} options={chartOptions} />
            </div>
        )}
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
                {specStatistics.map((spec) => (
                  <SelectItem key={spec.specName} value={spec.specName}>
                    {spec.specName}
                  </SelectItem>
                ))}
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
            <Select value={localEncounterName || "all"} onValueChange={(value) => setLocalEncounterName(value === "all" ? null : value)}>
              <SelectTrigger className="h-10 w-full">
                <SelectValue placeholder="Все энкаунтеры" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все энкаунтеры</SelectItem>
                {uniqueEncounters.map((encounter) => (
                  <SelectItem key={encounter.encounterEntry} value={encounter.encounterName}>
                    {encounter.encounterName}
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
          <div className="flex flex-col gap-1 sm:col-span-2">
            <label className="text-sm font-medium">Сравнение с игроком</label>
            <div className="relative">
              <Input
                type="text"
                value={localComparisonPlayerName}
                onChange={(e) => {
                  setLocalComparisonPlayerName(e.target.value);
                  handleSearchPlayers(e.target.value);
                }}
                placeholder="Введите имя игрока"
                className="h-10 bg-input/60"
              />
              {playerSearchResults.length > 0 && (
                <div className="absolute z-[100] w-full mt-1 bg-card border border-border/40 rounded-lg shadow-lg max-h-48 overflow-y-auto">
                  {playerSearchResults.map((player) => (
                    <button
                      key={player.id}
                      onClick={() => {
                        setLocalComparisonPlayerName(player.name);
                        setPlayerSearchResults([]);
                      }}
                      className="w-full text-left px-4 py-2 hover:bg-input/40 transition-colors text-white"
                    >
                      {player.name}
                    </button>
                  ))}
                </div>
              )}
              </div>
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
