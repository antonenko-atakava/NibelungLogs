"use client";

import { useState, useEffect, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import type { GuildProgressDto } from "@/types/api/Guild";

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

interface GuildProgressChartProps {
  guildId: number;
}

export function GuildProgressChart({ guildId }: GuildProgressChartProps) {
  const [data, setData] = useState<GuildProgressDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProgress = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await guildsApi.getGuildProgress(guildId);
        setData(result);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setData([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchProgress();
  }, [guildId]);

  const chartData = useMemo(() => {
    if (data.length === 0)
      return null;

    const labels = data.map(item => {
      const date = new Date(item.startTime);
      return date.toLocaleDateString("ru-RU", { day: "2-digit", month: "2-digit" });
    });

    const progressScores = data.map(item => item.progressScore);

    return {
      labels,
      datasets: [
        {
          label: "Прогресс",
          data: progressScores,
          borderColor: "rgb(59, 130, 246)",
          backgroundColor: "rgba(59, 130, 246, 0.1)",
          fill: true,
          tension: 0.4,
          pointRadius: 4,
          pointHoverRadius: 6,
          pointBackgroundColor: "rgb(59, 130, 246)",
          pointBorderColor: "#fff",
          pointBorderWidth: 2,
        },
      ],
    };
  }, [data]);

  const chartOptions = useMemo(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      title: {
        display: false,
      },
      tooltip: {
        mode: "index" as const,
        intersect: false,
        callbacks: {
          label: function(context: any) {
            const index = context.dataIndex;
            const item = data[index];
            return [
              `Прогресс: ${item.progressScore.toFixed(1)}`,
              `Убито боссов: ${item.completedBosses}/${item.totalBosses}`,
              `Вайпов: ${item.wipes}`,
              `Рейд: ${item.raidTypeName}`,
            ];
          },
        },
      },
    },
    scales: {
      x: {
        display: true,
        grid: {
          display: false,
        },
      },
      y: {
        display: true,
        beginAtZero: false,
        grid: {
          color: "rgba(0, 0, 0, 0.05)",
        },
        title: {
          display: true,
          text: "Показатель прогресса",
        },
      },
    },
  }), [data]);

  if (isLoading)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Прогресс гильдии</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-64">
            <div className="text-muted-foreground">Загрузка...</div>
          </div>
        </CardContent>
      </Card>
    );

  if (error)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Прогресс гильдии</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-64">
            <div className="text-destructive">{error}</div>
          </div>
        </CardContent>
      </Card>
    );

  if (!chartData)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Прогресс гильдии</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-64">
            <div className="text-muted-foreground">Нет данных для отображения</div>
          </div>
        </CardContent>
      </Card>
    );

  return (
    <Card>
      <CardHeader>
        <CardTitle>Прогресс гильдии</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-64">
          <Line data={chartData} options={chartOptions} />
        </div>
      </CardContent>
    </Card>
  );
}
