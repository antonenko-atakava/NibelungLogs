"use client";

import { useState, useEffect } from "react";
import { playersApi } from "@/utils/api/playersApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { TrendingUp, Activity } from "lucide-react";
import type { PlayerEncounterTimelineDto } from "@/types/api/Player";
import type { EncounterListItemDto } from "@/types/api/Encounter";

interface PlayerEncounterChartProps {
  playerId: number;
}

export function PlayerEncounterChart({ playerId }: PlayerEncounterChartProps) {
  const [uniqueEncounters, setUniqueEncounters] = useState<EncounterListItemDto[]>([]);
  const [selectedEncounter, setSelectedEncounter] = useState<string>("");
  const [timelineData, setTimelineData] = useState<PlayerEncounterTimelineDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUniqueEncounters = async () => {
      try {
        const data = await playersApi.getPlayerUniqueEncounters(playerId);
        setUniqueEncounters(data);
        if (data.length > 0 && !selectedEncounter) {
          setSelectedEncounter(data[0].encounterEntry);
        }
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
      }
    };

    fetchUniqueEncounters();
  }, [playerId]);

  useEffect(() => {
    if (!selectedEncounter)
      return;

    const fetchTimeline = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const data = await playersApi.getPlayerEncounterTimeline(playerId, selectedEncounter);
        setTimelineData(data);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setTimelineData([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchTimeline();
  }, [playerId, selectedEncounter]);

  const formatNumber = (value: number): string => {
    return Math.round(value).toLocaleString("ru-RU");
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

  if (uniqueEncounters.length === 0)
    return null;

  const selectedEncounterName = uniqueEncounters.find(e => e.encounterEntry === selectedEncounter)?.encounterName || selectedEncounter;

  const maxDps = timelineData.length > 0 ? Math.max(...timelineData.map(d => d.dps)) : 0;
  const maxHps = timelineData.length > 0 
    ? Math.max(...timelineData.filter(d => d.hps !== null).map(d => d.hps!)) 
    : 0;
  const maxValue = Math.max(maxDps, maxHps);

  return (
    <Card className="border-border/40 bg-card shadow-lg">
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg font-semibold flex items-center gap-2">
            <Activity className="size-5" />
            Динамика производительности
          </CardTitle>
          <Select value={selectedEncounter} onValueChange={setSelectedEncounter}>
            <SelectTrigger className="w-[250px]">
              <SelectValue placeholder="Выберите босса" />
            </SelectTrigger>
            <SelectContent>
              {uniqueEncounters.map((encounter) => (
                <SelectItem key={encounter.encounterEntry} value={encounter.encounterEntry}>
                  {encounter.encounterName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="flex items-center justify-center p-8">
            <div className="text-muted-foreground">Загрузка...</div>
          </div>
        ) : error ? (
          <div className="text-destructive text-center p-4">{error}</div>
        ) : timelineData.length === 0 ? (
          <div className="text-center p-8 text-muted-foreground">
            Нет данных для отображения
          </div>
        ) : (
          <div className="space-y-6">
            <div className="text-sm text-muted-foreground mb-4">
              Прогресс по энкаунтерам: {selectedEncounterName}
            </div>
            
            <div className="space-y-8">
              {timelineData.map((point, index) => {
                const dpsPercentage = maxValue > 0 ? (point.dps / maxValue) * 100 : 0;
                const hpsPercentage = point.hps && maxValue > 0 ? (point.hps / maxValue) * 100 : 0;
                const isLast = index === timelineData.length - 1;

                return (
                  <div key={point.encounterId} className="relative">
                    <div className="flex items-center gap-4">
                      <div className="w-24 text-xs text-muted-foreground shrink-0">
                        {formatDate(point.startTime)}
                      </div>
                      
                      <div className="flex-1 space-y-3">
                        <div>
                          <div className="flex items-center justify-between text-xs mb-1">
                            <span className="text-muted-foreground">DPS</span>
                            <span className="font-semibold text-primary">{formatNumber(point.dps)}</span>
                          </div>
                          <div className="h-3 bg-secondary/30 rounded-full overflow-hidden">
                            <div
                              className="h-full bg-gradient-to-r from-primary to-primary/60 rounded-full transition-all duration-500"
                              style={{ width: `${dpsPercentage}%` }}
                            />
                          </div>
                        </div>
                        
                        {point.hps !== null && (
                          <div>
                            <div className="flex items-center justify-between text-xs mb-1">
                              <span className="text-muted-foreground">HPS</span>
                              <span className="font-semibold text-green-500">{formatNumber(point.hps)}</span>
                            </div>
                            <div className="h-3 bg-secondary/30 rounded-full overflow-hidden">
                              <div
                                className="h-full bg-gradient-to-r from-green-500 to-green-500/60 rounded-full transition-all duration-500"
                                style={{ width: `${hpsPercentage}%` }}
                              />
                            </div>
                          </div>
                        )}
                      </div>

                      <div className="w-32 text-right shrink-0">
                        <div className={`text-xs font-medium ${point.success ? "text-green-500" : "text-red-500"}`}>
                          {point.success ? "Успех" : "Провал"}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {Math.floor(point.duration / 60)}:{(point.duration % 60).toString().padStart(2, "0")}
                        </div>
                      </div>
                    </div>

                    {!isLast && (
                      <div className="absolute left-[7rem] top-12 w-0.5 h-6 bg-border/30" />
                    )}
                  </div>
                );
              })}
            </div>

            <div className="mt-6 pt-6 border-t border-border/30">
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div>
                  <div className="text-muted-foreground mb-1">Всего энкаунтеров</div>
                  <div className="font-semibold text-lg">{timelineData.length}</div>
                </div>
                <div>
                  <div className="text-muted-foreground mb-1">Успешных</div>
                  <div className="font-semibold text-lg text-green-500">
                    {timelineData.filter(d => d.success).length}
                  </div>
                </div>
                <div>
                  <div className="text-muted-foreground mb-1">Лучший DPS</div>
                  <div className="font-semibold text-lg text-primary">
                    {formatNumber(Math.max(...timelineData.map(d => d.dps)))}
                  </div>
                </div>
                {maxHps > 0 && (
                  <div>
                    <div className="text-muted-foreground mb-1">Лучший HPS</div>
                    <div className="font-semibold text-lg text-green-500">
                      {formatNumber(maxHps)}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
