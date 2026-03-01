'use client';

import { useMemo, useState, useTransition } from 'react';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import type { RaidTypeDto } from '@/types/api/RaidTypeDto';

type Props = { raidTypes: RaidTypeDto[] };

export default function RaidsFilters({ raidTypes }: Props) {
    const router = useRouter();
    const pathname = usePathname();
    const sp = useSearchParams();
    const [isPending, startTransition] = useTransition();

    const currentRaidTypeId = sp.get('raidTypeId') ?? '';
    const currentRaidTypeName = sp.get('raidTypeName') ?? '';
    const currentGuild = sp.get('guild') ?? '';
    const currentLeader = sp.get('leader') ?? '';

    const [raidTypeId, setRaidTypeId] = useState<string>(currentRaidTypeId);
    const [guild, setGuild] = useState<string>(currentGuild);
    const [leader, setLeader] = useState<string>(currentLeader);

    const selectedLabel = useMemo(() => {
        if (!raidTypeId) return currentRaidTypeName ? currentRaidTypeName : 'Все рейды';
        const found = raidTypes.find((x) => String(x.id) === raidTypeId);
        return found?.name ?? 'Все рейды';
    }, [raidTypeId, raidTypes, currentRaidTypeName]);

    const push = (next: URLSearchParams) => {
        const qs = next.toString();
        startTransition(() => {
            router.push(qs ? `${pathname}?${qs}` : pathname);
        });
    };

    const apply = () => {
        const next = new URLSearchParams(sp.toString());
        next.delete('page');

        if (raidTypeId) {
            next.set('raidTypeId', raidTypeId);
            next.delete('raidTypeName');
        } else {
            next.delete('raidTypeId');
            next.delete('raidTypeName');
        }

        const g = guild.trim();
        const l = leader.trim();

        if (g) next.set('guild', g);
        else next.delete('guild');

        if (l) next.set('leader', l);
        else next.delete('leader');

        push(next);
    };

    const reset = () => {
        const next = new URLSearchParams(sp.toString());
        next.delete('page');
        next.delete('raidTypeId');
        next.delete('raidTypeName');
        next.delete('guild');
        next.delete('leader');

        setRaidTypeId('');
        setGuild('');
        setLeader('');

        push(next);
    };

    const onEnterApply: React.KeyboardEventHandler<HTMLInputElement | HTMLSelectElement> = (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            apply();
        }
    };

    return (
        <div className="rounded-xl border border-[var(--border-color)] bg-[var(--card-bg)] p-5 sm:p-6">
            <div className="flex flex-col gap-5">
                <div className="flex flex-col gap-1">
                    <div className="text-xs font-semibold uppercase tracking-wider text-[var(--foreground-muted)]">Фильтры</div>
                    <div className="text-sm text-[var(--foreground)]">
                        Тип: <span className="font-semibold text-[var(--foreground)]">{selectedLabel}</span>
                    </div>
                </div>

                <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
                        <label className="flex flex-col gap-1.5">
                            <span className="text-xs text-[var(--foreground-muted)]">Тип рейда</span>
                            <select
                                value={raidTypeId}
                                onChange={(e) => setRaidTypeId(e.target.value)}
                                onKeyDown={onEnterApply}
                                className="h-10 w-full sm:w-72 rounded-lg border border-[var(--border-color)] bg-[var(--background-elevated)] px-3 text-sm text-[var(--foreground)] outline-none focus:border-[var(--border-hover)] focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--card-bg)] disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                disabled={isPending}
                                aria-label="Выберите тип рейда"
                            >
                                <option value="">Все</option>
                                {raidTypes.map((rt) => (
                                    <option key={rt.id} value={rt.id}>
                                        {rt.name}
                                    </option>
                                ))}
                            </select>
                        </label>

                        <label className="flex flex-col gap-1.5">
                            <span className="text-xs text-[var(--foreground-muted)]">Гильдия</span>
                            <input
                                value={guild}
                                onChange={(e) => setGuild(e.target.value)}
                                onKeyDown={onEnterApply}
                                placeholder="Напр. Enigma"
                                className="h-10 w-full sm:w-64 rounded-lg border border-[var(--border-color)] bg-[var(--background-elevated)] px-3 text-sm text-[var(--foreground)] placeholder:text-[var(--foreground-subtle)] outline-none focus:border-[var(--border-hover)] focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--card-bg)] disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                disabled={isPending}
                                aria-label="Введите название гильдии"
                            />
                        </label>

                        <label className="flex flex-col gap-1.5">
                            <span className="text-xs text-[var(--foreground-muted)]">Лидер</span>
                            <input
                                value={leader}
                                onChange={(e) => setLeader(e.target.value)}
                                onKeyDown={onEnterApply}
                                placeholder="Напр. Rimma"
                                className="h-10 w-full sm:w-64 rounded-lg border border-[var(--border-color)] bg-[var(--background-elevated)] px-3 text-sm text-[var(--foreground)] placeholder:text-[var(--foreground-subtle)] outline-none focus:border-[var(--border-hover)] focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--card-bg)] disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                disabled={isPending}
                                aria-label="Введите имя лидера"
                            />
                        </label>
                    </div>

                    <div className="flex items-center gap-3">
                        <button
                            onClick={apply}
                            disabled={isPending}
                            className="h-10 rounded-lg border border-[var(--success)]/40 bg-[var(--success)]/10 px-4 text-sm font-semibold text-[var(--success)] hover:bg-[var(--success)]/20 transition-colors disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--card-bg)]"
                            aria-label="Применить фильтры"
                        >
                            {isPending ? 'Применяю…' : 'Применить'}
                        </button>

                        <button
                            onClick={reset}
                            disabled={isPending}
                            className="h-10 rounded-lg border border-[var(--border-color)] bg-[var(--background-elevated)] px-4 text-sm font-semibold text-[var(--foreground)] hover:bg-[var(--card-hover)] transition-colors disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-[var(--accent)] focus:ring-offset-2 focus:ring-offset-[var(--card-bg)]"
                            aria-label="Сбросить фильтры"
                        >
                            Сбросить
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
