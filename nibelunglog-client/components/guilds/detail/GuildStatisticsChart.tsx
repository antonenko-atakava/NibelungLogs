"use client";

import { useState, useEffect, useMemo, useRef, useCallback } from "react";
import Link from "next/link";
import Image from "next/image";
import { ArrowLeft, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
} from "chart.js";
import { Doughnut } from "react-chartjs-2";
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { ClassBadge } from "@/components/wow/ClassBadge";
import { RoleBadge } from "@/components/wow/RoleBadge";
import { getClassColor, getClassColorWithOpacity } from "@/utils/wow/classColors";
import { getSpecIcon, getClassIcon } from "@/utils/wow/specIcons";
import { getClassId, getSpecId } from "@/utils/wow/classMappings";
import { PlayersPagination } from "@/components/tables/players/PlayersPagination";
import type { GuildStatisticsDto, GuildMemberDto, PagedResult } from "@/types/api/Guild";

ChartJS.register(ArcElement, Tooltip, Legend);

interface GuildStatisticsChartProps {
  guildId: number;
}

type SortField = "characterName" | "totalEncounters" | "averageDps" | "maxDps" | "averageHps" | "maxHps" | null;
type SortDirection = "asc" | "desc";

export function GuildStatisticsChart({ guildId }: GuildStatisticsChartProps) {
  const [statistics, setStatistics] = useState<GuildStatisticsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<"classes" | "specs" | "roles">("classes");
  const [showPlayers, setShowPlayers] = useState(false);
  const [selectedClassName, setSelectedClassName] = useState<string | undefined>(undefined);
  const [selectedSpecName, setSelectedSpecName] = useState<string | undefined>(undefined);
  const [selectedRole, setSelectedRole] = useState<string | undefined>(undefined);
  const [playersData, setPlayersData] = useState<PagedResult<GuildMemberDto> | null>(null);
  const [playersLoading, setPlayersLoading] = useState(false);
  const [playersError, setPlayersError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [sortField, setSortField] = useState<SortField>("characterName");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const chartRef = useRef<any>(null);

  useEffect(() => {
    const fetchStatistics = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await guildsApi.getGuildStatistics(guildId);
        setStatistics(result);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setStatistics(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchStatistics();
  }, [guildId]);

  const formatPercentage = (value: number): string => {
    return `${value.toFixed(1)}%`;
  };

  const classChartData = useMemo(() => {
    if (!statistics || statistics.classes.length === 0)
      return null;

    const labels = statistics.classes.map(c => c.className);
    const data = statistics.classes.map(c => c.count);
    const colors = statistics.classes.map(c => getClassColorWithOpacity(c.className, 0.7));
    const borderColors = statistics.classes.map(c => getClassColor(c.className));

    return {
      labels,
      datasets: [
        {
          data,
          backgroundColor: colors,
          borderColor: borderColors,
          borderWidth: 2,
        },
      ],
    };
  }, [statistics]);

  const specChartData = useMemo(() => {
    if (!statistics || statistics.specs.length === 0)
      return null;

    const labels = statistics.specs.map(s => s.specName);
    const data = statistics.specs.map(s => s.count);
    const colors = statistics.specs.map(s => getClassColorWithOpacity(s.className, 0.7));
    const borderColors = statistics.specs.map(s => getClassColor(s.className));

    return {
      labels,
      datasets: [
        {
          data,
          backgroundColor: colors,
          borderColor: borderColors,
          borderWidth: 2,
        },
      ],
    };
  }, [statistics]);

  const roleChartData = useMemo(() => {
    if (!statistics || statistics.roles.length === 0)
      return null;

    const labels = statistics.roles.map(r => r.roleName);
    const data = statistics.roles.map(r => r.count);
    const roleColors: Record<string, string> = {
      "ДД": "rgba(196, 30, 58, 0.7)",
      "Танк": "rgba(0, 112, 221, 0.7)",
      "Хил": "rgba(0, 255, 150, 0.7)",
    };
    const roleBorderColors: Record<string, string> = {
      "ДД": "rgb(196, 30, 58)",
      "Танк": "rgb(0, 112, 221)",
      "Хил": "rgb(0, 255, 150)",
    };
    const colors = statistics.roles.map(r => roleColors[r.roleName] || "rgba(128, 128, 128, 0.7)");
    const borderColors = statistics.roles.map(r => roleBorderColors[r.roleName] || "rgb(128, 128, 128)");

    return {
      labels,
      datasets: [
        {
          data,
          backgroundColor: colors,
          borderColor: borderColors,
          borderWidth: 2,
        },
      ],
    };
  }, [statistics]);

  useEffect(() => {
    if (!showPlayers)
      return;

    const fetchPlayers = async () => {
      setPlayersLoading(true);
      setPlayersError(null);

      try {
        const classId = selectedClassName ? getClassId(selectedClassName) : undefined;

        const result = await guildsApi.getGuildMembers(guildId, {
          characterClass: classId,
          spec: selectedSpecName,
          role: selectedRole,
          page: page,
          pageSize: 25,
          sortField: sortField || undefined,
          sortDirection: sortDirection,
        });
        setPlayersData(result);
        setPlayersError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setPlayersError(errorMessage);
        setPlayersData(null);
      } finally {
        setPlayersLoading(false);
      }
    };

    fetchPlayers();
  }, [showPlayers, guildId, selectedClassName, selectedSpecName, selectedRole, page, sortField, sortDirection]);

  const handleClassClick = useCallback((className: string) => {
    setSelectedClassName(className);
    setSelectedSpecName(undefined);
    setSelectedRole(undefined);
    setPage(1);
    setSortField("characterName");
    setSortDirection("asc");
    setShowPlayers(true);
  }, []);

  const handleSpecClick = useCallback((className: string, specName: string) => {
    setSelectedClassName(className);
    setSelectedSpecName(specName);
    setSelectedRole(undefined);
    setPage(1);
    setSortField("characterName");
    setSortDirection("asc");
    setShowPlayers(true);
  }, []);

  const handleRoleClick = useCallback((role: string) => {
    setSelectedClassName(undefined);
    setSelectedSpecName(undefined);
    setSelectedRole(role);
    setPage(1);
    setSortField("characterName");
    setSortDirection("asc");
    setShowPlayers(true);
  }, []);

  const handleBack = () => {
    setShowPlayers(false);
    setSelectedClassName(undefined);
    setSelectedSpecName(undefined);
    setSelectedRole(undefined);
    setPage(1);
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
    setPage(1);
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

  const getPlayersTitle = (): string => {
    if (selectedSpecName && selectedClassName)
      return `${selectedClassName} - ${selectedSpecName}`;
    if (selectedClassName)
      return selectedClassName;
    if (selectedRole) {
      const roleNames: Record<string, string> = {
        "0": "ДД",
        "1": "Танк",
        "2": "Хил",
      };
      return roleNames[selectedRole] || selectedRole;
    }
    return "Игроки";
  };

  const classChartOptions = useMemo(() => {
    const handleClick = (event: any, elements: any[]) => {
      if (elements.length > 0 && statistics) {
        const elementIndex = elements[0].index;
        const className = statistics.classes[elementIndex]?.className;
        if (className)
          handleClassClick(className);
      }
    };

    return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        backgroundColor: "rgba(0, 0, 0, 0.8)",
        titleColor: "rgba(255, 255, 255, 1)",
        bodyColor: "rgba(255, 255, 255, 0.9)",
        borderColor: "rgba(255, 255, 255, 0.2)",
        borderWidth: 1,
        callbacks: {
          label: function(context: any) {
            const label = context.label || "";
            const value = context.parsed || 0;
            const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : "0";
            return `${label}: ${formatNumber(value)} (${percentage}%)`;
          },
        },
      },
    },
      onClick: handleClick,
    };
  }, [statistics, handleClassClick, formatNumber]);

  const specChartOptions = useMemo(() => {
    const handleClick = (event: any, elements: any[]) => {
      if (elements.length > 0 && statistics) {
        const elementIndex = elements[0].index;
        const specStat = statistics.specs[elementIndex];
        if (specStat)
          handleSpecClick(specStat.className, specStat.specName);
      }
    };

    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          backgroundColor: "rgba(0, 0, 0, 0.8)",
          titleColor: "rgba(255, 255, 255, 1)",
          bodyColor: "rgba(255, 255, 255, 0.9)",
          borderColor: "rgba(255, 255, 255, 0.2)",
          borderWidth: 1,
          callbacks: {
            label: function(context: any) {
              const label = context.label || "";
              const value = context.parsed || 0;
              const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
              const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : "0";
              return `${label}: ${formatNumber(value)} (${percentage}%)`;
            },
          },
        },
      },
      onClick: handleClick,
    };
  }, [statistics, handleSpecClick, formatNumber]);

  const roleChartOptions = useMemo(() => {
    const handleClick = (event: any, elements: any[]) => {
      if (elements.length > 0 && statistics) {
        const elementIndex = elements[0].index;
        const role = statistics.roles[elementIndex]?.role;
        if (role)
          handleRoleClick(role);
      }
    };

    return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        backgroundColor: "rgba(0, 0, 0, 0.8)",
        titleColor: "rgba(255, 255, 255, 1)",
        bodyColor: "rgba(255, 255, 255, 0.9)",
        borderColor: "rgba(255, 255, 255, 0.2)",
        borderWidth: 1,
        callbacks: {
          label: function(context: any) {
            const label = context.label || "";
            const value = context.parsed || 0;
            const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : "0";
            return `${label}: ${formatNumber(value)} (${percentage}%)`;
          },
        },
      },
    },
      onClick: handleClick,
    };
  }, [statistics, handleRoleClick, formatNumber]);

  if (isLoading)
    return (
      <Card className="border-border/40 bg-card shadow-lg">
        <CardContent className="p-6">
          <div className="flex items-center justify-center h-64">
            <div className="text-muted-foreground">Загрузка...</div>
          </div>
        </CardContent>
      </Card>
    );

  if (error)
    return (
      <Card className="border-border/40 bg-card shadow-lg">
        <CardContent className="p-6">
          <div className="flex items-center justify-center h-64">
            <div className="text-destructive text-sm">{error}</div>
          </div>
        </CardContent>
      </Card>
    );

  if (!statistics)
    return null;

  if (showPlayers) {
    return (
      <Card className="border-border/40 bg-card shadow-lg">
        <CardHeader>
          <div className="flex items-center justify-between flex-wrap gap-3">
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="sm"
                onClick={handleBack}
                className="gap-2"
              >
                <ArrowLeft className="h-4 w-4" />
                Назад
              </Button>
              <CardTitle className="text-lg font-semibold">{getPlayersTitle()}</CardTitle>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {playersLoading && (
            <div className="flex items-center justify-center p-8">
              <div className="text-muted-foreground">Загрузка...</div>
            </div>
          )}
          {playersError && (
            <div className="flex items-center justify-center p-8">
              <div className="text-destructive">{playersError}</div>
            </div>
          )}
          {!playersLoading && !playersError && playersData && playersData.items.length > 0 && (
            <div className="space-y-4">
              <div className="border border-border/40 rounded-2xl overflow-hidden">
                <Table>
                  <TableHeader>
                    <TableRow className="bg-secondary/30 border-b border-border/30">
                      <TableHead className="w-[80px] px-6 font-semibold text-xs uppercase tracking-wider text-muted-foreground">#</TableHead>
                      <TableHead className="font-semibold text-xs uppercase tracking-wider text-muted-foreground">Имя</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Класс</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Специализация</TableHead>
                      <TableHead className="text-center font-semibold text-xs uppercase tracking-wider text-muted-foreground">Роль</TableHead>
                      <TableHead className="text-center">
                        <SortButton field="totalEncounters">Энкаунтеров</SortButton>
                      </TableHead>
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
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {playersData.items.map((member, index) => {
                      const specIcon = member.specName && member.className ? getSpecIcon(member.className, member.specName) : null;
                      const classIcon = specIcon || (member.className ? getClassIcon(member.className) : null);
                      
                      return (
                        <TableRow
                          key={member.playerId}
                          className="hover:bg-secondary/20 transition-colors border-b border-border/20 last:border-b-0"
                        >
                          <TableCell className="font-medium text-muted-foreground text-sm px-6">
                            {(playersData.page - 1) * playersData.pageSize + index + 1}
                          </TableCell>
                          <TableCell className="font-semibold text-base">
                            <Link
                              href={`/players/${member.playerId}`}
                              className="flex items-center gap-2 hover:text-primary transition-colors"
                            >
                              {classIcon && (
                                <div className="relative w-6 h-6">
                                  <Image
                                    src={classIcon}
                                    alt={member.className || member.characterClass}
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
                          <TableCell className="text-center">
                            {member.className ? (
                              <ClassBadge className={member.className} variant="outline" />
                            ) : (
                              <span className="text-sm text-muted-foreground">-</span>
                            )}
                          </TableCell>
                          <TableCell className="text-center">
                            {member.specName && member.className ? (
                              <div className="flex items-center justify-center gap-2">
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
                          <TableCell className="text-center">
                            {member.role ? (
                              <RoleBadge role={member.role} />
                            ) : (
                              <span className="text-sm text-muted-foreground">-</span>
                            )}
                          </TableCell>
                          <TableCell className="text-center">
                            <span className="text-sm">{formatNumber(member.totalEncounters)}</span>
                          </TableCell>
                          <TableCell className="text-center">
                            <span className="text-sm">{formatDps(member.averageDps)}</span>
                          </TableCell>
                          <TableCell className="text-center">
                            <span className="text-sm font-semibold">{formatDps(member.maxDps)}</span>
                          </TableCell>
                          <TableCell className="text-center">
                            <span className="text-sm">{formatHps(member.averageHps)}</span>
                          </TableCell>
                          <TableCell className="text-center">
                            <span className="text-sm font-semibold">{formatHps(member.maxHps)}</span>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </div>
              {playersData.totalPages > 1 && (
                <PlayersPagination
                  currentPage={playersData.page}
                  totalPages={playersData.totalPages}
                  onPageChange={setPage}
                />
              )}
            </div>
          )}
          {!playersLoading && !playersError && playersData && playersData.items.length === 0 && (
            <div className="flex flex-col items-center justify-center p-8">
              <div className="text-muted-foreground text-lg mb-2">Игроки не найдены</div>
            </div>
          )}
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="border-border/40 bg-card shadow-lg">
      <CardHeader>
        <div className="flex items-center justify-between flex-wrap gap-3">
          <CardTitle className="text-lg font-semibold">Статистика гильдии</CardTitle>
          <div className="flex gap-2">
            <button
              onClick={() => setActiveTab("classes")}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                activeTab === "classes"
                  ? "bg-primary text-primary-foreground"
                  : "bg-secondary text-secondary-foreground hover:bg-secondary/80"
              }`}
            >
              Классы
            </button>
            <button
              onClick={() => setActiveTab("specs")}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                activeTab === "specs"
                  ? "bg-primary text-primary-foreground"
                  : "bg-secondary text-secondary-foreground hover:bg-secondary/80"
              }`}
            >
              Спеки
            </button>
            <button
              onClick={() => setActiveTab("roles")}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                activeTab === "roles"
                  ? "bg-primary text-primary-foreground"
                  : "bg-secondary text-secondary-foreground hover:bg-secondary/80"
              }`}
            >
              Роли
            </button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="flex items-center justify-center">
            {activeTab === "classes" && classChartData && (
              <div className="h-[352px] w-full max-w-xl cursor-pointer">
                <Doughnut ref={chartRef} data={classChartData} options={classChartOptions} />
              </div>
            )}
            {activeTab === "specs" && specChartData && (
              <div className="h-[352px] w-full max-w-xl cursor-pointer">
                <Doughnut ref={chartRef} data={specChartData} options={specChartOptions} />
              </div>
            )}
            {activeTab === "roles" && roleChartData && (
              <div className="h-[352px] w-full max-w-xl cursor-pointer">
                <Doughnut ref={chartRef} data={roleChartData} options={roleChartOptions} />
              </div>
            )}
          </div>
          <div className="flex flex-col">
            {activeTab === "classes" && (
              <div className="space-y-2">
                {statistics.classes.map((classStat) => (
                  <div
                    key={classStat.className}
                    onClick={() => handleClassClick(classStat.className)}
                    className="flex items-center justify-between gap-3 p-2 rounded-md bg-secondary/30 hover:bg-secondary/50 cursor-pointer transition-colors"
                  >
                    <div className="flex items-center gap-2">
                      <ClassBadge className={classStat.className} variant="outline" />
                      <span className="text-sm font-medium">{classStat.className}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="text-sm text-muted-foreground">{formatNumber(classStat.count)}</span>
                      <span className="text-sm font-semibold text-primary w-16 text-right">{formatPercentage(classStat.percentage)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
            {activeTab === "specs" && (
              <div className="space-y-2 max-h-96 overflow-y-auto [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-secondary/50 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:hover:bg-secondary/70 [&::-webkit-scrollbar-thumb]:transition-colors">
                {statistics.specs.map((specStat) => (
                  <div
                    key={`${specStat.className}-${specStat.specName}`}
                    onClick={() => handleSpecClick(specStat.className, specStat.specName)}
                    className="flex items-center justify-between gap-3 p-2 rounded-md bg-secondary/30 hover:bg-secondary/50 cursor-pointer transition-colors"
                  >
                    <div className="flex items-center gap-2">
                      <ClassBadge className={specStat.className} variant="outline" />
                      <span className="text-sm font-medium">{specStat.specName}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="text-sm text-muted-foreground">{formatNumber(specStat.count)}</span>
                      <span className="text-sm font-semibold text-primary w-16 text-right">{formatPercentage(specStat.percentage)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
            {activeTab === "roles" && (
              <div className="space-y-2">
                {statistics.roles.map((roleStat) => (
                  <div
                    key={roleStat.role}
                    onClick={() => handleRoleClick(roleStat.role)}
                    className="flex items-center justify-between gap-3 p-2 rounded-md bg-secondary/30 hover:bg-secondary/50 cursor-pointer transition-colors"
                  >
                    <div className="flex items-center gap-2">
                      <RoleBadge role={roleStat.role} />
                      <span className="text-sm font-medium">{roleStat.roleName}</span>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="text-sm text-muted-foreground">{formatNumber(roleStat.count)}</span>
                      <span className="text-sm font-semibold text-primary w-16 text-right">{formatPercentage(roleStat.percentage)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
