"use client";

import { useState, useEffect, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { Bar } from "react-chartjs-2";
import { playersApi } from "@/utils/api/playersApi";
import { raidTypesApi } from "@/utils/api/raidTypesApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { SpecBadge } from "@/components/wow/SpecBadge";
import { getClassColor, getClassColorWithOpacity } from "@/utils/wow/classColors";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { FileQuestion } from "lucide-react";
import type { PlayerSpecStatisticsDto, PlayerSpecComparisonDto } from "@/types/api/Player";
import type { RaidTypeDto } from "@/types/api/RaidType";

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

interface PlayerSpecsChartProps {
  specStatistics: PlayerSpecStatisticsDto[];
  playerId: number;
  className?: string | null;
}

export function PlayerSpecsChart({ specStatistics, playerId, className: playerClassName }: PlayerSpecsChartProps) {
  const [activeTab, setActiveTab] = useState<"average" | "max">("average");
  const [selectedSpec, setSelectedSpec] = useState<string | null>(null);
  const [selectedRaidTypeId, setSelectedRaidTypeId] = useState<number | null>(null);
  const [raidTypes, setRaidTypes] = useState<RaidTypeDto[]>([]);
  const [isLoadingRaidTypes, setIsLoadingRaidTypes] = useState(false);
  const [comparisonData, setComparisonData] = useState<PlayerSpecComparisonDto | null>(null);
  const [isLoadingComparison, setIsLoadingComparison] = useState(false);
  const [comparisonError, setComparisonError] = useState<string | null>(null);

  useEffect(() => {
    const fetchRaidTypes = async () => {
      setIsLoadingRaidTypes(true);
      try {
        const types = await raidTypesApi.getRaidTypes();
        setRaidTypes(types);
      } catch (err) {
        console.error("Failed to load raid types:", err);
      } finally {
        setIsLoadingRaidTypes(false);
      }
    };

    fetchRaidTypes();
  }, []);

  useEffect(() => {
    if (specStatistics.length > 0 && !selectedSpec)
      setSelectedSpec(specStatistics[0].specName);
  }, [specStatistics, selectedSpec]);

  useEffect(() => {
    if (!selectedSpec)
      return;

    const fetchComparison = async () => {
      setIsLoadingComparison(true);
      setComparisonError(null);

      try {
        const result = await playersApi.getPlayerSpecComparison(
          playerId,
          selectedSpec,
          activeTab === "average",
          18,
          selectedRaidTypeId
        );
        setComparisonData(result);
        setComparisonError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setComparisonError(errorMessage);
        setComparisonData(null);
      } finally {
        setIsLoadingComparison(false);
      }
    };

    fetchComparison();
  }, [playerId, selectedSpec, activeTab, selectedRaidTypeId]);

  if (specStatistics.length === 0)
    return null;

  const selectedSpecData = specStatistics.find(s => s.specName === selectedSpec);
  const isHealerSpec = selectedSpecData ? ((selectedSpecData.averageHps !== null && selectedSpecData.averageHps > 0) || (selectedSpecData.maxHps !== null && selectedSpecData.maxHps > 0)) : false;

  const formatNumber = (value: number): string => {
    return Math.round(value).toLocaleString("ru-RU");
  };

  const chartData = useMemo(() => {
    if (!comparisonData)
      return null;

    const labels = comparisonData.players.map((p) => `#${p.rank} ${p.characterName}`);
    const values = comparisonData.players.map(p => Math.round(p.value));
    const colors = comparisonData.players.map(p => {
      if (p.isCurrentPlayer)
        return "rgba(255, 255, 255, 0.9)";
      const classColor = getClassColor(p.className);
      return getClassColorWithOpacity(p.className, 0.6);
    });
    const borderColors = comparisonData.players.map(p => {
      if (p.isCurrentPlayer)
        return "rgba(255, 255, 255, 1)";
      return getClassColor(p.className);
    });

    return {
      labels,
      datasets: [
        {
          label: isHealerSpec
            ? (activeTab === "average" ? "Средний HPS" : "Максимальный HPS")
            : (activeTab === "average" ? "Средний DPS" : "Максимальный DPS"),
          data: values,
          backgroundColor: colors,
          borderColor: borderColors,
          borderWidth: 2,
        },
      ],
    };
  }, [comparisonData, activeTab, isHealerSpec]);

  const chartOptions = useMemo(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        backgroundColor: "rgba(0, 0, 0, 0.8)",
        titleColor: "rgba(255, 255, 255, 1)",
        bodyColor: "rgba(255, 255, 255, 0.9)",
        borderColor: "rgba(255, 255, 255, 0.2)",
        borderWidth: 1,
        callbacks: {
          title: function(context: any) {
            if (!comparisonData)
              return "";
            const index = context[0].dataIndex;
            const player = comparisonData.players[index];
            return player ? `${player.characterName}${player.isCurrentPlayer ? " (Вы)" : ""} - Место #${player.rank}` : "";
          },
          label: function(context: any) {
            const metric = isHealerSpec ? "HPS" : "DPS";
            return `${activeTab === "average" ? "Средний" : "Максимальный"} ${metric}: ${formatNumber(context.parsed.y)}`;
          },
        },
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          color: "rgba(255, 255, 255, 0.7)",
          callback: function(value: any) {
            return formatNumber(value);
          },
        },
        grid: {
          color: "rgba(255, 255, 255, 0.1)",
        },
      },
      x: {
        ticks: {
          color: "rgba(255, 255, 255, 0.7)",
        },
        grid: {
          color: "rgba(255, 255, 255, 0.1)",
        },
      },
    },
  }), [comparisonData, activeTab, isHealerSpec]);

  return (
    <div className="relative mt-16">
      <div className="absolute -top-12 left-0 flex justify-start z-10">
        <div className="flex gap-0 bg-card border border-border/40 rounded-t-2xl border-b-0 overflow-hidden shadow-lg">
          <button
            onClick={() => setActiveTab("average")}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              activeTab === "average"
                ? "text-foreground bg-card border-b-2 border-border/60"
                : "text-muted-foreground hover:text-foreground hover:bg-card/50"
            }`}
          >
            {isHealerSpec ? "Средний HPS" : "Средний DPS"}
          </button>
          <button
            onClick={() => setActiveTab("max")}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              activeTab === "max"
                ? "text-foreground bg-card border-b-2 border-border/60"
                : "text-muted-foreground hover:text-foreground hover:bg-card/50"
            }`}
          >
            {isHealerSpec ? "Максимальный HPS" : "Максимальный DPS"}
          </button>
        </div>
      </div>
      <Card className="border-border/40 bg-card shadow-lg pt-4 rounded-none rounded-tr-2xl rounded-br-2xl rounded-bl-2xl">
        <CardHeader>
          <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between flex-wrap gap-3">
              <CardTitle className="text-lg font-semibold">Производительность по специализациям</CardTitle>
              <div className="flex flex-wrap gap-2">
                {specStatistics.map((spec) => (
                  <button
                    key={spec.specName}
                    onClick={() => setSelectedSpec(spec.specName)}
                    className="transition-opacity hover:opacity-80"
                  >
                    <SpecBadge
                      specName={spec.specName}
                      className={playerClassName}
                      variant={selectedSpec === spec.specName ? "default" : "outline"}
                    />
                  </button>
                ))}
              </div>
            </div>
            <div className="flex items-center gap-3">
              <label className="text-sm font-medium whitespace-nowrap">Рейд:</label>
              <Select
                value={selectedRaidTypeId?.toString() || "all"}
                onValueChange={(value) => setSelectedRaidTypeId(value === "all" ? null : parseInt(value, 10))}
                disabled={isLoadingRaidTypes}
              >
                <SelectTrigger className="w-[200px] h-10">
                  <SelectValue placeholder="Выберите рейд" />
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
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {selectedSpec && (
              <>
                {isLoadingComparison && (
                  <div className="flex items-center justify-center h-80">
                    <div className="text-muted-foreground">Загрузка...</div>
                  </div>
                )}

                {comparisonError && (
                  <div className="flex items-center justify-center h-80">
                    <div className="text-destructive text-sm">{comparisonError}</div>
                  </div>
                )}

                {!isLoadingComparison && !comparisonError && !comparisonData && (
                  <div className="flex flex-col items-center justify-center h-80 gap-4">
                    <FileQuestion className="w-16 h-16 text-muted-foreground/50" strokeWidth={1.5} />
                    <div className="text-muted-foreground text-center">
                      <div className="text-lg font-medium mb-2">Нет данных</div>
                      <div className="text-sm">
                        У нас нет информации по этому игроку в рейде{" "}
                        <span className="font-semibold text-foreground">
                          {selectedRaidTypeId
                            ? raidTypes.find((rt) => rt.id === selectedRaidTypeId)?.name || "выбранном"
                            : "выбранном"}
                        </span>
                      </div>
                    </div>
                  </div>
                )}

                {!isLoadingComparison && !comparisonError && chartData && comparisonData && (
                  <div className="space-y-4">
                    <div className="flex items-center justify-between text-sm">
                      <div>
                        <span className="text-muted-foreground">Место: </span>
                        <span className="font-semibold text-white">#{comparisonData.currentPlayerRank}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">{isHealerSpec ? "HPS" : "DPS"}: </span>
                        <span className="font-semibold text-white">{formatNumber(comparisonData.currentPlayerValue)}</span>
                      </div>
                    </div>
                    <div className="h-80">
                      <Bar data={chartData} options={chartOptions} />
                    </div>
                  </div>
                )}
              </>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
