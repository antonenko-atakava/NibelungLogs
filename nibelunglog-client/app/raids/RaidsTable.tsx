'use client';

import { useRouter } from 'next/navigation';
import type { RaidDto } from '@/types/api/RaidDto';

type Props = {
    raids: RaidDto[];
};

function formatTime(seconds: number) {
    if (!Number.isFinite(seconds) || seconds < 0) return '—';
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}:${s.toString().padStart(2, '0')}`;
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

function formatNumber(value: number) {
    if (!Number.isFinite(value)) return '—';
    if (value >= 1_000_000_000) return `${(value / 1_000_000_000).toFixed(2)}B`;
    if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `${(value / 1_000).toFixed(1)}K`;
    return Math.round(value).toLocaleString('ru-RU');
}

export default function RaidsTable({ raids }: Props) {
    const router = useRouter();

    const go = (id: number, e?: React.MouseEvent) => {
        if (e && (e.ctrlKey || e.metaKey)) {
            window.open(`/raids/${id}`, '_blank', 'noopener,noreferrer');
            return;
        }
        router.push(`/raids/${id}`);
    };

    const onRowKeyDown = (id: number) => (e: React.KeyboardEvent<HTMLTableRowElement>) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            router.push(`/raids/${id}`);
        }
    };

    return (
        <section className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] overflow-hidden shadow-lg">
            <div className="overflow-x-auto scrollbar-custom">
                <table className="min-w-full" role="table" aria-label="Таблица рейдов">
                    <thead className="bg-[var(--table-header-bg)] sticky top-0 z-10">
                    <tr className="text-left text-xs font-semibold uppercase tracking-wider text-[var(--foreground-muted)]">
                        <th className="px-6 py-4">Рейд</th>
                        <th className="px-6 py-4">Гильдия</th>
                        <th className="px-6 py-4">Старт</th>
                        <th className="px-6 py-4">Время</th>
                        <th className="px-6 py-4">Лидер</th>
                        <th className="px-6 py-4">Прогресс</th>
                        <th className="px-6 py-4">Вайпы</th>
                        <th className="px-6 py-4 text-right">Урон</th>
                    </tr>
                    </thead>

                    <tbody className="divide-y divide-[var(--table-border)] bg-[var(--table-bg)]">
                    {raids.map((raid) => {
                        const progressPct =
                            raid.totalBosses > 0 ? Math.round((raid.completedBosses / raid.totalBosses) * 100) : 0;

                        return (
                            <tr
                                key={raid.id}
                                role="button"
                                tabIndex={0}
                                onClick={(e) => go(raid.id, e)}
                                onKeyDown={onRowKeyDown(raid.id)}
                                className="group cursor-pointer hover:bg-[var(--table-row-hover)] transition-colors outline-none focus:bg-[var(--table-row-hover)] focus:ring-2 focus:ring-[var(--accent)] focus:ring-inset"
                                title="Открыть детали рейда"
                                aria-label={`Открыть детали рейда ${raid.raidTypeName}`}
                            >
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <div className="inline-flex items-center gap-2 text-[var(--foreground)] font-semibold">
                                        <span className="h-2 w-2 rounded-full bg-[var(--success)] group-hover:bg-[var(--success)] transition-colors" />
                                        {raid.raidTypeName}
                                    </div>
                                    <div className="mt-1 text-xs text-[var(--foreground-subtle)]">ID: {raid.id}</div>
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap text-sm text-[var(--foreground)]">
                                    {raid.guildName || '—'}
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap text-sm text-[var(--foreground)]">
                                    {formatDate(raid.startTime)}
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap text-sm text-[var(--foreground)]">
                    <span className="rounded-md border border-[var(--border-color)] bg-[var(--card-bg)] px-2 py-1">
                      {formatTime(raid.totalTime)}
                    </span>
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap text-sm text-[var(--foreground)]">
                                    {raid.leaderName || '—'}
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap">
                                    <div className="flex items-center gap-3">
                                        <div className="w-36">
                                            <div className="h-2 rounded-full bg-[var(--card-bg)] overflow-hidden">
                                                <div className="h-full bg-[var(--success)] transition-all" style={{ width: `${progressPct}%` }} />
                                            </div>
                                            <div className="mt-1 text-xs text-[var(--foreground-muted)]">
                                                {raid.completedBosses} / {raid.totalBosses}
                                            </div>
                                        </div>
                                        <span className="text-xs font-semibold text-[var(--foreground)]">{progressPct}%</span>
                                    </div>
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap">
                    <span
                        className={[
                            'inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold border',
                            raid.wipes === 0
                                ? 'border-[var(--success)]/40 bg-[var(--success)]/10 text-[var(--success)]'
                                : 'border-[var(--warning)]/40 bg-[var(--warning)]/10 text-[var(--warning)]',
                        ].join(' ')}
                    >
                      {raid.wipes}
                    </span>
                                </td>

                                <td className="px-6 py-4 whitespace-nowrap text-right">
                                    <div className="text-sm font-semibold text-[var(--foreground)]">{formatNumber(raid.totalDamage)}</div>
                                    <div className="text-xs text-[var(--foreground-subtle)]">total</div>
                                </td>
                            </tr>
                        );
                    })}
                    </tbody>
                </table>
            </div>
        </section>
    );
}
