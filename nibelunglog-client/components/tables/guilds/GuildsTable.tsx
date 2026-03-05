"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowUpDown, ArrowUp, ArrowDown, Users, Crown, Sword, Trophy } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { GuildIcon } from "@/components/guilds/GuildIcon";
import { GuildBadge } from "@/components/guilds/GuildBadge";
import { RatingBadge } from "@/components/guilds/RatingBadge";
import type { GuildDto } from "@/types/api/Guild";

interface GuildsTableProps {
  guilds: GuildDto[];
  isLoading?: boolean;
  onSortChange?: (sortField: string | null, sortDirection: "asc" | "desc") => void;
}

type SortField = "guildName" | "membersCount" | "lastUpdated" | "rating" | null;
type SortDirection = "asc" | "desc";

export function GuildsTable({ guilds, isLoading, onSortChange }: GuildsTableProps) {
  const [sortField, setSortField] = useState<SortField>("rating");
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");

  if (isLoading)
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-muted-foreground">Загрузка...</div>
      </div>
    );

  if (guilds.length === 0)
    return (
      <div className="flex flex-col items-center justify-center p-8 border rounded-lg">
        <div className="text-muted-foreground text-lg mb-2">Гильдии не найдены</div>
        <div className="text-sm text-muted-foreground">
          Попробуйте изменить фильтры или убедитесь, что в базе данных есть данные
        </div>
      </div>
    );

  const formatDate = (dateString: string): string => {
    try {
      const date = new Date(dateString);
      return new Intl.DateTimeFormat("ru-RU", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      }).format(date);
    } catch {
      return "-";
    }
  };

  const handleSort = (field: SortField) => {
    let newDirection: SortDirection;
    if (sortField === field) {
      newDirection = sortDirection === "asc" ? "desc" : "asc";
    } else {
      newDirection = "desc";
    }
    setSortField(field);
    setSortDirection(newDirection);
    if (onSortChange)
      onSortChange(field, newDirection);
  };

  const SortButton = ({ field, children }: { field: SortField; children: React.ReactNode }) => {
    const isActive = sortField === field;
    return (
      <Button
        variant="ghost"
        size="sm"
        className="h-auto p-0 font-semibold text-xs uppercase tracking-wider text-muted-foreground hover:text-foreground"
        onClick={() => handleSort(field)}
      >
        <span className="flex items-center gap-1">
          {children}
          {isActive ? (
            sortDirection === "asc" ? (
              <ArrowUp className="h-3 w-3" />
            ) : (
              <ArrowDown className="h-3 w-3" />
            )
          ) : (
            <ArrowUpDown className="h-3 w-3 opacity-50" />
          )}
        </span>
      </Button>
    );
  };

  const formatMembersCount = (count: number): string => {
    return new Intl.NumberFormat("ru-RU").format(count);
  };

  return (
    <Table>
      <TableHeader>
        <TableRow className="bg-secondary/30 border-b border-border/30">
          <TableHead className="px-6 font-semibold text-xs uppercase tracking-wider text-muted-foreground">
            Гильдия
          </TableHead>
          <TableHead className="text-center">
            <SortButton field="rating">Рейтинг</SortButton>
          </TableHead>
          <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">
            Статистика
          </TableHead>
          <TableHead className="text-center">
            <SortButton field="membersCount">Участников</SortButton>
          </TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {guilds.map((guild, index) => (
          <TableRow
            key={guild.id}
            className="hover:bg-secondary/20 transition-colors border-b border-border/20 last:border-b-0"
          >
            <TableCell className="px-6">
              <Link
                href={`/guilds/${guild.id}`}
                className="flex items-center gap-3 hover:text-primary transition-colors group"
              >
                <GuildIcon guildName={guild.guildName} size={40} className="flex-shrink-0" />
                <div className="flex flex-col min-w-0">
                  <span className="font-semibold text-base text-foreground group-hover:text-primary transition-colors truncate">
                    {guild.guildName}
                  </span>
                  {guild.leaderName && (
                    <div className="flex items-center gap-1.5 mt-0.5">
                      <Crown className="h-3 w-3 text-amber-600/70 dark:text-amber-400/70" />
                      <span className="text-xs text-muted-foreground truncate">
                        {guild.leaderName}
                      </span>
                    </div>
                  )}
                </div>
              </Link>
            </TableCell>
            <TableCell className="text-center">
              <div className="flex justify-center">
                <RatingBadge rating={guild.rating} rank={index + 1} />
              </div>
            </TableCell>
            <TableCell>
              <div className="flex items-center justify-center gap-2 flex-wrap">
                <GuildBadge
                  value={guild.fullRaidsCount}
                  label="рейдов"
                  variant="success"
                />
                <GuildBadge
                  value={guild.uniqueRaidLeadersCount}
                  label="RL"
                  variant="primary"
                />
                <GuildBadge
                  value={guild.topDamageDealersCount}
                  label="топ ДД"
                  variant="outline"
                />
              </div>
            </TableCell>
            <TableCell className="text-center">
              <div className="flex justify-center">
                <GuildBadge
                  value={guild.membersCount}
                  label="участников"
                  variant="primary"
                >
                  <div className="flex items-center gap-1">
                    <Users className="h-3 w-3 opacity-70" />
                    {formatMembersCount(guild.membersCount)}
                  </div>
                </GuildBadge>
              </div>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
