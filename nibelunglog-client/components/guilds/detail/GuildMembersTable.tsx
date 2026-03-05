"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import Image from "next/image";
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
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { PlayersPagination } from "@/components/tables/players/PlayersPagination";
import { useGuildMemberFiltersStore } from "@/stores/guildMemberFiltersStore";
import { getClassId, getSpecId } from "@/utils/wow/classMappings";
import type { GuildMemberDto, PagedResult } from "@/types/api/Guild";

interface GuildMembersTableProps {
  guildId: number;
}

type SortField = "characterName" | "rank" | "totalEncounters" | "averageDps" | "maxDps" | "averageHps" | "maxHps" | null;
type SortDirection = "asc" | "desc";

export function GuildMembersTable({ guildId }: GuildMembersTableProps) {
  const { filters, updateFilter } = useGuildMemberFiltersStore();
  const [sortField, setSortField] = useState<SortField>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");
  const [data, setData] = useState<PagedResult<GuildMemberDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchMembers = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const classId = filters.characterClass ? getClassId(filters.characterClass) : undefined;
        const specId = filters.characterClass && filters.spec ? getSpecId(filters.characterClass, filters.spec) : undefined;
        
        const result = await guildsApi.getGuildMembers(guildId, {
          search: filters.search,
          role: filters.role,
          characterClass: classId,
          spec: specId,
          itemLevelMin: filters.itemLevelMin,
          itemLevelMax: filters.itemLevelMax,
          sortField: filters.sortField || sortField || undefined,
          sortDirection: filters.sortDirection || sortDirection,
          page: filters.page,
          pageSize: filters.pageSize,
        });
        setData(result);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setData(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchMembers();
  }, [guildId, filters, sortField, sortDirection]);

  if (isLoading)
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-muted-foreground">Загрузка...</div>
      </div>
    );

  if (error)
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-destructive">{error}</div>
      </div>
    );

  if (!data || data.items.length === 0)
    return (
      <div className="flex flex-col items-center justify-center p-8 border rounded-lg">
        <div className="text-muted-foreground text-lg mb-2">Участники не найдены</div>
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
    updateFilter("sortField", field || undefined);
    updateFilter("sortDirection", newDirection);
    updateFilter("page", 1);
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
    <div className="border border-border/40 rounded-2xl overflow-hidden bg-card shadow-lg">
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
              <SortButton field="totalEncounters">Энкаунтеров</SortButton>
            </TableHead>
            <TableHead className="text-center">
              <SortButton field="averageHps">Средний HPS</SortButton>
            </TableHead>
            <TableHead className="text-center">
              <SortButton field="maxHps">Макс. HPS</SortButton>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.items.map((member, index) => {
            const specIcon = getSpecIcon(member.className || member.characterClass, null);
            const classIcon = specIcon || getClassIcon(member.className || member.characterClass);
            
            return (
              <TableRow
                key={member.playerId}
                className="hover:bg-secondary/20 transition-colors border-b border-border/20 last:border-b-0"
              >
                <TableCell className="font-medium text-muted-foreground text-sm px-6">
                  {(data.page - 1) * data.pageSize + index + 1}
                </TableCell>
                <TableCell className="font-semibold text-base">
                  <Link
                    href={`/players/${member.playerId}`}
                    className="flex items-center gap-2 hover:text-primary transition-colors"
                  >
                    {classIcon && (
                      <Image
                        src={classIcon}
                        alt={member.className || member.characterClass || "Class"}
                        width={20}
                        height={20}
                        className="object-contain"
                      />
                    )}
                    <span style={{ color: getClassColor(member.className || member.characterClass) }}>
                      {member.characterName}
                    </span>
                  </Link>
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex justify-center">
                    <ClassBadge
                      className={member.className || member.characterClass}
                      variant="outline"
                    />
                  </div>
                </TableCell>
                <TableCell className="text-center text-sm text-muted-foreground">
                  {member.specName || "-"}
                </TableCell>
                <TableCell className="text-center">
                  <div className="flex justify-center">
                    <RoleBadge role={member.role} />
                  </div>
                </TableCell>
                <TableCell className="text-center font-medium text-sm">
                  {formatDps(member.averageDps)}
                </TableCell>
                <TableCell className="text-center font-semibold text-primary text-sm">
                  {formatDps(member.maxDps)}
                </TableCell>
                <TableCell className="text-center text-sm text-muted-foreground px-6">
                  {member.totalEncounters}
                </TableCell>
                <TableCell className="text-center font-medium text-sm">
                  {formatHps(member.averageHps)}
                </TableCell>
                <TableCell className="text-center font-semibold text-primary text-sm">
                  {formatHps(member.maxHps)}
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
      {data && (
        <PlayersPagination
          currentPage={data.page}
          totalPages={data.totalPages}
          totalCount={data.totalCount}
          pageSize={data.pageSize}
          onPageChange={(page) => updateFilter("page", page)}
        />
      )}
    </div>
  );
}
