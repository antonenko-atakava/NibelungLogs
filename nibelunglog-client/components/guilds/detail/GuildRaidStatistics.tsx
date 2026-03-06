"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { Shield, Target, Clock, Swords, Heart, Award, Users, TrendingUp, TrendingDown } from "lucide-react";
import type { GuildRaidStatisticsDto } from "@/types/api/Guild";

interface GuildRaidStatisticsProps {
  guildId: number;
}

export function GuildRaidStatistics({ guildId }: GuildRaidStatisticsProps) {
  const [statistics, setStatistics] = useState<GuildRaidStatisticsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchStatistics = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await guildsApi.getGuildRaidStatistics(guildId);
        setStatistics(result);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setStatistics(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchStatistics();
  }, [guildId]);

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 1 }).format(value);
  };

  const formatLargeNumber = (value: number): string => {
    if (value >= 1_000_000_000)
      return `${(value / 1_000_000_000).toFixed(2)} млрд`;
    if (value >= 1_000_000)
      return `${(value / 1_000_000).toFixed(2)} млн`;
    if (value >= 1_000)
      return `${(value / 1_000).toFixed(2)} тыс`;
    return formatNumber(value);
  };

  const formatTime = (minutes: number): string => {
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    if (hours > 0)
      return `${hours}ч ${mins}м`;
    return `${mins}м`;
  };

  if (isLoading)
    return (
      <Card>
        <CardHeader>
          <CardTitle>Статистика рейдов</CardTitle>
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
          <CardTitle>Статистика рейдов</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center h-32">
            <div className="text-destructive">{error}</div>
          </div>
        </CardContent>
      </Card>
    );

  if (!statistics)
    return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Статистика рейдов</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Shield className="h-4 w-4" />
              <span>Средние вайпы</span>
            </div>
            <div className="text-2xl font-bold">{formatNumber(statistics.averageWipesPerRaid)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Target className="h-4 w-4" />
              <span>Успешность</span>
            </div>
            <div className="text-2xl font-bold">{formatNumber(statistics.successRate)}%</div>
            <div className="text-xs text-muted-foreground">
              {statistics.totalSuccessfulEncounters} / {statistics.totalSuccessfulEncounters + statistics.totalFailedEncounters}
            </div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Clock className="h-4 w-4" />
              <span>Среднее время</span>
            </div>
            <div className="text-2xl font-bold">{formatTime(statistics.averageRaidTimeMinutes)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Swords className="h-4 w-4" />
              <span>Общий урон</span>
            </div>
            <div className="text-2xl font-bold">{formatLargeNumber(statistics.totalDamage)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Heart className="h-4 w-4" />
              <span>Общий хил</span>
            </div>
            <div className="text-2xl font-bold">{formatLargeNumber(statistics.totalHealing)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Award className="h-4 w-4" />
              <span>Средний ILVL</span>
            </div>
            <div className="text-2xl font-bold">{formatNumber(statistics.averageGearScore)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <TrendingUp className="h-4 w-4" />
              <span>Макс. ILVL</span>
            </div>
            <div className="text-2xl font-bold">{formatNumber(statistics.maxGearScore)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <Users className="h-4 w-4" />
              <span>Средний размер</span>
            </div>
            <div className="text-2xl font-bold">{formatNumber(statistics.averageRaidSize)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <TrendingUp className="h-4 w-4 text-green-600" />
              <span>Успешных</span>
            </div>
            <div className="text-2xl font-bold text-green-600">{formatNumber(statistics.totalSuccessfulEncounters)}</div>
          </div>

          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2 text-muted-foreground text-sm">
              <TrendingDown className="h-4 w-4 text-red-600" />
              <span>Провалов</span>
            </div>
            <div className="text-2xl font-bold text-red-600">{formatNumber(statistics.totalFailedEncounters)}</div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
