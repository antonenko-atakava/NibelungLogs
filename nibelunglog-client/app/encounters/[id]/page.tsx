import Link from 'next/link';
import { notFound } from 'next/navigation';
import { api } from '@/lib/api';
import type { PlayerEncounterDto } from '@/types/api/PlayerEncounterDto';

interface EncounterDetailPageProps {
  params: Promise<{ id: string }>;
  searchParams?: Promise<{ q?: string; role?: string; sort?: string; dir?: string }>;
}

function formatNumber(value: number) {
  if (!Number.isFinite(value)) return '—';

  const sign = value < 0 ? '-' : '';
  const abs = Math.abs(value);

  const fmt = (n: number, digits: number) => {
    const s = n.toFixed(digits);
    return s.endsWith('.0') ? s.slice(0, -2) : s;
  };

  if (abs >= 1_000_000_000) return `${sign}${fmt(abs / 1_000_000_000, 2)}B`;
  if (abs >= 1_000_000) return `${sign}${fmt(abs / 1_000_000, 1)}M`;
  if (abs >= 1_000) return `${sign}${fmt(abs / 1_000, 1)}K`;
  return `${sign}${Math.round(abs).toLocaleString('ru-RU')}`;
}

function safeLower(s: unknown) {
  return (typeof s === 'string' ? s : '').toLowerCase();
}

function pickRoleBadge(role: string) {
  const r = safeLower(role);
  if (r.includes('tank')) return 'border-cyan-900/50 bg-cyan-950/40 text-cyan-200';
  if (r.includes('heal')) return 'border-emerald-900/50 bg-emerald-950/40 text-emerald-200';
  if (r.includes('dps') || r.includes('damage')) return 'border-amber-900/50 bg-amber-950/40 text-amber-200';
  return 'border-gray-800 bg-gray-950/20 text-gray-300';
}

function sortPlayers(players: PlayerEncounterDto[], sort: string, dir: 'asc' | 'desc') {
  const m = dir === 'asc' ? 1 : -1;

  const by = (get: (p: PlayerEncounterDto) => number | string) =>
      [...players].sort((a, b) => {
        const va = get(a);
        const vb = get(b);
        if (typeof va === 'number' && typeof vb === 'number') return (va - vb) * m;
        return String(va).localeCompare(String(vb), 'ru') * m;
      });

  switch (sort) {
    case 'name':
      return by((p) => p.playerName ?? '');
    case 'class':
      return by((p) => p.characterClass ?? '');
    case 'spec':
      return by((p) => p.characterSpec ?? '');
    case 'role':
      return by((p) => p.role ?? '');
    case 'dps':
      return by((p) => Number(p.dps ?? 0));
    case 'damage':
      return by((p) => Number(p.damageDone ?? 0));
    case 'healing':
      return by((p) => Number(p.healingDone ?? 0));
    default:
      // по умолчанию DPS desc
      return by((p) => Number(p.dps ?? 0));
  }
}

