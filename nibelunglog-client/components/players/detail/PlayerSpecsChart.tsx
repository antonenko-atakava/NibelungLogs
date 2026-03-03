"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { getClassColor } from "@/utils/wow/classColors";
import type { PlayerSpecStatisticsDto } from "@/types/api/Player";

interface PlayerSpecsChartProps {
  specStatistics: PlayerSpecStatisticsDto[];
  className?: string;
}

export function PlayerSpecsChart({ specStatistics, className }: PlayerSpecsChartProps) {
  if (specStatistics.length === 0)
    return null;

  const maxDps = Math.max(...specStatistics.map(s => s.maxDps));
  const maxEncounters = Math.max(...specStatistics.map(s => s.encountersCount));

  const formatNumber = (value: number): string => {
    return Math.round(value).toLocaleString("ru-RU");
  };

  return (
    <Card className={`border-border/40 bg-card shadow-lg ${className || ""}`}>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">Производительность по специализациям</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-6">
          {specStatistics.map((spec) => {
            const dpsPercentage = maxDps > 0 ? (spec.maxDps / maxDps) * 100 : 0;
            const encountersPercentage = maxEncounters > 0 ? (spec.encountersCount / maxEncounters) * 100 : 0;

            return (
              <div key={spec.specName} className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{spec.specName}</span>
                  <div className="text-sm text-muted-foreground">
                    {spec.encountersCount} энкаунтеров
                  </div>
                </div>
                
                <div className="space-y-2">
                  <div>
                    <div className="flex items-center justify-between text-xs text-muted-foreground mb-1">
                      <span>Макс DPS</span>
                      <span className="font-medium text-foreground">{formatNumber(spec.maxDps)}</span>
                    </div>
                    <div className="h-2 bg-secondary/30 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-primary to-primary/60 rounded-full transition-all duration-500"
                        style={{ width: `${dpsPercentage}%` }}
                      />
                    </div>
                  </div>
                  
                  <div>
                    <div className="flex items-center justify-between text-xs text-muted-foreground mb-1">
                      <span>Средний DPS</span>
                      <span className="font-medium text-foreground">{formatNumber(spec.averageDps)}</span>
                    </div>
                    <div className="h-2 bg-secondary/30 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-gradient-to-r from-primary/60 to-primary/40 rounded-full transition-all duration-500"
                        style={{ width: `${(spec.averageDps / maxDps) * 100}%` }}
                      />
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
