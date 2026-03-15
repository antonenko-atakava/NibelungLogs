"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { getEncounterName } from "@/utils/wow/encounterMappings";
import { CheckCircle2, XCircle, Clock, Users } from "lucide-react";
import type { EncounterDto } from "@/types/api/Raid";

interface RaidEncountersTableProps {
  encounters: EncounterDto[];
}

export function RaidEncountersTable({ encounters }: RaidEncountersTableProps) {
  const formatTime = (dateString: string): string => {
    return new Date(dateString).toLocaleTimeString("ru-RU", {
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    });
  };

  const getDurationInSeconds = (startTime: string, endTime: string): number => {
    const start = new Date(startTime).getTime();
    const end = new Date(endTime).getTime();
    return Math.floor((end - start) / 1000);
  };

  const formatDuration = (startTime: string, endTime: string): string => {
    const duration = getDurationInSeconds(startTime, endTime);
    
    const minutes = Math.floor(duration / 60);
    const seconds = duration % 60;
    
    if (minutes > 0)
      return `${minutes}м ${seconds}с`;
    return `${seconds}с`;
  };

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

  const sortedEncounters = [...encounters].sort((a, b) => 
    new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
  );

  return (
    <Card className="border-border/40 bg-card shadow-lg">
      <CardHeader>
        <CardTitle className="text-lg font-semibold flex items-center gap-2">
          <Users className="size-5" />
          Энкаунтеры рейда
        </CardTitle>
      </CardHeader>
      <CardContent>
        {sortedEncounters.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            Нет энкаунтеров
          </div>
        ) : (
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[50px]">Статус</TableHead>
                  <TableHead>Энкаунтер</TableHead>
                  <TableHead>Время</TableHead>
                  <TableHead>Длительность</TableHead>
                  <TableHead className="text-right">Урон</TableHead>
                  <TableHead className="text-right">Лечение</TableHead>
                  <TableHead className="text-right">DPS</TableHead>
                  <TableHead className="text-right">HPS</TableHead>
                  <TableHead className="text-right">Состав</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedEncounters.map((encounter) => (
                  <TableRow key={encounter.id}>
                    <TableCell>
                      {encounter.success ? (
                        <CheckCircle2 className="size-5 text-green-500" />
                      ) : (
                        <XCircle className="size-5 text-red-500" />
                      )}
                    </TableCell>
                    <TableCell>
                      <span className="font-medium">
                        {getEncounterName(encounter.encounterEntry)}
                      </span>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <Clock className="size-4" />
                        {formatTime(encounter.startTime)}
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm">{formatDuration(encounter.startTime, encounter.endTime)}</span>
                    </TableCell>
                    <TableCell className="text-right">
                      <span className="font-medium">{formatLargeNumber(encounter.totalDamage)}</span>
                    </TableCell>
                    <TableCell className="text-right">
                      <span className="font-medium">{formatLargeNumber(encounter.totalHealing)}</span>
                    </TableCell>
                    <TableCell className="text-right">
                      {(() => {
                        const duration = getDurationInSeconds(encounter.startTime, encounter.endTime);
                        const dps = duration > 0 ? encounter.totalDamage / duration : 0;
                        return <span className="font-medium">{formatNumber(dps)}</span>;
                      })()}
                    </TableCell>
                    <TableCell className="text-right">
                      {(() => {
                        const duration = getDurationInSeconds(encounter.startTime, encounter.endTime);
                        const hps = duration > 0 ? encounter.totalHealing / duration : 0;
                        return <span className="font-medium">{formatNumber(hps)}</span>;
                      })()}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2 text-sm text-muted-foreground">
                        <span>T: {encounter.tanks}</span>
                        <span>H: {encounter.healers}</span>
                        <span>D: {encounter.damageDealers}</span>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
