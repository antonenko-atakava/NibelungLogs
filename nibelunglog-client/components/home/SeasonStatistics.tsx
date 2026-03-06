"use client";

import { useState, useEffect, useMemo } from "react";
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
import { seasonStatisticsApi } from "@/utils/api/seasonStatisticsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { getClassColor } from "@/utils/wow/classColors";
import { ClassBadge } from "@/components/wow/ClassBadge";
import type { SeasonClassStatisticsDto, SeasonSpecStatisticsDto } from "@/types/api/SeasonStatistics";

export function SeasonStatistics() {
  const [classStats, setClassStats] = useState<SeasonClassStatisticsDto[]>([]);
  const [specStats, setSpecStats] = useState<SeasonSpecStatisticsDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<"dps" | "hps">("dps");

  useEffect(() => {
    const fetchStatistics = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const [classes, specs] = await Promise.all([
          seasonStatisticsApi.getSeasonClassStatistics(),
          seasonStatisticsApi.getSeasonSpecStatistics(),
        ]);
        setClassStats(classes);
        setSpecStats(specs);
        setError(null);
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setClassStats([]);
        setSpecStats([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchStatistics();
  }, []);

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(value);
  };

  const season1ClassStats = useMemo(() => {
    return classStats
      .filter(s => s.season === 1)
      .filter(s => {
        if (activeTab === "dps")
          return s.averageDps > 0;
        return s.averageHps > 0;
      })
      .sort((a, b) => {
        const aValue = activeTab === "dps" ? a.averageDps : a.averageHps;
        const bValue = activeTab === "dps" ? b.averageDps : b.averageHps;
        return bValue - aValue;
      });
  }, [classStats, activeTab]);

  const season2ClassStats = useMemo(() => {
    return classStats
      .filter(s => s.season === 2)
      .filter(s => {
        if (activeTab === "dps")
          return s.averageDps > 0;
        return s.averageHps > 0;
      })
      .sort((a, b) => {
        const aValue = activeTab === "dps" ? a.averageDps : a.averageHps;
        const bValue = activeTab === "dps" ? b.averageDps : b.averageHps;
        return bValue - aValue;
      });
  }, [classStats, activeTab]);

  const season1SpecStats = useMemo(() => {
    return specStats
      .filter(s => s.season === 1)
      .filter(s => {
        if (activeTab === "dps")
          return s.averageDps > 0;
        return s.averageHps > 0;
      })
      .sort((a, b) => {
        const aValue = activeTab === "dps" ? a.averageDps : a.averageHps;
        const bValue = activeTab === "dps" ? b.averageDps : b.averageHps;
        return bValue - aValue;
      });
  }, [specStats, activeTab]);

  const season2SpecStats = useMemo(() => {
    return specStats
      .filter(s => s.season === 2)
      .filter(s => {
        if (activeTab === "dps")
          return s.averageDps > 0;
        return s.averageHps > 0;
      })
      .sort((a, b) => {
        const aValue = activeTab === "dps" ? a.averageDps : a.averageHps;
        const bValue = activeTab === "dps" ? b.averageDps : b.averageHps;
        return bValue - aValue;
      });
  }, [specStats, activeTab]);

  if (isLoading)
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-muted-foreground text-lg">Загрузка...</div>
      </div>
    );

  if (error)
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-destructive">{error}</div>
      </div>
    );

  return (
    <div className="space-y-8">
      <div className="text-center">
        <h1 className="text-4xl font-bold mb-2">Статистика по сезонам</h1>
        <p className="text-muted-foreground mb-4">
          Топ классов и спеков по {activeTab === "dps" ? "урону" : "исцелению"}
        </p>
        <div className="flex justify-center gap-2">
          <Button
            variant={activeTab === "dps" ? "default" : "outline"}
            onClick={() => setActiveTab("dps")}
          >
            DPS
          </Button>
          <Button
            variant={activeTab === "hps" ? "default" : "outline"}
            onClick={() => setActiveTab("hps")}
          >
            HPS
          </Button>
        </div>
      </div>

      {activeTab === "dps" && (
        <div className="space-y-6">
          <div className="grid md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Сезон 1 (17.12.2024 - 07.02.2025)</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Класс</TableHead>
                      <TableHead className="text-right">Средний DPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season1ClassStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.className}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageDps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Сезон 2 (с 07.02.2025)</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Класс</TableHead>
                      <TableHead className="text-right">Средний DPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season2ClassStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.className}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageDps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>

          <div className="grid md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Спеки - Сезон 1</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Спек</TableHead>
                      <TableHead className="text-right">Средний DPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season1SpecStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}-${stat.specName}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.specName}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageDps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Спеки - Сезон 2</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Спек</TableHead>
                      <TableHead className="text-right">Средний DPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season2SpecStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}-${stat.specName}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.specName}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageDps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </div>
      )}

      {activeTab === "hps" && (
        <div className="space-y-6">
          <div className="grid md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Сезон 1 (17.12.2024 - 07.02.2025)</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Класс</TableHead>
                      <TableHead className="text-right">Средний HPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season1ClassStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.className}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageHps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Сезон 2 (с 07.02.2025)</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Класс</TableHead>
                      <TableHead className="text-right">Средний HPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season2ClassStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.className}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageHps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>

          <div className="grid md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Спеки - Сезон 1</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Спек</TableHead>
                      <TableHead className="text-right">Средний HPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season1SpecStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}-${stat.specName}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.specName}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageHps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Спеки - Сезон 2</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Спек</TableHead>
                      <TableHead className="text-right">Средний HPS</TableHead>
                      <TableHead className="text-right">Игроков</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {season2SpecStats.map((stat, index) => (
                      <TableRow key={`${stat.season}-${stat.className}-${stat.specName}`}>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">#{index + 1}</span>
                            <ClassBadge className={stat.className} />
                            <span>{stat.specName}</span>
                          </div>
                        </TableCell>
                        <TableCell className="text-right font-medium">
                          {formatNumber(stat.averageHps)}
                        </TableCell>
                        <TableCell className="text-right text-muted-foreground">
                          {stat.totalPlayers}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
}
