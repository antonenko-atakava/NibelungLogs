"use client";

import { Card, CardContent } from "@/components/ui/card";
import { GuildIcon } from "@/components/guilds/GuildIcon";
import { RatingBadge } from "@/components/guilds/RatingBadge";
import { GuildBadge } from "@/components/guilds/GuildBadge";
import { Crown, Users, Swords, Target, UsersRound } from "lucide-react";
import type { GuildDetailDto } from "@/types/api/Guild";

interface GuildHeaderProps {
  guild: GuildDetailDto;
  rank?: number;
}

export function GuildHeader({ guild, rank }: GuildHeaderProps) {
  const formatDate = (dateString: string): string => {
    try {
      const date = new Date(dateString);
      return new Intl.DateTimeFormat("ru-RU", {
        year: "numeric",
        month: "long",
        day: "numeric",
      }).format(date);
    } catch {
      return "-";
    }
  };

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU").format(value);
  };

  return (
    <Card className="border-border/40 bg-card shadow-lg">
      <CardContent className="p-5">
        <div className="flex items-start gap-4">
          <GuildIcon guildName={guild.guildName} size={56} className="flex-shrink-0" />
          
          <div className="flex-1 min-w-0">
            <div className="flex items-start justify-between gap-4 mb-3">
              <div className="flex-1 min-w-0">
                <h1 className="text-2xl font-bold text-foreground mb-1.5 truncate">
                  {guild.guildName}
                </h1>
                {guild.leaderName && (
                  <div className="flex items-center gap-1.5 text-muted-foreground">
                    <Crown className="h-3.5 w-3.5 text-amber-600/70 dark:text-amber-400/70" />
                    <span className="text-xs">Лидер: {guild.leaderName}</span>
                  </div>
                )}
              </div>
              
              <div className="flex-shrink-0">
                <RatingBadge rating={guild.rating} rank={rank} />
              </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-2.5">
              <GuildBadge
                value={guild.totalRaidsCount}
                label="всего рейдов"
                variant="outline"
              >
                <div className="flex items-center gap-1">
                  <Swords className="h-3 w-3 opacity-70" />
                  {formatNumber(guild.totalRaidsCount)}
                </div>
              </GuildBadge>
              <GuildBadge
                value={guild.fullRaidsCount}
                label="полных"
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
              <GuildBadge
                value={guild.totalEncountersCount}
                label="энкаунтеров"
                variant="outline"
              >
                <div className="flex items-center gap-1">
                  <Target className="h-3 w-3 opacity-70" />
                  {formatNumber(guild.totalEncountersCount)}
                </div>
              </GuildBadge>
              <GuildBadge
                value={guild.membersCount}
                label="участников"
                variant="primary"
              >
                <div className="flex items-center gap-1">
                  <UsersRound className="h-3 w-3 opacity-70" />
                  {formatNumber(guild.membersCount)}
                </div>
              </GuildBadge>
            </div>

            {guild.lastUpdated && (
              <div className="mt-3 text-xs text-muted-foreground">
                Обновлено: {formatDate(guild.lastUpdated)}
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
