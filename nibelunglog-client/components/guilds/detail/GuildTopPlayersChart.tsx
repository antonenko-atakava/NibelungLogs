"use client";

import { useState, useEffect, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
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
import { guildsApi } from "@/utils/api/guildsApi";
import { raidTypesApi } from "@/utils/api/raidTypesApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { getClassColor, getClassColorWithOpacity } from "@/utils/wow/classColors";
import { getClassId } from "@/utils/wow/classMappings";
import type { GuildMemberDto } from "@/types/api/Guild";
import type { RaidTypeDto } from "@/types/api/RaidType";
import type { EncounterListItemDto } from "@/types/api/Encounter";

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

interface GuildTopPlayersChartProps {
  guildId: number;
}

export function GuildTopPlayersChart({ guildId }: GuildTopPlayersChartProps) {
  const [data, setData] = useState<GuildMemberDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [metricTab, setMetricTab] = useState<"dps" | "hps">("dps");
  const [metricType, setMetricType] = useState<"average" | "max">("max");
  const [filterTab, setFilterTab] = useState<"class" | "spec">("class");
  const [selectedClass, setSelectedClass] = useState<string | undefined>(undefined);
  const [selectedSpec, setSelectedSpec] = useState<string | undefined>(undefined);
  const [raidTypes, setRaidTypes] = useState<RaidTypeDto[]>([]);
  const [uniqueEncounters, setUniqueEncounters] = useState<EncounterListItemDto[]>([]);
  const [selectedRaidTypeId, setSelectedRaidTypeId] = useState<number | undefined>(undefined);
  const [selectedEncounterName, setSelectedEncounterName] = useState<string | undefined>(undefined);

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
        const encounters = await guildsApi.getGuildUniqueEncounters(guildId, selectedRaidTypeId);
        setUniqueEncounters(encounters);
        if (selectedEncounterName && !encounters.some(e => e.encounterName === selectedEncounterName)) {
          setSelectedEncounterName(undefined);
        }
      } catch (err) {
        console.error("Failed to load unique encounters:", err);
        setUniqueEncounters([]);
      }
    };

    fetchUniqueEncounters();
  }, [guildId, selectedRaidTypeId]);

  useEffect(() => {
    const fetchTopPlayers = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const classId = selectedClass ? getClassId(selectedClass) : undefined;

        const result = await guildsApi.getGuildMembers(guildId, {
          characterClass: classId,
          spec: selectedSpec,
          raidTypeId: selectedRaidTypeId,
          encounterName: selectedEncounterName,
          page: 1,
          pageSize: 25,
          sortField: metricTab === "dps" 
            ? (metricType === "average" ? "averageDps" : "maxDps")
            : (metricType === "average" ? "averageHps" : "maxHps"),
          sortDirection: "desc",
        });
        setData(result.items);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setData([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchTopPlayers();
  }, [guildId, metricTab, metricType, selectedClass, selectedSpec, selectedRaidTypeId, selectedEncounterName]);

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU").format(value);
  };

  const chartData = useMemo(() => {
    if (data.length === 0)
      return null;

    const labels = data.map((member, index) => `#${index + 1} ${member.characterName}`);
    const values = data.map(member => {
      if (metricTab === "dps")
        return metricType === "average" ? Math.round(member.averageDps) : Math.round(member.maxDps);
      return metricType === "average" 
        ? (member.averageHps ? Math.round(member.averageHps) : 0)
        : (member.maxHps ? Math.round(member.maxHps) : 0);
    });
    const colors = data.map(member => {
      if (!member.className)
        return "rgba(128, 128, 128, 0.6)";
      return getClassColorWithOpacity(member.className, 0.6);
    });
    const borderColors = data.map(member => {
      if (!member.className)
        return "rgb(128, 128, 128)";
      return getClassColor(member.className);
    });

    return {
      labels,
      datasets: [
        {
          label: metricTab === "dps"
            ? (metricType === "average" ? "Средний DPS" : "Максимальный DPS")
            : (metricType === "average" ? "Средний HPS" : "Максимальный HPS"),
          data: values,
          backgroundColor: colors,
          borderColor: borderColors,
          borderWidth: 2,
        },
      ],
    };
  }, [data, metricTab, metricType]);

  const chartOptions = useMemo(() => ({
    indexAxis: "y" as const,
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
          label: function(context: any) {
            const value = context.parsed.x || 0;
            return `${formatNumber(value)}`;
          },
        },
      },
    },
    scales: {
      x: {
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
      y: {
        ticks: {
          color: "rgba(255, 255, 255, 0.7)",
        },
        grid: {
          color: "rgba(255, 255, 255, 0.1)",
        },
      },
    },
  }), [formatNumber]);

  if (isLoading)
    return (
      <Card className="border-border/40 bg-card shadow-lg">
        <CardContent className="p-6">
          <div className="flex items-center justify-center h-64">
            <div className="text-muted-foreground">Загрузка...</div>
          </div>
        </CardContent>
      </Card>
    );

  if (error)
    return (
      <Card className="border-border/40 bg-card shadow-lg">
        <CardContent className="p-6">
          <div className="flex items-center justify-center h-64">
            <div className="text-destructive text-sm">{error}</div>
          </div>
        </CardContent>
      </Card>
    );

  const classList = [
    "Воин",
    "Паладин",
    "Охотник",
    "Разбойник",
    "Жрец",
    "Рыцарь смерти",
    "Шаман",
    "Маг",
    "Чернокнижник",
    "Друид",
  ];

  const getSpecsForClass = (className: string): string[] => {
    const specMap: Record<string, string[]> = {
      "Воин": ["Оружие", "Неистовство", "Защита"],
      "Паладин": ["Свет", "Защита", "Воздаяние"],
      "Охотник": ["Повелитель зверей", "Стрельба", "Выживание"],
      "Разбойник": ["Убийство", "Бой", "Скрытность"],
      "Жрец": ["Послушание", "Свет", "Тьма"],
      "Рыцарь смерти": ["Кровь", "Лед", "Нечестивость"],
      "Шаман": ["Стихии", "Улучшение", "Исцеление"],
      "Маг": ["Тайная магия", "Огонь", "Лед"],
      "Чернокнижник": ["Колдовство", "Демонология", "Разрушение"],
      "Друид": ["Баланс", "Сила зверя", "Исцеление"],
    };
    return specMap[className] || [];
  };

  return (
    <div className="relative mt-16">
      <div className="absolute -top-12 left-0 flex justify-start z-10 gap-3">
        <div className="flex gap-0 bg-card border border-border/40 rounded-t-2xl border-b-0 overflow-hidden shadow-lg">
          <button
            onClick={() => {
              setMetricTab("dps");
              setMetricType("max");
            }}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              metricTab === "dps"
                ? "text-foreground bg-card border-b-2 border-border/60"
                : "text-muted-foreground hover:text-foreground hover:bg-card/50"
            }`}
          >
            DPS
          </button>
          <button
            onClick={() => {
              setMetricTab("hps");
              setMetricType("max");
            }}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              metricTab === "hps"
                ? "text-foreground bg-card border-b-2 border-border/60"
                : "text-muted-foreground hover:text-foreground hover:bg-card/50"
            }`}
          >
            HPS
          </button>
        </div>
        <div className="flex gap-0 bg-card border border-border/40 rounded-t-2xl border-b-0 overflow-hidden shadow-lg">
          <button
            onClick={() => setMetricType("average")}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              metricType === "average"
                ? "text-foreground bg-card border-b-2 border-border/60"
                : "text-muted-foreground hover:text-foreground hover:bg-card/50"
            }`}
          >
            Средний
          </button>
          <button
            onClick={() => setMetricType("max")}
            className={`px-6 py-3 text-sm font-medium transition-colors ${
              metricType === "max"
                ? "text-foreground bg-card border-b-2 border-border/60"
                : "text-muted-foreground hover:text-foreground hover:bg-card/50"
            }`}
          >
            Макс.
          </button>
        </div>
      </div>
      <Card className="border-border/40 bg-card shadow-lg pt-4 rounded-none rounded-tr-2xl rounded-br-2xl rounded-bl-2xl">
        <CardHeader>
          <div className="flex items-center justify-between flex-wrap gap-3">
            <CardTitle className="text-lg font-semibold">Топ 25 игроков</CardTitle>
          </div>
          <div className="flex items-center gap-3 mt-4 flex-wrap">
          <div className="flex gap-2">
            <button
              onClick={() => {
                setFilterTab("class");
                setSelectedClass(undefined);
                setSelectedSpec(undefined);
              }}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                filterTab === "class"
                  ? "bg-primary text-primary-foreground"
                  : "bg-secondary text-secondary-foreground hover:bg-secondary/80"
              }`}
            >
              Класс
            </button>
            <button
              onClick={() => {
                setFilterTab("spec");
                setSelectedClass(undefined);
                setSelectedSpec(undefined);
              }}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                filterTab === "spec"
                  ? "bg-primary text-primary-foreground"
                  : "bg-secondary text-secondary-foreground hover:bg-secondary/80"
              }`}
            >
              Спек
            </button>
          </div>
          {filterTab === "class" && (
            <Select
              value={selectedClass || "all"}
              onValueChange={(value) => {
                setSelectedClass(value === "all" ? undefined : value);
                setSelectedSpec(undefined);
              }}
            >
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Все классы" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все классы</SelectItem>
                {classList.map((className) => (
                  <SelectItem key={className} value={className}>
                    {className}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
          {filterTab === "spec" && (
            <div className="flex gap-2">
              <Select
                value={selectedClass || "none"}
                onValueChange={(value) => {
                  setSelectedClass(value === "none" ? undefined : value);
                  setSelectedSpec(undefined);
                }}
              >
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Выберите класс" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">Выберите класс</SelectItem>
                  {classList.map((className) => (
                    <SelectItem key={className} value={className}>
                      {className}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {selectedClass && (
                <Select
                  value={selectedSpec || "all"}
                  onValueChange={(value) => setSelectedSpec(value === "all" ? undefined : value)}
                >
                  <SelectTrigger className="w-[180px]">
                    <SelectValue placeholder="Все спеки" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">Все спеки</SelectItem>
                    {getSpecsForClass(selectedClass).map((specName) => (
                      <SelectItem key={specName} value={specName}>
                        {specName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            </div>
          )}
          <Select
            value={selectedRaidTypeId?.toString() || "all"}
            onValueChange={(value) => {
              setSelectedRaidTypeId(value === "all" ? undefined : parseInt(value, 10));
              setSelectedEncounterName(undefined);
            }}
          >
            <SelectTrigger className="w-[200px]">
              <SelectValue placeholder="Все инстансы" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Все инстансы</SelectItem>
              {raidTypes.map((raidType) => (
                <SelectItem key={raidType.id} value={raidType.id.toString()}>
                  {raidType.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {selectedRaidTypeId && (
            <Select
              value={selectedEncounterName || "all"}
              onValueChange={(value) => setSelectedEncounterName(value === "all" ? undefined : value)}
            >
              <SelectTrigger className="w-[200px]">
                <SelectValue placeholder="Все боссы" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все боссы</SelectItem>
                {uniqueEncounters.map((encounter) => (
                  <SelectItem key={encounter.encounterName} value={encounter.encounterName}>
                    {encounter.encounterName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        </div>
      </CardHeader>
      <CardContent>
        {chartData ? (
          <div className="h-[600px] w-full">
            <Bar data={chartData} options={chartOptions} />
          </div>
        ) : (
          <div className="flex items-center justify-center h-64">
            <div className="text-muted-foreground">Нет данных для отображения</div>
          </div>
        )}
      </CardContent>
      </Card>
    </div>
  );
}
