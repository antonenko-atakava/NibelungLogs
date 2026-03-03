"use client";

import { useState, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { raidTypesApi } from "@/utils/api/raidTypesApi";
import type { RaidFilters } from "@/types/raids/RaidFilters";
import type { RaidTypeDto } from "@/types/api/RaidType";

interface RaidFiltersProps {
  filters: RaidFilters;
  onFiltersChange: (filters: RaidFilters) => void;
}

export function RaidFiltersComponent({ filters, onFiltersChange }: RaidFiltersProps) {
  const [localGuildName, setLocalGuildName] = useState(filters.guildName || "");
  const [localLeaderName, setLocalLeaderName] = useState(filters.leaderName || "");
  const [raidTypes, setRaidTypes] = useState<RaidTypeDto[]>([]);
  const [isLoadingTypes, setIsLoadingTypes] = useState(true);

  useEffect(() => {
    const fetchRaidTypes = async () => {
      try {
        const types = await raidTypesApi.getRaidTypes();
        setRaidTypes(types);
      } catch (error) {
        console.error("Failed to load raid types:", error);
        setRaidTypes([]);
      } finally {
        setIsLoadingTypes(false);
      }
    };

    fetchRaidTypes();
  }, []);

  const handleRaidTypeChange = (value: string) => {
    if (value === "all") {
      onFiltersChange({
        ...filters,
        raidTypeId: undefined,
        raidTypeName: undefined,
        page: 1,
      });
    } else {
      const raidType = raidTypes.find((rt) => rt.id.toString() === value);
      if (raidType) {
        onFiltersChange({
          ...filters,
          raidTypeId: raidType.id,
          raidTypeName: undefined,
          page: 1,
        });
      }
    }
  };

  const handleGuildNameChange = (value: string) => {
    setLocalGuildName(value);
  };

  const handleLeaderNameChange = (value: string) => {
    setLocalLeaderName(value);
  };

  const handleSearchSubmit = () => {
    onFiltersChange({
      ...filters,
      guildName: localGuildName.trim() || undefined,
      leaderName: localLeaderName.trim() || undefined,
      page: 1,
    });
  };

  const handleClear = () => {
    setLocalGuildName("");
    setLocalLeaderName("");
    onFiltersChange({
      raidTypeId: undefined,
      raidTypeName: undefined,
      guildName: undefined,
      leaderName: undefined,
      page: 1,
      pageSize: filters.pageSize,
    });
  };

  const selectedRaidTypeId = filters.raidTypeId?.toString() || "all";

  return (
    <div className="flex flex-col gap-4 p-6 border border-border/40 rounded-2xl bg-card shadow-md">
      <div className="flex flex-col sm:flex-row gap-3">
        <Select
          value={selectedRaidTypeId}
          onValueChange={handleRaidTypeChange}
          disabled={isLoadingTypes}
        >
          <SelectTrigger className="w-full sm:w-[200px] h-10">
            <SelectValue placeholder="Тип рейда" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Все рейды</SelectItem>
            {raidTypes.map((raidType) => (
              <SelectItem key={raidType.id} value={raidType.id.toString()}>
                {raidType.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <div className="flex-1">
          <Input
            placeholder="Название гильдии..."
            value={localGuildName}
            onChange={(e) => handleGuildNameChange(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter")
                handleSearchSubmit();
            }}
            className="h-10"
          />
        </div>
        <div className="flex-1">
          <Input
            placeholder="Имя лидера..."
            value={localLeaderName}
            onChange={(e) => handleLeaderNameChange(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter")
                handleSearchSubmit();
            }}
            className="h-10"
          />
        </div>
        <Button onClick={handleSearchSubmit} variant="default" className="h-10">
          Поиск
        </Button>
        <Button onClick={handleClear} variant="outline" className="h-10">
          Сбросить
        </Button>
      </div>
    </div>
  );
}
