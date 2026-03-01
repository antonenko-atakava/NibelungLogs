import Link from 'next/link';
import { notFound } from 'next/navigation';
import { api } from '@/lib/api';
import EncountersAccordion from './EncountersAccordion';

interface RaidDetailPageProps {
    params: Promise<{ id: string }>;
}

function formatDate(dateString: string) {
    const d = new Date(dateString);
    if (Number.isNaN(d.getTime())) return '—';
    return d.toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'short',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
    });
}

function formatDuration(seconds: number) {
    if (!Number.isFinite(seconds) || seconds <= 0) return '—';
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${s.toString().padStart(2, '0')}`;
}

function formatCompact(value: number) {
    if (!Number.isFinite(value)) return '—';
    const abs = Math.abs(value);
    if (abs >= 1_000_000_000) return `${(value / 1_000_000_000).toFixed(2)}B`;
    if (abs >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (abs >= 1_000) return `${(value / 1_000).toFixed(1)}K`;
    return Math.round(value).toLocaleString('ru-RU');
}

export default async function RaidDetailPage({ params }: RaidDetailPageProps) {
    const { id } = await params;
    const raidId = Number.parseInt(id, 10);

    if (!Number.isFinite(raidId) || raidId <= 0) notFound();

    let raid;
    try {
        raid = await api.getRaid(raidId);
    } catch {
        return (
            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
                <Link href="/raids" className="text-[var(--foreground-muted)] hover:text-[var(--foreground)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background)] rounded-md px-2 py-1 inline-block mb-4">
                    ← Назад к рейдам
                </Link>

                <div className="mt-8 rounded-xl border border-[var(--error)]/40 bg-[var(--error)]/10 p-6" role="alert">
                    <p className="text-[var(--error)] font-semibold">Не удалось загрузить рейд</p>
                    <p className="mt-2 text-sm text-[var(--error)]/80">Проверь API и NEXT_PUBLIC_API_URL.</p>
                </div>
            </main>
        );
    }

    const progressPct =
        raid.totalBosses > 0 ? Math.round((raid.completedBosses / raid.totalBosses) * 100) : 0;

    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
            <div className="flex items-center justify-between gap-4 mb-6">
                <Link href="/raids" className="text-[var(--foreground-muted)] hover:text-[var(--foreground)] transition-colors focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--background)] rounded-md px-2 py-1">
                    ← Назад
                </Link>

                <div className="text-sm text-[var(--foreground-muted)]">
                    Лог #{raid.id} • {formatDate(raid.startTime)}
                </div>
            </div>

            <header className="mb-8">
                <h1 className="text-3xl sm:text-4xl font-semibold tracking-tight text-[var(--foreground)] mb-2">
                    {raid.raidTypeName || 'Рейд'}
                </h1>
                <p className="text-sm text-[var(--foreground-muted)]">
                    {raid.guildName ? `Гильдия: ${raid.guildName}` : 'Гильдия: —'} •{' '}
                    {raid.leaderName ? `Лидер: ${raid.leaderName}` : 'Лидер: —'}
                </p>
            </header>

            <section className="mb-10 grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4" aria-label="Статистика рейда">
                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-4">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--foreground-muted)] mb-2">Прогресс</div>
                    <div className="text-xl font-semibold text-[var(--foreground)] mb-2">
                        {raid.completedBosses} / {raid.totalBosses}{' '}
                        <span className="text-sm text-[var(--foreground-muted)]">({progressPct}%)</span>
                    </div>
                    <div className="h-2 rounded-full bg-[var(--card-bg)] overflow-hidden">
                        <div className="h-full bg-[var(--success)] transition-all" style={{ width: `${progressPct}%` }} />
                    </div>
                </div>

                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-4">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--foreground-muted)] mb-2">Длительность рейда</div>
                    <div className="text-xl font-semibold text-[var(--foreground)]">{formatDuration(raid.totalTime)}</div>
                </div>

                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-4">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--foreground-muted)] mb-2">Вайпы</div>
                    <div className="text-xl font-semibold text-[var(--foreground)]">{raid.wipes}</div>
                </div>

                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-4">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--foreground-muted)] mb-2">Энкаунтеры</div>
                    <div className="text-xl font-semibold text-[var(--foreground)]">{raid.encounters?.length ?? 0}</div>
                </div>

                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-4">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--foreground-muted)] mb-2">Урон</div>
                    <div className="text-xl font-semibold text-[var(--foreground)]">{formatCompact(raid.totalDamage)}</div>
                </div>

                <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-4">
                    <div className="text-xs font-semibold uppercase tracking-wide text-[var(--foreground-muted)] mb-2">Лечение</div>
                    <div className="text-xl font-semibold text-[var(--foreground)]">{formatCompact(raid.totalHealing)}</div>
                </div>
            </section>

            <section className="mt-10">
                <div className="flex items-end justify-between gap-4 mb-4">
                    <h2 className="text-2xl font-semibold text-[var(--foreground)]">Энкаунтеры</h2>
                    <div className="text-sm text-[var(--foreground-muted)]">Клик по строке — раскрыть игроков</div>
                </div>

                {!raid.encounters || raid.encounters.length === 0 ? (
                    <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-8 text-center" role="status" aria-live="polite">
                        <p className="text-[var(--foreground)] font-medium">Энкаунтеры не найдены</p>
                    </div>
                ) : (
                    <EncountersAccordion encounters={raid.encounters} />
                )}
            </section>
        </main>
    );
}
