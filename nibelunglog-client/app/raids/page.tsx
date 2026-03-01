import { api } from '@/lib/api';
import Link from 'next/link';
import RaidsFilters from './RaidsFilters';
import RaidsTable from './RaidsTable';

interface RaidsPageProps {
    searchParams: Promise<{
        page?: string;
        raidTypeId?: string;
        raidTypeName?: string;
        guild?: string;
        leader?: string;
    }>;
}

export default async function RaidsPage({ searchParams }: RaidsPageProps) {
    const params = await searchParams;

    const page = Math.max(parseInt(params.page ?? '1', 10) || 1, 1);
    const raidTypeId = params.raidTypeId ? parseInt(params.raidTypeId, 10) : undefined;
    const raidTypeName = params.raidTypeName;
    const guild = params.guild;
    const leader = params.leader;

    // --- справочники (client-side фильтр)
    let raidTypes: Array<{ id: number; name: string; map: string; difficulty: string; instanceType: string }> = [];
    try {
        raidTypes = await api.getRaidTypes();
    } catch {
        raidTypes = [];
    }

    // --- данные рейдов
    let raidsData;
    try {
        raidsData = await api.getRaids({
            page,
            pageSize: 25,
            raidTypeId,
            raidTypeName,
            guildName: guild,
            leaderName: leader,
        });
    } catch {
        return (
            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
                <h1 className="text-3xl sm:text-4xl md:text-5xl font-semibold tracking-tight text-[var(--foreground)] mb-4">Рейды</h1>
                <div className="rounded-xl border border-[var(--error)]/40 bg-[var(--error)]/10 p-6" role="alert">
                    <p className="text-[var(--error)] font-semibold">Ошибка подключения к API</p>
                    <p className="mt-2 text-sm text-[var(--error)]/80">
                        Проверь, что API запущен и NEXT_PUBLIC_API_URL указывает на него.
                    </p>
                </div>
            </main>
        );
    }

    // --- helpers
    const formatTime = (seconds: number) => {
        const m = Math.floor(seconds / 60);
        const s = Math.floor(seconds % 60);
        return `${m}:${s.toString().padStart(2, '0')}`;
    };

    const formatDate = (dateString: string) =>
        new Date(dateString).toLocaleDateString('ru-RU', {
            year: 'numeric',
            month: 'short',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
        });

    const formatNumber = (value: number) => {
        if (value >= 1_000_000_000) return `${(value / 1_000_000_000).toFixed(2)}B`;
        if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
        if (value >= 1_000) return `${(value / 1_000).toFixed(1)}K`;
        return value.toLocaleString('ru-RU');
    };

    const buildPageHref = (nextPage: number) => {
        const sp = new URLSearchParams();

        sp.set('page', String(nextPage));
        if (raidTypeId !== undefined) sp.set('raidTypeId', String(raidTypeId));
        if (raidTypeName) sp.set('raidTypeName', raidTypeName);
        if (guild) sp.set('guild', guild);
        if (leader) sp.set('leader', leader);

        return `/raids?${sp.toString()}`;
    };

    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
            <header className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <h1 className="text-3xl sm:text-4xl md:text-5xl font-semibold tracking-tight text-[var(--foreground)] mb-2">
                        Рейды
                    </h1>
                    <p className="text-sm sm:text-base text-[var(--foreground-muted)]">
                        История рейдов и прогресс по боссам. Клик по строке — детали.
                    </p>
                </div>

                <div className="flex items-center gap-3 text-sm text-[var(--foreground-muted)]">
                    <div className="rounded-lg border border-[var(--border-color)] bg-[var(--card-bg)] px-4 py-2">
                        <span className="text-[var(--foreground-subtle)]">Записей:</span>{' '}
                        <span className="text-[var(--foreground)] font-medium">
              {raidsData.data.length}
            </span>
                    </div>
                    <div className="rounded-lg border border-[var(--border-color)] bg-[var(--card-bg)] px-4 py-2">
                        <span className="text-[var(--foreground-subtle)]">Страница:</span>{' '}
                        <span className="text-[var(--foreground)] font-medium">
              {raidsData.page} / {raidsData.totalPages}
            </span>
                    </div>
                </div>
            </header>

            <div className="mb-8">
                <RaidsFilters raidTypes={raidTypes} />
            </div>

            {raidsData.data.length === 0 ? (
                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-10 text-center" role="status" aria-live="polite">
                    <p className="text-[var(--foreground)] text-lg font-medium">Рейды не найдены</p>
                    <p className="mt-2 text-sm text-[var(--foreground-muted)]">
                        Попробуй изменить фильтры или страницу.
                    </p>
                </div>
            ) : (
                <>
                    <RaidsTable raids={raidsData.data} />

                    <footer className="mt-10 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                        <div className="text-sm text-[var(--foreground-muted)]">
                            Показано{' '}
                            <span className="text-[var(--foreground)] font-medium">
                {raidsData.data.length}
              </span>{' '}
                            рейдов
                        </div>

                        <nav className="flex items-center justify-center gap-4" aria-label="Пагинация">
                            {page > 1 ? (
                                <Link
                                    href={buildPageHref(page - 1)}
                                    className="px-4 py-2 rounded-lg border border-[var(--border-color)] bg-[var(--card-bg)] text-sm text-[var(--foreground)] hover:bg-[var(--card-hover)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background)]"
                                    aria-label="Предыдущая страница"
                                >
                                    Назад
                                </Link>
                            ) : (
                                <span className="px-4 py-2 text-sm text-[var(--foreground-subtle)]" aria-disabled="true">Назад</span>
                            )}

                            <div className="rounded-lg border border-[var(--border-color)] bg-[var(--card-bg)] px-4 py-2 text-sm text-[var(--foreground-muted)]">
                                Страница{' '}
                                <span className="font-semibold text-[var(--foreground)]">{page}</span> из{' '}
                                <span className="font-semibold text-[var(--foreground)]">
                  {raidsData.totalPages}
                </span>
                            </div>

                            {page < raidsData.totalPages ? (
                                <Link
                                    href={buildPageHref(page + 1)}
                                    className="px-4 py-2 rounded-lg border border-[var(--border-color)] bg-[var(--card-bg)] text-sm text-[var(--foreground)] hover:bg-[var(--card-hover)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background)]"
                                    aria-label="Следующая страница"
                                >
                                    Вперёд
                                </Link>
                            ) : (
                                <span className="px-4 py-2 text-sm text-[var(--foreground-subtle)]" aria-disabled="true">Вперёд</span>
                            )}
                        </nav>
                    </footer>
                </>
            )}
        </main>
    );
}
