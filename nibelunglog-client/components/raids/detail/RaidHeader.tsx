"use client";

import { Calendar, Users, Clock, Trophy } from "lucide-react";
import type { RaidDetailDto } from "@/types/api/Raid";

interface RaidHeaderProps {
  raid: RaidDetailDto;
}

export function RaidHeader({ raid }: RaidHeaderProps) {
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
    return new Date(dateString).toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "long",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const completionRate = raid.totalBosses > 0
    ? ((raid.completedBosses / raid.totalBosses) * 100).toFixed(0)
    : "0";

  return (
    <div className="relative overflow-hidden rounded-2xl border border-border/40 bg-card shadow-lg">
      <div className="absolute inset-0 opacity-10 bg-gradient-to-r from-[#C41F3B] via-[#F58CBA] to-[#FF7D0A]" />
      
      <div className="relative p-8">
        <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
          <div className="flex-1">
            <div className="flex flex-col md:flex-row md:items-center gap-4 mb-4">
              <h1 className="text-4xl md:text-5xl font-bold tracking-tight">
                {raid.raidTypeName}
              </h1>
            </div>

            <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground mb-4">
              <div className="flex items-center gap-2">
                <Users className="size-4" />
                <span>{raid.guildName}</span>
              </div>
              <span>•</span>
              <div className="flex items-center gap-2">
                <Trophy className="size-4" />
                <span>Лидер: {raid.leaderName}</span>
              </div>
              <span>•</span>
              <div className="flex items-center gap-2">
                <Calendar className="size-4" />
                <span>{formatDate(raid.startTime)}</span>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-6 text-sm">
              <div className="flex items-center gap-2">
                <Clock className="size-4 text-muted-foreground" />
                <span className="font-medium">Длительность: {formatTime(raid.totalTime)}</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-muted-foreground">Боссы:</span>
                <span className="font-medium text-green-500">{raid.completedBosses}</span>
                <span className="text-muted-foreground">/</span>
                <span className="font-medium">{raid.totalBosses}</span>
                <span className="text-muted-foreground">({completionRate}%)</span>
              </div>
              {raid.wipes > 0 && (
                <div className="flex items-center gap-2">
                  <span className="text-muted-foreground">Вайпы:</span>
                  <span className="font-medium text-red-500">{raid.wipes}</span>
                </div>
              )}
            </div>
          </div>

          <div className="text-right">
            <div className="text-2xl font-bold text-primary mb-1">
              {raid.encounters.length}
            </div>
            <div className="text-sm text-muted-foreground">Энкаунтеров</div>
          </div>
        </div>
      </div>
    </div>
  );
}
