"use client";

import Image from "next/image";
import Link from "next/link";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ClassBadge } from "@/components/wow/ClassBadge";
import { RoleBadge } from "@/components/wow/RoleBadge";
import { getClassColor } from "@/utils/wow/classColors";
import { getSpecIcon, getClassIcon } from "@/utils/wow/specIcons";
import type { PlayerDto } from "@/types/api/Player";

interface PlayersTableProps {
  players: PlayerDto[];
  isLoading?: boolean;
}

export function PlayersTable({ players, isLoading }: PlayersTableProps) {
  if (isLoading)
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-muted-foreground">Загрузка...</div>
      </div>
    );

  if (players.length === 0)
    return (
      <div className="flex flex-col items-center justify-center p-8 border rounded-lg">
        <div className="text-muted-foreground text-lg mb-2">Игроки не найдены</div>
        <div className="text-sm text-muted-foreground">
          Попробуйте изменить фильтры или убедитесь, что в базе данных есть данные
        </div>
      </div>
    );

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU").format(value);
  };

  const formatDps = (value: number): string => {
    return formatNumber(Math.round(value));
  };

  return (
    <div className="border border-border/40 rounded-2xl overflow-hidden bg-card shadow-lg">
      <Table>
        <TableHeader>
          <TableRow className="bg-secondary/30 border-b border-border/30">
            <TableHead className="w-[60px] font-semibold text-xs uppercase tracking-wider text-muted-foreground">#</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Имя</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Класс</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Специализация</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Роль</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Средний DPS</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Макс. DPS</TableHead>
            <TableHead className="text-right font-semibold text-xs uppercase tracking-wider text-muted-foreground">Энкаунтеров</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {players.map((player, index) => (
            <TableRow
              key={player.encounterId ? `${player.id}-${player.encounterId}` : `${player.id}-${index}`}
              className="hover:bg-secondary/20 transition-colors border-b border-border/20"
            >
              <TableCell className="font-medium text-muted-foreground text-sm">
                {player.rank}
              </TableCell>
              <TableCell className="font-semibold text-base">
                <Link
                  href={`/players/${player.id}`}
                  className="flex items-center gap-2 hover:text-primary transition-colors"
                >
                  {(() => {
                    const specIcon = getSpecIcon(player.className || player.characterClass, player.specName);
                    const classIcon = specIcon || getClassIcon(player.className || player.characterClass);
                    return classIcon ? (
                      <Image
                        src={classIcon}
                        alt={player.specName || player.className || player.characterClass || "Class"}
                        width={20}
                        height={20}
                        className="object-contain"
                      />
                    ) : null;
                  })()}
                  <span style={{ color: getClassColor(player.className || player.characterClass) }}>
                    {player.characterName}
                  </span>
                </Link>
              </TableCell>
              <TableCell>
                <ClassBadge 
                  className={player.className || player.characterClass} 
                  variant="outline"
                />
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {player.specName || "-"}
              </TableCell>
              <TableCell>
                <RoleBadge role={player.role} />
              </TableCell>
              <TableCell className="text-right font-medium text-sm">
                {formatDps(player.averageDps)}
              </TableCell>
              <TableCell className="text-right font-semibold text-primary text-sm">
                {formatDps(player.maxDps)}
              </TableCell>
              <TableCell className="text-right text-sm text-muted-foreground">
                {player.totalEncounters}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
