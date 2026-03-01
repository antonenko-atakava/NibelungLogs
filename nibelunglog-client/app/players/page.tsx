import { api } from '@/lib/api';
import RaidTabsSidebar from '@/components/sections/players/RaidTabsSidebar';
import PlayersFilters from '@/components/sections/players/PlayersFilters';
import PlayersTable from '@/components/sections/players/PlayersTable';

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

  let raidGroups: Array<{
    raidTypeName: string;
    encounters: Array<{ encounterEntry: string; encounterName: string | null }>;
  }> = [];
  try {
    raidGroups = await api.getEncountersGroupedByRaid();
    console.log('Raid groups loaded:', raidGroups);
  } catch (error) {
    console.error('Failed to load raid groups:', error);
    raidGroups = [];
  }

  const selectedRaid = raidGroups.find((rg) => 
    rg.encounters.some((e) => 
      e.encounterName === encounter || e.encounterEntry === encounter
    )
  )?.raidTypeName;

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
      playersData = { data: [], totalPages: 0, totalCount: 0 };
    }
  } catch (error) {
    return (
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="mb-12">
          <h1 className="text-3xl sm:text-4xl md:text-5xl font-bold text-[var(--foreground)] mb-4">Игроки</h1>
        </div>
        <div className="text-center py-20 rounded-xl border border-[var(--error)]/40 bg-[var(--error)]/10 p-6" role="alert">
          <p className="text-[var(--error)] text-lg mb-4 font-semibold">Ошибка подключения к API</p>
          <p className="text-[var(--foreground-muted)] text-sm">Убедитесь, что API сервер запущен на http://localhost:5097</p>
        </div>
      </main>
    );
  }

  return (
    <div className="w-full h-screen flex flex-col bg-[var(--background)] overflow-hidden">
      <header className="px-6 py-5 border-b border-[var(--border-color)] flex-shrink-0 bg-[var(--background-elevated)]">
        <h1 className="text-2xl md:text-3xl font-bold text-[var(--foreground)] tracking-tight">Игроки</h1>
        {encounter && (
          <p className="text-[var(--foreground-muted)] text-sm mt-2" aria-label="Выбранный босс">
            Босс: <span className="text-[var(--foreground)] font-medium">{encounter}</span>
          </p>
        )}
        {search && !encounter && (
          <p className="text-[var(--foreground-muted)] text-sm mt-2" aria-label="Поисковый запрос">
            Поиск: <span className="text-[var(--foreground)] font-medium">{search}</span>
          </p>
        )}
      </header>

      <div className="flex flex-1 overflow-hidden">
        <aside 
          className="w-[20%] min-w-[200px] flex-shrink-0 border-r border-[var(--border-color)] bg-[var(--sidebar-bg)] h-full overflow-y-auto scrollbar-custom"
          aria-label="Список рейдов и боссов"
        >
          <RaidTabsSidebar raidGroups={raidGroups} selectedEncounter={encounter} selectedRaid={selectedRaid} />
        </aside>

        <main className="w-[65%] flex-shrink-0 bg-[var(--table-bg)] flex flex-col h-full overflow-hidden">
          {!encounter ? (
            <div className="flex items-center justify-center h-full" role="status" aria-live="polite">
              <div className="text-center px-6">
                <p className="text-[var(--foreground-muted)] text-lg mb-2 font-medium">Выберите босса из списка рейдов</p>
                <p className="text-[var(--foreground-subtle)] text-sm">Выберите рейд слева и затем босса для просмотра статистики игроков</p>
              </div>
            </div>
          ) : playersData.data.length === 0 ? (
            <div className="text-center py-20" role="status" aria-live="polite">
              <p className="text-[var(--foreground-muted)] text-lg font-medium">Игроки не найдены</p>
            </div>
          ) : (
            <PlayersTable
              players={playersData.data}
              page={page}
              totalPages={playersData.totalPages}
            />
          )}
        </main>

        <aside 
          className="w-[15%] min-w-[180px] flex-shrink-0 border-l border-[var(--border-color)] bg-[var(--sidebar-bg)] h-full overflow-y-auto scrollbar-custom"
          aria-label="Фильтры"
        >
          <PlayersFilters />
        </aside>
      </div>
    </div>
  );
}

