"use client";

import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Slider } from "@/components/ui/slider";
import { useGuildMemberFiltersStore } from "@/stores/guildMemberFiltersStore";
import { specIconMapByClassAndSpec } from "@/utils/wow/specIcons";

interface GuildMemberFiltersModalProps {
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

const MIN_ILVL = 0;
const MAX_ILVL = 300;

export function GuildMemberFiltersModal({ open, onOpenChange }: GuildMemberFiltersModalProps) {
  const { filters, updateFilter, resetFilters } = useGuildMemberFiltersStore();
  const [localRole, setLocalRole] = useState(filters.role || "");
  const [localClass, setLocalClass] = useState(filters.characterClass || "");
  const [localSpec, setLocalSpec] = useState(filters.spec || "");
  const [localItemLevelRange, setLocalItemLevelRange] = useState<number[]>([
    filters.itemLevelMin ?? MIN_ILVL,
    filters.itemLevelMax ?? MAX_ILVL,
  ]);

  useEffect(() => {
    if (open) {
      setLocalRole(filters.role || "");
      setLocalClass(filters.characterClass || "");
      setLocalSpec(filters.spec || "");
      setLocalItemLevelRange([
        filters.itemLevelMin ?? MIN_ILVL,
        filters.itemLevelMax ?? MAX_ILVL,
      ]);
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

  const handleApply = () => {
    updateFilter("role", localRole === "all" ? undefined : localRole || undefined);
    updateFilter("characterClass", localClass === "all" ? undefined : localClass || undefined);
    updateFilter("spec", localSpec === "all" ? undefined : localSpec || undefined);
    updateFilter("itemLevelMin", localItemLevelRange[0] !== MIN_ILVL ? localItemLevelRange[0] : undefined);
    updateFilter("itemLevelMax", localItemLevelRange[1] !== MAX_ILVL ? localItemLevelRange[1] : undefined);
    updateFilter("page", 1);
    onOpenChange(false);
  };

  const handleClear = () => {
    setLocalRole("");
    setLocalClass("");
    setLocalSpec("");
    setLocalItemLevelRange([MIN_ILVL, MAX_ILVL]);
    resetFilters();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] overflow-hidden">
        <DialogHeader className="pb-2">
          <DialogTitle>Фильтры участников</DialogTitle>
        </DialogHeader>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Класс</label>
            <Select value={localClass || "all"} onValueChange={setLocalClass}>
              <SelectTrigger className="h-10 w-full">
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
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Специализация</label>
            <Select
              value={localSpec || "all"}
              onValueChange={setLocalSpec}
              disabled={!localClass}
            >
              <SelectTrigger className="h-10 w-full" disabled={!localClass}>
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
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium">Роль</label>
            <Select value={localRole || "all"} onValueChange={setLocalRole}>
              <SelectTrigger className="h-10 w-full">
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
          <div className="flex flex-col gap-1">
            <div className="flex items-center justify-between mb-1">
              <label className="text-sm font-medium">Илвл</label>
              <span className="text-sm text-muted-foreground">
                {localItemLevelRange[0]} - {localItemLevelRange[1]}
              </span>
            </div>
            <div className="pt-[22px]">
              <Slider
                value={localItemLevelRange}
                onValueChange={setLocalItemLevelRange}
                min={MIN_ILVL}
                max={MAX_ILVL}
                step={1}
                className="w-full"
              />
            </div>
          </div>
        </div>
        <DialogFooter className="pt-3 mt-3 border-t border-border/40 gap-2 justify-between">
          <Button variant="destructive" onClick={handleClear} className="w-full sm:w-auto">
            Сбросить
          </Button>
          <Button variant="outline" onClick={handleApply} className="w-full sm:w-auto bg-white text-black hover:bg-white/90 hover:text-black border-border/60">
            Применить
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
