"use client";

import { useState, useEffect } from "react";
import { raidsApi } from "@/utils/api/raidsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ErrorMessage } from "@/components/ui/error-message";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { BarChart, TableIcon } from "lucide-react";
import type { RaidDto } from "@/types/api/Raid";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ChartOptions
} from "chart.js";
import { Bar } from "react-chartjs-2";

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

interface GuildRaidsHistoryProps {
  guildId: number;
  guildName: string;
}

export function GuildRaidsHistory({ guildId, guildName }: GuildRaidsHistoryProps) {
  const [raids, setRaids] = useState<RaidDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<"table" | "chart">("table");
  const [tablePage, setTablePage] = useState(1);
  const [chartPage, setChartPage] = useState(1);

  const tablePageSize = 10;
  const chartPageSize = 20;

  useEffect(() => {
    const fetchRaids = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await raidsApi.getRaids({
          guildName: guildName,
          page: 1,
          pageSize: 100
        });
        setRaids(result.items);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRaids();
  }, [guildName]);

  const formatDuration = (minutes: number): string => {
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    if (hours > 0)
      return `${hours}ч ${mins}м`;
    return `${mins}м`;
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric"
    });
  };

  const getRowColor = (raid: RaidDto): string => {
    if (raid.completedBosses === raid.totalBosses)
      return "bg-green-50 dark:bg-green-950/20";
    if (raid.wipes > 5)
      return "bg-red-50 dark:bg-red-950/20";
    if (raid.wipes > 0)
      return "bg-yellow-50 dark:bg-yellow-950/20";
    return "";
  };

  const tablePaginatedRaids = raids.slice(
    (tablePage - 1) * tablePageSize,
    tablePage * tablePageSize
  );

  const chartPaginatedRaids = raids.slice(
    (chartPage - 1) * chartPageSize,
    chartPage * chartPageSize
  );

  const totalTablePages = Math.ceil(raids.length / tablePageSize);
  const totalChartPages = Math.ceil(raids.length / chartPageSize);

  const chartData = {
    labels: chartPaginatedRaids.map((raid) => formatDate(raid.startTime)),
    datasets: [
      {
        label: "Убито боссов",
        data: chartPaginatedRaids.map((raid) => raid.completedBosses),
        backgroundColor: "rgba(34, 197, 94, 0.8)",
        borderColor: "rgba(34, 197, 94, 1)",
        borderWidth: 1
      },
      {
        label: "Вайпы",
        data: chartPaginatedRaids.map((raid) => raid.wipes),
        backgroundColor: "rgba(239, 68, 68, 0.8)",
        borderColor: "rgba(239, 68, 68, 1)",
        borderWidth: 1
      }
    ]
  };

  const chartOptions: ChartOptions<"bar"> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "top" as const
      },
      title: {
        display: false
      },
      tooltip: {
        callbacks: {
          afterLabel: (context) => {
            const raid = chartPaginatedRaids[context.dataIndex];
            return [
              `Рейд: ${raid.raidTypeName}`,
              `РЛ: ${raid.leaderName}`,
              `Длительность: ${formatDuration(raid.totalTime)}`
            ];
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1
        }
      }
    }
  };

  if (isLoading)
    return (
      <Card className="p-6">
        <div className="text-center text-muted-foreground">Загрузка истории рейдов...</div>
      </Card>
    );

  if (error)
    return (
      <Card className="p-6">
        <ErrorMessage message={error} onRetry={() => window.location.reload()} />
      </Card>
    );

  if (raids.length === 0)
    return (
      <Card className="p-6">
        <div className="text-center text-muted-foreground">Нет данных о рейдах</div>
      </Card>
    );

  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-semibold">История рейдов</h2>
        <div className="flex gap-2">
          <Button
            variant={viewMode === "table" ? "default" : "outline"}
            size="sm"
            onClick={() => setViewMode("table")}
          >
            <TableIcon className="h-4 w-4 mr-2" />
            Таблица
          </Button>
          <Button
            variant={viewMode === "chart" ? "default" : "outline"}
            size="sm"
            onClick={() => setViewMode("chart")}
          >
            <BarChart className="h-4 w-4 mr-2" />
            График
          </Button>
        </div>
      </div>

      {viewMode === "table" ? (
        <>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Дата</TableHead>
                  <TableHead>Рейд</TableHead>
                  <TableHead>РЛ</TableHead>
                  <TableHead className="text-center">Боссы</TableHead>
                  <TableHead className="text-center">Вайпы</TableHead>
                  <TableHead>Длительность</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tablePaginatedRaids.map((raid) => (
                  <TableRow key={raid.id} className={getRowColor(raid)}>
                    <TableCell>{formatDate(raid.startTime)}</TableCell>
                    <TableCell className="font-medium">{raid.raidTypeName}</TableCell>
                    <TableCell>{raid.leaderName}</TableCell>
                    <TableCell className="text-center">
                      {raid.completedBosses}/{raid.totalBosses}
                    </TableCell>
                    <TableCell className="text-center">{raid.wipes}</TableCell>
                    <TableCell>{formatDuration(raid.totalTime)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {totalTablePages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setTablePage((prev) => Math.max(1, prev - 1))}
                disabled={tablePage === 1}
              >
                Назад
              </Button>
              <span className="text-sm text-muted-foreground">
                Страница {tablePage} из {totalTablePages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setTablePage((prev) => Math.min(totalTablePages, prev + 1))}
                disabled={tablePage === totalTablePages}
              >
                Вперёд
              </Button>
            </div>
          )}
        </>
      ) : (
        <>
          <div className="h-96">
            <Bar data={chartData} options={chartOptions} />
          </div>

          {totalChartPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setChartPage((prev) => Math.max(1, prev - 1))}
                disabled={chartPage === 1}
              >
                Назад
              </Button>
              <span className="text-sm text-muted-foreground">
                Страница {chartPage} из {totalChartPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setChartPage((prev) => Math.min(totalChartPages, prev + 1))}
                disabled={chartPage === totalChartPages}
              >
                Вперёд
              </Button>
            </div>
          )}
        </>
      )}
    </Card>
  );
}
