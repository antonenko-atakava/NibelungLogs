"use client";

import Image from "next/image";
import { ClassBadge } from "@/components/wow/ClassBadge";
import { getClassColor, getClassColorWithOpacity } from "@/utils/wow/classColors";
import { getSpecIcon, getClassIcon } from "@/utils/wow/specIcons";
import type { PlayerExtendedDetailDto } from "@/types/api/Player";

interface PlayerHeaderProps {
  player: PlayerExtendedDetailDto;
}

export function PlayerHeader({ player }: PlayerHeaderProps) {
  const classColor = getClassColor(player.className || player.characterClass);
  const classIcon = getClassIcon(player.className || player.characterClass);
  const bestSpec = player.specStatistics.length > 0
    ? player.specStatistics.reduce((best, current) => 
        current.averageDps > best.averageDps ? current : best
      )
    : null;
  const specIcon = bestSpec ? getSpecIcon(player.className || player.characterClass, bestSpec.specName) : null;

  return (
    <div className="relative overflow-hidden rounded-2xl border border-border/40 bg-card shadow-lg">
      <div
        className="absolute inset-0 opacity-10"
        style={{ backgroundColor: getClassColorWithOpacity(player.className || player.characterClass, 0.3) }}
      />
      
      <div className="relative p-8">
        <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
          <div className="flex items-center gap-4">
            {specIcon && (
              <div className="relative">
                <Image
                  src={specIcon}
                  alt={bestSpec?.specName || "Spec"}
                  width={80}
                  height={80}
                  className="rounded-full border-2"
                  style={{ borderColor: classColor }}
                />
                {classIcon && (
                  <div className="absolute -bottom-1 -right-1 rounded-full bg-background p-1 border-2 border-background">
                    <Image
                      src={classIcon}
                      alt={player.className || player.characterClass || "Class"}
                      width={24}
                      height={24}
                      className="rounded-full"
                    />
                  </div>
                )}
              </div>
            )}
            {!specIcon && classIcon && (
              <Image
                src={classIcon}
                alt={player.className || player.characterClass || "Class"}
                width={80}
                height={80}
                className="rounded-full border-2"
                style={{ borderColor: classColor }}
              />
            )}
          </div>

          <div className="flex-1">
            <div className="flex flex-col md:flex-row md:items-center gap-4 mb-4">
              <h1
                className="text-4xl md:text-5xl font-bold"
                style={{ color: classColor }}
              >
                {player.characterName}
              </h1>
              <div className="flex items-center gap-2">
                <ClassBadge className={player.className || player.characterClass} variant="outline" />
                {bestSpec && (
                  <span className="px-3 py-1 rounded-md text-sm font-medium bg-secondary/50 text-muted-foreground">
                    {bestSpec.specName}
                  </span>
                )}
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
              <span>{player.characterRace}</span>
              <span>•</span>
              <span>Уровень {player.characterLevel}</span>
              {player.currentItemLevel && (
                <>
                  <span>•</span>
                  <span>iLvl {player.currentItemLevel}</span>
                </>
              )}
            </div>
          </div>

          <div className="text-right">
            <div className="text-2xl font-bold text-primary mb-1">
              {Math.round(player.averageDps).toLocaleString("ru-RU")}
            </div>
            <div className="text-sm text-muted-foreground">Средний DPS</div>
            {player.maxDps > 0 && (
              <>
                <div className="text-lg font-semibold text-muted-foreground mt-2">
                  Макс: {Math.round(player.maxDps).toLocaleString("ru-RU")}
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
