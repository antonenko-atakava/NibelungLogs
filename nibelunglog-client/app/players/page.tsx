import { api } from '@/lib/api';
import PrimaryButton from '@/components/ui/buttons/PrimaryButton';
import SecondaryButton from '@/components/ui/buttons/SecondaryButton';
import Link from 'next/link';
import Image from 'next/image';
import EncounterSidebar from '@/components/sections/players/EncounterSidebar';
import PlayersFilters from '@/components/sections/players/PlayersFilters';
import { getClassIcon, getSpecIcon, getClassColor } from '@/lib/classIcons';

interface PlayersPageProps {
  searchParams: Promise<{ page?: string; search?: string; encounter?: string; class?: string; role?: string }>;
}

export const dynamic = 'force-dynamic';

export default async function PlayersPage({ searchParams }: PlayersPageProps) {
  const params = await searchParams;
  const page = parseInt(params.page || '1', 10);
  const search = params.search;
  const encounter = params.encounter;
  const characterClass = params.class;
  const role = params.role;

  let encountersList;
  try {
    encountersList = await api.getEncountersList();
  } catch (error) {
    encountersList = [];
  }

  let playersData;
  try {
    if (encounter) {
      if (characterClass) {
        playersData = await api.getPlayersByClass({
          characterClass,
          encounterName: encounter,
          role,
          search,
          page,
          pageSize: 25,
        });
      } else {
        playersData = await api.getPlayersByEncounter({
          encounterName: encounter,
          search,
          characterClass,
          role,
          page,
          pageSize: 25,
        });
      }
    } else if (characterClass) {
      playersData = await api.getPlayersByClass({
        characterClass,
        role,
        search,
        page,
        pageSize: 25,
      });
    } else {
      playersData = await api.getPlayers({
        page,
        pageSize: 25,
        search,
        role,
      });
    }
  } catch (error) {
    return (
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="mb-12">
          <h1 className="text-5xl font-bold text-white mb-4">Игроки</h1>
        </div>
        <div className="text-center py-20">
          <p className="text-red-400 text-lg mb-4">Ошибка подключения к API</p>
          <p className="text-gray-400 text-sm">Убедитесь, что API сервер запущен на http://localhost:5097</p>
        </div>
      </main>
    );
  }

  return (
    <div className="w-full h-screen flex flex-col bg-[#1a1a1a] overflow-hidden">
      <div className="px-6 py-4 border-b border-[#333333] flex-shrink-0 bg-[#1a1a1a]">
        <h1 className="text-2xl font-bold text-[#e5e5e5]">Игроки</h1>
        {encounter && (
          <p className="text-[#9ca3af] text-sm mt-1">Босс: {encounter}</p>
        )}
        {search && !encounter && (
          <p className="text-[#9ca3af] text-sm mt-1">Поиск: {search}</p>
        )}
      </div>

      <div className="flex flex-1 overflow-hidden">
        <div className="w-[20%] flex-shrink-0 border-r border-[#333333] bg-[#252525] h-full overflow-y-auto scrollbar-custom">
          <EncounterSidebar encounters={encountersList} selectedEncounter={encounter} />
        </div>

        <div className="w-[65%] flex-shrink-0 bg-[#1e1e1e] flex flex-col h-full overflow-hidden">
      {playersData.data.length === 0 ? (
        <div className="text-center py-20">
          <p className="text-gray-400 text-lg">Игроки не найдены</p>
        </div>
      ) : (
        <div className="flex flex-col flex-1 h-full overflow-hidden">
          <div className="flex-1 overflow-y-auto overflow-x-auto scrollbar-custom">
            <table className="w-full divide-y divide-[#2a2a2a]">
                <thead className="bg-[#2a2a2a]">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-[#9ca3af] uppercase tracking-wider">
                      Топ
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-[#9ca3af] uppercase tracking-wider">
                      Игрок
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-[#9ca3af] uppercase tracking-wider">
                      DPS / HPS
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-[#9ca3af] uppercase tracking-wider hidden md:table-cell">
                      iLVL
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-[#9ca3af] uppercase tracking-wider hidden lg:table-cell">
                      Длительность
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold text-[#9ca3af] uppercase tracking-wider">
                      Дата
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-[#1e1e1e] divide-y divide-[#2a2a2a]">
                  {playersData.data.map((player) => {
                    const getTopLabel = (rank: number): string => {
                      if (rank === 1) return 'Топ 1';
                      if (rank <= 10) return 'Топ 10';
                      if (rank <= 25) return 'Топ 25';
                      if (rank <= 50) return 'Топ 50';
                      if (rank <= 100) return 'Топ 100';
                      return `#${rank}`;
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
                    const itemLevel = player.itemLevel ? parseFloat(player.itemLevel).toFixed(0) : '-';
                    
                    return (
                      <tr key={player.id} className="hover:bg-[#2d2d2d] transition-colors border-b border-[#2a2a2a]">
                        <td className="px-6 py-3 whitespace-nowrap">
                          <span className="text-sm font-semibold text-[#e5e5e5]">
                            {getTopLabel(player.rank)}
                          </span>
                        </td>
                        <td className="px-6 py-3 whitespace-nowrap">
                          <Link href={`/players/${player.id}`} className="flex items-center gap-3 text-sm font-medium hover:opacity-80">
                            {(() => {
                              const specIcon = getSpecIcon(player.characterClass, player.specName, player.className);
                              
                              if (specIcon) {
                                return (
                                  <Image
                                    src={specIcon}
                                    alt={player.specName || ''}
                                    width={40}
                                    height={40}
                                    className="w-10 h-10 rounded flex-shrink-0"
                                  />
                                );
                              }
                              
                              return <span className="w-10 h-10 bg-[#3a3a3a] rounded flex-shrink-0"></span>;
                            })()}
                            <div className="flex flex-col">
                              <span style={{ color: getClassColor(player.characterClass, player.className) }} className="font-medium">
                                {player.characterName}
                              </span>
                              <span className="text-xs text-[#9ca3af]">
                                {player.className || player.characterClass}
                                {player.specName && ` • ${player.specName}`}
                              </span>
                            </div>
                          </Link>
                        </td>
                        <td className="px-6 py-3 whitespace-nowrap">
                          {player.role === "2" && player.maxHps ? (
                            <span className="text-sm font-semibold text-[#60a5fa]">
                              {Math.round(player.maxHps).toLocaleString()} HPS
                            </span>
                          ) : (
                            <span className="text-sm font-semibold text-[#4ade80]">
                              {Math.round(player.maxDps).toLocaleString()} DPS
                            </span>
                          )}
                        </td>
                        <td className="px-6 py-3 whitespace-nowrap text-sm font-medium text-[#d1d5db] hidden md:table-cell">
                          {itemLevel}
                        </td>
                        <td className="px-6 py-3 whitespace-nowrap text-sm font-medium text-[#9ca3af] hidden lg:table-cell">
                          {formatDuration(player.encounterDuration)}
                        </td>
                        <td className="px-6 py-3 whitespace-nowrap text-sm font-medium text-[#9ca3af]">
                          {formatDate(player.encounterDate)}
                        </td>
                      </tr>
                    );
                })}
              </tbody>
            </table>
          </div>

          <div className="flex justify-center items-center gap-6 py-4 border-t border-[#333333] flex-shrink-0 bg-[#1e1e1e] sticky bottom-0">
            {(() => {
              const buildUrl = (pageNum: number) => {
                const urlParams = new URLSearchParams();
                urlParams.append('page', pageNum.toString());
                if (encounter) urlParams.append('encounter', encounter);
                if (search) urlParams.append('search', search);
                if (characterClass) urlParams.append('class', characterClass);
                if (role) urlParams.append('role', role);
                return `/players?${urlParams.toString()}`;
              };
              
              return (
                <>
                  {page > 1 && (
                    <SecondaryButton href={buildUrl(page - 1)}>
                      Назад
                    </SecondaryButton>
                  )}
                  <span className="text-[#9ca3af] text-sm font-medium">
                    Страница {page} из {playersData.totalPages}
                  </span>
                  {page < playersData.totalPages && (
                    <PrimaryButton href={buildUrl(page + 1)}>
                      Вперед
                    </PrimaryButton>
                  )}
                </>
              );
            })()}
          </div>
        </div>
        )}
        </div>

        <div className="w-[15%] flex-shrink-0 border-l border-[#333333] bg-[#252525] h-full overflow-y-auto scrollbar-custom">
          <PlayersFilters />
        </div>
      </div>
    </div>
  );
}