export default async function EncounterDetailPage({ params, searchParams }: EncounterDetailPageProps) {
  const { id } = await params;
  const encounterId = Number.parseInt(id, 10);
  if (!Number.isFinite(encounterId) || encounterId <= 0) notFound();

  const sp = searchParams ? await searchParams : undefined;

  const q = (sp?.q ?? '').trim();
  const roleFilter = (sp?.role ?? '').trim(); // '', tank, healer, dps
  const sort = (sp?.sort ?? 'dps').trim(); // dps|damage|healing|name|class|spec|role
  const dir = ((sp?.dir ?? 'desc').trim().toLowerCase() === 'asc' ? 'asc' : 'desc') as 'asc' | 'desc';

  let players: PlayerEncounterDto[];
  try {
    players = await api.getEncounterPlayers(encounterId);
  } catch {
    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <Link href="/raids" className="text-gray-400 hover:text-white mb-8 inline-block transition-colors">
            ← Назад к рейдам
          </Link>

          <div className="rounded-2xl border border-red-900/40 bg-red-950/30 p-8 text-center">
            <p className="text-red-200 font-semibold text-lg">Ошибка подключения к API</p>
            <p className="mt-2 text-sm text-red-200/80">Убедитесь, что API сервер запущен (например http://localhost:5097)</p>
          </div>
        </main>
    );
  }

  // фильтрация
  const filtered = players.filter((p) => {
    const matchesQ =
        !q ||
        safeLower(p.playerName).includes(q.toLowerCase()) ||
        safeLower(p.characterClass).includes(q.toLowerCase()) ||
        safeLower(p.characterSpec).includes(q.toLowerCase());

    const r = safeLower(p.role);
    const matchesRole =
        !roleFilter ||
        (roleFilter === 'tank' && r.includes('tank')) ||
        (roleFilter === 'healer' && (r.includes('heal') || r.includes('healer'))) ||
        (roleFilter === 'dps' && (r.includes('dps') || r.includes('damage')));

    return matchesQ && matchesRole;
  });

  const sorted = sortPlayers(filtered, sort, dir);

  // сводка
  const totalPlayers = filtered.length;
  const totalDamage = filtered.reduce((s, p) => s + (Number(p.damageDone) || 0), 0);
  const totalHealing = filtered.reduce((s, p) => s + (Number(p.healingDone) || 0), 0);
  const avgDps = totalPlayers > 0 ? filtered.reduce((s, p) => s + (Number(p.dps) || 0), 0) / totalPlayers : 0;

  const topDps = Math.max(...filtered.map((p) => Number(p.dps) || 0), 0);
  const topDamage = Math.max(...filtered.map((p) => Number(p.damageDone) || 0), 0);
  const topHealing = Math.max(...filtered.map((p) => Number(p.healingDone) || 0), 0);

  const buildHref = (next: Partial<{ q: string; role: string; sort: string; dir: string }>) => {
    const usp = new URLSearchParams();
    const nq = next.q ?? q;
    const nr = next.role ?? roleFilter;
    const ns = next.sort ?? sort;
    const nd = next.dir ?? dir;

    if (nq) usp.set('q', nq);
    if (nr) usp.set('role', nr);
    if (ns && ns !== 'dps') usp.set('sort', ns);
    if (nd && nd !== 'desc') usp.set('dir', nd);
    const qs = usp.toString();
    return qs ? `/encounters/${encounterId}?${qs}` : `/encounters/${encounterId}`;
  };

  const SortLink = ({ label, col }: { label: string; col: string }) => {
    const isActive = sort === col;
    const nextDir = isActive && dir === 'desc' ? 'asc' : 'desc';
    return (
        <Link
            href={buildHref({ sort: col, dir: nextDir })}
            className={[
              'inline-flex items-center gap-1 hover:text-white transition-colors',
              isActive ? 'text-white' : 'text-gray-400',
            ].join(' ')}
            title="Сортировать"
        >
          {label}
          {isActive ? <span className="text-xs">{dir === 'desc' ? '↓' : '↑'}</span> : null}
        </Link>
    );
  };

  return (
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <Link href="/raids" className="text-gray-400 hover:text-white mb-8 inline-block transition-colors">
          ← Назад к рейдам
        </Link>

        <header className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h1 className="text-4xl font-bold text-white">Энкаунтер #{encounterId}</h1>
            <p className="mt-2 text-sm text-gray-400">
              Игроки: <span className="text-gray-200 font-semibold">{totalPlayers}</span> · Урон:{" "}
              <span className="text-gray-200 font-semibold">{formatNumber(totalDamage)}</span> · Хил:{" "}
              <span className="text-gray-200 font-semibold">{formatNumber(totalHealing)}</span> · Средний DPS:{" "}
              <span className="text-gray-200 font-semibold">{formatNumber(avgDps)}</span>
            </p>
          </div>

          <div className="flex items-center gap-3">
            <Link
                href={`/encounters/${encounterId}`}
                className="rounded-xl border border-gray-800 bg-gray-950/30 px-4 py-2 text-sm font-semibold text-gray-300 hover:bg-gray-950/50 transition-colors"
                title="Сбросить фильтры"
            >
              Сбросить
            </Link>
          </div>
        </header>

        {/* Filters */}
        <section className="mb-8 rounded-2xl border border-gray-800 bg-gray-900/40 p-4 sm:p-5">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex flex-col sm:flex-row gap-3 sm:items-center">
              <div className="text-xs font-semibold uppercase tracking-wider text-gray-500">Фильтры</div>

              <div className="flex flex-wrap gap-2">
                {[
                  { key: '', label: 'Все' },
                  { key: 'tank', label: 'Танки' },
                  { key: 'healer', label: 'Хилы' },
                  { key: 'dps', label: 'ДД' },
                ].map((x) => (
                    <Link
                        key={x.key}
                        href={buildHref({ role: x.key })}
                        className={[
                          'rounded-full border px-3 py-1 text-xs font-semibold transition-colors',
                          (roleFilter || '') === x.key
                              ? 'border-orange-700/60 bg-orange-950/30 text-orange-200'
                              : 'border-gray-800 bg-gray-950/20 text-gray-300 hover:bg-gray-950/40',
                        ].join(' ')}
                    >
                      {x.label}
                    </Link>
                ))}
              </div>
            </div>

            <div className="text-sm text-gray-400">
              Поиск делается через query string: добавь <span className="text-gray-300 font-semibold">?q=имя</span>
            </div>
          </div>

          {(q || roleFilter) ? (
              <div className="mt-4 text-xs text-gray-500">
                Активно: {q ? <span className="text-gray-300">поиск “{q}”</span> : null}
                {q && roleFilter ? <span className="text-gray-600"> · </span> : null}
                {roleFilter ? <span className="text-gray-300">роль: {roleFilter}</span> : null}
              </div>
          ) : null}
        </section>

        {/* Table */}
        <section className="bg-gray-900 rounded-2xl shadow-2xl border border-gray-800 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-800">
              <thead className="bg-gray-950">
              <tr>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <span className="text-gray-400">Ранг</span>
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="Игрок" col="name" />
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="Класс" col="class" />
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="Спек" col="spec" />
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="Роль" col="role" />
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="DPS" col="dps" />
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="Урон" col="damage" />
                </th>
                <th className="px-8 py-4 text-left text-xs font-bold uppercase tracking-wider">
                  <SortLink label="Лечение" col="healing" />
                </th>
              </tr>
              </thead>

              <tbody className="bg-gray-900 divide-y divide-gray-800">
              {sorted.map((player, index) => {
                const dps = Number(player.dps) || 0;
                const dmg = Number(player.damageDone) || 0;
                const heal = Number(player.healingDone) || 0;

                const dpsPct = topDps > 0 ? Math.round((dps / topDps) * 100) : 0;
                const dmgPct = topDamage > 0 ? Math.round((dmg / topDamage) * 100) : 0;
                const healPct = topHealing > 0 ? Math.round((heal / topHealing) * 100) : 0;

                return (
                    <tr key={`${player.playerName}-${index}`} className="hover:bg-gray-800 transition-colors">
                      <td className="px-8 py-5 whitespace-nowrap">
                        <span className="text-sm font-bold text-white">#{index + 1}</span>
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap">
                        <span className="text-base font-semibold text-white">{player.playerName}</span>
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                        {player.characterClass || '—'}
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                        {player.characterSpec || '—'}
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap text-sm">
                      <span className={['inline-flex rounded-full border px-3 py-1 text-xs font-semibold', pickRoleBadge(player.role)].join(' ')}>
                        {player.role || '—'}
                      </span>
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap">
                        <div className="flex items-center gap-3">
                          <span className="text-base font-bold text-white">{formatNumber(dps)}</span>
                          <div className="hidden sm:block w-24">
                            <div className="h-2 rounded-full bg-gray-800 overflow-hidden">
                              <div className="h-full bg-emerald-500/80" style={{ width: `${dpsPct}%` }} />
                            </div>
                          </div>
                        </div>
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-200">
                        <div className="flex items-center gap-3">
                          <span>{formatNumber(dmg)}</span>
                          <div className="hidden sm:block w-24">
                            <div className="h-2 rounded-full bg-gray-800 overflow-hidden">
                              <div className="h-full bg-amber-500/80" style={{ width: `${dmgPct}%` }} />
                            </div>
                          </div>
                        </div>
                      </td>

                      <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-200">
                        <div className="flex items-center gap-3">
                          <span>{formatNumber(heal)}</span>
                          <div className="hidden sm:block w-24">
                            <div className="h-2 rounded-full bg-gray-800 overflow-hidden">
                              <div className="h-full bg-cyan-500/80" style={{ width: `${healPct}%` }} />
                            </div>
                          </div>
                        </div>
                      </td>
                    </tr>
                );
              })}

              {sorted.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-8 py-14 text-center text-gray-400">
                      Ничего не найдено по текущим фильтрам.
                    </td>
                  </tr>
              ) : null}
              </tbody>
            </table>
          </div>
        </section>
      </main>
  );
}
