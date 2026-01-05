import { api } from '@/lib/api';
import { notFound } from 'next/navigation';
import Link from 'next/link';
import { PlayerEncounterDto } from '@/types/api/PlayerEncounterDto';

interface EncounterDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function EncounterDetailPage({ params }: EncounterDetailPageProps) {
  const { id } = await params;
  const encounterId = parseInt(id, 10);

  if (isNaN(encounterId))
    notFound();

  let players;
  try {
    players = await api.getEncounterPlayers(encounterId);
  } catch (error) {
    return (
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <Link href="/raids" className="text-gray-400 hover:text-white mb-8 inline-block transition-colors">
          ← Назад к рейдам
        </Link>
        <div className="text-center py-20">
          <p className="text-red-400 text-lg mb-4">Ошибка подключения к API</p>
          <p className="text-gray-400 text-sm">Убедитесь, что API сервер запущен на http://localhost:5097</p>
        </div>
      </main>
    );
  }

  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <Link href="/raids" className="text-gray-400 hover:text-white mb-8 inline-block transition-colors">
        ← Назад к рейдам
      </Link>

      <div className="mb-8">
        <h1 className="text-4xl font-bold text-white mb-6">Игроки энкаунтера</h1>
      </div>

      <div className="bg-gray-900 rounded-2xl shadow-2xl border border-gray-800 overflow-hidden">
        <table className="min-w-full divide-y divide-gray-800">
          <thead className="bg-gray-950">
            <tr>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Ранг
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Игрок
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Класс
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Специализация
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Роль
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                DPS
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Урон
              </th>
              <th className="px-8 py-4 text-left text-xs font-bold text-gray-400 uppercase tracking-wider">
                Лечение
              </th>
            </tr>
          </thead>
          <tbody className="bg-gray-900 divide-y divide-gray-800">
            {players.map((player, index) => (
              <tr key={`${player.playerName}-${index}`} className="hover:bg-gray-800 transition-colors">
                <td className="px-8 py-5 whitespace-nowrap">
                  <span className="text-sm font-bold text-white">#{index + 1}</span>
                </td>
                <td className="px-8 py-5 whitespace-nowrap">
                  <span className="text-base font-semibold text-white">
                    {player.playerName}
                  </span>
                </td>
                <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-400">
                  {player.characterClass}
                </td>
                <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-400">
                  {player.characterSpec}
                </td>
                <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-400">
                  {player.role}
                </td>
                <td className="px-8 py-5 whitespace-nowrap">
                  <span className="text-base font-bold text-white">
                    {Math.round(player.dps).toLocaleString()}
                  </span>
                </td>
                <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                  {(player.damageDone / 1000000).toFixed(1)}M
                </td>
                <td className="px-8 py-5 whitespace-nowrap text-sm font-medium text-gray-300">
                  {(player.healingDone / 1000000).toFixed(1)}M
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </main>
  );
}

