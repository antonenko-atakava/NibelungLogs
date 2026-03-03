"use client";

import { useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { ClassBadge } from "@/components/wow/ClassBadge";
import { RoleBadge } from "@/components/wow/RoleBadge";
import { getClassColor } from "@/utils/wow/classColors";
import { getSpecIcon, getClassIcon } from "@/utils/wow/specIcons";
import type { PlayerDto } from "@/types/api/Player";

interface PlayersTableProps {
  players: PlayerDto[];
  isLoading?: boolean;
  onSortChange?: (sortField: string | null, sortDirection: "asc" | "desc") => void;
}

type SortField = "averageDps" | "maxDps" | "averageHps" | "maxHps" | "totalEncounters" | null;
type SortDirection = "asc" | "desc";

export function PlayersTable({ players, isLoading, onSortChange }: PlayersTableProps) {
  const [sortField, setSortField] = useState<SortField>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");

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

  const formatHps = (value: number | null): string => {
    if (value === null || value === undefined)
      return "-";
    return formatNumber(Math.round(value));
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

  return (
    <Table>
        <TableHeader>
          <TableRow className="bg-secondary/30 border-b border-border/30">
            <TableHead className="w-[80px] px-6 font-semibold text-xs uppercase tracking-wider text-muted-foreground">#</TableHead>
            <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Имя</TableHead>
            <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Класс</TableHead>
            <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Специализация</TableHead>
            <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Роль</TableHead>
            <TableHead className="text-center">
              <SortButton field="averageDps">Средний DPS</SortButton>
            </TableHead>
            <TableHead className="text-center">
              <SortButton field="maxDps">Макс. DPS</SortButton>
            </TableHead>
            <TableHead className="text-center">
              <SortButton field="averageHps">Средний HPS</SortButton>
            </TableHead>
            <TableHead className="text-center">
              <SortButton field="maxHps">Макс. HPS</SortButton>
            </TableHead>
            <TableHead className="text-center px-6">
              <SortButton field="totalEncounters">Энкаунтеров</SortButton>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {players.map((player, index) => (
            <TableRow
              key={player.encounterId ? `${player.id}-${player.encounterId}` : `${player.id}-${index}`}
              className="hover:bg-secondary/20 transition-colors border-b border-border/20 last:border-b-0"
            >
              <TableCell className="font-medium text-muted-foreground text-sm px-6">
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
              <TableCell className="text-center">
                <div className="flex justify-center">
                  <ClassBadge 
                    className={player.className || player.characterClass} 
                    variant="outline"
                  />
                </div>
              </TableCell>
              <TableCell className="text-center text-sm text-muted-foreground">
                {player.specName || "-"}
              </TableCell>
              <TableCell className="text-center">
                <div className="flex justify-center">
                  <RoleBadge role={player.role} />
                </div>
              </TableCell>
              <TableCell className="text-center font-medium text-sm">
                {formatDps(player.averageDps)}
              </TableCell>
              <TableCell className="text-center font-semibold text-primary text-sm">
                {formatDps(player.maxDps)}
              </TableCell>
              <TableCell className="text-center font-medium text-sm">
                {formatHps(player.averageHps)}
              </TableCell>
              <TableCell className="text-center font-semibold text-primary text-sm">
                {formatHps(player.maxHps)}
              </TableCell>
              <TableCell className="text-center text-sm text-muted-foreground px-6">
                {player.totalEncounters}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
  );
}
