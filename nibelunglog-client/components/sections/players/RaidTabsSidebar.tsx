'use client';

import { useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useSearchParams } from 'next/navigation';
import { getEncounterImage } from '@/lib/encounterImages';
import { ChevronDown, ChevronRight } from 'lucide-react';

interface Encounter {
  encounterEntry: string;
  encounterName: string | null;
}

interface RaidGroup {
  raidTypeName: string;
  encounters: Encounter[];
}

interface RaidTabsSidebarProps {
  raidGroups: RaidGroup[];
  selectedEncounter?: string;
  selectedRaid?: string;
}

const encounterOrder: Record<string, string[]> = {
  'Наксрамас 25': [
    "Ануб'Рекан",
    "Великая вдова Фарлина",
    "Мексна",
    "Нот Чумной",
    "Инструктор Разувий",
    "Готик Жнец",
    "Гроббулус",
    "Глут",
    "Таддиус",
    "Барон Ривендер",
    "Сэр Зелиек",
    "Леди Бломе",
    "Тан Кортазз",
    "Лоскутик",
    "Лотхиб",
    "Сапфирон",
    "Кел'Тузад",
    "Хейган Нечестивый"
  ],
  'Око Вечности 25': [
    "Малигос"
  ],
  'Обсидиановое святилище 25': [
    "Сартарион"
  ],
  'Ульдуар 25': [
    "Огненный Левиафан",
    "Повелитель Горнов Игнис",
    "Острокрылая",
    "Разрушитель XT-002",
    "Железное собрание",
    "Кологарн",
    "Ауриайя",
    "Мимирон",
    "Фрейя",
    "Торим",
    "Ходир",
    "Генерал Везакс",
    "Йогг-Сарон",
    "Алгалон Наблюдатель"
  ]
};

function sortEncounters(encounters: Encounter[], raidTypeName: string): Encounter[] {
  const order = encounterOrder[raidTypeName];
  if (!order) return encounters;

  const sorted = [...encounters].sort((a, b) => {
    const nameA = a.encounterName ?? a.encounterEntry;
    const nameB = b.encounterName ?? b.encounterEntry;
    const indexA = order.indexOf(nameA);
    const indexB = order.indexOf(nameB);

    if (indexA === -1 && indexB === -1) return nameA.localeCompare(nameB);
    if (indexA === -1) return 1;
    if (indexB === -1) return -1;
    return indexA - indexB;
  });

  return sorted;
}

