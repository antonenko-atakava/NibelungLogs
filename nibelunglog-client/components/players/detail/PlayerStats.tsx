"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { TrendingUp, TrendingDown, Target, Shield, Heart, Zap, Award, Calendar } from "lucide-react";
import type { PlayerExtendedDetailDto } from "@/types/api/Player";

interface PlayerStatsProps {
  player: PlayerExtendedDetailDto;
}

export function PlayerStats({ player }: PlayerStatsProps) {
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

  const successRate = player.totalEncounters > 0
    ? ((player.successfulEncounters / player.totalEncounters) * 100).toFixed(1)
    : "0";

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
          <div className="text-3xl font-bold mb-1">{player.totalEncounters}</div>
          <div className="text-sm text-muted-foreground">
            <span className="text-green-500">{player.successfulEncounters}</span> успешных
            <span className="mx-1">•</span>
            <span className="text-red-500">{player.failedEncounters}</span> провалов
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
          <div className="text-3xl font-bold mb-1">{formatLargeNumber(player.totalDamage)}</div>
          <div className="text-sm text-muted-foreground space-y-1">
            <div>Средний DPS: {formatNumber(player.averageDps)}</div>
            <div className="flex items-center gap-2">
              <TrendingUp className="size-3 text-green-500" />
              <span>Макс: {formatNumber(player.maxDps)}</span>
            </div>
            {player.minDps > 0 && (
              <div className="flex items-center gap-2">
                <TrendingDown className="size-3 text-red-500" />
                <span>Мин: {formatNumber(player.minDps)}</span>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {player.totalHealing > 0 && (
        <Card className="border-border/40 bg-card shadow-lg">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Heart className="size-4" />
              Лечение
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold mb-1">{formatLargeNumber(player.totalHealing)}</div>
            {player.averageHps && (
              <div className="text-sm text-muted-foreground space-y-1">
                <div>Средний HPS: {formatNumber(player.averageHps)}</div>
                {player.maxHps && (
                  <div className="flex items-center gap-2">
                    <TrendingUp className="size-3 text-green-500" />
                    <span>Макс: {formatNumber(player.maxHps)}</span>
                  </div>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {player.bestItemLevel && (
        <Card className="border-border/40 bg-card shadow-lg">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Award className="size-4" />
              Предметы
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold mb-1">iLvl {player.bestItemLevel}</div>
            <div className="text-sm text-muted-foreground">
              Лучший уровень предметов
            </div>
          </CardContent>
        </Card>
      )}

      {player.firstEncounterDate && (
        <Card className="border-border/40 bg-card shadow-lg">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Calendar className="size-4" />
              Активность
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-sm space-y-1">
              <div>
                <div className="text-muted-foreground">Первый энкаунтер</div>
                <div className="font-medium">
                  {new Date(player.firstEncounterDate).toLocaleDateString("ru-RU")}
                </div>
              </div>
              {player.lastEncounterDate && (
                <div className="mt-2">
                  <div className="text-muted-foreground">Последний энкаунтер</div>
                  <div className="font-medium">
                    {new Date(player.lastEncounterDate).toLocaleDateString("ru-RU")}
                  </div>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {player.specStatistics.length > 1 && (
        <Card className="border-border/40 bg-card shadow-lg md:col-span-2 lg:col-span-3">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Статистика по специализациям
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {player.specStatistics.map((spec) => (
                <div key={spec.specName} className="p-4 rounded-lg bg-secondary/30 border border-border/20">
                  <div className="font-semibold mb-2">{spec.specName}</div>
                  <div className="text-sm text-muted-foreground space-y-1">
                    <div>Энкаунтеров: {spec.encountersCount}</div>
                    <div>Средний DPS: {formatNumber(spec.averageDps)}</div>
                    <div>Макс DPS: {formatNumber(spec.maxDps)}</div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </>
  );
}
