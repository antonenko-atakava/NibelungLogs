"use client";

import { useState, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import type { PlayerFilters } from "@/types/players/PlayerFilters";
import { specIconMapByClassAndSpec } from "@/utils/wow/specIcons";

interface PlayerFiltersProps {
  filters: PlayerFilters;
  onFiltersChange: (filters: PlayerFilters) => void;
}

const classList = [
  "Воин",
  "Паладин",
  "Охотник",
  "Разбойник",
  "Жрец",
  "Рыцарь смерти",
  "Шаман",
  "Маг",
  "Чернокнижник",
  "Друид",
];

export function PlayerFiltersComponent({ filters, onFiltersChange }: PlayerFiltersProps) {
  const [localSearch, setLocalSearch] = useState(filters.search || "");
  const [localRole, setLocalRole] = useState(filters.role || "");
  const [localClass, setLocalClass] = useState(filters.characterClass || "");
  const [localSpec, setLocalSpec] = useState(filters.spec || "");

  const availableSpecs = localClass && specIconMapByClassAndSpec[localClass]
    ? Object.keys(specIconMapByClassAndSpec[localClass])
    : [];

  useEffect(() => {
    if (!localClass && localSpec) {
      setLocalSpec("");
    }
  }, [localClass, localSpec]);

  const handleSearchChange = (value: string) => {
    setLocalSearch(value);
  };

  const handleRoleChange = (value: string) => {
    setLocalRole(value);
    onFiltersChange({
      ...filters,
      role: value === "all" ? undefined : value,
      page: 1,
    });
  };

  const handleClassChange = (value: string) => {
    setLocalClass(value);
    setLocalSpec("");
    onFiltersChange({
      ...filters,
      characterClass: value === "all" ? undefined : value,
      spec: undefined,
      page: 1,
    });
  };

  const handleSpecChange = (value: string) => {
    setLocalSpec(value);
    onFiltersChange({
      ...filters,
      spec: value === "all" ? undefined : value,
      page: 1,
    });
  };

  const handleSearchSubmit = () => {
    onFiltersChange({
      ...filters,
      search: localSearch.trim() || undefined,
      page: 1,
    });
  };

  const handleClear = () => {
    setLocalSearch("");
    setLocalRole("");
    setLocalClass("");
    setLocalSpec("");
    onFiltersChange({
      search: undefined,
      role: undefined,
      characterClass: undefined,
      spec: undefined,
      page: 1,
      pageSize: filters.pageSize,
    });
  };

  return (
    <div className="flex flex-col gap-4 p-6 border border-border/40 rounded-2xl bg-card shadow-md">
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="flex-1">
          <Input
            placeholder="Поиск по имени игрока..."
            value={localSearch}
            onChange={(e) => handleSearchChange(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter")
                handleSearchSubmit();
            }}
            className="h-10"
          />
        </div>
        <Select value={localClass || "all"} onValueChange={handleClassChange}>
          <SelectTrigger className="w-full sm:w-[180px] h-10">
            <SelectValue placeholder="Класс" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Все классы</SelectItem>
            {classList.map((className) => (
              <SelectItem key={className} value={className}>
                {className}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select
          value={localSpec || "all"}
          onValueChange={handleSpecChange}
          disabled={!localClass}
        >
          <SelectTrigger className="w-full sm:w-[180px] h-10" disabled={!localClass}>
            <SelectValue placeholder="Специализация" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Все специализации</SelectItem>
            {availableSpecs.map((specName) => (
              <SelectItem key={specName} value={specName}>
                {specName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={localRole || "all"} onValueChange={handleRoleChange}>
          <SelectTrigger className="w-full sm:w-[180px] h-10">
            <SelectValue placeholder="Роль" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Все роли</SelectItem>
            <SelectItem value="0">ДД</SelectItem>
            <SelectItem value="1">Танк</SelectItem>
            <SelectItem value="2">Хил</SelectItem>
          </SelectContent>
        </Select>
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
