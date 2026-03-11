"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { getEncounterName } from "@/utils/wow/encounterMappings";
import { Target, CheckCircle, XCircle, Clock } from "lucide-react";
import type { GuildBossStatisticsDto } from "@/types/api/Guild";

interface GuildBossStatisticsProps {
  guildId: number;
}

export function GuildBossStatistics({ guildId }: GuildBossStatisticsProps) {
  const [statistics, setStatistics] = useState<GuildBossStatisticsDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchStatistics = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await guildsApi.getGuildBossStatistics(guildId);
        setStatistics(result);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setStatistics([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchStatistics();
  }, [guildId]);

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 1 }).format(value);
  };

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    if (mins > 0)
      return `${mins}м ${secs}с`;
    return `${secs}с`;
  };

  if (isLoading)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Статистика по боссам</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-32">
            <div className="text-muted-foreground">Загрузка...</div>
          </div>
        </CardContent>
      </Card>
    );

  if (error)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Статистика по боссам</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-32">
            <div className="text-destructive">{error}</div>
          </div>
        </CardContent>
      </Card>
    );

  if (statistics.length === 0)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Статистика по боссам</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-32">
            <div className="text-muted-foreground">Нет данных для отображения</div>
          </div>
        </CardContent>
      </Card>
    );

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Target className="h-5 w-5" />
          Топ боссов
        </CardTitle>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Босс</TableHead>
              <TableHead className="text-center">Убийств</TableHead>
              <TableHead className="text-center">Попыток</TableHead>
              <TableHead className="text-center">Успешность</TableHead>
              <TableHead className="text-center">Среднее время</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {statistics.map((boss, index) => (
              <TableRow key={boss.encounterEntry}>
                <TableCell className="font-medium">
                  {getEncounterName(boss.encounterEntry)}
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex items-center justify-center gap-1">
                    <CheckCircle className="h-4 w-4 text-green-600" />
                    {boss.totalKills}
                  </div>
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex items-center justify-center gap-1">
                    <Target className="h-4 w-4 text-muted-foreground" />
                    {boss.totalAttempts}
                  </div>
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex items-center justify-center gap-1">
                    {boss.successRate >= 50 ? (
                      <CheckCircle className="h-4 w-4 text-green-600" />
                    ) : (
                      <XCircle className="h-4 w-4 text-red-600" />
                    )}
                    <span className={boss.successRate >= 50 ? "text-green-600" : "text-red-600"}>
                      {formatNumber(boss.successRate)}%
                    </span>
                  </div>
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex items-center justify-center gap-1">
                    <Clock className="h-4 w-4 text-muted-foreground" />
                    {boss.averageKillTimeSeconds > 0 ? formatTime(boss.averageKillTimeSeconds) : "-"}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
