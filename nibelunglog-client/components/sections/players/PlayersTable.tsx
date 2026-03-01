'use client';

import { useState } from 'react';
import Image from 'next/image';
import { useRouter, useSearchParams } from 'next/navigation';
import type { PlayerDto } from '@/types/api/PlayerDto';
import { getSpecIcon, getClassColor } from '@/lib/classIcons';
import EncounterModal from '@/components/ui/modals/EncounterModal';
import PrimaryButton from '@/components/ui/buttons/PrimaryButton';
import SecondaryButton from '@/components/ui/buttons/SecondaryButton';

interface PlayersTableProps {
  players: PlayerDto[];
  page: number;
  totalPages: number;
}

export default function PlayersTable({ players, page, totalPages }: PlayersTableProps) {
  const [selectedEncounterId, setSelectedEncounterId] = useState<number | null>(null);
  const router = useRouter();
  const searchParams = useSearchParams();

  const buildUrl = (pageNum: number) => {
    const urlParams = new URLSearchParams();
    urlParams.append('page', pageNum.toString());
    const encounter = searchParams.get('encounter');
    const search = searchParams.get('search');
    const characterClass = searchParams.get('class');
    const role = searchParams.get('role');
    if (encounter) urlParams.append('encounter', encounter);
    if (search) urlParams.append('search', search);
    if (characterClass) urlParams.append('class', characterClass);
    if (role) urlParams.append('role', role);
    return `/players?${urlParams.toString()}`;
  };


  const formatDuration = (seconds?: number) => {
    if (!seconds) return '-';
    const minutes = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: 'short',
      year: '2-digit'
    });
  };

  const handlePlayerClick = (player: PlayerDto) => {
    if (player.encounterId) {
      setSelectedEncounterId(player.encounterId);
    }
  };

  return (
    <>
      <div className="flex flex-col flex-1 h-full overflow-hidden">
        <div className="flex-1 overflow-y-auto overflow-x-auto scrollbar-custom">
          <table className="w-full" role="table" aria-label="Таблица игроков">
            <thead className="bg-[var(--table-header-bg)] sticky top-0 z-10">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider">
                  Топ
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider">
                  Игрок
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider">
                  DPS / HPS
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider hidden md:table-cell">
                  iLVL
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider hidden lg:table-cell">
                  Длительность
                </th>
                <th className="px-6 py-4 text-left text-xs font-semibold text-[var(--foreground-muted)] uppercase tracking-wider">
                  Дата
                </th>
              </tr>
            </thead>
            <tbody className="bg-[var(--table-bg)] divide-y divide-[var(--table-border)]">
              {players.map((player, index) => {
                const itemLevel = player.itemLevel ? parseFloat(player.itemLevel).toFixed(0) : '-';
                const uniqueKey = player.encounterId ? `${player.id}-${player.encounterId}` : `${player.id}-${player.encounterDate || index}`;
                
                return (
                  <tr 
                    key={uniqueKey} 
                    className="hover:bg-[var(--table-row-hover)] transition-colors border-b border-[var(--table-border)]"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm font-semibold text-[var(--foreground)]">
                        {player.rank}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <button
                        onClick={() => handlePlayerClick(player)}
                        className="flex items-center gap-3 text-sm font-medium hover:opacity-80 transition-opacity w-full text-left focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--table-bg)] rounded-md px-1 py-1"
                        disabled={!player.encounterId}
                        aria-label={`Открыть детали для ${player.characterName}`}
                      >
                        {(() => {
                          const specIcon = getSpecIcon(player.characterClass, player.specName, player.className);
                          
                          if (specIcon) {
                            return (
                              <Image
                                src={specIcon}
                                alt={player.specName || player.characterClass}
                                width={40}
                                height={40}
                                className="w-10 h-10 rounded-md flex-shrink-0"
                              />
                            );
                          }
                          
                          return <span className="w-10 h-10 bg-[var(--card-bg)] rounded-md flex-shrink-0"></span>;
                        })()}
                        <div className="flex flex-col min-w-0">
                          <span style={{ color: getClassColor(player.characterClass, player.className) }} className="font-medium truncate">
                            {player.characterName}
                          </span>
                          <span className="text-xs text-[var(--foreground-muted)] truncate">
                            {player.className || player.characterClass}
                            {player.specName && ` • ${player.specName}`}
                          </span>
                        </div>
                      </button>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {player.role === "2" && player.maxHps ? (
                        <span className="text-sm font-semibold text-[var(--accent)]">
                          {Math.round(player.maxHps).toLocaleString('ru-RU')} HPS
                        </span>
                      ) : (
                        <span className="text-sm font-semibold text-[var(--success)]">
                          {Math.round(player.maxDps).toLocaleString('ru-RU')} DPS
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-[var(--foreground)] hidden md:table-cell">
                      {itemLevel}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-[var(--foreground-muted)] hidden lg:table-cell">
                      {formatDuration(player.encounterDuration)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-[var(--foreground-muted)]">
                      {formatDate(player.encounterDate)}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        <div className="flex justify-center items-center gap-6 py-5 border-t border-[var(--border-color)] flex-shrink-0 bg-[var(--table-bg)] sticky bottom-0">
          {page > 1 && (
            <SecondaryButton href={buildUrl(page - 1)} aria-label="Предыдущая страница">
              Назад
            </SecondaryButton>
          )}
          <span className="text-[var(--foreground-muted)] text-sm font-medium" aria-label={`Страница ${page} из ${totalPages}`}>
            Страница {page} из {totalPages}
          </span>
          {page < totalPages && (
            <PrimaryButton href={buildUrl(page + 1)} aria-label="Следующая страница">
              Вперед
            </PrimaryButton>
          )}
        </div>
      </div>

      <EncounterModal
        encounterId={selectedEncounterId}
        onClose={() => setSelectedEncounterId(null)}
      />
    </>
  );
}
