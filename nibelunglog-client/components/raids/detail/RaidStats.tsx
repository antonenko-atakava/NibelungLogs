"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Zap, Heart, Target, Award, Clock, AlertTriangle } from "lucide-react";
import type { RaidDetailDto } from "@/types/api/Raid";

interface RaidStatsProps {
  raid: RaidDetailDto;
}

export function RaidStats({ raid }: RaidStatsProps) {
  const formatNumber = (value: number): string => {
    return Math.round(value).toLocaleString("ru-RU");
  };

  const formatLargeNumber = (value: number): string => {
    if (value >= 1_000_000_000)
      return `${(value / 1_000_000_000).toFixed(2)}B`;
    if (value >= 1_000_000)
      return `${(value / 1_000_000).toFixed(2)}M`;
    if (value >= 1_000)
      return `${(value / 1_000).toFixed(2)}K`;
    return value.toLocaleString("ru-RU");
  };

  const formatTime = (seconds: number): string => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    
    if (hours > 0)
      return `${hours}ч ${minutes}м`;
    if (minutes > 0)
      return `${minutes}м ${secs}с`;
    return `${secs}с`;
  };

  const successfulEncounters = raid.encounters.filter(e => e.success).length;
  const failedEncounters = raid.encounters.filter(e => !e.success).length;
  const successRate = raid.encounters.length > 0
    ? ((successfulEncounters / raid.encounters.length) * 100).toFixed(1)
    : "0";

  const totalEncounterTime = raid.encounters.reduce((sum, e) => {
    const start = new Date(e.startTime).getTime();
    const end = new Date(e.endTime).getTime();
    return sum + (end - start) / 1000;
  }, 0);

  const averageDps = raid.encounters.length > 0
    ? raid.encounters.reduce((sum, e) => sum + (e.averageDps || 0), 0) / raid.encounters.length
    : 0;

  return (
    <>
      <Card className="border-border/40 bg-card shadow-lg">
        <CardHeader className="pb-3">
          <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
            <Target className="size-4" />
            Энкаунтеры
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-3xl font-bold mb-1">{raid.encounters.length}</div>
          <div className="text-sm text-muted-foreground">
            <span className="text-green-500">{successfulEncounters}</span> успешных
            <span className="mx-1">•</span>
            <span className="text-red-500">{failedEncounters}</span> провалов
          </div>
          <div className="mt-2 text-xs text-muted-foreground">
            Успешность: {successRate}%
          </div>
        </CardContent>
      </Card>

      <Card className="border-border/40 bg-card shadow-lg">
        <CardHeader className="pb-3">
          <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
            <Zap className="size-4" />
            Урон
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-3xl font-bold mb-1">{formatLargeNumber(raid.totalDamage)}</div>
          <div className="text-sm text-muted-foreground">
            Общий урон рейда
          </div>
          {averageDps > 0 && (
            <div className="mt-2 text-xs text-muted-foreground">
              Средний DPS: {formatNumber(averageDps)}
            </div>
          )}
        </CardContent>
      </Card>

      {raid.totalHealing > 0 && (
        <Card className="border-border/40 bg-card shadow-lg">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Heart className="size-4" />
              Лечение
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold mb-1">{formatLargeNumber(raid.totalHealing)}</div>
            <div className="text-sm text-muted-foreground">
              Общее лечение рейда
            </div>
          </CardContent>
        </Card>
      )}

      <Card className="border-border/40 bg-card shadow-lg">
        <CardHeader className="pb-3">
          <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
            <Award className="size-4" />
            Прогресс
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-3xl font-bold mb-1">
            {raid.completedBosses}/{raid.totalBosses}
          </div>
          <div className="text-sm text-muted-foreground">
            Завершено боссов
          </div>
          <div className="mt-2 text-xs text-muted-foreground">
            {((raid.completedBosses / raid.totalBosses) * 100).toFixed(0)}% прогресса
          </div>
        </CardContent>
      </Card>

      <Card className="border-border/40 bg-card shadow-lg">
        <CardHeader className="pb-3">
          <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
            <Clock className="size-4" />
            Время
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-3xl font-bold mb-1">{formatTime(raid.totalTime)}</div>
          <div className="text-sm text-muted-foreground">
            Общее время рейда
          </div>
          {totalEncounterTime > 0 && (
            <div className="mt-2 text-xs text-muted-foreground">
              Время в бою: {formatTime(totalEncounterTime)}
            </div>
          )}
        </CardContent>
      </Card>

      {raid.wipes > 0 && (
        <Card className="border-border/40 bg-card shadow-lg">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <AlertTriangle className="size-4" />
              Вайпы
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold mb-1 text-red-500">{raid.wipes}</div>
            <div className="text-sm text-muted-foreground">
              Всего вайпов
            </div>
          </CardContent>
        </Card>
      )}
    </>
  );
}
