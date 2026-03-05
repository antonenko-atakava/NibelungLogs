"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import Image from "next/image";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
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
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { getClassId, getSpecId } from "@/utils/wow/classMappings";
import { PlayersPagination } from "@/components/tables/players/PlayersPagination";
import type { GuildMemberDto, PagedResult } from "@/types/api/Guild";

interface GuildPlayersModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  guildId: number;
  className?: string;
  specName?: string;
  role?: string;
}

export function GuildPlayersModal({
  open,
  onOpenChange,
  guildId,
  className,
  specName,
  role,
}: GuildPlayersModalProps) {
  const [data, setData] = useState<PagedResult<GuildMemberDto> | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  useEffect(() => {
    if (!open) {
      setPage(1);
      return;
    }

    const fetchPlayers = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const classId = className ? getClassId(className) : undefined;
        const specId = className && specName ? getSpecId(className, specName) : undefined;

        const result = await guildsApi.getGuildMembers(guildId, {
          characterClass: classId,
          spec: specId,
          role: role,
          page: page,
          pageSize: 100,
          sortField: "characterName",
          sortDirection: "asc",
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

    fetchPlayers();
  }, [open, guildId, className, specName, role, page]);

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU").format(value);
  };

  const getTitle = (): string => {
    if (specName && className)
      return `${className} - ${specName}`;
    if (className)
      return className;
    if (role) {
      const roleNames: Record<string, string> = {
        "0": "ДД",
        "1": "Танк",
        "2": "Хил",
        "DPS": "ДД",
        "TANK": "Танк",
        "HEALER": "Хил",
      };
      return roleNames[role] || role;
    }
    return "Игроки";
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[80vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>{getTitle()}</DialogTitle>
        </DialogHeader>
        <div className="flex-1 overflow-y-auto [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-secondary/50 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:hover:bg-secondary/70 [&::-webkit-scrollbar-thumb]:transition-colors">
          {isLoading && (
            <div className="flex items-center justify-center p-8">
              <div className="text-muted-foreground">Загрузка...</div>
            </div>
          )}
          {error && (
            <div className="flex items-center justify-center p-8">
              <div className="text-destructive">{error}</div>
            </div>
          )}
          {!isLoading && !error && data && data.items.length > 0 && (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Игрок</TableHead>
                  <TableHead>Специализация</TableHead>
                  <TableHead>Роль</TableHead>
                  <TableHead className="text-right">Энкаунтеров</TableHead>
                  <TableHead className="text-right">Средний DPS</TableHead>
                  <TableHead className="text-right">Макс. DPS</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((member) => (
                  <TableRow key={member.playerId}>
                    <TableCell>
                      <Link
                        href={`/players/${member.playerId}`}
                        className="flex items-center gap-2 hover:text-primary transition-colors"
                      >
                        {member.className && (
                          <div className="relative w-6 h-6">
                            <Image
                              src={getClassIcon(member.className)}
                              alt={member.className}
                              width={24}
                              height={24}
                              className="rounded"
                            />
                          </div>
                        )}
                        <span className="font-medium" style={{ color: member.className ? getClassColor(member.className) : undefined }}>
                          {member.characterName}
                        </span>
                      </Link>
                    </TableCell>
                    <TableCell>
                      {member.specName && member.className ? (
                        <div className="flex items-center gap-2">
                          <div className="relative w-5 h-5">
                            <Image
                              src={getSpecIcon(member.className, member.specName)}
                              alt={member.specName}
                              width={20}
                              height={20}
                              className="rounded"
                            />
                          </div>
                          <span className="text-sm">{member.specName}</span>
                        </div>
                      ) : (
                        <span className="text-sm text-muted-foreground">-</span>
                      )}
                    </TableCell>
                    <TableCell>
                      {member.role ? (
                        <RoleBadge role={member.role} />
                      ) : (
                        <span className="text-sm text-muted-foreground">-</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <span className="text-sm">{formatNumber(member.totalEncounters)}</span>
                    </TableCell>
                    <TableCell className="text-right">
                      <span className="text-sm">{formatNumber(member.averageDps)}</span>
                    </TableCell>
                    <TableCell className="text-right">
                      <span className="text-sm font-semibold">{formatNumber(member.maxDps)}</span>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
          {!isLoading && !error && data && data.items.length === 0 && (
            <div className="flex flex-col items-center justify-center p-8">
              <div className="text-muted-foreground text-lg mb-2">Игроки не найдены</div>
            </div>
          )}
        </div>
        {!isLoading && !error && data && data.totalPages > 1 && (
          <div className="border-t border-border/40 pt-4">
            <PlayersPagination
              currentPage={data.page}
              totalPages={data.totalPages}
              onPageChange={setPage}
            />
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
