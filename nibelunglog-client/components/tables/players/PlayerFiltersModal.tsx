"use client";

import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { usePlayerFiltersStore } from "@/stores/playerFiltersStore";
import { specIconMapByClassAndSpec } from "@/utils/wow/specIcons";
import { raceList, factionByRaceName, getRaceId } from "@/utils/wow/raceMappings";

interface PlayerFiltersModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
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

const factionList = [
  "Альянс",
  "Орда",
];

export function PlayerFiltersModal({ open, onOpenChange }: PlayerFiltersModalProps) {
  const { filters, updateFilter, resetFilters } = usePlayerFiltersStore();
  const [localRole, setLocalRole] = useState(filters.role || "");
  const [localClass, setLocalClass] = useState(filters.characterClass || "");
  const [localSpec, setLocalSpec] = useState(filters.spec || "");
  const [localItemLevelMin, setLocalItemLevelMin] = useState(filters.itemLevelMin?.toString() || "");
  const [localItemLevelMax, setLocalItemLevelMax] = useState(filters.itemLevelMax?.toString() || "");
  const [localRace, setLocalRace] = useState(filters.race || "");
  const [localFaction, setLocalFaction] = useState(filters.faction || "");

  useEffect(() => {
    if (open) {
      setLocalRole(filters.role || "");
      setLocalClass(filters.characterClass || "");
      setLocalSpec(filters.spec || "");
      setLocalItemLevelMin(filters.itemLevelMin?.toString() || "");
      setLocalItemLevelMax(filters.itemLevelMax?.toString() || "");
      setLocalRace(filters.race || "");
      setLocalFaction(filters.faction || "");
    }
  }, [open, filters]);

  const availableSpecs = localClass && specIconMapByClassAndSpec[localClass]
    ? Object.keys(specIconMapByClassAndSpec[localClass])
    : [];

  useEffect(() => {
    if (!localClass && localSpec) {
      setLocalSpec("");
    }
  }, [localClass, localSpec]);

  useEffect(() => {
    if (localFaction && localFaction !== "all") {
      const racesForFaction = raceList.filter(race => factionByRaceName[race] === localFaction);
      if (localRace && !racesForFaction.includes(localRace)) {
        setLocalRace("");
      }
    }
  }, [localFaction, localRace]);

  const availableRaces = localFaction && localFaction !== "all"
    ? raceList.filter(race => factionByRaceName[race] === localFaction)
    : raceList;

  const handleApply = () => {
    updateFilter("role", localRole === "all" ? undefined : localRole || undefined);
    updateFilter("characterClass", localClass === "all" ? undefined : localClass || undefined);
    updateFilter("spec", localSpec === "all" ? undefined : localSpec || undefined);
    updateFilter("itemLevelMin", localItemLevelMin ? parseFloat(localItemLevelMin) : undefined);
    updateFilter("itemLevelMax", localItemLevelMax ? parseFloat(localItemLevelMax) : undefined);
    
    if (localFaction && localFaction !== "all") {
      updateFilter("faction", localFaction);
      if (localRace && localRace !== "all") {
        const raceId = getRaceId(localRace);
        updateFilter("race", raceId || undefined);
      } else {
        updateFilter("race", undefined);
      }
    } else {
      updateFilter("faction", undefined);
      const raceId = localRace && localRace !== "all" ? getRaceId(localRace) : undefined;
      updateFilter("race", raceId || undefined);
    }
    
    updateFilter("page", 1);
    onOpenChange(false);
  };

  const handleClear = () => {
    setLocalRole("");
    setLocalClass("");
    setLocalSpec("");
    setLocalItemLevelMin("");
    setLocalItemLevelMax("");
    setLocalRace("");
    setLocalFaction("");
    resetFilters();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Фильтры</DialogTitle>
        </DialogHeader>
        <div className="flex flex-col gap-4 py-4">
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium">Класс</label>
            <Select value={localClass || "all"} onValueChange={setLocalClass}>
              <SelectTrigger>
                <SelectValue placeholder="Выберите класс" />
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
          </div>
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium">Специализация</label>
            <Select
              value={localSpec || "all"}
              onValueChange={setLocalSpec}
              disabled={!localClass}
            >
              <SelectTrigger disabled={!localClass}>
                <SelectValue placeholder="Выберите специализацию" />
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
          </div>
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium">Роль</label>
            <Select value={localRole || "all"} onValueChange={setLocalRole}>
              <SelectTrigger>
                <SelectValue placeholder="Выберите роль" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все роли</SelectItem>
                <SelectItem value="0">ДД</SelectItem>
                <SelectItem value="1">Танк</SelectItem>
                <SelectItem value="2">Хил</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium">Илвл от</label>
              <Input
                type="number"
                placeholder="Минимум"
                value={localItemLevelMin}
                onChange={(e) => setLocalItemLevelMin(e.target.value)}
              />
            </div>
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium">Илвл до</label>
              <Input
                type="number"
                placeholder="Максимум"
                value={localItemLevelMax}
                onChange={(e) => setLocalItemLevelMax(e.target.value)}
              />
            </div>
          </div>
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium">Фракция</label>
            <Select value={localFaction || "all"} onValueChange={setLocalFaction}>
              <SelectTrigger>
                <SelectValue placeholder="Выберите фракцию" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все фракции</SelectItem>
                {factionList.map((faction) => (
                  <SelectItem key={faction} value={faction}>
                    {faction}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium">Раса</label>
            <Select value={localRace || "all"} onValueChange={setLocalRace} disabled={localFaction && localFaction !== "all" && availableRaces.length === 0}>
              <SelectTrigger disabled={localFaction && localFaction !== "all" && availableRaces.length === 0}>
                <SelectValue placeholder="Выберите расу" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Все расы</SelectItem>
                {availableRaces.map((race) => (
                  <SelectItem key={race} value={race}>
                    {race}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={handleClear}>
            Сбросить
          </Button>
          <Button onClick={handleApply}>
            Применить
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
