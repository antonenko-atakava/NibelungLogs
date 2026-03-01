'use client';

import Link from 'next/link';
import Image from 'next/image';
import { Fragment, useMemo, useState } from 'react';
import { ArrowUp, ArrowDown } from 'lucide-react';
import { api } from '@/lib/api';
import type { EncounterDto } from '@/types/api/EncounterDto';
import type { PlayerEncounterDto } from '@/types/api/PlayerEncounterDto';
import { getClassColor, getSpecIcon } from '@/lib/classIcons';
import { getEncounterImage } from '@/lib/encounterImages';

type Props = {
    encounters: EncounterDto[];
};

function formatClock(dateString: string) {
    const d = new Date(dateString);
    if (Number.isNaN(d.getTime())) return '—';
    return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
}

function diffSeconds(start: string, end: string) {
    const s = new Date(start).getTime();
    const e = new Date(end).getTime();
    if (Number.isNaN(s) || Number.isNaN(e)) return 0;
    const sec = Math.floor((e - s) / 1000);
    return sec > 0 ? sec : 0;
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

function roleInfo(role?: string | null) {
    const r = (role ?? '').trim();
    if (r === '1') return { emoji: '🛡️', text: 'Танк' };
    if (r === '2') return { emoji: '💚', text: 'Хил' };
    return { emoji: '⚔️', text: 'ДД' };
}

function SortIcon({ field, currentField, direction }: { field: string; currentField: string | null; direction: 'asc' | 'desc' | null }) {
    if (currentField !== field) {
        return <ArrowUp className="w-3 h-3 opacity-30" />;
    }
    return direction === 'desc' ? <ArrowDown className="w-3 h-3" /> : <ArrowUp className="w-3 h-3" />;
}

/** Босс-иконка: берём из lib/encounterImages.ts */
function BossIcon({ bossName }: { bossName: string }) {
    const src = getEncounterImage(bossName);

    if (src) {
        return (
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-xl border border-gray-700 bg-gray-950/30 overflow-hidden flex-shrink-0">
        <Image
            src={src}
            alt={bossName}
            width={36}
            height={36}
            className="h-9 w-9 object-cover"
        />
      </span>
        );
    }

    const letter = ((bossName ?? '').trim()[0] ?? '?').toUpperCase();
    return (
        <span className="inline-flex h-9 w-9 items-center justify-center rounded-xl border border-gray-700 bg-gray-950/50 text-xs font-bold text-gray-200 flex-shrink-0">
      {letter}
    </span>
    );
}

type PlayerRow = PlayerEncounterDto & {
    /** у тебя реально приходит так (см. JSON): */
    characterClass: string;          // "6"
    className?: string | null;       // "Рыцарь смерти"
    specName?: string | null;        // "Нечестивость"
    characterSpec?: string | null;   // "2" (не используем как имя)
    maxAverageGearScore?: string | null; // "202.00"
    maxGearScore?: string | null;         // "213"
};

export default function EncountersAccordion({ encounters }: Props) {
    const [openMap, setOpenMap] = useState<Record<number, boolean>>({});
    const [loadingMap, setLoadingMap] = useState<Record<number, boolean>>({});
    const [playersMap, setPlayersMap] = useState<Record<number, PlayerRow[]>>({});
    const [errorMap, setErrorMap] = useState<Record<number, string>>({});

    const [sortConfig, setSortConfig] = useState<{ encounterId: number; field: string; direction: 'asc' | 'desc' } | null>(null);

    const onToggle = async (enc: EncounterDto) => {
        const nextOpen = !openMap[enc.id];
        setOpenMap((m) => ({ ...m, [enc.id]: nextOpen }));

        if (!nextOpen) return;
        if (playersMap[enc.id]) return;

        setLoadingMap((m) => ({ ...m, [enc.id]: true }));
        setErrorMap((m) => ({ ...m, [enc.id]: '' }));

        try {
            const players = (await api.getEncounterPlayers(enc.id)) as unknown as PlayerRow[];
            const fightSec = diffSeconds(enc.startTime, enc.endTime);
            const sorted = [...players].sort((a, b) => {
                const aHps = a.role === "2" && fightSec > 0 ? (a.healingDone + (a.absorbProvided || 0)) / fightSec : 0;
                const bHps = b.role === "2" && fightSec > 0 ? (b.healingDone + (b.absorbProvided || 0)) / fightSec : 0;
                const aValue = a.role === "2" ? aHps : (a.dps ?? 0);
                const bValue = b.role === "2" ? bHps : (b.dps ?? 0);
                return bValue - aValue;
            });
            setPlayersMap((m) => ({ ...m, [enc.id]: sorted }));
        } catch {
            setErrorMap((m) => ({ ...m, [enc.id]: 'Не удалось загрузить игроков' }));
        } finally {
            setLoadingMap((m) => ({ ...m, [enc.id]: false }));
        }
    };

    const handleSort = (encounterId: number, field: string) => {
        const currentConfig = sortConfig?.encounterId === encounterId ? sortConfig : null;
        const direction = currentConfig?.field === field && currentConfig.direction === 'desc' ? 'asc' : 'desc';
        setSortConfig({ encounterId, field, direction });

        const players = playersMap[encounterId];
        if (!players) return;

        const sorted = [...players].sort((a, b) => {
            let aValue: number | string = 0;
            let bValue: number | string = 0;

            switch (field) {
                case 'player':
                    aValue = a.playerName?.toLowerCase() || '';
                    bValue = b.playerName?.toLowerCase() || '';
                    break;
                case 'role':
                    aValue = a.role || '';
                    bValue = b.role || '';
                    break;
                case 'dps':
                    aValue = a.dps ?? 0;
                    bValue = b.dps ?? 0;
                    break;
                case 'damage':
                    aValue = a.damageDone ?? 0;
                    bValue = b.damageDone ?? 0;
                    break;
                case 'hps':
                    const fightSec = diffSeconds(
                        encounters.find(e => e.id === encounterId)?.startTime || '',
                        encounters.find(e => e.id === encounterId)?.endTime || ''
                    );
                    aValue = a.role === "2" && fightSec > 0 ? (a.healingDone + (a.absorbProvided || 0)) / fightSec : 0;
                    bValue = b.role === "2" && fightSec > 0 ? (b.healingDone + (b.absorbProvided || 0)) / fightSec : 0;
                    break;
                case 'healing':
                    aValue = a.healingDone ?? 0;
                    bValue = b.healingDone ?? 0;
                    break;
                case 'absorb':
                    aValue = a.absorbProvided || 0;
                    bValue = b.absorbProvided || 0;
                    break;
                case 'ilvl':
                    const aIlvl = parseFloat(a.maxGearScore || a.maxAverageGearScore || '0');
                    const bIlvl = parseFloat(b.maxGearScore || b.maxAverageGearScore || '0');
                    aValue = aIlvl;
                    bValue = bIlvl;
                    break;
                default:
                    return 0;
            }

            if (typeof aValue === 'string' && typeof bValue === 'string') {
                return direction === 'desc' ? bValue.localeCompare(aValue) : aValue.localeCompare(bValue);
            }

            return direction === 'desc' ? (bValue as number) - (aValue as number) : (aValue as number) - (bValue as number);
        });

        setPlayersMap((m) => ({ ...m, [encounterId]: sorted }));
    };

    const rows = useMemo(() => {
        return encounters.map((e) => {
            const bossName = e.encounterName || e.encounterEntry || '—';
            const fightSec = diffSeconds(e.startTime, e.endTime);
            const avgDps = fightSec > 0 ? e.totalDamage / fightSec : 0;
            const avgHps = fightSec > 0 ? e.totalHealing / fightSec : 0;
            return { e, bossName, fightSec, avgDps, avgHps };
        });
    }, [encounters]);

    return (
        <div className="rounded-2xl border border-gray-800 bg-gray-900/40 overflow-hidden">
            <div className="overflow-x-auto">
                <table className="min-w-full">
                    <thead className="bg-gray-950/60">
                    <tr className="text-left text-xs font-semibold uppercase tracking-wider text-gray-400">
                        <th className="px-6 py-4">Время</th>
                        <th className="px-6 py-4">Босс</th>
                        <th className="px-6 py-4 text-right">Средн. DPS</th>
                        <th className="px-6 py-4 text-right">Средн. HPS</th>
                        <th className="px-6 py-4 text-right">Бой</th>
                        <th className="px-6 py-4 text-right">Роли</th>
                    </tr>
                    </thead>

                    <tbody className="divide-y divide-gray-800">
                    {rows.map(({ e, bossName, fightSec, avgDps, avgHps }) => {
                        const isOpen = !!openMap[e.id];
                        const isLoading = !!loadingMap[e.id];
                        const players = playersMap[e.id];
                        const err = errorMap[e.id];

                        return (
                            <Fragment key={e.id}>
                                {/* ENCOUNTER ROW */}
                                <tr
                                    className="cursor-pointer hover:bg-gray-800/40 transition-colors"
                                    onClick={() => onToggle(e)}
                                    title="Нажми, чтобы раскрыть игроков"
                                >
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                                        {formatClock(e.startTime)}
                                    </td>

                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="flex items-center gap-3">
                                            <BossIcon bossName={bossName} />

                                            <div className="min-w-0">
                                                <div className="flex items-center gap-2">
                                                    <div className="truncate font-semibold text-white">{bossName}</div>

                                                    <span
                                                        className={[
                                                            'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold border',
                                                            e.success
                                                                ? 'border-emerald-900/40 bg-emerald-950/30 text-emerald-200'
                                                                : 'border-amber-900/40 bg-amber-950/30 text-amber-200',
                                                        ].join(' ')}
                                                    >
                              {e.success ? 'Kill' : 'Wipe'}
                            </span>

                                                    <span className="text-gray-500 text-sm ml-2">{isOpen ? '▲' : '▼'}</span>
                                                </div>

                                                <div className="text-xs text-gray-500">
                                                    {fightSec > 0 ? `длительность: ${formatDuration(fightSec)}` : ''}
                                                </div>
                                            </div>
                                        </div>
                                    </td>

                                    <td className="px-6 py-4 whitespace-nowrap text-right">
                                        <div className="text-sm font-semibold text-white">{formatCompact(avgDps)}</div>
                                        <div className="text-xs text-gray-500">dps</div>
                                    </td>

                                    <td className="px-6 py-4 whitespace-nowrap text-right">
                                        <div className="text-sm font-semibold text-white">{formatCompact(avgHps)}</div>
                                        <div className="text-xs text-gray-500">hps</div>
                                    </td>

                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-200">
                      <span className="rounded-lg border border-gray-800 bg-gray-950/40 px-2 py-1">
                        {formatDuration(fightSec)}
                      </span>
                                    </td>

                                    <td className="px-6 py-4 whitespace-nowrap text-right">
                                        <div className="inline-flex items-center gap-3 text-sm text-gray-200">
                                            <span title="Танки">🛡️ {e.tanks}</span>
                                            <span title="Хилы">💚 {e.healers}</span>
                                            <span title="ДД">⚔️ {e.damageDealers}</span>
                                        </div>
                                    </td>
                                </tr>

                                {/* PLAYERS PANEL */}
                                {isOpen && (
                                    <tr className="bg-gray-950/20">
                                        <td colSpan={8} className="px-6 py-4">
                                            {isLoading && <div className="text-sm text-gray-400">Загрузка игроков…</div>}
                                            {!isLoading && err && <div className="text-sm text-red-300">{err}</div>}

                                            {!isLoading && !err && players && (
                                                <div className="rounded-xl border border-gray-800 bg-gray-900/30 overflow-hidden">
                                                    <div className="overflow-x-auto">
                                                        <table className="min-w-full">
                                                            <thead className="bg-gray-950/60">
                                                            <tr className="text-left text-xs font-semibold uppercase tracking-wider text-gray-400">
                                                                <th className="px-5 py-3">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'player')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors"
                                                                    >
                                                                        Игрок
                                                                        <SortIcon field="player" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'role')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors"
                                                                    >
                                                                        Роль
                                                                        <SortIcon field="role" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3 text-right">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'dps')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors ml-auto"
                                                                    >
                                                                        DPS
                                                                        <SortIcon field="dps" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3 text-right">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'damage')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors ml-auto"
                                                                    >
                                                                        Общий урон
                                                                        <SortIcon field="damage" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3 text-right">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'hps')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors ml-auto"
                                                                    >
                                                                        HPS
                                                                        <SortIcon field="hps" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3 text-right">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'healing')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors ml-auto"
                                                                    >
                                                                        Общий отхил
                                                                        <SortIcon field="healing" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3 text-right">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'absorb')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors ml-auto"
                                                                    >
                                                                        Поглощения
                                                                        <SortIcon field="absorb" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                                <th className="px-5 py-3 text-right">
                                                                    <button
                                                                        onClick={() => handleSort(e.id, 'ilvl')}
                                                                        className="flex items-center gap-1 hover:text-gray-200 transition-colors ml-auto"
                                                                    >
                                                                        iLVL
                                                                        <SortIcon field="ilvl" currentField={sortConfig?.encounterId === e.id ? sortConfig.field : null} direction={sortConfig?.encounterId === e.id ? sortConfig.direction : null} />
                                                                    </button>
                                                                </th>
                                                            </tr>
                                                            </thead>

                                                            <tbody className="divide-y divide-gray-800">
                                                            {players.map((p, idx) => {
                                                                const specIcon = getSpecIcon(
                                                                    String(p.characterClass ?? ''),
                                                                    p.specName ?? null,
                                                                    p.className ?? null
                                                                );

                                                                const nameColor = getClassColor(
                                                                    String(p.characterClass ?? ''),
                                                                    p.className ?? null
                                                                );

                                                                const role = roleInfo(p.role);
                                                                const fightSec = diffSeconds(e.startTime, e.endTime);

                                                                return (
                                                                    <tr key={`${p.playerName}-${idx}`} className="hover:bg-gray-800/30">
                                                                        <td className="px-5 py-3 whitespace-nowrap">
                                                                            <div className="flex items-center gap-3">
                                                                                {specIcon ? (
                                                                                    <Image
                                                                                        src={specIcon}
                                                                                        alt={p.specName ?? 'spec'}
                                                                                        width={32}
                                                                                        height={32}
                                                                                        className="w-8 h-8 rounded-lg flex-shrink-0"
                                                                                    />
                                                                                ) : (
                                                                                    <span className="w-8 h-8 rounded-lg bg-gray-800 flex-shrink-0" />
                                                                                )}

                                                                                <div className="min-w-0">
                                                                                    <div className="font-semibold truncate" style={{ color: nameColor }}>
                                                                                        {p.playerName}
                                                                                    </div>
                                                                                    <div className="text-xs text-gray-500 truncate">
                                                                                        {p.className || p.characterClass} • {p.specName ?? '—'}
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-sm text-gray-200">
                                          <span className="inline-flex items-center gap-2">
                                            <span>{role.emoji}</span>
                                            <span className="text-gray-400">{role.text}</span>
                                          </span>
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-right text-sm font-semibold text-white">
                                                                            {Number.isFinite(p.dps) ? Math.round(p.dps).toLocaleString('ru-RU') : '—'}
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-right text-sm text-gray-200">
                                                                            {Number.isFinite(p.damageDone) ? formatCompact(p.damageDone) : '—'}
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-right text-sm font-semibold text-blue-400">
                                                                            {(() => {
                                                                                const hps = fightSec > 0 && p.role === "2"
                                                                                    ? (p.healingDone + (p.absorbProvided || 0)) / fightSec
                                                                                    : 0;
                                                                                return Number.isFinite(hps) && hps > 0 ? Math.round(hps).toLocaleString('ru-RU') : '—';
                                                                            })()}
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-right text-sm text-gray-200">
                                                                            {Number.isFinite(p.healingDone) ? formatCompact(p.healingDone) : '—'}
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-right text-sm text-gray-200">
                                                                            {Number.isFinite(p.absorbProvided) ? formatCompact(p.absorbProvided) : '—'}
                                                                        </td>

                                                                        <td className="px-5 py-3 whitespace-nowrap text-right text-sm text-gray-200">
                                                                            {(() => {
                                                                                const ilvl = parseFloat(p.maxGearScore || p.maxAverageGearScore || '0');
                                                                                return Number.isFinite(ilvl) && ilvl > 0 ? Math.round(ilvl).toString() : '—';
                                                                            })()}
                                                                        </td>
                                                                    </tr>
                                                                );
                                                            })}
                                                            </tbody>
                                                        </table>
                                                    </div>

                                                    <div className="px-5 py-3 text-xs text-gray-500 border-t border-gray-800 flex items-center justify-between">
                                                        <span>Игроков: {players.length}</span>
                                                        <Link href={`/encounters/${e.id}`} className="text-gray-300 hover:text-white">
                                                            Открыть страницу энкаунтера →
                                                        </Link>
                                                    </div>
                                                </div>
                                            )}

                                            {!isLoading && !err && !players && <div className="text-sm text-gray-400">Нет данных</div>}
                                        </td>
                                    </tr>
                                )}
                            </Fragment>
                        );
                    })}
                    </tbody>
                </table>
            </div>

            <div className="px-6 py-4 text-xs text-gray-500 border-t border-gray-800">
                DPS/HPS сверху = totalDamage/totalHealing ÷ длительность боя (endTime - startTime).
            </div>
        </div>
    );
}
