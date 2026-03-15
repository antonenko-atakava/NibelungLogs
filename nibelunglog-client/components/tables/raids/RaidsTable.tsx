"use client";

import Link from "next/link";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { RaidDto } from "@/types/api/Raid";

interface RaidsTableProps {
  raids: RaidDto[];
  isLoading?: boolean;
}

export function RaidsTable({ raids, isLoading }: RaidsTableProps) {
  if (isLoading)
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-muted-foreground">Загрузка...</div>
      </div>
    );

  if (raids.length === 0)
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-muted-foreground">Рейды не найдены</div>
      </div>
    );

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU").format(value);
  };

  const formatTime = (seconds: number): string => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    
    if (hours > 0)
      return `${hours}ч ${minutes}м ${secs}с`;
    if (minutes > 0)
      return `${minutes}м ${secs}с`;
    return `${secs}с`;
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    }).format(date);
  };

  return (
    <div className="border border-border/40 rounded-2xl overflow-hidden bg-card shadow-lg">
      <Table>
        <TableHeader>
          <TableRow className="bg-secondary/30 border-b border-border/30">
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Рейд</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Гильдия</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Лидер</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Дата</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Время</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Боссы</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Вайпы</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Урон</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Хил</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {raids.map((raid) => (
            <TableRow
              key={raid.id}
              className="hover:bg-secondary/20 transition-colors border-b border-border/20"
            >
              <TableCell>
                <Link
                  href={`/raids/${raid.id}`}
                  className="font-semibold text-base hover:text-primary transition-colors"
                >
                  {raid.raidTypeName}
                </Link>
              </TableCell>
              <TableCell className="text-sm">{raid.guildName || "-"}</TableCell>
              <TableCell className="text-sm">{raid.leaderName || "-"}</TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {formatDate(raid.startTime)}
              </TableCell>
              <TableCell className="text-right font-medium text-sm">
                {formatTime(raid.totalTime)}
              </TableCell>
              <TableCell className="text-right">
                <span className="inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium bg-primary/10 text-primary border border-primary/20">
                  {raid.completedBosses}/{raid.totalBosses}
                </span>
              </TableCell>
              <TableCell className="text-right text-sm text-muted-foreground">
                {raid.wipes}
              </TableCell>
              <TableCell className="text-right font-medium text-sm">
                {formatNumber(raid.totalDamage)}
              </TableCell>
              <TableCell className="text-right font-medium text-sm">
                {formatNumber(raid.totalHealing)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