export default function RaidTabsSidebar({ raidGroups, selectedEncounter, selectedRaid }: RaidTabsSidebarProps) {
  const searchParams = useSearchParams();
  const currentSearch = searchParams.get('search') || '';
  const currentClass = searchParams.get('class') || '';
  const currentRole = searchParams.get('role') || '';
  
  console.log('RaidTabsSidebar received raidGroups:', raidGroups);
  
  const targetRaids = ['Наксрамас 25', 'Око Вечности 25', 'Обсидиановое святилище 25', 'Ульдуар 25'];
  
  const buildEncounterUrl = (encounterName: string) => {
    const params = new URLSearchParams();
    params.append('encounter', encounterName);
    if (currentSearch)
      params.append('search', currentSearch);
    if (currentClass)
      params.append('class', currentClass);
    if (currentRole)
      params.append('role', currentRole);
    return `/players?${params.toString()}`;
  };
  const filteredRaidGroups = raidGroups.filter((rg) => targetRaids.includes(rg.raidTypeName));
  console.log('Filtered raid groups:', filteredRaidGroups);
  
  const raidOrder = ['Наксрамас 25', 'Око Вечности 25', 'Обсидиановое святилище 25', 'Ульдуар 25'];
  const sortedRaidGroups = filteredRaidGroups.map(rg => ({
    ...rg,
    encounters: sortEncounters(rg.encounters, rg.raidTypeName)
  })).sort((a, b) => {
    const indexA = raidOrder.indexOf(a.raidTypeName);
    const indexB = raidOrder.indexOf(b.raidTypeName);
    if (indexA === -1 && indexB === -1) return 0;
    if (indexA === -1) return 1;
    if (indexB === -1) return -1;
    return indexA - indexB;
  });
  
  if (sortedRaidGroups.length === 0 && raidGroups.length > 0) {
    console.warn('No raid groups match target raids. Available:', raidGroups.map(rg => rg.raidTypeName));
  }

  const [expandedRaids, setExpandedRaids] = useState<Set<string>>(
    new Set(selectedRaid ? [selectedRaid] : sortedRaidGroups.length > 0 ? [sortedRaidGroups[0].raidTypeName] : [])
  );

  const toggleRaid = (raidName: string) => {
    const newExpanded = new Set(expandedRaids);
    if (newExpanded.has(raidName)) {
      newExpanded.delete(raidName);
    } else {
      newExpanded.add(raidName);
    }
    setExpandedRaids(newExpanded);
  };

  if (sortedRaidGroups.length === 0) {
    return (
      <div className="p-5" suppressHydrationWarning>
        <h2 className="text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider mb-4 px-2">Рейды</h2>
        <div className="text-center py-8">
          <p className="text-[var(--foreground-muted)] text-sm">Рейды не найдены</p>
          {raidGroups.length > 0 && (
            <p className="text-[var(--foreground-subtle)] text-xs mt-2">
              Доступно рейдов: {raidGroups.length}
            </p>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="p-5" suppressHydrationWarning>
      <h2 className="text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider mb-5 px-2">Рейды</h2>
      <nav className="space-y-2" role="navigation" aria-label="Список рейдов">
        {sortedRaidGroups.map((raidGroup) => {
          const isExpanded = expandedRaids.has(raidGroup.raidTypeName);
          const hasEncounters = raidGroup.encounters.length > 0;

          return (
            <div key={raidGroup.raidTypeName} className="mb-2">
              <button
                onClick={() => toggleRaid(raidGroup.raidTypeName)}
                className="w-full flex items-center justify-between px-3 py-2.5 rounded-lg text-sm font-semibold transition-colors text-[var(--foreground)] hover:bg-[var(--sidebar-hover)] bg-[var(--card-bg)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--sidebar-bg)]"
                aria-expanded={isExpanded}
                aria-controls={`raid-${raidGroup.raidTypeName}`}
              >
                <span>{raidGroup.raidTypeName}</span>
                {hasEncounters && (
                  isExpanded ? (
                    <ChevronDown className="w-4 h-4 transition-transform" aria-hidden="true" />
                  ) : (
                    <ChevronRight className="w-4 h-4 transition-transform" aria-hidden="true" />
                  )
                )}
              </button>
              {isExpanded && hasEncounters && (
                <div id={`raid-${raidGroup.raidTypeName}`} className="mt-1 ml-2 space-y-0.5 border-l border-[var(--border-color)] pl-3">
                  {raidGroup.encounters.map((encounter) => {
                    const encounterName = encounter.encounterName ?? encounter.encounterEntry;
                    const imagePath = getEncounterImage(encounter.encounterName);
                    const isSelected = selectedEncounter === encounter.encounterName || selectedEncounter === encounter.encounterEntry;
                    const encounterUrl = buildEncounterUrl(encounterName);

                    return (
                      <Link
                        key={encounter.encounterEntry}
                        href={encounterUrl}
                        className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--sidebar-bg)] ${
                          isSelected
                            ? 'bg-[var(--selected-bg)] text-[var(--foreground)]'
                            : 'text-[var(--foreground-muted)] hover:text-[var(--foreground)] hover:bg-[var(--sidebar-hover)]'
                        }`}
                        aria-current={isSelected ? 'page' : undefined}
                      >
                        {imagePath ? (
                          <Image
                            src={imagePath}
                            alt={encounterName}
                            width={40}
                            height={40}
                            className="w-10 h-10 rounded-md flex-shrink-0 object-cover"
                            unoptimized
                          />
                        ) : (
                          <span className="w-10 h-10 bg-[var(--card-bg)] rounded-md flex-shrink-0"></span>
                        )}
                        <span className="truncate">{encounterName}</span>
                      </Link>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })}
      </nav>
    </div>
  );
}
